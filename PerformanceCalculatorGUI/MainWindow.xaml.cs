using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Documents;
using Microsoft.Win32;
using Alba.CsConsoleFormat;

namespace PerformanceCalculatorGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataSource data_source = new DataSource();
        private Difficulty.DifficultyCalc difficultyCalc = new Difficulty.DifficultyCalc();
        private Profile.ProfileCalc profileCalc = new Profile.ProfileCalc();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = data_source;
        }

        private void btnDiffOpenBmapClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "osu! Beatmap Files (*.osu)|*.osu|All files (*.*)|*.*",
                InitialDirectory = data_source.OsuBeatmapPath
            };
            if (openFileDialog.ShowDialog() == true)
                data_source.OsuBeatmapPath = openFileDialog.FileName;

        }
        private async void btnProfileExecuteClick(object sender, RoutedEventArgs e)
        {
            await btnProfileExecuteClickAsync(sender, e);
            return;
        }
        private async System.Threading.Tasks.Task btnProfileExecuteClickAsync(object sender, RoutedEventArgs e)
        {
            if (profileUsernameTextBox.Text.Length > 16)
            {
                profileStatusTextBox.Content = "Username can\'t be longer than 16 characters I think...";
            }
            profileCalc.Key = profileApiKeyTextBox.Password;
            profileCalc.ProfileName = profileUsernameTextBox.Text;
            await profileCalc.ExecuteAsync(profileStatusTextBox);
            outputToTextBlock(profileOutputTextBlock, profileCalc.ResultsDoc);

        }

        private void HandleLinkClick(object sender, RoutedEventArgs e) {
            Hyperlink hl = (Hyperlink)sender;
            string navigateUri = hl.NavigateUri.ToString();
            Process.Start(new ProcessStartInfo(navigateUri) { UseShellExecute = true});
            e.Handled = true;
        }
        private void btnDiffProcessBmapClick(object sender, RoutedEventArgs e)
        {
            if (!data_source.OsuBeatmapPath.EndsWith(".osu"))
            {
                return;
            }
            difficultyCalc.ProcessBeatmap(data_source.OsuBeatmapPath, getEnabledMods());
            outputToTextBlock(diffProcessResult, difficultyCalc.CalcResults);
        }

        private void outputToTextBlock(TextBlock t, Document d) {
            var writer = new StringWriter();
            ConsoleRenderer.RenderDocumentToText(d, new TextRenderTarget(writer), new Alba.CsConsoleFormat.Rect(0, 0, 250, Alba.CsConsoleFormat.Size.Infinity));

            var str = writer.GetStringBuilder().ToString();

            var lines = str.Split('\n');
            for (int i = 0; i < lines.Length; i++)
                lines[i] = lines[i].TrimEnd();
            str = string.Join('\n', lines);
            t.Text = str;
        }

        private string[] getEnabledMods()
        {
            List<string> mods = new List<string>();
            List<ListBox> listBoxes = new List<ListBox>();
            foreach (var item in UpperGrid.Children)
            {
                if (item.GetType().Equals(typeof(ListBox)))
                {
                    listBoxes.Add((ListBox)item);
                }
            }

            foreach (var item in listBoxes)
            {
                foreach (var c in item.Items)
                {
                    var chkbx = (CheckBox)c;
                    if (chkbx.IsChecked ?? true)
                    {
                        mods.Add(chkbx.Content.ToString());
                    }
                }
            }
            return mods.ToArray();
        }

    }

}
