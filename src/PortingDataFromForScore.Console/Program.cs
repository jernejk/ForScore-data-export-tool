using Newtonsoft.Json;
using PortingDataFromForScore.Data;
using PortingDataFromForScore.Providers;
using PortingDataFromForScore.TpsToCsv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Szds.ParsingOldResults.View.Data;
using TeamResults.ExcelService;

namespace PortingDataFromForScore.Console
{
    public class Program
    {
        private static string settingsFilePath = "settings.json";

        static void Main(string[] args)
        {
            int matchIdForTeamsResults = -1;
            if (args.Length > 1)
            {
                int.TryParse(args[2], out matchIdForTeamsResults);
            }

            string json;
            Settings settings = GetSettings();

            System.Console.WriteLine("Initializing...");

            var provider = new CsvDataProvider();
            provider.RootFolder = Path.Combine(settings.ResultsStorePath, "csv") + "\\";

            System.Console.WriteLine("TPS to CSV converting...");
            TpsToCsvConverter.ConvertTpsToCsv(settings.ForScoreBackupPath, provider.RootFolder);

            System.Console.WriteLine("Importing CSV data...");
            System.Console.WriteLine("Importing matches data...");
            var matches = provider.GetAllMatchesInfo();

            System.Console.WriteLine("Importing shooters data...");
            var shooters = provider.GetAllShooters();

            System.Console.WriteLine("Importing stages data...");
            var stages = provider.GetAllStages();

            System.Console.WriteLine("Importing stages with shooter data...");
            var stagesWithShooters = provider.GetConnectShootersAndStages();

            System.Console.WriteLine("Exporting to JSON data...");

            string path = Path.Combine(settings.ResultsStorePath, "json");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (MatchData match in matches)
            {
                string matchName = match.Date.ToString("yyyy-MM-dd");
                System.Console.WriteLine("Exporting " + matchName + ".json...");

                List<ShooterStageData> sbs = stagesWithShooters.Where(s => s.MatchId == match.MatchId).ToList();

                json = JsonConvert.SerializeObject(sbs);

                File.WriteAllText(Path.Combine(path, matchName + ".json"), json);
            }

            System.Console.WriteLine("Exporting matches.json...");
            File.WriteAllText(Path.Combine(path, "matches.json"), JsonConvert.SerializeObject(matches));
            System.Console.WriteLine("Exporting shooters.json...");
            File.WriteAllText(Path.Combine(path, "shooters.json"), JsonConvert.SerializeObject(shooters));
            System.Console.WriteLine("Exporting stages.json...");
            File.WriteAllText(Path.Combine(path, "stages.json"), JsonConvert.SerializeObject(stages));
            System.Console.WriteLine("Exporting stagesWithShooters.json...");
            File.WriteAllText(Path.Combine(path, "stagesWithShooters.json"), JsonConvert.SerializeObject(stagesWithShooters));

            System.Console.WriteLine("Backup completed!");

            if (matchIdForTeamsResults == -1 && matches.Count > 0)
            {
                matchIdForTeamsResults = matches.OrderByDescending(m => m.Date).First().MatchId;
            }

            System.Console.WriteLine("Get team results for match ID: {0}", matchIdForTeamsResults);
            ProcessTeams(stagesWithShooters.Where(s => s.MatchId == matchIdForTeamsResults).ToList(), GetTeams(settings.TeamListPath), settings);

            System.Console.WriteLine();
            System.Console.WriteLine("Press any key to continue...");

            System.Console.ReadKey();
        }

        private static void ProcessTeams(List<ShooterStageData> results, List<TeamData> teams, Settings settings)
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

            File.Copy(settings.TeamResultsTemplatePath, settings.TeamResultsPath, true);

            using (FileStream stream = File.Open(settings.TeamResultsPath, FileMode.Open, FileAccess.ReadWrite))
            {
                ExportTeamResultsAsExcel exporter = new ExportTeamResultsAsExcel();
                var s = exporter.Export(finalResults, stream);
            }

            string content = GenerateTextResults(finalResults);
            File.WriteAllText(Path.GetFileNameWithoutExtension(settings.TeamResultsPath) + ".txt", content);
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

            System.Console.WriteLine(content);

            return content;
        }

        private static List<TeamData> GetTeams(string excelPath)
        {
            using (Stream stream = File.OpenRead(excelPath))
            {
                TeamsExcelProvider excelProvider = new TeamsExcelProvider();

                return excelProvider.GetTeams(stream);
            }
        }

        public static Settings GetSettings()
        {
            Settings settings;

            try
            {
                string json = File.ReadAllText(settingsFilePath);
                settings = JsonConvert.DeserializeObject<Settings>(json);
            }
            catch
            {
                settings = new Settings();
                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(settings));
            }

            return settings;
        }
    }
}
