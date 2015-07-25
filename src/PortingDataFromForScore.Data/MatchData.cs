using System;
using System.Collections.Generic;

namespace Szds.ParsingOldResults.View.Data
{
    public class MatchData
    {
        public MatchData()
        {
            Title = string.Empty;
            Champions = new List<ChampionsData>();
            ScoresByShooter = new List<ShooterMatchData>();
        }

        public int MatchId { get; set; }

        public string Title { get; set; }

        public DateTime Date { get; set; }

        public int NumberOfStages { get; set; }

        public int MaxStrings { get; set; }

        public bool IsClassification { get; set; }

        public int NumberOfRegisteredShooters { get; set; }

        public List<ChampionsData> Champions { get; set; }

        public List<ShooterMatchData> ScoresByShooter { get; set; }
    }
}
