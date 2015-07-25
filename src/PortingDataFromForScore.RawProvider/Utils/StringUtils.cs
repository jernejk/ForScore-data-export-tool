using System;
using JetBrains.Annotations;

namespace Szds.ParsingOldResults.View.Providers
{
    public static class StringUtils
    {
        /// <exception cref="ParseException">Varies parse errors!</exception>
        [Pure]
        public static int IndexOfOrThrow(this string text, string searchString, string parseError = "Unable to find {0} at offset {1}")
        {
            return IndexOfOrThrow(text, searchString, 0, parseError);
        }

        /// <exception cref="ParseException">Varies parse errors!</exception>
        [Pure]
        public static int IndexOfOrThrow(this string text, string searchString, int offset, string parseError = "Unable to find {0} at offset {1}")
        {
            if (text == null)
            {
                throw new ParseException("Text is null while trying to use IndexOf.",
                    new ParseException(string.Format(parseError, searchString, offset)));
            }

            int i = text.IndexOf(searchString, offset, StringComparison.OrdinalIgnoreCase);

            if (i < 0)
            {
                throw new ParseException(string.Format(parseError, searchString, offset));
            }

            return i;
        }

        /// <exception cref="ParseException">Varies parse errors!</exception>
        [Pure]
        public static int LastIndexOfOrThrow(this string text, string searchString, string parseError = "Unable to find {0} at offset {1}")
        {
            if (text == null)
            {
                throw new ParseException("Text is null while trying to use IndexOf.",
                    new ParseException(string.Format(parseError, searchString, -1)));
            }

            return LastIndexOfOrThrow(text, searchString, text.Length - 1, parseError);
        }

        /// <exception cref="ParseException">Varies parse errors!</exception>
        [Pure]
        public static int LastIndexOfOrThrow(this string text, string searchString, int offset, string parseError = "Unable to find {0} at offset {1}")
        {
            if (text == null)
            {
                throw new ParseException("Text is null while trying to use IndexOf.",
                    new ParseException(string.Format(parseError, searchString, offset)));
            }

            int i = text.LastIndexOf(searchString, offset, StringComparison.OrdinalIgnoreCase);

            if (i < 0)
            {
                throw new ParseException(string.Format(parseError, searchString, offset));
            }

            return i;
        }

        /// <exception cref="ParseException">Unable to cast string to integer.</exception>
        public static int CastToIntOrThrow(this string text)
        {
            int temp;

            if (!int.TryParse(text, out temp))
            {
                throw new ParseException(string.Format("Unable to case {0} to integer!", text));
            }

            return temp;
        }

        /// <exception cref="ParseException">Unable to cast string to integer.</exception>
        public static double CastToDoubleOrThrow(this string text)
        {
            double temp;

            if (!double.TryParse(text.Replace(",", "."), out temp))
            {
                throw new ParseException(string.Format("Unable to case {0} to integer!", text));
            }

            return temp;
        }

        /// <exception cref="ArgumentOutOfRangeException">Condition. </exception>
        public static int StringToMonth(string month)
        {
            switch (month.ToLower())
            {
                case "januar":
                case "january":
                    return 1;

                case "februar":
                case "february":
                    return 2;

                case "marec":
                case "march":
                    return 3;

                case "april":
                    return 4;

                case "maj":
                case "may":
                    return 5;

                case "junij":
                case "june":
                    return 6;

                case "julij":
                case "july":
                    return 7;

                case "avgust":
                case "august":
                    return 8;

                case "september":
                    return 9;

                case "oktober":
                case "october":
                    return 10;

                case "november":
                    return 11;

                case "december":
                    return 12;

                default:
                    throw new ArgumentOutOfRangeException(month, "Unknown month " + month);
            }
        }

        public static bool IsNumber(this string text)
        {
            int temp;

            return int.TryParse(text, out temp);
        }

        public static string RemoveInitialDigits(this string text)
        {
            int subFrom = 0;
            while (subFrom < text.Length)
            {
                char digit = text[subFrom];
                if (digit >= '0' && digit <= '9')
                {
                    ++subFrom;
                }
                else
                {
                    break;
                }
            }

            return text.Substring(subFrom);
        }
    }
}
