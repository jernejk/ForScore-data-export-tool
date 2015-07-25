using System.Collections.Generic;
using System.Diagnostics;

namespace PortingDataFromForScore.Data
{
    [DebuggerDisplay("{FullName}: {TotalScore}")]
    public class ShooterStageData
    {
        public ShooterStageData()
        {
            Stages = new List<StageData>();
        }

        public int RecordNumber { get; set; }
        public int ResultNumber { get; set; }
        public int ShooterId { get; set; }
        public string FullName { get; set; }
        public int MatchId { get; set; }
        public string Division { get; set; }
        public string Classification { get; set; }
        public bool Completed { get; set; }
        public List<StageData> Stages { get; set; }

        public bool Industry { get; set; }
        public bool International { get; set; }
        public bool Junior { get; set; }
        public bool Lady { get; set; }
        public bool LawEnforcementOfficer { get; set; }
        public bool Military { get; set; }
        public bool Press { get; set; }
        public bool Senior { get; set; }
        public bool Staff { get; set; }
        public bool SecurityOfficer { get; set; }
        public bool Veteran { get; set; }

        public double TotalPenalty { get; set; }
        public double TotalStringTime { get; set; }
        public double TotalScoreTime { get; set; }

    }
}
