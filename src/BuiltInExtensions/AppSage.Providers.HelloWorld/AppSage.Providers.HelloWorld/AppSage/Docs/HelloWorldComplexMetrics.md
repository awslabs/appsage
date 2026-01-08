# HelloWorld Complex Metrics Documentation

## AppSage.HelloWorld.CountryGDP

### Overview
The `AppSage.HelloWorld.CountryGDP` metric provides a sample dataset containing Gross Domestic Product (GDP) information for various countries. This metric demonstrates how to work with structured tabular data in the AppSage framework using a `DataTable` format.

### Metric Details
- **Metric Name**: `AppSage.HelloWorld.CountryGDP`
- **Data Type**: `DataTable`
- **Provider**: `AppSage.Providers.HelloWorld.AppSageHelloWorldProvider`
- **Purpose**: Educational/demonstration purposes for handling complex tabular metrics

### Table Schema

The `CountryGDP` metric contains the following columns:

#### Year (int)
- **Data Type**: Integer
- **Purpose**: Represents the calendar year for which the GDP data was recorded
- **Example Values**: 2023
- **Description**: This column allows for time-series analysis and tracking GDP changes over different years. It enables filtering and grouping of data by specific time periods.

#### Country (string)
- **Data Type**: String
- **Purpose**: Contains the name of the country or economic entity
- **Example Values**: "USA", "Germany"
- **Description**: The country identifier that allows for geographic analysis and comparison between different nations. This field enables filtering by specific countries and regional analysis.

#### GDPInBillion (int)
- **Data Type**: Integer
- **Purpose**: Represents the Gross Domestic Product value expressed in billions of currency units
- **Example Values**: 29000 (representing $29 trillion), 4000 (representing $4 trillion)
- **Description**: The core economic indicator measuring the total monetary value of all goods and services produced within a country during the specified year. Values are simplified and expressed in billions for easier readability.

### Sample Data
The current implementation includes the following sample records:

| Year | Country | GDPInBillion |
|------|---------|--------------|
| 2023 | USA     | 29000        |
| 2023 | Germany | 4000         |

### Usage Examples

#### Accessing the Metric
```csharp
// When processing metrics in a consumer
if (metric is IMetricValue<DataTable> tableMetric && 
    metric.Name == "AppSage.HelloWorld.CountryGDP")
{
    DataTable gdpData = tableMetric.Value;
    
    // Access individual rows
    foreach (DataRow row in gdpData.Rows)
    {
  int year = (int)row["Year"];
        string country = (string)row["Country"];
        int gdpInBillion = (int)row["GDPInBillion"];
        
        Console.WriteLine($"{country} had a GDP of ${gdpInBillion} billion in {year}");
  }
}
```

#### Filtering Data
```csharp
// Filter by specific country
var usaRows = gdpData.Select("Country = 'USA'");

// Filter by GDP threshold
var largeEconomies = gdpData.Select("GDPInBillion > 10000");

// Filter by year range (if multiple years present)
var recentData = gdpData.Select("Year >= 2020");
```

### Extensibility
This metric can be extended to include additional columns such as:
- **Population**: Country population for per-capita calculations
- **Currency**: The currency unit for the GDP measurement
- **GDPGrowthRate**: Year-over-year growth percentage
- **Region**: Geographic region classification
- **Source**: Data source attribution

### Integration with AppSage Framework
This metric demonstrates the AppSage framework's capability to handle complex, structured data beyond simple scalar values. It showcases:
- Tabular data handling with strongly-typed columns
- Multi-dimensional data organization
- Support for analytical queries and data manipulation
- Framework flexibility for various data types and structures

### Notes
- GDP values are simplified for demonstration purposes and may not reflect actual economic data
- This metric serves as a template for implementing more complex, real-world economic or business metrics
- The data structure follows standard database normalization principles suitable for analytical processing