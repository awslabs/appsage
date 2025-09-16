using AppSage.Core.Configuration;
using AppSage.Core.Localization;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using AppSage.Infrastructure.Caching;
using AppSage.Infrastructure.Workspace;
using AppSage.Run.CommandSet;
using AppSage.Run.CommandSet.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.CommandLine;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Reflection;
namespace AppSage.Run
{


    internal class Program
    {
        static CountingSink _countingSink;
        static int Main(string[] args)
        {
            // Initialize the counting sink BEFORE configuring services
            _countingSink = new CountingSink();
            IServiceCollection services = new ServiceCollection();
            services = InitializeCoreServices(services);
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<IAppSageLogger>();
            var config= serviceProvider.GetRequiredService<IAppSageConfiguration>();

            try
            {
                //args=new string[] {"init"};
                //args = new string[] { "init", "-ws", @"C:\Temp\bingo" };
                RootCommand rootCommand = new RootCommand
                    {
                        Description = "AppSage Run Command Line Interface"
                    };

                    ISubCommand[] subCommands = new ISubCommand[]
                    {
                        new AppSage.Run.CommandSet.Init.InitCommand(config,logger),
                    };
                    foreach (var subCommand in subCommands)
                    {
                        rootCommand.Add(subCommand.Build());
                    }

                    var parseResult = rootCommand.Parse(args);
                    return parseResult.Invoke();
            }
            catch (Exception ex)
            {
                logger.LogError("AppSage application terminated unexpectedly", ex);
                return -1;
            }
            finally
            {
                // Dispose of the service provider when the application exits
                if (serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                // Close and flush the log
                Log.CloseAndFlush();
            }

            _countingSink.SummarizeToConsoel();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return 0;
        }



        /// <summary>
        /// Configure core services like logging, configuration, caching, localization & workspace mangement
        /// </summary>
        /// <returns>Service collection</returns>
        /// 


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
        

            //Initialize the localization
            var cultureName = Environment.GetEnvironmentVariable("APPSAGE_CULTURE") ?? "en-US";
            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureName);
            // Initialize all LocalizationManager derived classes automatically
            LocalizationManager.InitializeAll();


            return services;
        }
        private static void ConfigureSerilog(IAppSageConfiguration config)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Sink(_countingSink)
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithMachineName();


            string logFolder = config.Get<string>("AppSage.Core:LogFolder");
            if (!string.IsNullOrWhiteSpace(logFolder) && Directory.Exists(logFolder))
            {
                logger.WriteTo.File(Path.Combine(logFolder, "appSage-.log"), rollingInterval: RollingInterval.Day);
            }
            else
            {
                Console.WriteLine($"Warning: Log folder '{logFolder}' does not exist or is invalid. Logs will be written to the console only.");
            }

            // Ensure logs directory exists
            Directory.CreateDirectory(logFolder);

            // Create Serilog logger
            Log.Logger = logger.CreateLogger();
        }







    }
}
