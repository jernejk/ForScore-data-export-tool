using System.Collections.Generic;
using System.Linq;

namespace PortingDataFromForScore.Data
{
    public class TeamResult
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
