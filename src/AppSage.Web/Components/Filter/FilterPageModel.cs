using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using AppSage.Infrastructure.Metric;
using AppSage.Infrastructure.Workspace;
using AppSage.Web.Components.Filter.Table;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Data;
using System.Security.Cryptography.Xml;
namespace AppSage.Web.Components.Filter
{
    public abstract class MetricFilterPageModel : PageModel
    {

        private const string EMPTY_SEGMENT = "<Empty Segment>";

        [IdName("segment", "Segment")]
        public FilterModel SegmentFilter { get; set; } = new FilterModel();

        [IdName("provider", "Provider")]
        public FilterModel ProviderFilter { get; set; } = new FilterModel();

        IMetricReader _metircReader;

        public MetricFilterPageModel(IMetricReader metricReader)
        {
            _metircReader= metricReader ?? throw new ArgumentNullException(nameof(metricReader));
            PopulateFilters();
          
        }

        //public MetricFilterPageModel()
        //{
        //    PopulateFilters();
        //}

        /// <summary>
        /// Default handler for GET requests. This can be overridden in derived classes to customize the initial data loading.
        /// By default, it initializes the filters with all available values and loads the data.
        /// </summary>
        public virtual void OnGet()
        {
            // Initialize filters with all available values
            SegmentFilter.AddSelectionRange(SegmentFilter.Values);
            ProviderFilter.AddSelectionRange(ProviderFilter.Values);
            LoadData();
        }

        /// <summary>
        /// Default handler for POST requests. This method processes the form data submitted by the user.
        /// By default processes checkbox selections for both SegmentFilter and ProviderFilter
        /// This can be overridden in derived classes to customize the behavior after form submission.
        /// </summary>
        public virtual void OnPost()
        {

            var form = Request.Form;

            // Clear previous selections
            SegmentFilter.ClearSelectedSegments();

            // Process checkbox selections
            foreach (var item in SegmentFilter.Values)
            {
                string checkboxName = IdNameAttribute.GetHtmlId(() => SegmentFilter, item);

                if (form.ContainsKey(checkboxName))
                {
                    SegmentFilter.AddSelection(item);
                }
            }

            foreach (var item in ProviderFilter.Values)
            {
                string checkboxName = IdNameAttribute.GetHtmlId(() => ProviderFilter, item);

                if (form.ContainsKey(checkboxName))
                {
                    ProviderFilter.AddSelection(item);
                }
            }
            LoadData();
        }


        /// <summary>
        /// How data export is handled. This method is called when the user requests a data export.
        /// It retrieves the data based on the provided data export name and converts it to an Excel workbook.
        /// By default, it calls the deligated method ExportData to get the data to be exported.
        /// ExportData can be overridden in derived classes to customize the data export logic.
        /// By default, it calls the default implementation of ExportData which returns metrics 
        /// that are of type IMetricValue<DataTable> with the dataExportName matching the name of the metric.
        /// If no dataExportName is provided, it exports all metrics of type IMetricValue<DataTable>.
        /// Before overriding this method, think of overriding the ExportData method in the derived classes.
        /// </summary>
        /// <param name="dataExportName">An identifier to identify name of the data export</param>
        /// <returns></returns>
        public virtual IActionResult OnGetDataExport(string dataExportName)
        {
            IEnumerable<DataTable> tableToBeExported = ExportData(dataExportName);


            if (tableToBeExported != null && tableToBeExported.Any())
            {
                XLWorkbook workbook = ConverToWorkBook(tableToBeExported);

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                string fileName = "DataExport.xlsx";

                if (!string.IsNullOrEmpty(dataExportName))
                {
                    fileName = $"DataExport-{dataExportName}.xlsx";
                }
                return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            fileName);

            }
            else
            {
                return NotFound($"The given data export name [{dataExportName}] is not known");
            }
        }

        /// <summary>
        /// Provide a way to render a table for a given metric table name.
        /// </summary>
        /// <param name="metricTableName">name of the metric that contains a IMetricValue of DataTable. If you want to export a custom table, you need to add that table in GetMyMetrics </param>
        /// <param name="selectedPageIndex">During pagination, selected page index</param>
        /// <param name="pageSize">During pagination, max number of rows to show per page</param>
        /// <param name="sortColumn">Which column to use for sorting</param>
        /// <param name="sortDirection">Sort order</param>
        /// <returns></returns>
        public IActionResult OnGetTable(string metricTableName, int selectedPageIndex = 1, int pageSize = 10, string sortColumn = "", string sortDirection = "asc")
        {
            return GetTable(metricTableName, selectedPageIndex, pageSize, sortColumn, sortDirection);
        }


        /// <summary>
        /// Returns all metrics related to the current workspace
        /// Flow is as follows
        /// GetAllMetrics()> GetMyMetrics() [Customizable: add/remove/augment additoinal metrics] => GetFilteredMetrics() [Customizable: Apply UI filters]
        /// </summary>
        /// <returns>All metrics read from the current workspace</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        protected IEnumerable<IMetric> GetAllMetrics()
        {
            return _metircReader.GetMetricSet();
        }


        /// <summary>
        /// Returns a focused set of data. This can be overridden in derived classes to customize the data retrieval logic.
        /// By default, it returns all data metrics that are related to the current workspace.
        /// Flow is as follows
        /// GetAllMetrics()> GetMyMetrics() [Customizable: add/remove/augment additoinal metrics] => GetFilteredMetrics() [Customizable: Apply UI filters]
        /// </summary>
        /// <example>
        /// First get all metrics by calling base.GetAllMetrics() and then filter them based on your requirements. 
        /// You can also add addtional metrics (may be aggregated metrics or derived metrics, or brand new metrics) to the result set.
        /// </example>
        /// <returns>Set of metrics to operate on</returns>
        public virtual IEnumerable<IMetric> GetMyMetrics()
        {
            return GetAllMetrics();
        }

        /// <summary>
        /// Returns a filtered set of metrics based on the selected providers and segments.
        /// By default, the filtering is done based on the values selected in the UI filters. 
        /// This can be overridden in derived classes to customize the filtering logic.
        /// Flow is as follows
        /// GetAllMetrics()> GetMyMetrics() [Customizable: add/remove/augment additoinal metrics] => GetFilteredMetrics() [Customizable: Apply UI filters]
        /// </summary>
        protected virtual IEnumerable<IMetric> GetFilteredMetrics()
        {
            var allMetrics = GetMyMetrics();
            var metricsRelatedToSelectedProviders = allMetrics.Where(x => ProviderFilter.SelectedValues.Contains(x.Provider)).ToList();
            SegmentFilter.Values.Clear();
            metricsRelatedToSelectedProviders.Select(x => x.Segment).Distinct().ToList().ForEach(x => SegmentFilter.Values.Add(x));


            var selectedMetrics = metricsRelatedToSelectedProviders.Where(x => SegmentFilter.SelectedValues.Contains(x.Segment)).ToList();
            if (SegmentFilter.SelectedValues.Contains(EMPTY_SEGMENT))
            {
                selectedMetrics.AddRange(allMetrics.Where(x => string.IsNullOrEmpty(x.Segment)));
            }
            return selectedMetrics;
        }


        /// <summary>
        /// Abstract method to load data. This method should be implemented in derived classes to load the specific data required for the page.
        /// This should populate the view model with the necessary data for rendering the page. 
        /// At every Get and Post request, this method is called to load the data.
        /// </summary>
        protected abstract void LoadData();

        /// <summary>
        /// Called when the user requests a data export. This can be overridden in derived classes to customize the data export logic.
        /// By default, it retrieves all metrics of type IMetricValue<DataTable> that match the filter condition and exports them.
        /// If a dataExportName is provided, it filters the metrics by that name.
        /// </summary>
        /// <param name="dataExportName">An identifier to identify name of the data expor</param>
        /// <returns></returns>
        protected virtual IEnumerable<DataTable> ExportData(string dataExportName)
        {
            var metrics = GetFilteredMetrics();

            IEnumerable<DataTable?> tableSet = null;

            if (string.IsNullOrEmpty(dataExportName))
            {
                tableSet = metrics.Where(m => m is IMetricValue<DataTable>)
                                 .Select(m => (IMetricValue<DataTable>)m)
                                 .Where(m=>m.Value != null)
                                 .Select(m => m.Value);
            }
            else
            {
                tableSet = metrics.Where(m => m is IMetricValue<DataTable>)
                                  .Select(m => (IMetricValue<DataTable>)m)
                                  .Where(m => m.Name == dataExportName && m.Value != null)
                                  .Select(m => m.Value);
            }

            return tableSet.Where(t => t != null).Select(t => t!);
        }


        /// <summary>
        /// Populates the filters with distinct values from the metrics.
        /// If you want to customize the values appearing here, you need to change what metrics you add/remove/augment in the GetMyMetrics method.
        /// </summary>
        private void PopulateFilters()
        {
            var allMetrics = GetMyMetrics();
            var providerSet = new List<string>(allMetrics.Select(x => x.Provider).Distinct());
            providerSet.ForEach(p => ProviderFilter.Values.Add(p));

            var segmentSet = new List<string>(allMetrics.Select(x => x.Segment).Distinct());
            segmentSet.Add(EMPTY_SEGMENT);
            segmentSet.ForEach(s => SegmentFilter.Values.Add(s));
        }




        public IActionResult OnGetTableExport(string metricTableName)
        {
            var data = GetMyMetrics();
            var tableDataSet = data.Where(d => d is IMetricValue<DataTable>).Select(d => (IMetricValue<DataTable>)d).Where(t => metricTableName == t.Name && t.Value != null).Select(t => t.Value);

            if (tableDataSet == null || !tableDataSet.Any())
            {
                return NotFound("Table not found");
            }

            var collectionTable = new DataTable();
            //clone all table data and merge them into a single table
            foreach (DataTable t in tableDataSet)
            {
                if (collectionTable.Columns.Count == 0)
                {
                    collectionTable = t.Clone();
                }
                foreach (DataRow row in t.Rows)
                {
                    collectionTable.ImportRow(row);
                }
            }

            // Generate CSV
            var csv = ExportTableToCsv(collectionTable);

            // Return as file
            return File(
                System.Text.Encoding.UTF8.GetBytes(csv),
                "text/csv",
                $"{metricTableName}.csv"
            );
        }

        private string ExportTableToCsv(DataTable table, IEnumerable<string>? columnNames = null)
        {
            var columns = columnNames != null && columnNames.Any()
                ? table.Columns.Cast<DataColumn>().Where(c => columnNames.Contains(c.ColumnName))
                : table.Columns.Cast<DataColumn>();

            var sb = new System.Text.StringBuilder();

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

        private IActionResult GetTable(string metricTableName, int selectedPageIndex = 1, int pageSize = 10, string sortColumn = "", string sortDirection = "asc")
        {
            var data = GetMyMetrics();
            var tableDataSet = data.Where(d => d is IMetricValue<DataTable>).Select(d => (IMetricValue<DataTable>)d).Where(t => metricTableName == t.Name && t.Value != null).Select(t => t.Value);

            if (tableDataSet == null || !tableDataSet.Any())
            {
                return NotFound("Table not found");
            }
            var schmeaSample = tableDataSet.FirstOrDefault();

            foreach (DataTable t in tableDataSet)
            {
                if (t.Columns.Count != schmeaSample.Columns.Count)
                {
                    return BadRequest("Inconsistent table schema");
                }
                //check for column names and types
                for (int i = 0; i < t.Columns.Count; i++)
                {
                    if (t.Columns[i].ColumnName != schmeaSample.Columns[i].ColumnName || t.Columns[i].DataType != schmeaSample.Columns[i].DataType)
                    {
                        return BadRequest("Inconsistent table schema");
                    }
                }
            }
            var collectionTable = new DataTable();
            //clone all table data and merge them into a single table
            foreach (DataTable t in tableDataSet)
            {
                if (collectionTable.Columns.Count == 0)
                {
                    collectionTable = t.Clone();
                }
                foreach (DataRow row in t.Rows)
                {
                    collectionTable.ImportRow(row);
                }
            }

            // Store the total count before pagination
            var totalRowCount = collectionTable.Rows.Count;

            //sort the table and take the page
            if (!string.IsNullOrEmpty(sortColumn) && collectionTable.Columns.Contains(sortColumn))
            {
                var columnIndex = collectionTable.Columns.IndexOf(sortColumn);
                if (columnIndex != -1)
                {
                    if (sortDirection == "asc")
                        collectionTable.DefaultView.Sort = $"{sortColumn} ASC";
                    else
                        collectionTable.DefaultView.Sort = $"{sortColumn} DESC";
                }
            }
            var pagedData = collectionTable.AsEnumerable()
                .Skip((selectedPageIndex - 1) * pageSize)
                .Take(pageSize)
                .CopyToDataTable();

            var tableModel = new TableModel();
            tableModel.MetricTableName = metricTableName;
            tableModel.Table = pagedData;
            tableModel.Title = metricTableName;
            tableModel.PageSize = pageSize;
            tableModel.CurrentPage = selectedPageIndex;
            tableModel.SortColumn = sortColumn;
            tableModel.SortDirection = sortDirection;
            tableModel.TotalRowCount = totalRowCount; // Set the total count for proper pagination
            tableModel.ColumnNames = collectionTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
            // Return the Razor Partial View with the model using PageModel's Partial method
            return new PartialViewResult
            {
                ViewName = Table.ConstString.TABLE_VIEW_PATH,
                ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<TableModel>(ViewData, tableModel)
            };
        }
        private XLWorkbook ConverToWorkBook(IEnumerable<DataTable> tableSet)
        {
            var workbook = new XLWorkbook();

            if (tableSet != null && !tableSet.Any())
            {
                int sheetIndex = 1;
                foreach (var table in tableSet)
                {
                    string worksheetName = $"Sheet{sheetIndex++}";
                    if (!string.IsNullOrEmpty(table.TableName))
                    {
                        if (table.TableName.Length > 31)
                        {
                            // Excel worksheet names cannot exceed 31 characters
                            worksheetName = table.TableName.Substring(0, 31);
                        }
                        else
                        {
                            worksheetName = table.TableName;
                        }
                    }

                    var worksheet = workbook.Worksheets.Add(worksheetName);
                    worksheet.Cell(1, 1).InsertTable(table);
                }
            }else { 
                var worksheet = workbook.Worksheets.Add("Empty");
            }
            return workbook;
        }


    }



}
