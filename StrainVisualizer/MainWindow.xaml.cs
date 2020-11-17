using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using osu.Game.Rulesets.Mods;
using Microsoft.Win32;
using osu.Game.Beatmaps;
using PerformanceCalculator;
using osu.Game.Rulesets.Osu.Difficulty.Skills;
using System.Linq;
using osu.Game.Rulesets;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Audio.Track;
using osu_database_reader.BinaryFiles;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Data;
using System.Globalization;
using osu_database_reader.Components.Beatmaps;
using System.Windows.Input;

namespace StrainVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    public partial class MainWindow : Window
    {
        private object dummyNode;
        private BeatmapEntry selectedNode;
        public Ruleset Ruleset = LegacyHelper.GetRulesetFromLegacyID(0);
        public List<Mod> AvailableMods = new List<Mod>();
        private string osu_path = "";
        private OsuDb osu_database;
        private const int max_beatmaps_shown = 100;
        private CancellationTokenSource bmap_load_cancellation = new CancellationTokenSource();
        private IEnumerable<BeatmapEntry> std_beatmaps;
        private CultureInfo culture = CultureInfo.InvariantCulture;

        public MainWindow()
        {

            AvailableMods = Ruleset.GetAllMods().ToList();
            osu_path = find_osu_path();
            var osu_db_path = Path.Combine(osu_path, "osu!.db");
            osu_database = OsuDb.Read(osu_db_path);
            InitializeComponent();
            std_beatmaps = osu_database.Beatmaps.Where(o => culture.CompareInfo.IndexOf(o.GameMode.ToString(), "Standard", CompareOptions.IgnoreCase) >= 0);
            AddBeatmapsToUI("");
        }

        public void AddBeatmapsToUI(string search_term)
        {
            LoadedBeatmapsVirtualStack.Items.Clear();

            foreach (var bmap in std_beatmaps)
            {
                if (LoadedBeatmapsVirtualStack.Items.Count < max_beatmaps_shown)
                {
                    var searchables = new List<string> { bmap.Title, bmap.Artist, bmap.Version, bmap.SongTags };

                    var match_found = false;
                    if (!string.IsNullOrEmpty(search_term))
                    {
                        foreach (var searchable in searchables)
                        {
                            if(culture.CompareInfo.IndexOf(searchable, search_term, CompareOptions.IgnoreCase) >= 0)
                            {
                                match_found = true;
                                break;
                            }
                            
                        }
                    }
                    else
                    {
                        match_found = true;
                    }
                    if (match_found) {
                        TextBlock new_text = new TextBlock {
                            Text = $"{bmap.Artist} - {bmap.Title} [{bmap.Version}]",
                            Tag = bmap
                        };
                        LoadedBeatmapsVirtualStack.Items.Add(new_text);
                    }
                }
            }
        }
        public static string GetFileFolderName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            var normalizedPath = path.Replace('/', '\\');
            var lastIndex = normalizedPath.LastIndexOf('\\');

            if (lastIndex <= 0)
            {
                return path;
            }

            return path[(lastIndex + 1)..];

        }
        private void LoadBeatmapFromUICallback(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var selected = (TextBlock)e.AddedItems[0];
                selectedNode = (BeatmapEntry)selected.Tag;
                Modifier_Toggled(sender, e);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var osu_path = find_osu_path();
            var search_text = BeatmapSearchTextBox.Text;
            //Task.Factory.StartNew(() => show_beatmaps(search_text), bmap_load_cancellation.Token);

        }


        private string find_osu_path()
        {
            string return_this = "";
            try
            {
                using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("osu\\shell\\open\\command"))
                {
                    if (key != null)
                    {
                        object o = key.GetValue("");
                        if (o != null)
                        {
                            var path_string = o as string;
                            path_string = path_string.Split(' ')[0].Trim('"');
                            var path_slices = path_string.Split(Path.DirectorySeparatorChar);
                            Array.Resize(ref path_slices, path_slices.Length - 1);
                            var final_osu_path = string.Join(Path.DirectorySeparatorChar, path_slices);
                            return final_osu_path;
                        }
                    }
                    return return_this;
                }
            }
            catch (Exception ex)  //just for demonstration...it's always best to handle specific exceptions
            {
                //react appropriately
                return return_this;
            }
        }

        void loadBeatmap_Click(object sender, RoutedEventArgs e)
        {
            Modifier_Toggled(sender, e);
        }
        void calculate_strain(string Beatmap, Mod[] mods)
        {
            var workingBeatmap = new ProcessorWorkingBeatmap(Beatmap);
            var ruleset = LegacyHelper.GetRulesetFromLegacyID(workingBeatmap.BeatmapInfo.RulesetID);
            var diff_calc = new OsuDifficultyCalculatorForVisualizer(ruleset, workingBeatmap);

            IBeatmap playableBeatmap = workingBeatmap.GetPlayableBeatmap(ruleset.RulesetInfo, mods);

            var track = new TrackVirtual(10000);
            mods.OfType<IApplicableToTrack>().ForEach(m => m.ApplyToTrack(track));

            var skills = diff_calc.CalculateStrains(playableBeatmap, mods, track.Rate);
            PlotViewModel.AddDataToModel(skills);
            PlotViewModel.UpdateGraph();

        }

        private void PlotContents_Toggled(object sender, RoutedEventArgs e)
        {
            foreach (var x in graphPlotContents.Children)
            {
                var chkbx = (CheckBox)x;
                PlotViewModel.SeriesIsVisible[chkbx.Content.ToString()] = chkbx.IsChecked ?? false;
            }
            PlotViewModel.UpdateGraph();
        }

        private void BeatmapSearchUICallback(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                AddBeatmapsToUI(BeatmapSearchTextBox.Text);
            }
        }
        private void Modifier_Toggled(object sender, RoutedEventArgs e)
        {
            if (selectedNode is null)
            {
                return;
            }
            var selectedNodePath = Path.Join(osu_path, "Songs", selectedNode.FolderName, selectedNode.BeatmapFileName);
            
            List<Mod> selected_mods = new List<Mod>();
            foreach (var x in strainModifiers.Children)
            {
                var chkbx = (CheckBox)x;
                if (!chkbx.IsChecked ?? false)
                    continue;
                var modString = chkbx.Content.ToString();
                Mod newMod = AvailableMods.FirstOrDefault(m => string.Equals(m.Acronym, modString, StringComparison.CurrentCultureIgnoreCase));
                selected_mods.Add(newMod);
            }
            calculate_strain(selectedNodePath, selected_mods.ToArray());
        }
        
    }
}
