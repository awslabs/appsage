using System.Data;

namespace AppSage.Web.Components.Filter.Table
{
    /// <summary>
    /// Model for the dynamic table component
    /// </summary>
    public class TableModel
    {
        public string MetricTableName { get; set; } = string.Empty;
        public DataTable Table { get; set; } = new DataTable();
        public string Title { get; set; } = string.Empty;
        public int PageSize { get; set; } = 10;
        public bool ShowFooter { get; set; } = true;
        public bool AllowExport { get; set; } = true;
        public IEnumerable<string>? ColumnNames { get; set; }
        public int CurrentPage { get; set; } = 1;
        public string SortColumn { get; set; } = string.Empty;
        public string SortDirection { get; set; } = "asc";
        
        /// <summary>
        /// Total number of rows in the full dataset (before pagination)
        /// This should be set by the server-side logic
        /// </summary>
        public int TotalRowCount { get; set; } = 0;

        // Computed properties
        public int TotalPages => TotalRowCount > 0 
            ? (int)Math.Ceiling(TotalRowCount / (double)PageSize) 
            : (Table != null && Table.Rows.Count > 0 ? 1 : 0);

        public IEnumerable<DataColumn> ColumnsToShow => ColumnNames != null && ColumnNames.Any()
            ? Table?.Columns.Cast<DataColumn>().Where(c => ColumnNames.Contains(c.ColumnName)) ?? Enumerable.Empty<DataColumn>()
            : Table?.Columns.Cast<DataColumn>() ?? Enumerable.Empty<DataColumn>();

        /// <summary>
        /// Returns the rows to display. Since server-side pagination and sorting is already applied,
        /// this simply returns all rows from the Table without additional processing.
        /// </summary>
        public IEnumerable<DataRow> DisplayRows
        {
            get
            {
                if (Table == null || Table.Rows.Count == 0)
                    return Enumerable.Empty<DataRow>();

                // The Table already contains the correctly sorted and paginated data
                // from the server-side GetTable method, so we just return all rows
                return Table.Rows.Cast<DataRow>();
            }
        }
    }
}