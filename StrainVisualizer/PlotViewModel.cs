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
            StrainModel = new PlotModel { };
            SeriesIsVisible.Add("Speed", true);
            SeriesIsVisible.Add("Aim", true);
        }

        public static void AddDataToModel(Dictionary<string, List<(double, double)>> skill_strains)
        {
            StrainModel.Series.Clear();
            StrainModel.Axes.Clear();
            line_series.Clear();
            foreach (var keypair in skill_strains)
            {
                var skill_name = keypair.Key;
                var lineSerie = new LineSeries
                {
                    IsVisible = SeriesIsVisible[skill_name],
                    Title = skill_name
                };
                var strain_tuple = keypair.Value;
                foreach ((var strain, var cur_time) in strain_tuple)
                    lineSerie.Points.Add(new DataPoint(cur_time, strain));

                line_series.Add(lineSerie);
            }

            foreach (var ls in line_series)
                StrainModel.Series.Add(ls);

            StrainModel.InvalidatePlot(true);

            foreach (var ax in StrainModel.Axes)
            {
                if (ax.Position == OxyPlot.Axes.AxisPosition.Left)
                {
                    ax.IsZoomEnabled = false;
                    ax.IsPanEnabled = false;
                }
            }
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
