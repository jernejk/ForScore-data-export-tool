using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Szds.ParsingOldResults.View.Data;

namespace Szds.ParsingOldResults.View.Providers
{
    public interface IProvider
    {
        /// <exception cref="ArgumentException">No new lines in input text!</exception>
        /// <exception cref="ParseException">Varies parse errors!</exception>
        [Pure]
        [NotNull]
        MatchData Parse([NotNull] string text, [NotNull] List<ShooterData> existingShooters);
    }
}
