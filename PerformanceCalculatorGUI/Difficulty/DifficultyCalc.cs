using System.Collections.Generic;
using PerformanceCalculator;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using Alba.CsConsoleFormat;
using System.Linq;
using System;

namespace PerformanceCalculatorGUI.Difficulty
{
    internal class DifficultyCalc
    {
        public DifficultyCalc()
        {
        }

        public void ProcessBeatmap(string beatmap_path, string[] mods)
        {
            ProcessorWorkingBeatmap beatmap = new ProcessorWorkingBeatmap(beatmap_path);
            // Get the ruleset
            var ruleset = LegacyHelper.GetRulesetFromLegacyID(beatmap.BeatmapInfo.RulesetID);
            var attributes = ruleset.CreateDifficultyCalculator(beatmap).Calculate(getMods(ruleset, mods).ToArray());

            var beatmapName = $"{beatmap.BeatmapInfo.OnlineBeatmapID} - {beatmap.BeatmapInfo}";
            if (beatmapName.Length > 100)
            {
                beatmapName = beatmapName.Substring(0, 100) + "...";
            }

            var result = new Result
            {
                RulesetId = ruleset.RulesetInfo.ID ?? 0,
                Beatmap = beatmapName,
                Stars = attributes.StarRating.ToString("N3")
            };

            result.AttributeData = new List<(string, object)>
            {
            };

            CalcResults = new Document();

            CalcResults.Children.Add(new Span($"Ruleset: {ruleset.ShortName}"), "\n");

            var grid = new Grid();

            grid.Columns.Add(GridLength.Auto, GridLength.Auto);
            grid.Children.Add(new Cell("beatmap"), new Cell("star rating"));

            foreach (var attribute in result.AttributeData)
            {
                grid.Columns.Add(GridLength.Auto);
                grid.Children.Add(new Cell(attribute.name));
            }

            grid.Children.Add(new Cell(result.Beatmap), new Cell(result.Stars) { Align = Align.Right });
            foreach (var attribute in result.AttributeData)
                grid.Children.Add(new Cell(attribute.value) { Align = Align.Right });

            CalcResults.Children.Add(grid);

            CalcResults.Children.Add("\n");
        }

        private List<Mod> getMods(Ruleset ruleset, string[] Mods)
        {
            var mods = new List<Mod>();
            if (Mods == null)
                return mods;

            var availableMods = ruleset.GetAllMods().ToList();

            foreach (var modString in Mods)
            {
                Mod newMod = availableMods.FirstOrDefault(m => string.Equals(m.Acronym, modString, StringComparison.CurrentCultureIgnoreCase));
                mods.Add(newMod);
            }

            return mods;
        }

        private struct Result
        {
            public int RulesetId;
            public string Beatmap;
            public string Stars;
            public List<(string name, object value)> AttributeData;
        }

        public Document CalcResults = new Document();
    }
}
