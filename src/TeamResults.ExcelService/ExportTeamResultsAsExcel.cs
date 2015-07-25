using PortingDataFromForScore.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace TeamResults.ExcelService
{
    public class ExportTeamResultsAsExcel
    {
        public Stream Export(List<TeamResult> results, Stream excelTemplate)
        {
            using (var z = new ZipArchive(excelTemplate, ZipArchiveMode.Update))
            {
                ZipArchiveEntry sharedString = z.GetEntry("xl/sharedStrings.xml");

                byte[] data;
                using (Stream stream = sharedString.Open())
                {
                    data = new byte[stream.Length];
                    stream.Read(data, 0, (int)stream.Length);
                }

                string content = Encoding.UTF8.GetString(data, 0, data.Length);

                string teamNameFormat = "[$teamName{0}]";
                string teamScoreFormat = "[$teamScore{0}]";
                for (int i = 0; i < results.Count; ++i)
                {
                    content = content.Replace(string.Format(teamNameFormat, (i + 1)), results[i].Name);
                    content = content.Replace(string.Format(teamScoreFormat, (i + 1)), Math.Round(results[i].Score, 2).ToString());
                }

                for (int i = results.Count; i < 200; ++i)
                {
                    string teamName = string.Format(teamNameFormat, (i + 1));
                    if (!content.Contains(teamName))
                    {
                        break;
                    }

                    content = content.Replace(teamName, string.Empty);
                    content = content.Replace(string.Format(teamScoreFormat, (i + 1)), string.Empty);
                }

                sharedString.Delete();

                sharedString = z.CreateEntry("xl/sharedStrings.xml");

                using (StreamWriter stream = new StreamWriter(sharedString.Open()))
                {
                    stream.Write(content);
                }
            }

            return excelTemplate;
        }
    }
}
