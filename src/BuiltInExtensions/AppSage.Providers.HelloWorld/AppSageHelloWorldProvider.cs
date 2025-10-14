using AppSage.Core.Metric;
using System.Data;

namespace AppSage.Providers.HelloWorld
{
    public class AppSageHelloWorldProvider : IMetricProvider
    {
        public string FullQualifiedName => GetType().FullName;

        public string Description => "Output hello world as a metric";

        public void Run(IMetricCollector collectorQueue)
        {
            var metric1= new MetricValue<string>(
                name: "AppSage.HelloWorld.Message",
                value: "Hello, World!"
            );

            var metric2= new ResourceMetricValue<int>(
                name: "AppSage.HelloWorld.WorldGDP",
                resource: "2015/worldbank",
                value: 75720
            );

            DataTable gdpTable = new DataTable();
            gdpTable.Columns.Add("Year", typeof(int));
            gdpTable.Columns.Add("Country", typeof(string));
            gdpTable.Columns.Add("GDPInBillion", typeof(int));
            
            gdpTable.Rows.Add(2023,"USA", 29000);
            gdpTable.Rows.Add(2023, "Germany", 4000);

            var metric3= new MetricValue<DataTable>(
                name: "AppSage.HelloWorld.CountryGDP",
                value: gdpTable
            );

            collectorQueue.Add(metric1);
            collectorQueue.Add(metric2);
            collectorQueue.Add(metric3);
        }
    }
}
