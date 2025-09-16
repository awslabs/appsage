using AppSage.Core.ComplexType;
using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using AppSage.Infrastructure.Caching;
using AppSage.Infrastructure.Workspace;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.Run.CommandSet.Providers
{
    internal class Start
    {

        public static void Execute(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            //this now has all the services including providers
            var appSageConfig = serviceProvider.GetRequiredService<IAppSageConfiguration>();
            string workspaceRoot = appSageConfig.Get<string>("AppSage.Core:WorkspaceRoot");
            
            services.AddSingleton<IAppSageWorkspace>(sp =>
            {
                var logger = sp.GetRequiredService<IAppSageLogger>();
                return new AppSageWorkspaceManager(workspaceRoot, logger);
            });

            ProviderRegistry.RegisterProviders(services);
            services.AddSingleton<IAppSageCache, FileSystemCache>();

            //tentatively registering all metric providers they will be added later based on configuration
            services.AddTransient<IMetricProvider[]>(sp => sp.GetServices<IMetricProvider>().ToArray());

            // Register the main runner service that will execute all providers
            services.AddTransient<Runner>();

            //_logger.LogInformation("Registering providers");
            serviceProvider = services.BuildServiceProvider();

            //_logger.LogInformation("Building final service collection");
           
            // Get the runner service and execute it to run the registered providers
            var runner = serviceProvider.GetRequiredService<Runner>();
            runner.Run();
        }
    }
}
