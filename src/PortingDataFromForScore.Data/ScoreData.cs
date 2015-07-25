using System.Diagnostics;
using System.Globalization;

namespace Szds.ParsingOldResults.View.Data
{
    [DebuggerDisplay("{Time} ({PointsDown})")]
    public class ScoreData
    {
        public double Time { get; set; }

        public int PointsDown { get; set; }

        public static ScoreData Parse(string text)
        {
            string[] points = text.Split('(');

            ScoreData score = new ScoreData();
            score.Time = double.Parse(points[0].Replace(",", "."), CultureInfo.InvariantCulture);
            score.PointsDown = int.Parse(points[1].Replace(")", string.Empty), CultureInfo.InvariantCulture);

            return score;
        }

        public static ScoreData Parse(string time, string points)
        {
            ScoreData score = new ScoreData();
            score.Time = double.Parse(time.Replace(",", "."), CultureInfo.InvariantCulture);
            score.PointsDown = int.Parse(points.Replace(")", string.Empty).Replace("(", string.Empty), CultureInfo.InvariantCulture);

            return score;
        }
    }
}
