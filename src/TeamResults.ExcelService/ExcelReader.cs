using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace TeamResults.ExcelService
{
    public class ExcelReader
    {
        List<string> sharedStrings;

        public ExcelResult Read(Stream stream)
        {
            List<Dictionary<CellData, CellData>> data;
            List<CellData> headers;

            using (var z = new ZipArchive(stream))
            {
                ZipArchiveEntry worksheet = z.GetEntry("xl/worksheets/sheet1.xml");
                ZipArchiveEntry sharedString = z.GetEntry("xl/sharedStrings.xml");

                // get shared string
                sharedStrings = new List<string>();
                using (var sr = sharedString.Open())
                {
                    XDocument xdoc = XDocument.Load(sr);
                    sharedStrings = (from e in xdoc.Root.Elements() select e.Elements().First().Value).ToList();
                }

                // get header
                using (var sr = worksheet.Open())
                {
                    XDocument xdoc = XDocument.Load(sr);

                    // get element to first sheet data
                    XNamespace xmlns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
                    XElement sheetData = xdoc.Root.Element(xmlns + "sheetData");

                    headers = new List<CellData>();

                    // build header first
                    var firstRow = sheetData.Elements().First();

                    // full of c
                    foreach (var c in firstRow.Elements())
                    {
                        headers.Add(GetCell(c));
                    }

                    // build content now
                    data = new List<Dictionary<CellData, CellData>>();

                    foreach (var row in sheetData.Elements().Skip(1))
                    {
                        var rowData = new Dictionary<CellData, CellData>();

                        foreach (var c in row.Elements())
                        {
                            CellData cell = GetCell(c);
                            CellData cellHeader = headers.FirstOrDefault(ch => ch.Range[0] == cell.Range[0]);

                            if (cellHeader != null)
                            {
                                rowData.Add(cellHeader, cell);
                            }
                        }

                        data.Add(rowData);
                    }
                }
            }

            return new ExcelResult(data, headers);
        }

        /// <summary>
        /// Get value of current element. Shared values are automatically resolved.
        /// NOTE: This will not calculate or execute any Excel code or formula.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns>Returns correct value.</returns>
        private string GetValue(XElement c)
        {
            // the c element, if have attribute t, will need to consult sharedStrings
            var firstElement = c.Elements().FirstOrDefault();

            if (firstElement != null)
            {
                string val = c.Elements().First().Value;

                int sharedIndex;
                if (c.Attribute("t") != null && int.TryParse(val, out sharedIndex))
                {
                    val = sharedStrings[sharedIndex];
                }

                return val;
            }

            return string.Empty;
        }

        private string GetRange(XElement c)
        {
            XAttribute attribute = c.Attribute("r");
            return attribute != null ? attribute.Value : string.Empty;
        }

        private CellData GetCell(XElement c)
        {
            var cell = new CellData();
            cell.Range = GetRange(c);
            cell.Value = GetValue(c);

            return cell;
        }
    }
}
