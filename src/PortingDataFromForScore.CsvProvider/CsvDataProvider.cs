using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using PortingDataFromForScore.Data;
using Szds.ParsingOldResults.View.Data;
using System.Globalization;

namespace PortingDataFromForScore.Providers
{
    public class CsvDataProvider
    {
        public string RootFolder = "csv/";

        private Dictionary<int, ShooterData> shooters = new Dictionary<int, ShooterData>();
        private Dictionary<int, MatchData> matchesInfo = new Dictionary<int, MatchData>();
        private List<StageData> stages = new List<StageData>();
        private Dictionary<int, List<StageData>> stagesPerShooter = new Dictionary<int, List<StageData>>();
        
        public List<StageData> GetAllStages()
        {
            if (shooters == null)
            {
                GetAllShooters();
            }

            if (matchesInfo == null)
            {
                GetAllMatchesInfo();
            }

            string text = File.ReadAllText(RootFolder + "matches.STA.csv");
            TextReader reader = new StringReader(text);
            CsvReader csv = new CsvReader(reader);

            List<StageData> list = new List<StageData>();
            while (csv.Read())
            {
                StageData stage = new StageData();
                stage.RecordNumber = csv.GetField<int>(0);
                stage.ShooterId = csv.GetField<int>("Shooterid");
                stage.StageNumber = csv.GetField<int>("Stagenumber");
                stage.MatchId = csv.GetField<int>("Matchid");

                for (int i = 1; i <= 9; ++i)
                {
                    double stringTime = GetDoubleFromCsv(csv, "String" + i);

                    if (Math.Abs(stringTime) < 0.00001)
                    {
                        break;
                    }

                    stage.StringTimes.Add(stringTime);
                }

                stage.PointsDown = csv.GetField<int>("Pointsdown");

                for (int i = 1; i <= 13; ++i)
                {
                    int target = csv.GetField<int>("Target" + i);

                    if (target > 0)
                    {
                        stage.TargetScores.Add(target);
                    }
                }

                stage.ProceduralErrors = csv.GetField<int>("Procedurals");
                stage.NonThreats = csv.GetField<int>("Nonthreats");
                stage.FailureToNeutralize = csv.GetField<int>("Ftns");
                stage.FailureToDoRight = csv.GetField<int>("Ftdrs") > 0;
                stage.DidNotFinished = csv.GetField<int>("Dnf") > 0;
                stage.Disqualified = csv.GetField<int>("Dq") > 0;
                stage.ShootTime = GetDoubleFromCsv(csv, "Nettime");
                stage.TotalPenaltyTime = GetDoubleFromCsv(csv, "Totalpenaltysecs");
                stage.TotalTime = GetDoubleFromCsv(csv, "Totalstagescore");

                if (!stagesPerShooter.ContainsKey(stage.ShooterId))
                {
                    stagesPerShooter.Add(stage.ShooterId, new List<StageData>());
                }

                List<StageData> s = stagesPerShooter[stage.ShooterId];
                s.Add(stage);

                list.Add(stage);
            }

            stages = list;

            return list;
        }

        public List<ShooterStageData> GetConnectShootersAndStages()
        {
            if (stages.Count == 0)
            {
                GetAllStages();
            }

            string text = File.ReadAllText(RootFolder + "matches.RES.csv");
            TextReader reader = new StringReader(text);
            CsvReader csv = new CsvReader(reader);

            List<ShooterStageData> list = new List<ShooterStageData>();
            while (csv.Read())
            {
                ShooterStageData data = new ShooterStageData();
                data.RecordNumber = csv.GetField<int>(0);
                data.ResultNumber = csv.GetField<int>(1);
                data.Completed = csv.GetField<bool>("Completed");

                data.Industry = csv.GetField<bool>("Industry");
                data.International = csv.GetField<bool>("International");
                data.Junior = csv.GetField<bool>("Junior");
                data.Lady = csv.GetField<bool>("Lady");
                data.LawEnforcementOfficer = csv.GetField<bool>("Leo");
                data.Military = csv.GetField<bool>("Military");
                data.Press = csv.GetField<bool>("Press");
                data.Senior = csv.GetField<bool>("Senior");
                data.Staff = csv.GetField<bool>("Staff");
                data.SecurityOfficer = csv.GetField<bool>("So");
                data.Veteran = csv.GetField<bool>("Veteran");

                data.MatchId = csv.GetField<int>("Matchid");
                data.ShooterId = csv.GetField<int>("Shooterid");
                data.FullName = csv.GetField<string>("Fullname").Trim();
                data.Division = csv.GetField<string>("Division").Trim();
                data.Classification = csv.GetField<string>("Shooterclassification").Trim();

                data.TotalPenalty = GetDoubleFromCsv(csv, "Totalpenalty");
                data.TotalStringTime = GetDoubleFromCsv(csv, "Totalstringtime");
                data.TotalScoreTime = GetDoubleFromCsv(csv, "Totalscoretime");

                if (stagesPerShooter.ContainsKey(data.ShooterId))
                {
                    int maxStages = 0;
                    List<StageData> ss = stagesPerShooter[data.ShooterId].Where(s => s.MatchId == data.MatchId).ToList();

                    for (int i = 1; i <= 32; ++i)
                    {
                        double time = GetDoubleFromCsv(csv, "Scoregroup.stage" + i + "score");
                        if (time == 0)
                        {
                            continue;
                        }

                        maxStages = i;
                        List<StageData> stageData = ss.Where(s => s.StageNumber == i && Math.Abs(s.TotalTime - time) < 0.1).ToList();
                        if (stageData.Count == 1)
                        {
                            data.Stages.Add(stageData[0]);
                        }
                        else
                        {
                        }
                    }

                    if (ss.Count > maxStages)
                    {
                        
                    }
                }

                list.Add(data);
            }

            return list;
        }

        private static double GetDoubleFromCsv(CsvReader csv, string header, double fallback = 0)
        {
            string text = csv.GetField<string>(header);
            double temp;

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out temp))
            {
                return temp;
            }

            return fallback;
        }

        public List<ShooterData> GetAllShooters()
        {
            string text = File.ReadAllText(RootFolder + "shooters.SHO.csv");
            TextReader reader = new StringReader(text);
            CsvReader csv = new CsvReader(reader);

            List<ShooterData> list = new List<ShooterData>();
            while (csv.Read())
            {
                ShooterData data = new ShooterData();
                data.Id = csv.GetField<int>("Shooterid");
                data.IdpaNumber = csv.GetField<string>("Idpa Number").Trim();
                data.Inactive = csv.GetField<bool>("Inactive");

                DateTime date;
                string temp = csv.GetField<string>("Lastshoot");
                if (DateTime.TryParse(temp, out date))
                {
                    data.LastShoot = date;
                }

                data.LastDivision = csv.GetField<string>("Lastdivision").Trim();

                temp = csv.GetField<string>("Priormatch");
                if (DateTime.TryParse(temp, out date))
                {
                    data.PriorMatch = date;
                }

                data.FirstName = csv.GetField<string>("Firstname").Trim();
                data.LastName = csv.GetField<string>("Lastname").Trim();
                data.FullName = csv.GetField<string>("Fullname").Trim();

                data.Address = csv.GetField<string>("Address").Trim();
                data.Zip = csv.GetField<string>("Zip").Trim();
                data.State = csv.GetField<string>("State").Trim();
                data.City = csv.GetField<string>("City").Trim();
                data.Country = csv.GetField<string>("Country").Trim();
                data.Email = csv.GetField<string>("Email").Trim();

                data.CdpClass = csv.GetField<string>("Cdp Class").Trim();
                data.EspClass = csv.GetField<string>("Esp Class").Trim();
                data.SspClass = csv.GetField<string>("Ssp Class").Trim();
                data.EsrClass = csv.GetField<string>("Esr Class").Trim();
                data.SsrClass = csv.GetField<string>("Ssr Class").Trim();
                data.HighClass = csv.GetField<string>("Highclass").Trim();

                data.Industry = csv.GetField<bool>("Industry");
                data.Junior = csv.GetField<bool>("Junior");
                data.Lady = csv.GetField<bool>("Lady");
                data.LawEnforcementOfficer = csv.GetField<bool>("Leo");
                data.Military = csv.GetField<bool>("Military");
                data.Press = csv.GetField<bool>("Press");
                data.Senior = csv.GetField<bool>("Senior");
                data.Staff = csv.GetField<bool>("Staff");
                data.Veteran = csv.GetField<bool>("Veteran");
                data.SecurityOfficer = csv.GetField<bool>("So");

                list.Add(data);
                shooters.Add(data.Id, data);
            }

            return list;
        }

        private string Result(string id, CsvReader csv)
        {
            return id + ": " + csv.GetField<string>(id) + "; ";
        }

        public List<MatchData> GetAllMatchesInfo()
        {
            string text = File.ReadAllText(RootFolder + "matches.MAT.csv");
            TextReader reader = new StringReader(text);
            CsvReader csv = new CsvReader(reader);

            List<MatchData> matches = new List<MatchData>();
            while (csv.Read())
            {
                MatchData data = new MatchData();
                data.Title = csv.GetField<string>("Description").Trim();
                data.MatchId = csv.GetField<int>("Matchid");
                data.Date = DateTime.Parse(csv.GetField<string>("Date")); // yyyy-mm-dd
                data.IsClassification = csv.GetField<string>("Classifier") == "Y";
                data.MaxStrings = csv.GetField<int>("Strings");
                data.NumberOfRegisteredShooters = csv.GetField<int>("Registered");
                data.NumberOfStages = csv.GetField<int>("Numstages");
                matches.Add(data);
                matchesInfo.Add(data.MatchId, data);

                //string d = string.Empty;
                //d += "Matchid: " + csv.GetField<string>("Matchid") + "; ";
                //d += "Date: " + csv.GetField<string>("Date") + "; ";
                //d += "Strings: " + csv.GetField<string>("Strings") + "; ";
                //d += "Numstages: " + csv.GetField<string>("Numstages") + "; ";
                //d += "Description: " + csv.GetField<string>("Description") + "; ";
                //d += "Classifier: " + csv.GetField<string>("Classifier") + "; ";
                //d += "Registered: " + csv.GetField<string>("Registered") + "; ";

                //Debug.WriteLine(d);
            }

            return matches;
        }
    }
}
