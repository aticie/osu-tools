using System.Windows.Controls;
using System.Collections.Generic;
using osu.Framework.IO.Network;
using osu.Game.Beatmaps;
using PerformanceCalculator;
using System.IO;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Mods;
using System.Linq;
using osu.Game.Scoring;
using osu.Game.Rulesets.Scoring;
using Alba.CsConsoleFormat;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using osu.Game.Rulesets.Osu.Difficulty;

namespace PerformanceCalculatorGUI.Profile
{
    internal class ProfileCalc
    {
        public ProfileCalc()
        {
        }

        private const string base_url = "https://osu.ppy.sh";
        public string Key = "";
        public string ProfileName = "";
        public Document ResultsDoc = new Document();

        public async Task ExecuteAsync(Label status_block)
        {
            if (Key == "" || ProfileName == "")
            {
                return;
            }

            var displayPlays = new List<UserPlayInfo>();
            var ruleset = LegacyHelper.GetRulesetFromLegacyID(0);
            status_block.Dispatcher.Invoke(() =>
            {
                status_block.Content = "Getting user data...";
            });

            dynamic userData = await getJsonFromApi($"get_user?k={Key}&u={ProfileName}&m={0}");

            if (!((JArray)userData).Any())
            {
                ResultsDoc = new Document(new Span("Could not find user " + ProfileName));
                return;
            }

            userData = userData[0];

            status_block.Dispatcher.Invoke(() =>
            {
                status_block.Content = "Getting user top scores...";
            });

            foreach (var play in await getJsonFromApi($"get_user_best?k={Key}&u={ProfileName}&m={0}&limit=100"))
            {
                string beatmapID = play.beatmap_id;

                string cachePath = Path.Combine("cache", $"{beatmapID}.osu");

                if (!File.Exists(cachePath))
                {
                    status_block.Dispatcher.Invoke(() =>
                    {
                        status_block.Content = $"Downloading {beatmapID}.osu...";
                    });
                    await new FileWebRequest(cachePath, $"{base_url}/osu/{beatmapID}").PerformAsync();
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
                        { HitResult.Good, (int)play.countkatu },
                        { HitResult.Ok, (int)play.count100 },
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
                    AimPP = categoryAttribs["Aim"],
                    TapPP = categoryAttribs["Speed"],
                    AccPP = categoryAttribs["Accuracy"],
                    LivePP = play.pp,
                    Mods = mods.Length > 0 ? mods.Select(m => m.Acronym).Aggregate((c, n) => $"{c}, {n}") : "",
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

            ResultsDoc = new Document(
                new Span($"User:     {userData.username}"), "\n",
                new Span($"Live PP:  {totalLivePP:F1} (including {playcountBonusPP:F1}pp from playcount)"), "\n",
                new Span($"Local PP: {totalLocalPP:F1} ({totalDiffPP:+0.0;-0.0;-})"), "\n",
                new Alba.CsConsoleFormat.Grid
                {
                    Columns =
                    {
                        GridLength.Auto, GridLength.Auto, GridLength.Auto, GridLength.Auto, GridLength.Auto, GridLength.Auto,
                        GridLength.Auto, GridLength.Auto, GridLength.Auto, GridLength.Auto, GridLength.Auto, GridLength.Auto
                    },
                    Children =
                    {
                        new Cell("beatmap"),
                        new Cell("mods") { Align = Align.Center },
                        new Cell("live pp"),
                        new Cell("acc") { Align = Align.Center },
                        new Cell("miss"),
                        new Cell("combo") { Align = Align.Center },
                        new Cell("aim pp"),
                        new Cell("tap pp"),
                        new Cell("acc pp"),
                        new Cell("local pp"),
                        new Cell("pp change"),
                        new Cell("position change"),
                        localOrdered.Select(item => new[]
                        {
                            new Cell($"{item.Beatmap.OnlineBeatmapID} - {item.Beatmap.ToString().Substring(0, Math.Min(80, item.Beatmap.ToString().Length))}"),
                            new Cell(item.Mods) { Align = Align.Center },
                            new Cell($"{item.LivePP:F1}") { Align = Align.Right },
                            new Cell($"{item.PlayAccuracy * 100f:F2}" + " %") { Align = Align.Center },
                            new Cell($"{item.MissCount}") { Align = Align.Center },
                            new Cell($"{item.PlayMaxCombo}/{item.BeatmapMaxCombo}") { Align = Align.Center },
                            new Cell($"{item.AimPP:F1}") { Align = Align.Right },
                            new Cell($"{item.TapPP:F1}") { Align = Align.Right },
                            new Cell($"{item.AccPP:F1}") { Align = Align.Right },
                            new Cell($"{item.LocalPP:F1}") { Align = Align.Right },
                            new Cell($"{item.LocalPP - item.LivePP:F1}") { Align = Align.Right },
                            new Cell($"{liveOrdered.IndexOf(item) - localOrdered.IndexOf(item):+0;-0;-}") { Align = Align.Center },
                        })
                    }
                }
            );
        }

        private async Task<dynamic> getJsonFromApi(string request)
        {
            using (var req = new JsonWebRequest<dynamic>($"{base_url}/api/{request}"))
            {
                await req.PerformAsync();
                return req.ResponseObject;
            }
        }
    }

    public struct UserPlayInfo
    {
        public double LocalPP;
        public double LivePP;
        public double AimPP;
        public double TapPP;
        public double AccPP;

        public double PlayAccuracy;
        public int PlayMaxCombo;
        public int BeatmapMaxCombo;
        public int MissCount;

        public BeatmapInfo Beatmap;

        public string Mods;
    }
}
