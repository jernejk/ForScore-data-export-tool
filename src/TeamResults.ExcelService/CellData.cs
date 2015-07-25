using System.Diagnostics;

namespace TeamResults.ExcelService
{
    [DebuggerDisplay("{Range}: {Value}")]
    public class CellData
    {
        public string Range { get; set; }

        public string Value { get; set; }
    }
}
