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
using MathNet.Numerics;

namespace StrainVisualizer
{
    public class OsuDifficultyCalculatorForVisualizer : DifficultyCalculator
    {
        private const double difficulty_multiplier = 0.0675;
        private const double star_rating_scale_factor = 1.485 * 3.0;
        private const double aim_star_factor = 1.1;
        private const double tapSpeed_star_factor = 2.0;
        private const double total_star_factor = 2.2;

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
            for (int i = 1; i < beatmap.HitObjects.Count; i++)
            {
                var lastLast = i > 1 ? beatmap.HitObjects[i - 2] : null;
                var last = beatmap.HitObjects[i - 1];
                var current = beatmap.HitObjects[i];

                yield return new OsuDifficultyHitObject(current, lastLast, last, clockRate);
            }
        }

        protected override Skill[] CreateSkills(IBeatmap beatmap) => new Skill[]
        {
            new AimSnap(),
            new AimFlow(),
            new TapStamina(),
            new TapSpeed(),
            new AimHybrid(),
            new TapRhythm()
        };

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            throw new NotImplementedException();
        }

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new OsuModDoubleTime(),
            new OsuModHalfTime(),
            new OsuModEasy(),
            new OsuModHardRock(),
        };

        public Skill[] CalculateStrains(IBeatmap beatmap, Mod[] mods, double clockRate)
        {
            var skills = CreateSkills(beatmap);

            if (!beatmap.HitObjects.Any())
                return null;

            var difficultyHitObjects = CreateDifficultyHitObjects(beatmap, clockRate).OrderBy(h => h.BaseObject.StartTime).ToList();

            foreach (DifficultyHitObject h in difficultyHitObjects)
            {
                foreach (Skill s in skills)
                    s.Process(h);
            }

            foreach (Skill s in skills)
            {
                s.Calculate(beatmap.BeatmapInfo.OnlineBeatmapID);
            }

            return skills;
        }
    }
}
