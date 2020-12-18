// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;

namespace PerformanceCalculator.Profile
{
    /// <summary>
    /// Holds the live pp value, beatmap name, and mods for a user play.
    /// </summary>
    public class UserPlayInfo
    {
        public double LocalPP;
        public double LivePP;

        public Dictionary<string, double> MapCategoryAttribs;

        public double PlayAccuracy;
        public int PlayMaxCombo;
        public int BeatmapMaxCombo;
        public int MissCount;
        public int Position;

        public BeatmapInfo Beatmap;
        public string MapName;

        public string Mods;
    }
}
