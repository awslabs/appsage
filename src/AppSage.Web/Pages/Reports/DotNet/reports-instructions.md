# AppSage Reports UI Structure

## Report Architecture Overview

The AppSage web application follows a consistent pattern for creating analytical reports. Each report is structured as an ASP.NET Razor Page with the following components:

### 1. Core File Structure Pattern

Each report should have these files:
- `Index.cshtml` - The main view/template
- `Index.cshtml.cs` - The page model (code-behind)
- `IndexViewModel.cs` - The view model for data binding

### 2. Page Model Pattern (`Index.cshtml.cs`)

**Base Class**: All report page models should inherit from `MetricFilterPageModel`

```csharp
public class IndexModel : MetricFilterPageModel
{
    public IndexViewModel Dashboard { get; set; } = new IndexViewModel();

    // Override to specify which metrics/providers this report uses
    public override List<IMetric> GetMyMetrics()
    {
        string providerName = "AppSage.Providers.[Technology].[AnalysisType]Provider";
        var allMetrics = GetAllMetrics();
        return allMetrics.Where(x => x.Provider == providerName).ToList();
    }

    // Override to populate the Dashboard view model with processed data
    protected override void LoadData()
    {
        var metrics = GetFilteredMetrics();
        // Process metrics and populate Dashboard properties
        // Extract metric values, build charts data, create tables, etc.
    }
}
```

**Key Inherited Features from MetricFilterPageModel**:
- Built-in segment and provider filtering (`SegmentFilter`, `ProviderFilter`)
- Default GET/POST request handling
- Data export functionality (Excel/CSV)
- Dynamic table rendering capabilities
- Metric aggregation and filtering methods

** Table data export functionality**:
- All tables can be exported to Excel using the `ExportData` method.
- The dataExportName parameter is used to specify which data to export, allowing for flexibility in what metrics are included.
- Export data returns a collection of `DataTable` objects, which can be processed and returned as needed. Each data table represent an excel sheet in an exported file.

** Dashboard property**:
- The `Dashboard` property is an instance of `IndexViewModel`, which holds all the data needed for rendering the report.
- Index.cshtml should not access metrics directly; instead, it should use the `Dashboard` property to bind data to the view.

### 3. View Model Pattern (`IndexViewModel.cs`)

Structure the view model to organize different types of data: If multiple classes are needed to represent the data
they all shoud be in the same file as `IndexViewModel.cs` to maintain consistency and avoid unnecessary complexity.

An example view model might look like following. You of cause need extra classes like `MetricBox`, `ChartDataPoint`, `GraphData`, etc. to represent the data in a structured way in the 
same file as `IndexViewModel.cs`: 

```csharp
public class IndexViewModel
{
    // Metric boxes/summary data
    public MetricBox TotalCount { get; set; } = new MetricBox();
    public int SomeValue { get; set; }
    
    // Chart data (organized for Chart.js/ECharts)
    public Dictionary<string, int> SomeDistribution { get; set; } = new();
    public List<ChartDataPoint> ChartData { get; set; } = new();
    
    // Table data (metric table names for dynamic rendering)
    public string SomeMetricTableName { get; set; }
    
    // Complex data structures (for graphs, trees, etc.). Use Cytoscape.js for graphs
    public List<GraphData> DependencyGraphs { get; set; } = new();
    
    // Export data definitions
    public List<Tuple<string, string, string>> DataExports { get; set; } = new();
}
```

### 4. View Structure Pattern (`Index.cshtml`)

**Standard Layout Structure**:
An example Razor view structure that follows the established patterns:
```razor
@page
@model IndexModel
@{
    ViewData["Title"] = "[Technology] [Analysis Type]";
}
@using AppSage.Core.Metric;
@using AppSage.Web.Extensions;

<style>
    /* Consistent styling patterns */
    .card {
        margin-bottom: 30px;
        border-radius: 8px;
        box-shadow: 0 4px 8px rgba(88, 87, 87, 0.1);
    }
    
    .card-header {
        background-color: #7a8086;
        color: white;
        font-weight: bold;
        border-radius: 8px 8px 0 0 !important;
    }
    
    /* Use themed card headers for different sections */
    .section-header { background-color: #5a6268; }
    .graph-header { background-color: #5a6268; }
    .solution-header { background-color: #5a6268; }
</style>

<div class="container-fluid px-4 px-xxl-5" style="max-width: 2400px; margin: 0 auto;">
    <!-- Page Header Section -->
    <div class="page-header mb-4">
        <h1 class="text-center">
            <i class="fa-solid fa-[icon] me-2"></i>[Technology] [Analysis Type] Dashboard
        </h1>
        <p class="text-center mb-0">Generated on @DateTime.Now.ToString("MMMM d, yyyy")</p>
    </div>

    <!-- Example graph section Section -->
    <div class="card mb-4">
        <div class="card-header solution-header">
            <h2 class="mb-0 py-2">
                <i class="fa-solid fa-chart-pie me-2"></i>Summary
            </h2>
        </div>
        <div class="card-body">
              <!-- graph components -->
        </div>
    </div>

    <!-- Example Summary/Metrics Section -->
    <div class="card mb-4">
        <div class="card-header solution-header">
            <h2 class="mb-0 py-2">
                <i class="fa-solid fa-chart-pie me-2"></i>Summary
            </h2>
        </div>
        <div class="card-body">
            <!-- Metric boxes row -->
            <div class="row mb-4">
                @* Use metric-box pattern for key statistics *@
            </div>
            
            <!-- Charts row -->
            <div class="row">
                @* Chart containers *@
            </div>
        </div>
    </div>

    <!-- Example Dynamic Tables Section -->
    <div class="card mb-4">
        <div class="card-header section-header">
            <h2 class="mb-0 py-2">
                <i class="fa-solid fa-table me-2"></i>[Data Section Name]
            </h2>
        </div>
        <div class="card-body">
            @await Html.RenderDynamicTableAsync(
                Model.Dashboard.SomeMetricTableName,
                "Table Description",
                pageSize: 50,
                showExport: true,
                showPagination: true
            )
        </div>
    </div>
</div>

@section Filters {
    <div class="d-flex align-items-center gap-2">
        @await Html.FilterPartialAsync(x => x.SegmentFilter, "basicFilterGroup", 600)
        @await Html.FilterPartialAsync(x => x.ProviderFilter, "basicFilterGroup", 300)
    </div>
}

@section Scripts {
    @* Chart.js or ECharts scripts and initialization *@
}
```

### 5. UI Component Patterns

**Metric Boxes**:
```razor
<div class="col-lg-2 col-md-4 col-sm-6 mb-3">
    <div class="metric-box metric-primary">
        <div class="metric-value">@Model.Dashboard.SomeCount</div>
        <div class="metric-label">Label Text</div>
    </div>
</div>
```

**Available metric-box classes**: `metric-primary`, `metric-success`, `metric-info`, `metric-warning`, `metric-danger`

**Dynamic Tables**:
```razor
@await Html.RenderDynamicTableAsync(
    metricTableName: "metric.name.from.MetricName.Constants",
    title: "Human Readable Title", 
    pageSize: 50,
    showExport: true,
    showPagination: true
)
```

**Collapsible Sections**:
```razor
<div class="project-header" data-bs-toggle="collapse" data-bs-target="#section-id">
    <i class="fa-solid fa-chevron-down collapse-icon me-2"></i>
    Section Title
</div>
<div class="collapse" id="section-id">
    <div class="project-details">
        <!-- Content -->
    </div>
</div>
```

### 6. Data Export Pattern

**In Page Model**:
```csharp
protected override IEnumerable<DataTable> ExportData(string dataExportName)
{
    if(Enum.TryParse(dataExportName, out DataExportName exportType))
    {
        var metrics = GetAllMetrics();
        var allTables = metrics.Where(m => m is IMetricValue<DataTable>)
                              .Select(m => m as IMetricValue<DataTable>);
        
        switch (exportType)
        {
            case DataExportName.SomeExport:
                return allTables.Where(m => m.Name == MetricName.Some.METRIC_NAME)
                               .Select(m => m.Value);
        }
    }
    return base.ExportData(dataExportName);
}
```

**In View**:
```razor
<a asp-page-handler="DataExport" 
   asp-route-dataExportName="@exportName" 
   class="btn btn-success btn-sm">
    <i class="fa-solid fa-download me-2"></i>Download Excel
</a>
```

### 7. Chart Integration Patterns

**Chart.js** for basic charts:
```javascript
new Chart(document.getElementById('chartId'), {
    type: 'doughnut', // or 'bar', 'pie', 'line'
    data: {
        labels: @Json.Serialize(Model.Dashboard.Labels),
        datasets: [{
            data: @Json.Serialize(Model.Dashboard.Data),
            backgroundColor: ['#0d6efd', '#20c997', '#ffc107']
        }]
    },
    options: { responsive: true, maintainAspectRatio: false }
});
```

**ECharts** for complex visualizations (graphs, dependency trees):
```javascript
const chart = echarts.init(document.getElementById('chartContainer'));
const option = {
    series: [{
        type: 'graph',
        data: @Html.Raw(JsonSerializer.Serialize(Model.Dashboard.GraphData)),
        // ... configuration
    }]
};
chart.setOption(option);
```

### 8. Filter Integration

The `@section Filters` is automatically integrated with the layout and provides:
- Segment filtering (organizational grouping)
- Provider filtering (data source filtering)
- Consistent styling and behavior

### 9. Responsive Design Guidelines

- Use Bootstrap 5 grid system (`container-fluid`, `row`, `col-*`)
- Set max-width: 2400px for large screens
- Ensure charts are responsive with `maintainAspectRatio: false`
- Use appropriate column breakpoints for metric boxes: `col-lg-2 col-md-4 col-sm-6`

### 10. Icon Usage Pattern

Use Font Awesome icons consistently:
- Technology icons: `fa-brands fa-[technology]` (e.g., `fa-microsoft`, `fa-java`)
- Generic analysis: `fa-solid fa-code`, `fa-solid fa-chart-pie`
- Specific features: `fa-solid fa-project-diagram`, `fa-solid fa-sitemap`, `fa-solid fa-table`

### 11. Color Scheme

Primary colors for consistency:
- Cards: `#7a8086` (header), `#5a6268` (sections)
- Metrics: Use predefined classes (`metric-primary`, `metric-success`, etc.)
- Charts: Bootstrap color palette (`#0d6efd`, `#20c997`, `#ffc107`, `#dc3545`)

## Implementation Checklist

When creating a new report:

1. ✅ Create the three core files (`Index.cshtml`, `Index.cshtml.cs`, `IndexViewModel.cs`)
2. ✅ Inherit from `MetricFilterPageModel`
3. ✅ Override `GetMyMetrics()` to specify data sources current report will use
4. ✅ Override `LoadData()` to process metrics into view model. This will populate the `Dashboard` property with all necessary data for rendering
5. ✅ Structure view model with appropriate data organization
6. ✅ Use consistent HTML structure and styling
7. ✅ Implement metric boxes for key statistics
8. ✅ Add dynamic tables using `Html.RenderDynamicTableAsync()`
9. ✅ Include filter section using `@section Filters`
10. ✅ Add charts with appropriate libraries in `@section Scripts`
11. ✅ Implement data export functionality if needed
12. ✅ Test responsive design across screen sizes

This pattern ensures consistency across all reports while maintaining flexibility for specific analysis needs.