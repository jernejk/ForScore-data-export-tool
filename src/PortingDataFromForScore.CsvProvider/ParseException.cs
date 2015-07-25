using System;

namespace Szds.ParsingOldResults.View.Providers
{
    public class ParseException : Exception
    {
        public ParseException()
        {
        }

        public ParseException(string message)
            : base(message)
        {
        }

        public ParseException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
