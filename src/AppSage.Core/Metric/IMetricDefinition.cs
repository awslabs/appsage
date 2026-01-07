using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.Core.Metric
{
    public interface IMetricDefinition
    {
        string Provider { get; }
        string MetricName { get; }

        string ShortDescription { get; }

        string DetailedDescription { get; }
    }
}
