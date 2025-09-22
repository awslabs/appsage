using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Workspace;
using ClosedXML.Excel;
using ModelContextProtocol.Protocol;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text;

namespace AppSage.MCPServer.Capabilty
{
    public class ResultBuilder
    {
        IAppSageLogger _logger;
        private readonly (int MaxReturnedBlobSizeInBytes,string ResultOutputFolder) _config;
        public ResultBuilder(IAppSageLogger logger, IAppSageConfiguration configuration,IAppSageWorkspace workspace)
        {
            _logger = logger;
            _config.MaxReturnedBlobSizeInBytes = configuration.Get<int>("AppSage.MCPServer.Capabilty.Utility:MaxBase64EncodedReturnBlobSizeInKB") * 1024;
            _config.ResultOutputFolder = workspace.MCPServerOutputFolder;
            if(!Directory.Exists(_config.ResultOutputFolder))
            {
                Directory.CreateDirectory(_config.ResultOutputFolder);
            }
        }
        /// <summary>
        /// Creates a CallToolResult from any object, with smart type detection and content formatting
        /// </summary>
        /// <param name="result">The result object to convert</param>
        /// <returns>A properly formatted CallToolResult</returns>
        public CallToolResult CreateCallToolResult(object? result)
        {
            // If result is already a CallToolResult, return as-is
            if (result != null && result is CallToolResult passThru)
                return passThru;

            if (result == null)
            {
                return new CallToolResult
                {
                    Content = [new TextContentBlock { Text = string.Empty }]
                };
            }

            try
            {
                var contentBlocks = GetContentBlock(result);

                return new CallToolResult
                {
                    Content = contentBlocks
                };

            }
            catch (Exception ex)
            {
                return new CallToolResult
                {
                    Content = [new TextContentBlock { Text = $"Error processing result: {ex.Message}" }],
                    IsError = true
                };
            }
        }


        private IList<ContentBlock> GetContentBlock(Object? result)
        {
            if (result == null)
            {
                return [new TextContentBlock { Text = string.Empty }];
            }


            // Handle IEnumerable<DataTable> - convert to Excel file
            if (result is IEnumerable<DataTable> dataTables)
            {
                try
                {
                    var workbook = ConvertToExcelDoc(dataTables);
                    var fileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    var collector = new List<ContentBlock>();
                    // Use memory stream instead of temporary files
                    using var stream = new MemoryStream();
                    workbook.SaveAs(stream);

                    string filePath = Path.Combine(_config.ResultOutputFolder, fileName);
                    File.WriteAllBytes(filePath, stream.ToArray());
                    
                    var fileResult = new ResourceLinkBlock
                    {
                        Name = fileName,
                        Uri = new Uri(filePath).AbsoluteUri,
                        MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    };

                    collector.Add(new TextContentBlock() { Text = "Excel file has been created. Please view it with Excel" });
                    collector.Add(fileResult);
                    if (workbook.Worksheets.Count == 1)
                    {
                        var ws = workbook.Worksheet(1);
                        var previewBuilder = new StringBuilder();
                        previewBuilder.AppendLine("A small preview of the ouptput is given below.");
                        string csvPreview = GetCsvPreview(ws, 10);
                        previewBuilder.AppendLine(csvPreview);
                        previewBuilder.AppendLine("...");
                        previewBuilder.AppendLine("For a detailed preview, remember to open the excel file in the result");
                        var previewBlock = new TextContentBlock() { Text = previewBuilder.ToString() };
                        collector.Add(previewBlock);
                    }



                    return collector;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error converting DataTables to Excel", ex);
                }
            }

            // Handle JObject from Newtonsoft.Json - convert to JSON file
            if (result is JObject jObject)
            {
                try
                {
                    var jsonString = jObject.ToString(Newtonsoft.Json.Formatting.Indented);

                    // For small JSON objects, consider returning as text instead of file
                    // For now, always return as downloadable file for consistency
                    using var stream = new MemoryStream();
                    using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
                    writer.Write(jsonString);
                    writer.Flush();

                    var base64Content = Convert.ToBase64String(stream.ToArray());
                    var fileName = $"JObject_{DateTime.Now:yyyyMMdd_HHmmss}.json";

                    return [new EmbeddedResourceBlock
                        {
                            Resource = new BlobResourceContents
                            {
                                Blob = base64Content,
                                MimeType = "application/json"
                            }
                        }];
                }
                catch (Exception ex)
                {
                    throw new Exception("Error converting JObject to json content", ex);
                }
            }

            // Default: convert to string
            var text = Convert.ToString(result) ?? string.Empty;
            return [new TextContentBlock { Text = text }];
        }

        private static XLWorkbook ConvertToExcelDoc(IEnumerable<DataTable> tableSet)
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
