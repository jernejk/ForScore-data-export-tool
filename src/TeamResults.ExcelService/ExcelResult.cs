using System.Collections.Generic;

namespace TeamResults.ExcelService
{
    public class ExcelResult
    {
        public ExcelResult() { }

        public ExcelResult(List<Dictionary<CellData, CellData>> derivedData, List<CellData> headers)
        {
            DerivedData = derivedData;
            Headers = headers;
        }

        public List<Dictionary<CellData, CellData>> DerivedData { get; set; }
        public List<CellData> Headers { get; set; }
    }
}
