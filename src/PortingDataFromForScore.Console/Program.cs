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

            foreach (var s in shooters)
            {
                if (ContainsNonAsciiChar(s.FirstName) || ContainsNonAsciiChar(s.LastName))
                {
                    System.Console.WriteLine(s.FullName);
                }
            }

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
            List<string> missingShooters = new List<string>();
            foreach (var team in teams)
            {
                var scores = new List<ShooterStageData>();
                foreach (string shooterName in team.ShooterNames)
                {
                    ShooterStageData score = results.FirstOrDefault(s => CompareShootersNames(s.FullName, shooterName));

                    if (score != null)
                    {
                        scores.Add(score);
                    }
                    else
                    {
                        missingShooters.Add(shooterName);
                    }
                }

                finalResults.Add(new TeamResult(team.Name, scores));
            }

            if (missingShooters.Count > 0)
            {
                System.Console.WriteLine();
                System.Console.WriteLine("Missing shooters: ");

                foreach (var shooter in missingShooters)
                {
                    System.Console.WriteLine(shooter);
                }

                System.Console.WriteLine();
            }

            File.Copy(settings.TeamResultsTemplatePath, settings.TeamResultsPath, true);

            using (FileStream stream = File.Open(settings.TeamResultsPath, FileMode.Open, FileAccess.ReadWrite))
            {
                ExportTeamResultsAsExcel exporter = new ExportTeamResultsAsExcel();
                var s = exporter.Export(finalResults, stream);
            }

            string content = GenerateTextResults(finalResults);
            string path = Path.ChangeExtension(settings.TeamResultsPath, "txt");
            File.WriteAllText(path, content);
        }

        private static string GenerateTextResults(List<TeamResult> finalResults)
        {
            string content = string.Empty;

            int place = 0;
            foreach (TeamResult result in finalResults.Where(t => t.Scores.Count >= 3).OrderBy(f => f.Score))
            {
                content += string.Format("#{0, -2}\t{1}{2}", ++place, result.Name, Environment.NewLine);
                content += "-------------------------------------------------------------------------------" + Environment.NewLine;

                foreach (var best in result.BestThree)
                {
                    content += string.Format("\t{0, -49}  \t{1,8:###.00} sekund{2}", best.FullName + ":", best.TotalScoreTime, Environment.NewLine);
                }

                content += "-------------------------------------------------------------------------------" + Environment.NewLine;
                content += string.Format("\t{1, -49} \t{2,8:###.00} sekund{3}", " ", " ", Math.Round(result.Score, 2), Environment.NewLine);

                content += Environment.NewLine;
            }

            System.Console.WriteLine("Team results:" + Environment.NewLine);
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

        private static bool CompareShootersNames(string nameA, string nameB)
        {
            return ToAscii(nameA.ToLower()) == ToAscii(nameB.ToLower());
        }

        private static string ToAscii(string name)
        {
            return name.Replace("č", "c").Replace("ž", "z").Replace("š", "s").Replace("ć", "c").Replace("đ", "dz").Replace(((char)240).ToString(), "dz")
                       .Replace("Č", "C").Replace("Ž", "Z").Replace("Š", "S").Replace("Ć", "C").Replace("Đ", "DZ").Replace(((char)208).ToString(), "DZ");
        }

        private static bool ContainsNonAsciiChar(string name)
        {
            foreach (char c in ToAscii(name))
            {
                if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == ' ' || c == '-'))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
