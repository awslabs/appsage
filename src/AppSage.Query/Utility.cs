using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.Query
{
    public class Utility
    {
        public static XLWorkbook ConvertToExcelDoc(IEnumerable<DataTable> tableSet)
        {
            var workbook = new XLWorkbook();

            if (tableSet == null || !tableSet.Any())
            {
                // Create an empty worksheet if no tables provided
                var emptyWorksheet = workbook.Worksheets.Add("Empty");
                return workbook;
            }

            int sheetIndex = 1;
            var usedNames = new HashSet<string>();

            foreach (var table in tableSet)
            {
                // Generate a unique worksheet name
                string worksheetName = GetWorksheetName(table.TableName, sheetIndex, usedNames);
                usedNames.Add(worksheetName);

                var worksheet = workbook.Worksheets.Add(worksheetName);

                // Use ClosedXML's built-in method to insert the table, which handles all the data types properly
                if (table.Rows.Count > 0)
                {
                    worksheet.Cell(1, 1).InsertTable(table);
                }
                else
                {
                    // If table is empty, at least add the headers
                    for (int col = 0; col < table.Columns.Count; col++)
                    {
                        worksheet.Cell(1, col + 1).Value = table.Columns[col].ColumnName;
                        worksheet.Cell(1, col + 1).Style.Font.Bold = true;
                    }
                }

                // Adjust column widths
                worksheet.Columns().AdjustToContents();

                sheetIndex++;
            }

            return workbook;
        }

        /// <summary>
        /// Returns a valid and unique worksheet name compatible with Excel's limitations
        /// </summary>
        private static string GetWorksheetName(string? tableName, int sheetIndex, HashSet<string> usedNames)
        {
            string baseName;

            if (string.IsNullOrWhiteSpace(tableName))
            {
                baseName = $"Sheet{sheetIndex}";
            }
            else
            {
                //Excel sheets cannot contain certain characters
                var invalidChars = new char[] { '\\', '/', '?', '*', '[', ']' };
                string sanitized = tableName;
                invalidChars.ToList().ForEach(c => sanitized = sanitized.Replace(c, '_'));

                // Excel worksheet names cannot exceed 31 characters and cannot contain certain characters
                if (sanitized.Length > 31)
                {
                    sanitized = sanitized.Substring(0, 31);
                }

                baseName = sanitized;
            }

            // Ensure uniqueness
            string uniqueName = baseName;
            int counter = 1;
            while (usedNames.Contains(uniqueName))
            {
                string suffix = $"_{counter}";
                if (baseName.Length + suffix.Length > 31)
                {
                    uniqueName = baseName.Substring(0, 31 - suffix.Length) + suffix;
                }
                else
                {
                    uniqueName = baseName + suffix;
                }
                counter++;
            }

            return uniqueName;
        }


        public static string GetCsvPreview(IXLWorksheet worksheet, int maxRows = 10)
        {
            var sb = new StringBuilder();

            // Get first N rows (or fewer if sheet is shorter)
            var rows = worksheet.RowsUsed().Take(maxRows);

            foreach (var row in rows)
            {
                var cells = row.CellsUsed().Select(c => EscapeCsv(c.GetValue<string>()));
                sb.AppendLine(string.Join(",", cells));
            }

            return sb.ToString();
        }

        // Helper to escape commas, quotes, newlines
        private static string EscapeCsv(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
                return $"\"{field.Replace("\"", "\"\"")}\"";

            return field;
        }
    }
}
