using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using PortingDataFromForScore.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Szds.ParsingOldResults.View.Data;

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

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            List<TeamData> list = new List<TeamData>();
            TeamData team = new TeamData();
            team.Name = "DEV/marketing team";
            team.ShooterNames.Add("Slegl, Andrej");
            team.ShooterNames.Add("Vrhovnik, Bojan");
            team.ShooterNames.Add("Kavka, Jernej");
            team.ShooterNames.Add("Ivansek, Danica");

            list.Add(team);

            team = new TeamData();
            team.Name = "SAC";
            team.ShooterNames.Add("Cucek, Matjaz");
            team.ShooterNames.Add("Dautovic, Saso");
            team.ShooterNames.Add("KAVAZOVIC, ELDAR");
            team.ShooterNames.Add("Purkart, Simona");
            team.ShooterNames.Add("Kastelic, Matjaz");

            list.Add(team);

            File.WriteAllText("teams.json", JsonConvert.SerializeObject(list));
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

                json = File.ReadAllText("teams.json");
                var teams = JsonConvert.DeserializeObject<List<TeamData>>(json);

                ProcessTeams(results, teams);
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

            string content = "Team results:" + Environment.NewLine;
            content += "-------------------------------------------------------------------------------" + Environment.NewLine;

            int place = 8;
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

            File.WriteAllText("team_results.txt", content);
        }

        class TeamResult
        {
            public TeamResult(string name, List<ShooterStageData> scores)
            {
                Name = name;
                Scores = scores;

                Score = CalculateScore();
            }

            public string Name { get; set; }

            public List<ShooterStageData> Scores { get; set; }

            public double Score { get; set; }

            public List<ShooterStageData> BestThree
            {
                get { return Scores.Where(s => s.Completed).OrderBy(s => s.TotalScoreTime).Take(3).ToList(); }
            }

            public double CalculateScore()
            {
                if (Scores == null || Scores.Count == 0)
                {
                    return 9999;
                }

                var bestThree = BestThree;

                double totalScore = 0;
                for (int i = 0; i < bestThree.Count; ++i)
                {
                    totalScore += bestThree[i].TotalScoreTime;
                }

                for (int i = bestThree.Count; i < 3; ++i)
                {
                    totalScore += 9999;
                }

                return totalScore / 3d;
            }
        }
    }
}
