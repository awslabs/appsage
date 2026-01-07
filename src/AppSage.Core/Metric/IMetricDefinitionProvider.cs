using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.Core.Metric
{
    public interface IMetricDefinitionProvider
    {
        IEnumerable<IMetricDefinition> GetMetricDefinitions();
    }
}
