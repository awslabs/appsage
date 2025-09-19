using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Run.CommandSet.Root;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.Run.CommandSet.Provider
{

    public sealed class ProviderListCommand : ISubCommandWithNoOptions
    {
        private IMetricProvider[] _providers;
        private readonly IAppSageLogger _logger;
        public ProviderListCommand(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            ProviderRegistry.RegisterProviders(services);
            //tentatively registering all metric providers they will be added later based on configuration
            services.AddTransient<IMetricProvider[]>(sp => sp.GetServices<IMetricProvider>().ToArray());
            // Register the main runner service that will execute all providers
            serviceProvider = services.BuildServiceProvider();
            _providers= serviceProvider.GetRequiredService<IMetricProvider[]>();
            _logger = serviceProvider.GetRequiredService<IAppSageLogger>();
        }

        public string Name => "list";
        public string Description => "List the availble providers";

        public Command Build()
        {
            var cmd = new Command(this.Name, this.Description);
            var argWorkspaceFolder = AppSageRootCommand.GetWorkspaceArgument();
            cmd.Add(argWorkspaceFolder);
            cmd.SetAction(pr =>
            {
                return this.Execute();
            });
            return cmd;
        }
        public int Execute()
        {
            _logger.LogInformation($"Available Providers:[Name]:[Description]");
            _providers.ToList().ForEach(p =>
            {
                _logger.LogInformation($"{p.FullQualifiedName}:{p.Description}");
            });

            return 0;
        }


    }
}
