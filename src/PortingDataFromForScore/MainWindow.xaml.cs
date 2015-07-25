using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using MethodTimer;
using Newtonsoft.Json;
using PortingDataFromForScore.Data;
using PortingDataFromForScore.Providers;
using Szds.ParsingOldResults.View.Data;
using Microsoft.WindowsAPICodePack.Dialogs;
using PortingDataFromForScore.TpsToCsv;
using System.Threading.Tasks;

namespace PortingDataFromForScore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CsvDataProvider provider = new CsvDataProvider();
        List<MatchData> matches;
        List<ShooterData> shooters;
        List<StageData> stages;
        List<ShooterStageData> stagesWithShooters;

        [Time]
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                Status.Items.Insert(0, "Initializing...");

                provider = new CsvDataProvider();
                provider.RootFolder = Path.Combine(dialog.FileName, "csv") + "\\";

                await LogAsync("TPS to CSV converting...");
                await Task.Run(() => TpsToCsvConverter.ConvertTpsToCsv(dialog.FileName, provider.RootFolder));

                await LogAsync("Importing CSV data...");
                await ImportDataAsync();

                await LogAsync("Exporting to JSON data...");
                await ExportDataAsync(Path.Combine(dialog.FileName, "json"));

                await LogAsync("Done!");
            }
        }

        [Time]
        private async Task ImportDataAsync()
        {
            await LogAsync("Importing matches data...");
            matches = provider.GetAllMatchesInfo();

            await LogAsync("Importing shooters data...");
            shooters = provider.GetAllShooters();

            await LogAsync("Importing stages data...");
            stages = provider.GetAllStages();

            await LogAsync("Importing stages with shooter data...");
            stagesWithShooters = provider.GetConnectShootersAndStages();
        }

        [Time]
        private async Task ExportDataAsync(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (MatchData match in matches)
            {
                string matchName = match.Date.ToString("yyyy-MM-dd");
                await LogAsync("Exporting " + matchName + ".json...");

                List<ShooterStageData> sbs = stagesWithShooters.Where(s => s.MatchId == match.MatchId).ToList();
                
                string json = JsonConvert.SerializeObject(sbs);

                File.WriteAllText(Path.Combine(path, matchName + ".json"), json);
            }

            await LogAsync("Exporting matches.json...");
            File.WriteAllText(Path.Combine(path, "matches.json"), JsonConvert.SerializeObject(matches));
            await LogAsync("Exporting shooters.json...");
            File.WriteAllText(Path.Combine(path, "shooters.json"), JsonConvert.SerializeObject(shooters));
            await LogAsync("Exporting stages.json...");
            File.WriteAllText(Path.Combine(path, "stages.json"), JsonConvert.SerializeObject(stages));
            await LogAsync("Exporting stagesWithShooters.json...");
            File.WriteAllText(Path.Combine(path, "stagesWithShooters.json"), JsonConvert.SerializeObject(stagesWithShooters));
        }

        private async Task LogAsync(string message)
        {
            Status.Items.Insert(0, message);

            await Task.Delay(1);
        }
    }
}
