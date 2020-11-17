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

namespace StrainVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    public partial class MainWindow : Window
    {
        private object dummyNode;
        private string selectedNodePath;
        public Ruleset Ruleset = LegacyHelper.GetRulesetFromLegacyID(0);
        public List<Mod> AvailableMods = new List<Mod>();
        private string osu_path = "";
        private OsuDb osu_database;

        public MainWindow()
        {
            AvailableMods = Ruleset.GetAllMods().ToList();
            osu_path = find_osu_path();
            var osu_db_path = Path.Combine(osu_path, "osu!.db");
            osu_database = OsuDb.Read(osu_db_path);
            InitializeComponent();
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

            return path.Substring(lastIndex + 1);


        }
        private void SelectedItem(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selected = (TreeViewItem)e.NewValue;
            var fullPath = (string)selected.Tag;
            selectedNodePath = (string)selected.Tag;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var osu_path = find_osu_path();
            osu_database.load_db(osu_path);

            foreach (string s in Directory.GetLogicalDrives())
            {
                TreeViewItem item = new TreeViewItem
                {
                    Header = s,
                    Tag = s,
                    FontWeight = FontWeights.Normal
                };
                item.Items.Add(dummyNode);
                item.Expanded += new RoutedEventHandler(FolderExpanded);
                
                FolderView.Items.Add(item);
            }
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

        void FolderExpanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                try
                {
                    foreach (string s in Directory.GetDirectories(item.Tag.ToString()))
                    {
                        TreeViewItem subitem = new TreeViewItem
                        {
                            Header = s[(s.LastIndexOf("\\") + 1)..],
                            Tag = s,
                            FontWeight = System.Windows.FontWeights.Normal
                        };
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(FolderExpanded);
                        item.Items.Add(subitem);
                    }
                    foreach (string s in Directory.GetFiles(item.Tag.ToString()))
                    {
                        TreeViewItem subitem = new TreeViewItem
                        {
                            Header = s[(s.LastIndexOf("\\") + 1)..],
                            Tag = s,
                            FontWeight = System.Windows.FontWeights.Normal
                        };
                        item.Items.Add(subitem);
                    }

                }
                catch (Exception) { }
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

        private void Modifier_Toggled(object sender, RoutedEventArgs e)
        {

            if (selectedNodePath is null)
            {
                return;
            }
            if (!selectedNodePath.EndsWith(".osu"))
            {
                return;
            }

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
