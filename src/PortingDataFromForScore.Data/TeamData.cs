using System.Collections.Generic;

namespace PortingDataFromForScore.Data
{
    public class TeamData
    {
        public TeamData()
        {
            Name = string.Empty;
            ShooterNames = new List<string>();
        }

        public string Name { get; set; }

        public List<string> ShooterNames { get; set; }
    }
}
