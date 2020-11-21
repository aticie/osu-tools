// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Scoring;

namespace PerformanceCalculator.LocalScores
{
    public class LocalReplayInfo
    {
        public double TotalPP;
        public Dictionary<string, double> MapCategoryAttribs;
        public ScoreInfo ScoreInfo;
        public string MapName;

        public LocalReplayInfo(double totalPP, Dictionary<string, double> mapCategoryAttribs, ScoreInfo scoreInfo, string mapName)
        {
            TotalPP = totalPP;
            MapCategoryAttribs = mapCategoryAttribs;
            ScoreInfo = scoreInfo;
            MapName = mapName;
        }
    }

}
