using AppSage.Web.Components.DataExport;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace AppSage.Web.Extensions
{
    public static class ComponentDataExportExtensions
    {
        public static async Task<IHtmlContent> RenderDataExportAsync<K, V>(
      this IHtmlHelper htmlHelper,
      Dictionary<K, V> data,
      string title = "",
      string dataExportId = ""
      )
        {
            DataTable table = new DataTable();
            if (data != null && data.Any())
            {
                // Create columns based on the first item
                var firstItem = data.First();
                table.Columns.Add("Key", typeof(K));
                table.Columns.Add("Value", typeof(V));
                // Add rows for each key-value pair
                foreach (var kvp in data)
                {
                    table.Rows.Add(kvp.Key, kvp.Value);
                }
            }

            return await htmlHelper.RenderDataExportAsync(table, title, dataExportId);
        }
        public static async Task<IHtmlContent> RenderDataExportAsync(
            this IHtmlHelper htmlHelper,
            DataTable table,
            string title = "",
            string dataExportId = ""
            )
        {
            // Get the TableController from services
            var controller = htmlHelper.ViewContext.HttpContext.RequestServices
                .GetService(typeof(DataExportController)) as DataExportController;

            if (controller == null)
            {
                return new HtmlString("<div class='alert alert-danger'>data export controller not found in services.</div>");
            }

            var model = new DataExportModel
            {
                Table = table,
                Title = title,

            };

            // Save the dataset in the controller for future AJAX requests
            controller.RenderDataExport(model);

            try
            {
                // Render the table view using PartialAsync to avoid deadlocks
                return await htmlHelper.PartialAsync(Components.DataExport.ConstString.DATA_EXPORT_VIEW_PATH, model);
            }
            catch (Exception ex)
            {
                return new HtmlString($"<div class='alert alert-danger'>Error rendering table: {ex.Message}</div>");
            }
        }
    }
}