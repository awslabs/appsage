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
        private static List<IMetric> _allMetricCache = null;
        private static List<IMetric> _filterMetricCache = null;
       
        public MetricReader(IAppSageLogger logger, IAppSageConfiguration config, IAppSageWorkspace workspace)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        }

        public IEnumerable<IMetric> GetMetricSet()
        {
            bool takeDataFromLastRun = _config.Get<bool>("AppSage.Infrastructure.Metric.MetricReader:TakeProviderOutputDataFromLatestRun");
            int documentMaxParallelism=_config.Get<int>("AppSage.Infrastructure.Metric.MetricReader:DocumentMaxParallelism");
            bool cacheMetricDataInMemory = _config.Get<bool>("AppSage.Infrastructure.Metric.MetricReader:CacheMetricDataInMemory");
            DirectoryInfo dataDir = null;

            long before = GC.GetTotalMemory(forceFullCollection: false)/(1024*1024);

            if (cacheMetricDataInMemory && _allMetricCache != null)
            {
                _logger.LogInformation("Returning cached metric data from memory. Total metrics count: {MetricCount}", _allMetricCache.Count);
                return _allMetricCache;
            }


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
            foreach (var file in fileSet.AsParallel().WithDegreeOfParallelism(documentMaxParallelism))
            {
                if (file.Exists)
                {
                    string json = System.IO.File.ReadAllText(file.FullName);

                    var metrics = AppSageSerializer.DeserializeFromFile<IEnumerable<IMetric>>(file.FullName);
                    lock (result)
                    {
                        result.AddRange(metrics);
                    }
                }
            }

            if(cacheMetricDataInMemory)
            {
                _allMetricCache = result;
                _logger.LogInformation("Cached metric data in memory. Total metrics count: {MetricCount}", _allMetricCache.Count);
                //Get the size of the object _allMetricCache in MB using the fastest way possible

            }

            long after = GC.GetTotalMemory(forceFullCollection: false)/(1024 * 1024) ;
            long approxSize = after - before;

            Console.WriteLine($"Before:  {before:N0} MB");
            Console.WriteLine($"After:   {after:N0} MB");
            Console.WriteLine($"Delta:   {approxSize:N0} MB (approx)");

            return result;
        }
    }
}
