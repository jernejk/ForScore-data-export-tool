using System.Collections.Generic;

namespace Szds.ParsingOldResults.View.Data
{
    public class ShooterMatchData
    {
        public ShooterMatchData()
        {
            ShooterFullName = string.Empty;
            Division = string.Empty;
            Classification = string.Empty;

            Scores = new List<ScoreData>();
        }

        public int ShooterId { get; set; }

        public string ShooterFullName { get; set; }

        public string Division { get; set; }

        public string Classification { get; set; }

        public bool DC { get; set; }

        public int PD { get; set; }

        public int NT { get; set; }

        public int PE { get; set; }

        public int FN { get; set; }

        public bool DNF { get; set; }

        public double TotalTime { get; set; }

        public List<ScoreData> Scores { get; set; }

        public bool HasPotentialErrors { get; set; }
    }
}
