using AppSage.Core.Metric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.Providers.Samples
{
    internal class SampleInfoProvider : IMetricProvider
    {
        public string FullQualifiedName => "MyCompany.MyStatProviderExtension";
        public string Description => "Returns some statistics";
        public void Run(IMetricCollector collectorQueue)
        {
            var countryInfo = new Dictionary<string, string>();
            countryInfo.Add("India", "1.44B");
            countryInfo.Add("China", "1.41B");
            countryInfo.Add("USA", "345M");
            var metric1 = new MetricValue<Dictionary<string, string>>("MyMetrics.LargeCountriesByPopulation", countryInfo);
            var metric2 = new MetricValue<DateTime>("MyMetrics.MyBirthDay", new DateTime(1984,2,15));
            var metric3 = new ResourceMetricValue<int>("MyMetric.RegionPopulationInMillion","ASEAN", 671);
            var metric4 = new ResourceMetricValue<double>("MyMetric.CPUUtilization", "AWS/VM/DBServer", 23.5);
            
            
            collectorQueue.Add(metric1);
            collectorQueue.Add(metric2);
            collectorQueue.Add(metric3);
            collectorQueue.Add(metric4);

        }
    }
}
