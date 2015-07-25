using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using PortingDataFromForScore.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using TeamResults.ExcelService;

namespace TeamResults
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.Multiselect = false;
            CommonFileDialogResult result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                string json = File.ReadAllText(dialog.FileName);
                List<ShooterStageData> results = JsonConvert.DeserializeObject<List<ShooterStageData>>(json);

                ProcessTeams(results, GetTeams());
            }
        }

        private void ProcessTeams(List<ShooterStageData> results, List<TeamData> teams)
        {
            List<TeamResult> finalResults = new List<TeamResult>();
            foreach (var team in teams)
            {
                var scores = new List<ShooterStageData>();
                foreach (var shooterName in team.ShooterNames)
                {
                    ShooterStageData score = results.FirstOrDefault(s => s.FullName.Equals(shooterName, System.StringComparison.CurrentCultureIgnoreCase));

                    if (score != null)
                    {
                        scores.Add(score);
                    }
                }

                finalResults.Add(new TeamResult(team.Name, scores));
            }

            File.Copy("team_results_template.xlsx", "team_results.xlsx", true);

            using (FileStream stream = File.Open("team_results.xlsx", FileMode.Open, FileAccess.ReadWrite))
            {
                ExportTeamResultsAsExcel exporter = new ExportTeamResultsAsExcel();
                var s = exporter.Export(finalResults, stream);
            }

            string content = GenerateTextResults(finalResults);
            File.WriteAllText("team_results.txt", content);
        }

        private static string GenerateTextResults(List<TeamResult> finalResults)
        {
            string content = "Team results:" + Environment.NewLine;
            content += "-------------------------------------------------------------------------------" + Environment.NewLine;

            int place = 0;
            foreach (TeamResult result in finalResults.OrderBy(f => f.Score))
            {
                content += string.Format("#{0, -2}\t{1, -20} \t{2,7:###.00} seconds {3}", ++place, result.Name, Math.Round(result.Score, 2), Environment.NewLine);
                content += "-------------------------------------------------------------------------------" + Environment.NewLine;

                foreach (var best in result.BestThree)
                {
                    content += string.Format("\t{0, -20}  \t{1,7:###.00} seconds {2}", best.FullName + ":", best.TotalScoreTime, Environment.NewLine);
                }

                content += "===============================================================================" + Environment.NewLine;
                content += Environment.NewLine;
            }

            return content;
        }

        private List<TeamData> GetTeams()
        {
            using (Stream stream = File.OpenRead("teams.xlsx"))
            {
                TeamsExcelProvider excelProvider = new TeamsExcelProvider();

                return excelProvider.GetTeams(stream);
            }
        }
    }
}
