using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Osu.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Skills;
using OxyPlot;
using OxyPlot.Series;

namespace StrainVisualizer
{
    public class PlotViewModel
    {
        public PlotViewModel()
        {
            StrainModel = new PlotModel {};
            SeriesIsVisible.Add("AimSnap", false);
            SeriesIsVisible.Add("AimFlow", false);
            SeriesIsVisible.Add("AimHybrid", false);
            SeriesIsVisible.Add("TapSpeed", false);
            SeriesIsVisible.Add("TapStamina", false);
            SeriesIsVisible.Add("TapRhythm", false);
        }

        public static void AddDataToModel(Skill[] skills)
        {
            StrainModel.Series.Clear();
            line_series.Clear();
            foreach (var skill in skills)
            {
                OsuSkill s = (OsuSkill)skill;
                var lineSerie = new LineSeries
                {
                    IsVisible = SeriesIsVisible[s.GetType().Name],
                    Title = s.GetType().Name
                };
                double prev_strain = 1;
                foreach (Tuple<double, double> point in s.grapher)
                {
                    double strain_diff = point.Item2 - prev_strain;
                    lineSerie.Points.Add(new DataPoint(point.Item1, point.Item2));
                    prev_strain = point.Item2;
                }
                line_series.Add(lineSerie);
            }

            foreach (var ls in line_series)
                StrainModel.Series.Add(ls);
        }

        public static void UpdateGraph()
        {
            foreach (var ls in StrainModel.Series)
                ls.IsVisible = SeriesIsVisible[ls.Title];
            StrainModel.InvalidatePlot(true);
        }

        public static Dictionary<string, bool> SeriesIsVisible = new Dictionary<string, bool>();
        
        private static List<LineSeries> line_series = new List<LineSeries>();

        public static PlotModel StrainModel { get; private set; }
    }
}
