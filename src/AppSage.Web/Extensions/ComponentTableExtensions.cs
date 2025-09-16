using AppSage.Web.Components.Filter.Table;
using AppSage.Web.Components.Filter;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using TableConstants = AppSage.Web.Components.Filter.Table.ConstString;

namespace AppSage.Web.Extensions
{
    public static class ComponentTableExtensions
    {
        /// <summary>
        /// Renders a dynamic table that leverages MetricFilterPageModel's table handlers.
        /// This creates an initial placeholder that loads data via AJAX on page load.
        /// </summary>
        public static async Task<IHtmlContent> RenderDynamicTableAsync(
            this IHtmlHelper htmlHelper,
            string metricTableName,
            string title = "",
            int pageSize = 10,
            bool showFooter = true,
            bool allowExport = true,
            IEnumerable<string>? columnNames = null)
        {
            // Validate that we're in a MetricFilterPageModel context
            var pageModel = htmlHelper.ViewContext.ViewData.Model as MetricFilterPageModel;
            if (pageModel == null)
            {
                return new HtmlString("<div class='alert alert-warning'>This table component can only be used in pages that inherit from MetricFilterPageModel.</div>");
            }

            // Create the model for the loading partial view
            var loadingModel = new TableLoadingModel
            {
                MetricTableName = metricTableName,
                Title = title,
                PageSize = pageSize,
                ShowFooter = showFooter,
                AllowExport = allowExport
            };

            // Render the loading partial view
            return await htmlHelper.PartialAsync(TableConstants.TABLE_LOADING_VIEW_PATH, loadingModel);
        }

    }
}