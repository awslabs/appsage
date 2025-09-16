using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text;

namespace AppSage.Web.Components.DataExport
{
    [Route("api/dataexport")]
    [ApiController]
    public class DataExportController : Controller
    {
        // Store tables in a static dictionary to keep their state between requests
        private static Dictionary<string, DataExportModel> _dataSets = new Dictionary<string, DataExportModel>();

        [NonAction]
        public IActionResult RenderDataExport(DataExportModel model)
        {
            // Save the table in the static dictionary
            _dataSets[model.DataExportId] = model;
            return Ok();
        }

        [HttpGet("{id}")]
        public IActionResult GetDataExport(string id)
        {
            if (!_dataSets.ContainsKey(id))
                return NotFound("Data set not found");

            var model = _dataSets[id];
            // Return the Razor View with the model
            return PartialView(ConstString.DATA_EXPORT_VIEW_PATH, model);
        }

        [HttpGet("{id}/export")]
        public IActionResult ExportData(string id)
        {
            if (!_dataSets.ContainsKey(id))
                return NotFound("Data set not found");

            var model = _dataSets[id];

            // Generate CSV
            var csv = ExportTableToCsv(model.Table);

            // Return as file
            return File(
                Encoding.UTF8.GetBytes(csv),
                "text/csv",
                $"{model.Title ?? "Table"}.csv"
            );
        }

        private string ExportTableToCsv(DataTable table, IEnumerable<string>? columnNames = null)
        {
            var columns = columnNames != null && columnNames.Any()
                ? table.Columns.Cast<DataColumn>().Where(c => columnNames.Contains(c.ColumnName))
                : table.Columns.Cast<DataColumn>();

            var sb = new StringBuilder();

            // Add headers
            sb.AppendLine(string.Join(",", columns.Select(c => $"\"{c.ColumnName}\"")));

            // Add rows
            foreach (DataRow row in table.Rows)
            {
                var values = columns.Select(c => $"\"{row[c].ToString()?.Replace("\"", "\"\"")}\"");
                sb.AppendLine(string.Join(",", values));
            }

            return sb.ToString();
        }
    }
}