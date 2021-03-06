// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Osu.Scoring;

namespace StrainVisualizer
{
    public class OsuDifficultyCalculatorForVisualizer : DifficultyCalculator
    {
        private const double difficulty_multiplier = 0.0675;

        private readonly List<OsuDifficultyHitObject> difficultyHitObjects = new List<OsuDifficultyHitObject>();

        public OsuDifficultyCalculatorForVisualizer(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        public double PointsTransformation(double skillRating) => Math.Pow(5.0f * Math.Max(1.0f, skillRating / difficulty_multiplier) - 4.0f, 3.0f) / 100000.0f;
        public double StarTransformation(double pointsRating) => difficulty_multiplier * (Math.Pow(100000.0f * pointsRating, 1.0f / 3.0f) + 4.0f) / 5.0f;

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            // The first jump is formed by the first two hitobjects of the map.
            // If the map has less than two OsuHitObjects, the enumerator will not return anything.
            difficultyHitObjects.Clear();
            for (int i = 1; i < beatmap.HitObjects.Count; i++)
            {
                var lastLast = i > 1 ? beatmap.HitObjects[i - 2] : null;
                var last = beatmap.HitObjects[i - 1];
                var current = beatmap.HitObjects[i];

                var difficultyHitObject = new OsuDifficultyHitObject(current, lastLast, last, difficultyHitObjects, clockRate);

                yield return difficultyHitObject;

                difficultyHitObjects.Add(difficultyHitObject);
            }
        }

        protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods) => new Skill[]
        {
            new Aim(mods),
            new Speed(mods)
        };

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new OsuDifficultyAttributes { Mods = mods, Skills = skills };

            double aimRating = Math.Sqrt(skills[0].DifficultyValue()) * difficulty_multiplier;
            double speedRating = Math.Sqrt(skills[1].DifficultyValue()) * difficulty_multiplier;
            double starRating = aimRating + speedRating + Math.Abs(aimRating - speedRating) / 2;

            HitWindows hitWindows = new OsuHitWindows();
            hitWindows.SetDifficulty(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty);

            // Todo: These int casts are temporary to achieve 1:1 results with osu!stable, and should be removed in the future
            double hitWindowGreat = (int)(hitWindows.WindowFor(HitResult.Great)) / clockRate;
            double preempt = (int)BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.ApproachRate, 1800, 1200, 450) / clockRate;

            int maxCombo = beatmap.HitObjects.Count;
            // Add the ticks + tail of the slider. 1 is subtracted because the head circle would be counted twice (once for the slider itself in the line above)
            maxCombo += beatmap.HitObjects.OfType<Slider>().Sum(s => s.NestedHitObjects.Count - 1);

            int hitCirclesCount = beatmap.HitObjects.Count(h => h is HitCircle);
            int spinnerCount = beatmap.HitObjects.Count(h => h is Spinner);

            return new OsuDifficultyAttributes
            {
                StarRating = starRating,
                Mods = mods,
                AimStrain = aimRating,
                SpeedStrain = speedRating,
                ApproachRate = preempt > 1200 ? (1800 - preempt) / 120 : (1200 - preempt) / 150 + 5,
                OverallDifficulty = (80 - hitWindowGreat) / 6,
                MaxCombo = maxCombo,
                HitCircleCount = hitCirclesCount,
                SpinnerCount = spinnerCount,
                Skills = skills
            };
        }

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new OsuModDoubleTime(),
            new OsuModHalfTime(),
            new OsuModEasy(),
            new OsuModHardRock(),
        };

        public Dictionary<string, List<(double, double)>> CalculateStrains(IBeatmap beatmap, Mod[] mods, double clockRate)
        {
            var skills = CreateSkills(beatmap, mods);

            if (!beatmap.HitObjects.Any())
                return null;

            var difficultyHitObjects = CreateDifficultyHitObjects(beatmap, clockRate).OrderBy(h => h.BaseObject.StartTime).ToList();

            var skill_strains = new Dictionary<string, List<(double, double)>> {};
            foreach (DifficultyHitObject h in difficultyHitObjects)
            {
                foreach (Skill s in skills)
                {
                    s.Process(h);
                    var skill_name = s.GetType().Name;
                    if (skill_strains.ContainsKey(skill_name))
                    {
                        skill_strains[skill_name].Add((s.CurrentStrain, h.BaseObject.StartTime));
                    }
                    else
                    {
                        skill_strains.Add(skill_name, new List<(double, double)> { (s.CurrentStrain, h.BaseObject.StartTime) });
                    }
                }
            }

            return skill_strains;
        }
    }
}
