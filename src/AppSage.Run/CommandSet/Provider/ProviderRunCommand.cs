using AppSage.Core.ComplexType;
using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using AppSage.Infrastructure.Caching;
using AppSage.Infrastructure.Workspace;
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
    public record ProviderRunOptions
    {

    }
    public sealed class ProviderRunCommand : ISubCommand<ProviderOptions>
    {
        private Runner _runner;
        public ProviderRunCommand(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            ProviderRegistry.RegisterProviders(services);
            //tentatively registering all metric providers they will be added later based on configuration
            services.AddTransient<IMetricProvider[]>(sp => sp.GetServices<IMetricProvider>().ToArray());
            // Register the main runner service that will execute all providers
            services.AddTransient<Runner>();

            serviceProvider = services.BuildServiceProvider();

            // Get the runner service and execute it to run the registered providers
            _runner = serviceProvider.GetRequiredService<Runner>();
        }

        public string Name => "run";
        public string Description => "Run the set of AppSage providers";

        public Command Build()
        {
            var cmd = new Command(this.Name, this.Description);
            var argWorkspaceFolder = AppSageRootCommand.GetWorkspaceArgument();
            cmd.Add(argWorkspaceFolder);
            cmd.SetAction(pr =>
            {
                ProviderOptions options = new ProviderOptions();
                return this.Execute(options);
            });
            return cmd;
        }
        public int Execute(ProviderOptions opt)
        {
            _runner.Run();


            return 0;
        }


    }
}
