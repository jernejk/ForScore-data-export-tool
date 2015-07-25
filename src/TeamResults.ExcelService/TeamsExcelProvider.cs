using PortingDataFromForScore.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TeamResults.ExcelService
{
    public class TeamsExcelProvider
    {
        public List<TeamData> GetTeams(Stream stream)
        {
            List<TeamData> teams = new List<TeamData>();

            ExcelReader reader = new ExcelReader();
            ExcelResult excel = reader.Read(stream);

            CellData teamNameCell = excel.Headers.FirstOrDefault(h => h.Value.Equals("Ime ekipe", StringComparison.OrdinalIgnoreCase));
            CellData firstNameCell = excel.Headers.FirstOrDefault(h => h.Value.Equals("Ime člana", StringComparison.OrdinalIgnoreCase));
            CellData lastNameCell = excel.Headers.FirstOrDefault(h => h.Value.Equals("Priimek člana", StringComparison.OrdinalIgnoreCase));

            string lastTeamName = string.Empty;
            TeamData team = new TeamData();
            foreach (Dictionary<CellData, CellData> rowData in excel.DerivedData)
            {
                // Get current team name
                string teamName = ExcelUtils.GetValue(teamNameCell, rowData);
                if (teamName != lastTeamName && !string.IsNullOrWhiteSpace(teamName))
                {
                    team = new TeamData();
                    team.Name = teamName;

                    teams.Add(team);

                    lastTeamName = teamName;
                }

                string firstName = ExcelUtils.GetValue(firstNameCell, rowData);
                string lastName = ExcelUtils.GetValue(lastNameCell, rowData);

                team.ShooterNames.Add(lastName + ", " + firstName);
            }

            return teams;
        }
    }
}
