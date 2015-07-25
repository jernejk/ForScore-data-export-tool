using System.Collections.Generic;

namespace TeamResults.ExcelService
{
    public class ExcelUtils
    {
        public static string GetValue(CellData headerCell, Dictionary<CellData, CellData> row)
        {
            if (headerCell != null && row.ContainsKey(headerCell))
            {
                string value = row[headerCell].Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    value = value.Trim();
                }

                return value;
            }

            return string.Empty;
        }
    }
}
