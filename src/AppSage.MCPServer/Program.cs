using AppSage.Core.Configuration;
using AppSage.Core.Localization;
using AppSage.Core.Logging;
using AppSage.Core.Workspace;
using AppSage.Infrastructure.Workspace;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.Globalization;

namespace AppSage.MCPServer
{
    internal class Program
    {
        private static IAppSageLogger _logger;
        public static async Task Main(string[] args)
        {
            IServiceCollection bootstrapCollection = new ServiceCollection();
            bootstrapCollection = InitializeCoreServices(bootstrapCollection, null);
            var bootstrapService = bootstrapCollection.BuildServiceProvider();
            IAppSageWorkspace workspace = bootstrapService.GetRequiredService<IAppSageWorkspace>();
            IAppSageLogger logger = bootstrapService.GetRequiredService<IAppSageLogger>();
            bootstrapService.Dispose();

            logger.LogInformation($"AppSage workspace folder is [{workspace.RootFolder}]");

            var builder = WebApplication.CreateBuilder(args);
            var serviceCollection= InitializeCoreServices(builder.Services, workspace);
            
            Runner runner = new Runner(serviceCollection);
            await runner.Run(builder);
        }



        private static IServiceCollection InitializeCoreServices(IServiceCollection services, IAppSageWorkspace workspace)
        {
            var logger = new LoggerConfiguration()
           .MinimumLevel.Debug()
           .Enrich.FromLogContext()
           .Enrich.WithThreadId()
           .Enrich.WithMachineName();

            //We will always log to the console with the look and feel of a Console.WriteLine for Information level messages. 
            logger.WriteTo.Logger(lc => lc
                        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
                        .WriteTo.Console(formatter: new MessageTemplateTextFormatter("{Message:lj}{NewLine}", null)
            ));
            // Console sink for all *non-Information* levels (default format)
            logger.WriteTo.Logger(lc => lc
                .Filter.ByExcluding(e => e.Level == LogEventLevel.Information)
                .WriteTo.Console() // default Serilog console theme/formatter
            );

            if (workspace != null && !String.IsNullOrEmpty(workspace.LogsFolder) && Directory.Exists(workspace.LogsFolder))
            {
                logger.WriteTo.File(Path.Combine(workspace.LogsFolder, "appSage-.log"), rollingInterval: RollingInterval.Day);
            }
            // Create Serilog logger
            Log.Logger = logger.CreateLogger();


            // Add Serilog
            services.AddSingleton(Log.Logger);
            // Register the logger as a singleton (one instance for the entire application)
            services.AddSingleton<IAppSageLogger, AppSageLogger>();


            IAppSageConfiguration appSageConfiguration = new AppSageConfiguration(AppSageConfiguration.GetDefaultConfigTemplateFilePath());
            services.AddSingleton<IAppSageConfiguration>(appSageConfiguration);

            services.AddSingleton<IAppSageWorkspace>(sp =>
            {
                IAppSageConfiguration config = sp.GetRequiredService<IAppSageConfiguration>();
                string workspaceFolder = config.Get<string>("AppSage.MCPServer.Program:WorkspaceRootFolder");

                return new AppSageWorkspaceManager(new DirectoryInfo(workspaceFolder), sp.GetRequiredService<IAppSageLogger>());
            });

            //Initialize the localization
            var cultureName = Environment.GetEnvironmentVariable("APPSAGE_CULTURE") ?? "en-US";
            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureName);
            // Initialize all LocalizationManager derived classes automatically
            LocalizationManager.InitializeAll();

            return services;
        }




    }
}
