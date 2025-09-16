using AppSage.Core.Configuration;
using AppSage.Core.Localization;
using AppSage.Core.Logging;
using AppSage.Core.Workspace;
using AppSage.Infrastructure.Workspace;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Globalization;
using System.Reflection;

namespace AppSage.MCPServer
{
    internal class Program
    {
        private static IAppSageLogger _logger;
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            InitializeCoreServices(builder.Services);
            
            Runner runner = new Runner(builder.Services);
            await runner.Run(builder);
        }

      

        private static IServiceCollection InitializeCoreServices(IServiceCollection services)
        {
            // Get the current assembly directory
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string configDirectory = Path.Combine(directory, "Configuration");

#if DEBUG
            string configFileName = "appsettings.Development.json";
#elif RELEASE
            string configFileName = "appsettings.Production.json";
#else
            throw new InvalidOperationException("Unknown build configuration. Please define DEBUG or RELEASE. Make sure you have the correct configuration file.");
#endif

            var config = new ConfigurationBuilder()
                .SetBasePath(configDirectory)
                .AddJsonFile(configFileName, optional: true, reloadOnChange: true)
                .Build();

            // Add the configuration to the service collection
            services.AddSingleton<IConfiguration>(sp => config);
            services.AddSingleton<IAppSageConfiguration, AppSageConfiguration>();

            IServiceProvider preSetupProvider = services.BuildServiceProvider();
            var appSageConfig = preSetupProvider.GetService<IAppSageConfiguration>();
            ConfigureSerilog(appSageConfig);
            // Add Serilog
            services.AddSingleton(Log.Logger);
            // Register the logger as a singleton (one instance for the entire application)
            services.AddSingleton<IAppSageLogger, AppSageLogger>();

            preSetupProvider = services.BuildServiceProvider();

            // Add services required for localization
            services.AddLocalization();

            //Initialize the localization
            var cultureName = Environment.GetEnvironmentVariable("APPSAGE_CULTURE") ?? "en-US";
            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureName);
            // Initialize all LocalizationManager derived classes automatically
            LocalizationManager.InitializeAll();

            string workspaceRoot = appSageConfig.Get<string>("AppSage.Core:WorkspaceRoot");
            services.AddSingleton<IAppSageWorkspace>(sp =>
            {
                var logger = sp.GetRequiredService<IAppSageLogger>();
                return new AppSageWorkspaceManager(workspaceRoot, logger);
            });
            return services;
        }
        private static void ConfigureSerilog(IAppSageConfiguration config)
        {
            string logFolder = config.Get<string>("AppSage.Core:LogFolder");
            // Ensure logs directory exists
            Directory.CreateDirectory(logFolder);

            // Create Serilog logger
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(logFolder, "appSage-.log"), rollingInterval: RollingInterval.Day)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithMachineName()
                .CreateLogger();
        }



    }
}
