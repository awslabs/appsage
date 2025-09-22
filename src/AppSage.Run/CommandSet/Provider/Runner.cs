using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using AppSage.Infrastructure.Metric;

namespace AppSage.Run.CommandSet.Provider
{
    public class Runner
    {
        private readonly IAppSageConfiguration _configure;
        private readonly IAppSageWorkspace _workspace;
        private readonly IAppSageLogger _logger;
        private readonly IMetricProvider[] _providers;
        public Runner(IAppSageLogger logger, IMetricProvider[] providers,IAppSageWorkspace workspace,  IAppSageConfiguration config)
        {
            _logger = logger;
            _providers = providers;
            _workspace = workspace;
            _configure = config;
        }
        public void Run()
        {

            foreach (var provider in _providers)
            {
                string providerType = provider.GetType().FullName;
                string providerVersion = provider.GetType().Assembly.GetName().Version.ToString();

                _logger.LogInformation($"Running the provider: {providerType}, version:{providerVersion}");

                try
                {
                    using (MetricCollector collector = new MetricCollector(provider.FullQualifiedName,providerVersion, _logger, _workspace, _configure))
                    {
                        provider.Run(collector);
                        _logger.LogInformation($"Finished running the provider: {provider.GetType().FullName}. Collected Metrics {collector.TotalCollectedMetricCount}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error running provider {provider.FullQualifiedName}: {ex.Message}", ex);
                }
            }

            //Save the fingerprint of the run
            //Get the version of the runner
            string appSageType = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            string appSageRunnerVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            using (MetricCollector collector = new MetricCollector(appSageType,appSageRunnerVersion, _logger, _workspace, _configure))
            {
                _logger.LogInformation($"AppSage Fingerprint");
            }

        }

    }
}
