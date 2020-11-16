using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace PerformanceCalculatorGUI
{
    internal class DataSource : INotifyPropertyChanged
    {
        public DataSource()
        {
            OsuBeatmapPath = Directory.GetCurrentDirectory();
        }
        private string osuBeatmapPath;
        public string OsuBeatmapPath
        {
            get => osuBeatmapPath;
            set
            {
                osuBeatmapPath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OsuBeatmapPath"));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
