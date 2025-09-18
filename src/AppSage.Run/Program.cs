using AppSage.Core.Configuration;
using AppSage.Core.Localization;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using AppSage.Infrastructure.Caching;
using AppSage.Infrastructure.Workspace;
using AppSage.Run.CommandSet;
using AppSage.Run.CommandSet.Init;
using AppSage.Run.CommandSet.Provider;
using AppSage.Run.CommandSet.Root;
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
            var config = serviceProvider.GetRequiredService<IAppSageConfiguration>();

            try
            {

                //args=new string[] {"init"};
                //args = new string[] { "init", "-ws", @"C:\Temp\bingo2" };
                //args = new string[] { "init" };
                args = new string[] { "provider", "-ws", "C:\\Temp\\bingo2\\Logs" };

                RootCommand rootCommand = new RootCommand
                {
                    Description = "AppSage Run Command Line Interface"
                };
                var argWorkspaceFolder = AppSageRootCommand.GetWorkspaceArgument();
                rootCommand.Add(argWorkspaceFolder);


                ISubCommand initSubCommand = new InitCommand(logger);

                ISubCommand[] subCommands = new ISubCommand[]
                {
                    initSubCommand
                };

                foreach (var subCommand in subCommands)
                {
                    rootCommand.Add(subCommand.Build());
                }

                var parseResult = rootCommand.Parse(args);

                //We need to handle the init command differently because unlike other commands it does not need a resolved appsage workspace
                var initCommand = rootCommand.Children.OfType<Command>().FirstOrDefault(c => c.Name == (initSubCommand.Name));
                if (parseResult.CommandResult.Command == initCommand)
                {
                    return parseResult.Invoke();
                }
                else
                {
                    var workspaceRoot = ResolveWorkspaceRoot(args);
                    if (string.IsNullOrEmpty(workspaceRoot))
                    {
                        logger.LogError("Failed to resolve the workspace root folder. Ensure that the specified folder is a valid AppSage workspace or contains an AppSage workspace.");
                        logger.LogError($"If you want to initalize an AppSage workspace in a given empty folder you may use the command {initSubCommand.Name}");
                        return -1;
                    }
                    else
                    {
                        //All other commands need a workspace. The workspace has to be resolved first. 
                        logger.LogInformation($"Using workspace root folder: {workspaceRoot}");
                        var aliases = new List<string>() { argWorkspaceFolder.Name };
                        aliases.AddRange(argWorkspaceFolder.Aliases);
                        var newArgs = ForceOptionValue(args, workspaceRoot, aliases.ToArray());
                        var parse2Result = rootCommand.Parse(newArgs);

                        return parse2Result.Invoke();
                    }
                }



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

        private static string[] ForceOptionValue(string[] args,string forcedValue,string[] optionAliases)
        {
            var result = new List<string>();
            var skipNext = false;

            foreach (var arg in args)
            {
                if (skipNext)
                {
                    skipNext = false;
                    continue;
                }

                // match against any of the provided aliases
                if (optionAliases.Contains(arg, StringComparer.OrdinalIgnoreCase))
                {
                    // drop this alias and its value
                    skipNext = true;
                    continue;
                }
                result.Add(arg);
            }

            // inject forced value at the end
            result.Add(optionAliases[0]);  // use the "primary" name
            result.Add(forcedValue);

            return result.ToArray();
        }

        private static string ResolveWorkspaceRoot(string[] args)
        {
            //if the workspace argument is provided, use it. If not, use the current directory
            RootCommand parseCommand = new RootCommand();
            var argWorkspaceFolder = AppSageRootCommand.GetWorkspaceArgument();
            parseCommand.Add(argWorkspaceFolder);
            var result = parseCommand.Parse(args);
            string value = result.GetValue(argWorkspaceFolder);
            if (string.IsNullOrWhiteSpace(value))
            {
                value = Environment.CurrentDirectory;
            }

            string workspaceRoot = AppSageWorkspaceManager.ResolveWorkspaceRootFolder(value);

            return workspaceRoot;
        }



        /// <summary>
        /// Configure core services like logging, configuration, caching, localization & workspace mangement
        /// </summary>
        /// <returns>Service collection</returns>
        /// 


        private static IServiceCollection InitializeCoreServices(IServiceCollection services)
        {
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

            string logKey = "AppSage.Core:LogFolder";

            if (config.KeyExist(logKey))
            {
                string logFolder = config.Get<string>(logKey);
                if (!string.IsNullOrWhiteSpace(logFolder) && Directory.Exists(logFolder))
                {
                    logger.WriteTo.File(Path.Combine(logFolder, "appSage-.log"), rollingInterval: RollingInterval.Day);
                }
                else
                {
                    Console.WriteLine($"Warning: Log folder '{logFolder}' does not exist. Logs will be written to the console only.");
                }
            }
            // Create Serilog logger
            Log.Logger = logger.CreateLogger();
        }







    }
}
