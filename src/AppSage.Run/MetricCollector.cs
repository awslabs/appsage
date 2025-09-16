using AppSage.Core.Configuration;
using AppSage.Core.Const;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace AppSage.Run
{
    internal class MetricCollector : IMetricCollector,IDisposable
    {
        private (bool AddProviderNameToOutputFile, int OutputMetricsPerFile, string OutputFilePrefix, string OutputFolder) _config;
        private readonly IAppSageLogger _logger;
        private string _providerName = string.Empty;
        private string _providerVersion = string.Empty;
        private int _totalCollectedMetricCount = 0;
        private int _outputFileIndex = 0;

        ConcurrentQueue<IMetric> _metrics = new ConcurrentQueue<IMetric>();
        bool _isCompleted = false;
        private bool _isDisposed = false;

        public int TotalCollectedMetricCount => _totalCollectedMetricCount;

        public MetricCollector(string providerName,string providerVersion,IAppSageLogger logger, IAppSageConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _providerName = providerName ?? throw new ArgumentNullException(nameof(providerName));
            _providerVersion = providerVersion;
            _config.AddProviderNameToOutputFile = config.Get<bool>("AppSage.Run.Runner:AddProviderNameToOutputFile");
            _config.OutputMetricsPerFile = config.Get<int>("AppSage.Run.Runner:MaxOutputMetricsPerFile");
            _config.OutputFilePrefix = config.Get<string>("AppSage.Run.Runner:OutputFilePrefix");
            _config.OutputFolder= config.Get<string>("AppSage.Run.Runner:OutputFolder");
            AddRoolRunInfo();
        }



        public void Dispose()
        {
            if (_isDisposed)
            {
                return; // Already disposed
            }
            SaveMetrics();
            _isCompleted = true;
            _metrics = null;
            _isDisposed = true;
        }

        void IMetricCollector.Add(IMetric metric)
        {
            if (!_isCompleted && !_isDisposed)
            {
                lock (_metrics)
                {
                    _totalCollectedMetricCount++;
                    _metrics.Enqueue(metric);
                    if(IsTimeToFlush())
                    {
                        SaveMetrics();
                    }
                }  
            }
            else
            {
                throw new InvalidOperationException("Cannot add metrics after CompleteAdding is invoked or the collector is disposed");
            }
        }

        void IMetricCollector.CompleteAdding()
        {
            _isCompleted = true;
            SaveMetrics();
        }

        void AddRoolRunInfo() {
            //Get the version of the current assembly
            var toolFingerprint = new Dictionary<string, string>();
            toolFingerprint["Name"] = _providerName;
            toolFingerprint["Version"] = _providerVersion;
            toolFingerprint["RunDateTime"]= DateTime.UtcNow.ToString();
            toolFingerprint["UserName"] = Environment.UserName;
            toolFingerprint["MachineName"] = Environment.MachineName;

            var fingerPrintMetric=new ResourceMetricValue<Dictionary<string, string>>(MetricName.AppSage.TOOL_RUN_INFO,_providerName,_providerName, toolFingerprint);
            ((IMetricCollector) this).Add(fingerPrintMetric);

        }

        private bool IsTimeToFlush() {

            IEnumerable<IMetric> metrics = _metrics.ToList();
            bool hasLargeObject = metrics.Where(m => m.IsLargeMetric).Any();
            int smallMetricsCount = metrics.Where(m => !m.IsLargeMetric).Count();

            if(hasLargeObject || smallMetricsCount >= _config.OutputMetricsPerFile)
            {
                return true;
            }
            return false;
        }

        private void SaveMetrics()
        {
            try
            {
                lock (_metrics)
                {

                    IEnumerable<IMetric> metrics = _metrics.ToList();

                    var largeMetrics = metrics.Where(m => m.IsLargeMetric).Select(m => new[] { m });

                    //select small metrics and group them into chunks
                    var smallMetrics = metrics.Where(m => !m.IsLargeMetric)
                        .Select((metric, index) => new { metric, index })
                        .GroupBy(x => x.index / _config.OutputMetricsPerFile)
                        .Select(g => g.Select(x => x.metric));



                    // Serialize the metrics to JSON
                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        TypeNameHandling = TypeNameHandling.Objects,
                        NullValueHandling = NullValueHandling.Ignore
                    };

                    // Ensure the output directory exists
                    Directory.CreateDirectory(_config.OutputFolder);



                    var metricChunks = Enumerable.Concat(largeMetrics, smallMetrics);

                    foreach (var mc in metricChunks)
                    {
                        if (mc is IValidateMetric validateMetric)
                        {
                            var errors = validateMetric.Validate();
                            if (errors.Count > 0)
                            {
                                foreach (var error in errors)
                                {
                                    _logger.LogError($"Validation error: {error}");
                                }
                            }
                        }

                        string fileName = $"{_config.OutputFilePrefix}_{_outputFileIndex + 1}.json";

                        if (_config.AddProviderNameToOutputFile)
                        {
                            fileName = $"{_config.OutputFilePrefix}_{_providerName}_{_outputFileIndex + 1}.json";
                        }

                        string filePath = Path.Combine(_config.OutputFolder, fileName);

                        using (var writer = new StreamWriter(filePath))
                        {
                            using (var jsonWriter = new JsonTextWriter(writer))
                            {
                                var serializer = JsonSerializer.Create(settings);
                                serializer.Serialize(jsonWriter, mc.ToArray());
                            }
                        }
                        _outputFileIndex++;
                    }

                    // Clear the metrics after saving
                    _metrics.Clear();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving metrics: {ex.Message}",ex);
            }
        }

    }
}
