namespace PortingDataFromForScore.Data
{
    using System.Collections.Generic;

    public class StageData
    {

        public StageData()
        {
            StringTimes = new List<double>(1);
            TargetScores = new List<int>();
        }

        public int RecordNumber { get; set; }

        public int StageNumber { get; set; }

        public int ShooterId { get; set; }

        public int MatchId { get; set; }

        public List<double> StringTimes { get; set; }

        public List<int> TargetScores { get; set; }

        public double ShootTime { get; set; }

        public int PointsDown { get; set; }

        public double TotalPenaltyTime { get; set; }

        public double TotalTime { get; set; }

        public bool FailureToDoRight { get; set; }

        public bool Disqualified { get; set; }

        public int NonThreats { get; set; }

        public int ProceduralErrors { get; set; }

        public int FailureToNeutralize { get; set; }

        public bool DidNotFinished { get; set; }
    }
}
