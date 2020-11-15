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

        public MainWindow()
        {
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

        private void SingleFileSelected(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine(sender);

            var source = (ListBox)e.Source;
            var selected = (FileDetails)source.SelectedItem;

        }
        private void ItemMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            ListBox items = sender as ListBox;
            var selected = items.SelectedItem as FileDetails;
            var path = selected.Path;
            var allFolders = new DirectoryInfo(path).GetDirectories();

            var details = new List<FileDetails>();

            for (int i = 0; i < allFolders.Length; i++)
            {
                var detail = new FileDetails();
                detail.FileName = allFolders[i].Name;

                details.Add(detail);
            }

            Console.WriteLine(selected);
        }

        void JumpToNode(TreeViewItem tvi, string NodeName)
        {
            if (tvi.Tag.ToString() == NodeName)
            {
                tvi.IsExpanded = true;
                tvi.BringIntoView();
                return;
            }
            else
                tvi.IsExpanded = false;

            if (tvi.HasItems)
            {
                foreach (var item in tvi.Items)
                {
                    TreeViewItem temp = item as TreeViewItem;
                    JumpToNode(temp, NodeName);
                }
            }
        }

        private void SelectedItem(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selected = (TreeViewItem)e.NewValue;
            var fullPath = (string)selected.Tag;
            selectedNodePath = (string)selected.Tag;
            var metaData = new FileInfo(fullPath);
            //labelResult.Content = metaData.CreationTime;


            //myDataGrid.ItemsSource = new DirectoryInfo(fullPath).Name;

            //myGrid.DataContext = new DirectoryInfo(fullPath).GetFiles();

            try
            {
                var files = new List<DirectoryInfo>();

                var details = new List<FileDetails>();
                var allFiles = new DirectoryInfo(fullPath).GetFiles();
                var allFolders = new DirectoryInfo(fullPath).GetDirectories();



                for (int i = 0; i < allFiles.Length; i++)
                {
                    var fd = new FileDetails();
                    fd.FileName = allFiles[i].Name;
                    fd.FileCreation = allFiles[i].CreationTime.ToString();
                    fd.FileImage = $"pack://application:,,,/Images/file.ico";
                    fd.IsFile = true;
                    details.Add(fd);
                }


                for (int i = 0; i < allFolders.Length; i++)
                {
                    var fd = new FileDetails();
                    fd.FileName = allFolders[i].Name;
                    fd.FileCreation = allFolders[i].CreationTime.ToString();
                    fd.FileImage = $"pack://application:,,,/Images/folder.ico";
                    fd.IsFolder = true;
                    fd.Path = fullPath + "\\" + allFolders[i].Name;
                    details.Add(fd);
                }

                Console.WriteLine(fullPath);
            }
            catch { }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var osu_path_slices = find_osu_path();

            foreach (string s in Directory.GetLogicalDrives())
            {
                TreeViewItem item = new TreeViewItem
                {
                    Header = s,
                    Tag = s,
                    FontWeight = System.Windows.FontWeights.Normal
                };
                item.Items.Add(dummyNode);
                item.Expanded += new RoutedEventHandler(FolderExpanded);
                
                FolderView.Items.Add(item);
            }
            /* TO-DO later
            if (osu_path_slices.Length > 0) {
                TreeViewItem osu_drive = new TreeViewItem();
                foreach (var item in FolderView.Items)
                {
                    var drive = ((string)((TreeViewItem)item).Tag).Trim('\\');
                    if (drive == osu_path_slices[0]){
                        osu_drive = (TreeViewItem)item;
                    }
                }
                osu_path_slices = osu_path_slices.Skip(1).ToArray();
                foreach (var folder in osu_path_slices)
                {
                    var items = osu_drive.Items;
                    TreeViewItem osu_folder_item = new TreeViewItem
                    {
                        Header = folder,
                        Tag = folder,
                        FontWeight = FontWeights.Normal
                    };
                    items.Add(osu_folder_item);
                }
            }
            */
        }

        private string[] find_osu_path()
        {
            string[] return_this = { };
            try
            {
                using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("osu\\shell\\open\\command"))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("");
                        if (o != null)
                        {
                            var path_string = o as String;
                            path_string = path_string.Split(' ')[0].Trim('"');
                            var path_slices = path_string.Split(Path.DirectorySeparatorChar);
                            Array.Resize(ref path_slices, path_slices.Length - 1);
                            return path_slices;
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
                            Header = s.Substring(s.LastIndexOf("\\") + 1),
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
                            Header = s.Substring(s.LastIndexOf("\\") + 1),
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
            if (selectedNodePath is null)
            {
                return;
            }
            if (!selectedNodePath.EndsWith(".osu")){
                return;
            }

            calculate_strain(selectedNodePath);

        }
        void calculate_strain(string Beatmap)
        {
            var workingBeatmap = new ProcessorWorkingBeatmap(Beatmap);
            var ruleset = LegacyHelper.GetRulesetFromLegacyID(workingBeatmap.BeatmapInfo.RulesetID);
            var diff_calc = new OsuDifficultyCalculatorForVisualizer(ruleset, workingBeatmap);
            var mods = new List<Mod>().ToArray();

            IBeatmap playableBeatmap = workingBeatmap.GetPlayableBeatmap(ruleset.RulesetInfo, mods);
            var skills = diff_calc.CalculateStrains(playableBeatmap, mods, 10000);
            PlotViewModel.AddDataToModel(skills);
            PlotViewModel.UpdateGraph();

        }

        private void AimFlow_Checked(object sender, RoutedEventArgs e)
        {
            PlotViewModel.SeriesIsVisible["AimFlow"] = true;
            PlotViewModel.UpdateGraph();
        }

        private void AimFlow_Unchecked(object sender, RoutedEventArgs e)
        {
            PlotViewModel.SeriesIsVisible["AimFlow"] = false;
            PlotViewModel.UpdateGraph();
        }

        private void AimSnap_Checked(object sender, RoutedEventArgs e)
        {
            PlotViewModel.SeriesIsVisible["AimSnap"] = true;
            PlotViewModel.UpdateGraph();
        }
        private void AimSnap_Unchecked(object sender, RoutedEventArgs e)
        {
            PlotViewModel.SeriesIsVisible["AimSnap"] = false;
            PlotViewModel.UpdateGraph();
        }
        private void AimHybrid_Checked(object sender, RoutedEventArgs e)
        {
            PlotViewModel.SeriesIsVisible["AimHybrid"] = true;
            PlotViewModel.UpdateGraph();
        }
        private void AimHybrid_Unchecked(object sender, RoutedEventArgs e)
        {
            PlotViewModel.SeriesIsVisible["AimHybrid"] = false;
            PlotViewModel.UpdateGraph();
        }
        private void TapStamina_Checked(object sender, RoutedEventArgs e)
        {
            PlotViewModel.SeriesIsVisible["TapStamina"] = true;
            PlotViewModel.UpdateGraph();
        }
        private void TapStamina_Unchecked(object sender, RoutedEventArgs e)
        {
            PlotViewModel.SeriesIsVisible["TapStamina"] = false;
            PlotViewModel.UpdateGraph();
        }
        private void TapSpeed_Checked(object sender, RoutedEventArgs e)
        {
            PlotViewModel.SeriesIsVisible["TapSpeed"] = true;
            PlotViewModel.UpdateGraph();
        }
        private void TapSpeed_Unchecked(object sender, RoutedEventArgs e)
        {
            PlotViewModel.SeriesIsVisible["TapSpeed"] = false;
            PlotViewModel.UpdateGraph();
        }
        private void TapRhythm_Checked(object sender, RoutedEventArgs e)
        {
            PlotViewModel.SeriesIsVisible["TapRhythm"] = true;
            PlotViewModel.UpdateGraph();
        }
        private void TapRhythm_Unchecked(object sender, RoutedEventArgs e)
        {
            PlotViewModel.SeriesIsVisible["TapRhythm"] = false;
            PlotViewModel.UpdateGraph();

        }
    }
}
