namespace Szds.ParsingOldResults.View.Utils
{
    public class ClassificationHelper
    {
        public static bool IsDivision(string division)
        {
            switch (division.ToUpper())
            {
                case "CDP":
                case "SSP":
                case "ESP":
                case "ESR":
                case "SSR":
                    return true;
            }

            return false;
        }

        public static bool IsClassification(string classification)
        {
            switch (classification.ToUpper())
            {
                case "DM":
                case "DISTINGUISHED MASTER":
                case "MA":
                case "MASTER":
                case "EX":
                case "EXPERT":
                case "SS":
                case "SHARPSHOOTER":
                case "MM":
                case "MARKSMAN":
                case "NV":
                case "NOVICE":
                case "UN":
                case "UNCLASSIFIED":
                    return true;
            }

            return false;
        }
    }
}
