using System.Diagnostics;
using System.IO;

namespace PortingDataFromForScore.TpsToCsv
{
    /// <summary>
    /// Links:
    /// http://www.ctrl-alt-dev.nl/Projects/TPS-to-CSV-HowTo/TPS-to-CSV-HowTo.html
    /// https://github.com/ctrl-alt-dev/tps-parse
    /// http://www.ctrl-alt-dev.nl/Projects/TPS-to-CSV/TPS-to-CSV.html
    /// </summary>
    public static class TpsToCsvConverter
    {
        public static void ConvertTpsToCsv(string tpsPath, string csvPath)
        {
            var files = Directory.GetFiles(tpsPath, "*.tps");
            if (files.Length == 0)
            {
                return;
            }

            if (!Directory.Exists(csvPath))
            {
                Directory.CreateDirectory(csvPath);
            }

            foreach (var file in files)
            {
                string csvFile = Path.Combine(csvPath, Path.GetFileNameWithoutExtension(file) + ".csv");
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = Directory.GetCurrentDirectory() + @"\Tools\tps-to-csv.jar";
                startInfo.Arguments = "-s \"" + file + "\" -t \"" + csvFile + "\"";

                var process = Process.Start(startInfo);
                process.WaitForExit(20000);

                if (process.ExitCode != 0)
                {
                    // TODO: Handle error
                }
            }
        }
    }
}
