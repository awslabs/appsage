# Chart Visualization System

## Overview

The AppSage Chart Viewer provides interactive chart visualization capabilities for DataTable array data using Apache ECharts. It supports multiple chart types and allows customization of appearance and data presentation.

## Architecture

### Modular Component Design
- **ChartRenderer**: Core ECharts integration, data processing, and chart type management
- **ChartControls**: User interface controls, configuration management, and user interactions
- **Main Orchestrator**: Component coordination and VS Code extension integration

### Data Flow
1. **Extension** → Loads and validates `.appsagechart` files
2. **ChartViewer** → Parses DataTable array format and sends to webview
3. **ChartRenderer** → Converts data to ECharts format based on selected chart type
4. **ChartControls** → Manages user interactions and configuration updates

## File Format

Chart files use the `.appsagechart` extension and contain JSON-serialized DataTable arrays:

```json
[
  {
    "TableName": "SampleData",
    "Columns": [
      {
        "Name": "Metric",
        "DataType": "System.String"
      },
      {
        "Name": "Value", 
        "DataType": "System.Int64"
      }
    ],
    "Rows": [
      {
        "Metric": "Performance",
        "Value": 95
      }
    ]
  }
]
```

## Supported Chart Types

### Bar Chart (`bar`)
- **Best for**: Comparing categorical data, showing rankings
- **Data requirements**: At least one numeric column
- **Features**: Supports multiple series, customizable colors

### Line Chart (`line`)
- **Best for**: Time series data, trend analysis
- **Data requirements**: At least one numeric column
- **Features**: Smooth curves, multiple lines, area fill option

### Pie Chart (`pie`)
- **Best for**: Part-to-whole relationships, distribution analysis
- **Data requirements**: One numeric column for values, one categorical for labels
- **Features**: Interactive legend, percentage display

### Scatter Plot (`scatter`)
- **Best for**: Correlation analysis, pattern detection
- **Data requirements**: Two numeric columns (X and Y coordinates)
- **Features**: Customizable point sizes, hover tooltips

### Area Chart (`area`)
- **Best for**: Cumulative data, filled trend visualization
- **Data requirements**: Same as line chart
- **Features**: Filled areas under curves, stacked options

### Heatmap (`heatmap`)
- **Best for**: Matrix data, intensity visualization
- **Data requirements**: Three numeric columns (X, Y, intensity)
- **Features**: Color gradients, interactive tooltips

### Radar Chart (`radar`)
- **Best for**: Multi-dimensional comparisons
- **Data requirements**: Multiple numeric columns
- **Features**: Polygon shapes, multi-series comparison

### Gauge (`gauge`)
- **Best for**: Single metric visualization, progress indicators
- **Data requirements**: One numeric value
- **Features**: Customizable ranges, color thresholds

## User Interface

### Control Panel
- **Chart Type Selector**: Dropdown to choose visualization type
- **Single Table Dropdown**: Choose which table to display
- **Refresh Button**: Reload chart with current settings
- **Export Button**: Save chart (future feature)

### Customization Options (Collapsible)
- **Title**: Custom chart title
- **Legend**: Show/hide and position control
- **Grid**: Show/hide grid lines
- **Animation**: Enable/disable animations
- **Color Theme**: Predefined color schemes

### Sidebar Panel
- **Table Information**: Selected tables with row/column counts
- **Chart Statistics**: Current configuration summary

## Data Conversion Logic

### Automatic Column Detection
- **Numeric Columns**: Detects `Int64`, `Decimal`, `Double`, `Float`, `Long` types
- **Category Columns**: Uses `String` types or non-numeric columns
- **Fallback Handling**: Graceful degradation for missing or invalid data

### Chart-Specific Processing
1. **Bar/Line Charts**: Uses first categorical column for X-axis, all numeric columns as series
2. **Pie Charts**: Uses first categorical and first numeric column
3. **Scatter Plots**: Uses first two numeric columns as X/Y coordinates
4. **Multi-table Support**: Combines data from selected tables when applicable

## Customization Features

### Color Themes
- **Default**: ECharts standard palette
- **Blue**: Blue-focused color scheme
- **Green**: Nature-inspired colors
- **Purple**: Purple-dominant palette
- **Orange**: Warm color scheme

### Legend Positioning
- Top, Bottom, Left, Right options
- Automatic orientation (horizontal/vertical)
- Show/hide toggle

### Interactive Features
- Hover tooltips with detailed information
- Responsive design for different screen sizes
- Zoom and pan capabilities (chart-dependent)

## Technical Implementation

### TypeScript Interfaces
- `AppSageChartTable`: DataTable structure definition
- `ChartConfiguration`: User settings and preferences
- `EChartsOption`: ECharts configuration object

### Error Handling
- Data validation with detailed error messages
- Graceful fallbacks for invalid configurations
- User-friendly error display

### Performance Considerations
- Large dataset handling (automatic limiting)
- Efficient data conversion algorithms
- Memory-conscious chart updates

## Usage Examples

### Basic Bar Chart
1. Open `.appsagechart` file
2. Select "Bar Chart" from type dropdown
3. Choose desired tables from multi-select
4. Customize title and colors as needed

### Time Series Analysis
1. Use "Line Chart" type for temporal data
2. Ensure data has chronological order
3. Enable smooth curves for better visualization
4. Add area fill for cumulative effects

### Comparative Analysis
1. Use "Radar Chart" for multi-dimensional comparison
2. Select tables with similar metric structures
3. Adjust legend positioning for clarity

## Future Enhancements

- Chart export functionality (PNG, SVG, PDF)
- Advanced filtering and data transformation
- Custom color palette creation
- Chart combination capabilities
- Real-time data updates
- Drill-down interactions

## Integration Points

### VS Code Extension
- Custom editor registration for `.appsagechart` files
- Webview panel management
- Message passing between extension and webview

### ECharts Library
- Version 5.4.3 integration
- Local library hosting for security
- Option object generation and management

### AppSage Ecosystem
- Compatible with DataTable serialization format
- Consistent with other AppSage viewers (Graph, Table)
- Shared logging and error handling patterns
