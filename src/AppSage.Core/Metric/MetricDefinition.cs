using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.Core.Metric
{
    public class MetricDefinition : IMetricDefinition
    {
        public string Provider { get; set; }
        public string MetricName { get; set; }

        public string ShortDescription { get; set; }

        public string DetailedDescription { get; set; }
    }
}
