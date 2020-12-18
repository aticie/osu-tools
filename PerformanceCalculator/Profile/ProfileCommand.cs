// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Alba.CsConsoleFormat;
using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using osu.Framework.IO.Network;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace PerformanceCalculator.Profile
{
    [Command(Name = "profile", Description = "Computes the total performance (pp) of a profile.")]
    public class ProfileCommand : ProcessorCommand
    {
        [UsedImplicitly]
        [Required]
        [Argument(0, Name = "user", Description = "User ID is preferred, but username should also work.")]
        public string ProfileName { get; }

        [UsedImplicitly]
        [Required]
        [Argument(1, Name = "api key", Description = "API Key, which you can get from here: https://osu.ppy.sh/p/api")]
        public string Key { get; }

        [UsedImplicitly]
        [Option(Template = "-r|--ruleset:<ruleset-id>", Description = "The ruleset to compute the profile for. 0 - osu!, 1 - osu!taiko, 2 - osu!catch, 3 - osu!mania. Defaults to osu!.")]
        [AllowedValues("0", "1", "2", "3")]
        public int? Ruleset { get; }

        [Option(CommandOptionType.MultipleValue, Template = "-c|--columns <attribute_name>", Description =
            "Extra columns to display from beatmap category attribs, for example 'Tap Rhythm pp'. Multiple can be added at once -c col1 -c col2")]
        public string[] ExtraColumns { get; set; }

        [Option(CommandOptionType.SingleValue, Template = "-s|--sort <attribute_name>", Description = "What column to sort by (defaults to pp of the play)")]
        public string SortColumnName { get; set; }

        private const int max_name_length = 90;

        private const string base_url = "https://osu.ppy.sh";

        public override void Execute()
        {
            var displayPlays = new List<UserPlayInfo>();

            var ruleset = LegacyHelper.GetRulesetFromLegacyID(Ruleset ?? 0);

            Console.WriteLine("Getting user data...");
            dynamic userData = getJsonFromApi($"get_user?k={Key}&u={ProfileName}&m={Ruleset}")[0];

            Console.WriteLine("Getting user top scores...");

            foreach (var play in getJsonFromApi($"get_user_best?k={Key}&u={ProfileName}&m={Ruleset}&limit=100"))
            {
                string beatmapID = play.beatmap_id;

                string cachePath = Path.Combine("cache", $"{beatmapID}.osu");

                if (!File.Exists(cachePath))
                {
                    Console.WriteLine($"Downloading {beatmapID}.osu...");
                    new FileWebRequest(cachePath, $"{base_url}/osu/{beatmapID}").Perform();
                }

                Mod[] mods = ruleset.ConvertFromLegacyMods((LegacyMods)play.enabled_mods).ToArray();

                var working = new ProcessorWorkingBeatmap(cachePath, (int)play.beatmap_id);

                var scoreInfo = new ScoreInfo
                {
                    Ruleset = ruleset.RulesetInfo,
                    MaxCombo = play.maxcombo,
                    Mods = mods,
                    Statistics = new Dictionary<HitResult, int>
                    {
                        { HitResult.Perfect, (int)play.countgeki },
                        { HitResult.Great, (int)play.count300 },
                        { HitResult.Good, (int)play.count100 },
                        { HitResult.Ok, (int)play.countkatu },
                        { HitResult.Meh, (int)play.count50 },
                        { HitResult.Miss, (int)play.countmiss }
                    }
                };
                var score = new ProcessorScoreDecoder(working).Parse(scoreInfo);

                var categoryAttribs = new Dictionary<string, double>();
                OsuPerformanceCalculator calculator = (OsuPerformanceCalculator)ruleset.CreatePerformanceCalculator(working, score.ScoreInfo);
                double localPP = calculator.Calculate(categoryAttribs);

                var thisPlay = new UserPlayInfo
                {
                    Beatmap = working.BeatmapInfo,
                    LocalPP = localPP,
                    MapCategoryAttribs = categoryAttribs,
                    LivePP = play.pp,
                    Mods = mods.Length > 0 ? mods.Select(m => m.Acronym).Aggregate((c, n) => $"{c}|{n}") : "",
                    PlayMaxCombo = scoreInfo.MaxCombo,
                    BeatmapMaxCombo = calculator.Attributes.MaxCombo,
                    PlayAccuracy = scoreInfo.Accuracy,
                    MissCount = scoreInfo.Statistics[HitResult.Miss]
                };

                displayPlays.Add(thisPlay);
            }

            var localOrdered = displayPlays.OrderByDescending(p => p.LocalPP).ToList();
            var liveOrdered = displayPlays.OrderByDescending(p => p.LivePP).ToList();

            int index = 0;
            double totalLocalPP = localOrdered.Sum(play => Math.Pow(0.95, index++) * play.LocalPP);

            double totalLivePP = userData.pp_raw;

            index = 0;
            double nonBonusLivePP = liveOrdered.Sum(play => Math.Pow(0.95, index++) * play.LivePP);

            //todo: implement properly. this is pretty damn wrong.
            var playcountBonusPP = (totalLivePP - nonBonusLivePP);
            totalLocalPP += playcountBonusPP;
            double totalDiffPP = totalLocalPP - totalLivePP;

            for (int i = 0; i < localOrdered.Count; i++)
            {
                localOrdered[i].Position = i + 1;
            }

            if (SortColumnName != null)
            {
                localOrdered.Sort((s1, s2) =>
                    s2.MapCategoryAttribs[SortColumnName].CompareTo(s1.MapCategoryAttribs[SortColumnName]));
            }

            foreach (var playInfo in localOrdered)
            {
                if (playInfo.Beatmap.ToString().Length > max_name_length)
                {
                    playInfo.MapName = "..." + playInfo.Beatmap.ToString().Substring(playInfo.Beatmap.ToString().Length - max_name_length);
                }
                else
                {
                    playInfo.MapName = playInfo.Beatmap.ToString();
                }
            }

            Grid grid = new Grid();
            grid.Columns.Add(createColumns(10 + (ExtraColumns?.Length ?? 0)));
            grid.Children.Add(
                new Cell("#") { Align = Align.Center },
                new Cell("beatmap"),
                new Cell("mods") { Align = Align.Center },
                new Cell("live pp"),
                new Cell("acc") { Align = Align.Center },
                new Cell("miss"),
                new Cell("combo") { Align = Align.Center }
            );

            if (ExtraColumns != null)
            {
                foreach (var extraColumn in ExtraColumns)
                {
                    grid.Children.Add(new Cell(extraColumn) { Align = Align.Center });
                }
            }

            grid.Children.Add(
                new Cell("local pp"),
                new Cell("pp change"),
                new Cell("position change")
            );

            grid.Children.Add(localOrdered.Select((item) =>
            {
                List<Cell> cells = new List<Cell>
                {
                    new Cell(item.Position) { Align = Align.Left },
                    new Cell($" {item.Beatmap.OnlineBeatmapID} - {item.MapName}"),
                    new Cell(item.Mods) { Align = Align.Right },
                    new Cell($"{item.LivePP:F1}") { Align = Align.Right },
                    new Cell($"{item.PlayAccuracy * 100f:F2}" + " %") { Align = Align.Right },
                    new Cell($"{item.MissCount}") { Align = Align.Right },
                    new Cell($"{item.PlayMaxCombo}/{item.BeatmapMaxCombo}") { Align = Align.Right },
                };

                if (ExtraColumns != null)
                {
                    cells.AddRange(ExtraColumns.Select(extraColumn => new Cell($"{item.MapCategoryAttribs[extraColumn]:F1}") { Align = Align.Right }));
                }

                cells.AddRange(new List<Cell>
                    {
                        new Cell($"{item.LocalPP:F1}") { Align = Align.Right },
                        new Cell($"{item.LocalPP - item.LivePP:F1}") { Align = Align.Right },
                        new Cell($"{liveOrdered.IndexOf(item) - localOrdered.IndexOf(item):+0;-0;-}") { Align = Align.Center },
                    }
                );

                return cells;
            }));

            OutputDocument(new Document(
                new Span($"User:     {userData.username}"), "\n",
                new Span($"Live PP:  {totalLivePP:F1} (including {playcountBonusPP:F1}pp from playcount)"), "\n",
                new Span($"Local PP: {totalLocalPP:F1} ({totalDiffPP:+0.0;-0.0;-})"), "\n",
                grid
            ));
        }

        private dynamic getJsonFromApi(string request)
        {
            using (var req = new JsonWebRequest<dynamic>($"{base_url}/api/{request}"))
            {
                req.Perform();
                return req.ResponseObject;
            }
        }

        private List<Column> createColumns(int size)
        {
            List<Column> result = new List<Column>();

            for (int i = 0;
                 i < size;
                 i++)
            {
                result.Add(new Column { Width = GridLength.Auto });
            }

            return result;
        }
    }
}
