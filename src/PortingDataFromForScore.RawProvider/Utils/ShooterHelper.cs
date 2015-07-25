using System;
using System.Collections.Generic;
using System.Linq;
using Szds.ParsingOldResults.View.Data;

namespace Szds.ParsingOldResults.View.Providers
{
    public class ShooterHelper
    {
        public static ShooterData AddOrGet(string fullName, List<ShooterData> shooters, MatchData match)
        {
            var shooter = shooters.FirstOrDefault(s => s.FullName.Equals(fullName, StringComparison.CurrentCultureIgnoreCase));

            if (shooter == null)
            {
                shooter = new ShooterData();

                shooter.Id = 1;
                if (shooters.Count > 0)
                {
                    shooter.Id = shooters.Max(s => s.Id) + 1;
                }

                shooter.FullName = fullName;

                shooters.Add(shooter);
            }

            return shooter;
        }

        public static void UpdateShooter(ShooterData shooter, string division, string classification, MatchData match)
        {
            string value = division + " " + classification;

            if (shooter.UsedDivisionClassifications.All(d => d != value))
            {
                shooter.UsedDivisionClassifications.Add(value);
            }
        }

        public static void UpdateStats(ShooterData shooter, ShooterMatchData data, MatchData match)
        {
            if (shooter.DatesOfMatches.All(d => d != match.Date))
            {
                shooter.DatesOfMatches.Add(match.Date);

                if (match.Champions.Any(c => c.ShooterId == shooter.Id))
                {
                    ++shooter.DC;
                }

                if (data.DNF)
                {
                    ++shooter.DNF;
                }

                shooter.FN += data.FN;
                shooter.NT += data.NT;
                shooter.PD += data.PD;
                shooter.PE += data.PE;
                ++shooter.TotalMatches;
                shooter.TotalTime += data.TotalTime;

                if (!data.DNF)
                {
                    shooter.TotalTimeWithoutDnf += data.TotalTime;
                }
            }
        }
    }
}
