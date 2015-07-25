using System;
using System.Collections.Generic;
using System.Linq;
using Szds.ParsingOldResults.View.Data;
using JetBrains.Annotations;
using Szds.ParsingOldResults.View.Utils;

namespace Szds.ParsingOldResults.View.Providers
{
    public class RawDataProvider : IProvider
    {
        private MatchData match;

        /// <exception cref="ArgumentException">No new lines in input text!</exception>
        /// <exception cref="ParseException">Varies parse errors!</exception>
        public MatchData Parse(string text, List<ShooterData> existingShooters)
        {
            match = new MatchData();

            string[] lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
            {
                throw new ArgumentException("No new lines in input text!");
            }

            ExtractMatchData(lines);
            ExtractChampions(lines, existingShooters);
            ExtractShootersScores(lines, existingShooters);

            return match;
        }

        /// <exception cref="ParseException">Varies parse errors!</exception>
        private void ExtractMatchData(string[] lines)
        {
            // Get match data All Participants
            string matchDataLine = lines.FirstOrDefault(l => l.IndexOf("Full Results", StringComparison.OrdinalIgnoreCase) > 0 && l.IndexOf("All Participants", StringComparison.OrdinalIgnoreCase) > 0);

            if (string.IsNullOrWhiteSpace(matchDataLine))
            {
                throw new ParseException("Unable to find match header data!");
            }

            // 1. Tekma za Državno prvenstvo 2010 April 3, 2010 Full Results  -   All Participants
            int end = matchDataLine.IndexOfOrThrow(",");
            string titlePart = matchDataLine.Substring(0, end).Replace("  ", " ").Replace("  ", " ");

            end = titlePart.LastIndexOfOrThrow(" ");
            int day = titlePart.Substring(end).CastToIntOrThrow();

            int start = titlePart.LastIndexOfOrThrow(" ", end - 1);
            string monthS = titlePart.Substring(start + 1, end - start - 1);
            int month = StringUtils.StringToMonth(monthS);

            end = start;
            start = titlePart.LastIndexOfOrThrow(" ", end - 1);
            int year = titlePart.Substring(start + 1, end - start - 1).CastToIntOrThrow();

            match.Date = new DateTime(year, month, day);
            match.Title = matchDataLine.Substring(0, end);
        }

        private void ExtractChampions(string[] lines, List<ShooterData> shooters)
        {
            string[] champions =
                lines.First(l => l.IndexOf("champion:", StringComparison.OrdinalIgnoreCase) > 0)
                    .Split(new[] {"Champion:"}, StringSplitOptions.RemoveEmptyEntries);

            string division = champions[0].Trim();

            for (int i = 1; i < champions.Length; ++i)
            {
                string[] data = champions[i].Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries);

                string fullName = data[0].Trim() + " " + data[1].Trim();
                string classification = data[2].Trim();

                var shooter = ShooterHelper.AddOrGet(fullName, shooters, match);
                
                ShooterHelper.UpdateShooter(shooter, division, classification, match);

                ChampionsData champion = new ChampionsData();
                champion.ShooterId = shooter.Id;
                champion.ShooterFullName = shooter.FullName;
                champion.Division = division;
                champion.Classification = classification;
                champion.Score = ScoreData.Parse(data[3], data[4]);

                match.Champions.Add(champion);

                // This is for next shooter
                division = data[data.Length - 1].Trim();
            }

        }

        private void ExtractShootersScores(string[] lines, List<ShooterData> shooters)
        {
            int startLine = 0;

            for (int i = 0; i < lines.Length; ++i)
            {
                if (lines[i].StartsWith("Penalties"))
                {
                    startLine = i;
                    break;
                }
            }

            int stages = lines[startLine].Trim().Last().ToString().CastToIntOrThrow();
            match.NumberOfStages = stages;

            string division = string.Empty;
            string classification = string.Empty;
            string[] columns = new string[0];
            int offset = 0;

            while (startLine < lines.Length)
            {
                if (offset >= columns.Length)
                {
                    ++startLine;
                    offset = 0;

                    if (startLine >= lines.Length)
                    {
                        break;
                    }

                    columns = lines[startLine].Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                }

                if (lines[startLine].Contains("Total Match Penalties:"))
                {
                    break;
                }

                if (columns.Length == 0 || string.IsNullOrWhiteSpace(lines[startLine]) || lines[startLine].Contains("Penalties") ||
                    lines[startLine].Contains("Key to Abbreviations") || lines[startLine].Contains("Specific Results:") ||
                    lines[startLine].Contains("All Participants") || lines[startLine].Contains("IDPA") || lines[startLine].Contains("file://") ||
                    lines[startLine].Contains("1 od "))
                {
                    offset = columns.Length;
                    continue;
                }

                if (columns[offset].IsNumber() && columns.Length < 3)
                {
                    offset = columns.Length;
                    continue;
                }

                if (ClassificationHelper.IsDivision(columns[offset]))
                {
                    division = columns[offset];
                    classification = columns[offset + 1];

                    offset += 2;
                }

                if (offset >= columns.Length)
                {
                    continue;
                }

                ShooterMatchData data = new ShooterMatchData();
                data.Division = division;
                data.Classification = classification;

                if (columns[offset] == "DC")
                {
                    data.DC = true;
                    ++offset;

                    // May happen sometimes
                    if (offset >= columns.Length)
                    {
                        ++startLine;
                        offset = 0;

                        if (startLine >= lines.Length)
                        {
                            break;
                        }

                        columns = lines[startLine].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                }
                else if (columns[offset].IsNumber())
                {
                    ++offset;
                }
                else if (columns[offset].Length > 3 && columns[offset].StartsWith("DC"))
                {
                    columns[offset] = columns[offset].Substring(2);
                    data.DC = true;
                }

                string fullName = columns[offset].RemoveInitialDigits() + " " + columns[offset + 1];
                offset += 2;

                if (columns[offset] == "-")
                {
                    fullName += " - " + columns[offset + 1];
                    offset += 2;
                }

                var shooter = ShooterHelper.AddOrGet(fullName, shooters, match);
                ShooterHelper.UpdateShooter(shooter, division, classification, match);

                data.ShooterId = shooter.Id;
                data.ShooterFullName = shooter.FullName;

                // If shooter has completed entire match
                if (columns[offset] != "---")
                {
                    data.TotalTime = columns[offset].CastToDoubleOrThrow();
                }
                else
                {
                    data.DNF = true;
                }

                ++offset;

                try
                {
                    // PD,NT,PE,FN
                    data.PD = columns[offset++].CastToIntOrThrow();
                    data.NT = columns[offset++].CastToIntOrThrow();
                    data.PE = columns[offset++].CastToIntOrThrow();
                    data.FN = columns[offset++].CastToIntOrThrow();
                }
                catch
                {
                    data.HasPotentialErrors = true;

                    --offset;
                    while (offset < columns.Length)
                    {
                        if (columns[offset].Contains("("))
                        {
                            break;
                        }

                        ++offset;
                    }

                    if (offset >= columns.Length)
                    {
                        throw new ParseException();
                    }
                }

                for (int i = 0; i < stages; ++i)
                {
                    var score = ScoreData.Parse(columns[offset]);
                    data.Scores.Add(score);

                    ++offset;
                }

                ShooterHelper.UpdateStats(shooter, data, match);
                match.ScoresByShooter.Add(data);
            }
        }
    }
}
