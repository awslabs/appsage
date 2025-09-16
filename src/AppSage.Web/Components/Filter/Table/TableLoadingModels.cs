namespace AppSage.Web.Components.Filter.Table
{
    /// <summary>
    /// Model for the table loading partial view
    /// </summary>
    public class TableLoadingModel
    {
        public string MetricTableName { get; set; } = string.Empty;
        public string ContainerId { get { return $"table-container-{MetricTableName}"; } }
        public string Title { get; set; } = string.Empty;
        public int PageSize { get; set; } = 10;
        public bool ShowFooter { get; set; } = true;
        public bool AllowExport { get; set; } = true;
    }
}