using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using AppSage.Infrastructure.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.Infrastructure.Metric
{
    public class MetricReader : IMetricReader
    {
        IAppSageLogger _logger;
        IAppSageConfiguration _config;
        IAppSageWorkspace _workspace;

        public MetricReader(IAppSageLogger logger, IAppSageConfiguration config, IAppSageWorkspace workspace)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        }

        public IEnumerable<IMetric> GetMetricSet()
        {
            bool takeDataFromLastRun = _config.Get<bool>("AppSage.Infrastructure.Metric.MetricReader:TakeProviderOutputDataFromLatestRun");
            DirectoryInfo dataDir = null;

            if (takeDataFromLastRun)
            {
                DirectoryInfo outputFolder = new DirectoryInfo(_workspace.ProviderOutputFolder);
                dataDir = outputFolder.GetDirectories("*", SearchOption.TopDirectoryOnly).OrderByDescending(d => d.Name).FirstOrDefault();
            }
            else
            {
                dataDir = new DirectoryInfo(_workspace.ProviderOutputFolder);
            }

            if (!dataDir.Exists)
            {
                throw new DirectoryNotFoundException($"The directory {dataDir.FullName} does not exist.");
            }
            _logger.LogInformation("---   Loading metrics from [{DirectoryName}]   ---", dataDir.FullName);

            var fileSet = dataDir.GetFiles("*.json", System.IO.SearchOption.AllDirectories);

            List<IMetric> result = new List<IMetric>();
            foreach (var file in fileSet)
            {
                if (file.Exists)
                {
                    string json = System.IO.File.ReadAllText(file.FullName);

                    var metrics= AppSageSerializer.DeserializeFromFile<IEnumerable<IMetric>>(file.FullName);
                    result.AddRange(metrics);
                }
            }
            return result;
        }
    }
}
