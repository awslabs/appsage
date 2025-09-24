using AppSage.Core.Configuration;
using AppSage.Core.Localization;
using AppSage.Core.Logging;
using AppSage.Core.Workspace;
using AppSage.Infrastructure.Workspace;
using AppSage.Run.CommandSet;
using AppSage.Run.CommandSet.Init;
using AppSage.Run.CommandSet.MCP;
using AppSage.Run.CommandSet.Provider;
using AppSage.Run.CommandSet.Root;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.CommandLine;
using System.Globalization;
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
            services = InitializeCoreServices(services, null);
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<IAppSageLogger>();


            try
            {

                //args=new string[] {"init"};
                //args = new string[] { "init", "-ws", @"C:\temp\bingo" };
                //args = new string[] { "init" };
                //args = new string[] { "provider","run","-ws", "C:\\Temp\\MyAppSageWorkspace" };
                //args= new string[] { "mcpserver", "run", "-ws", "C:\\Temp\\MyAppSageWorkspace" };

                RootCommand rootCommand = new RootCommand
                {
                    Description = "AppSage Run Command Line Interface"
                };
                var argWorkspaceFolder = AppSageRootCommand.GetWorkspaceArgument();
                rootCommand.Add(argWorkspaceFolder);

                ISubCommand initSubCommand = new InitCommand(logger);
                rootCommand.Add(initSubCommand.Build());
                var parseResult = rootCommand.Parse(args);
                var initCommand = rootCommand.Children.OfType<Command>().FirstOrDefault(c => c.Name == (initSubCommand.Name));

                if (parseResult.CommandResult.Command == initCommand)
                {
                    //We need to handle the init command differently because unlike other commands it does not need a resolved AppSage workspace
                    return parseResult.Invoke();
                }
                else
                {   //All other commands need an AppSage workspace. The workspace has to be resolved first.
                    var workspaceRoot = ResolveWorkspaceRoot(args);

                    if (workspaceRoot==null)
                    {
                        logger.LogError("Failed to resolve the workspace root folder. Ensure that the specified folder is a valid AppSage workspace or contains an AppSage workspace.");
                        logger.LogError($"If you want to initalize an AppSage workspace in a given empty folder you may use the command {initSubCommand.Name}");
                        return -1;
                    }
                    else
                    {
                        IAppSageWorkspace appSageWorkspace = new AppSageWorkspaceManager(workspaceRoot, logger);
                        IAppSageConfiguration appSageConfig = new AppSageConfiguration(appSageWorkspace.AppSageConfigFilePath);
                        //Re-initialize core services with the configuration now
                        //Dispose the previous service provider
                        if (serviceProvider is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                        services = new ServiceCollection();
                        services.AddSingleton<IAppSageWorkspace>(appSageWorkspace);
                        services.AddSingleton<IAppSageConfiguration>(appSageConfig);
                        services = InitializeCoreServices(services, appSageWorkspace);

                        var commandRegistry = GetCommandRegistry(services);
                        foreach (var cmd in commandRegistry)
                        {
                            rootCommand.Add(cmd.Build());
                        }

                        logger.LogInformation("Using workspace root folder: {WorkspaceRoot}", workspaceRoot);
                        var aliases = new List<string>() { argWorkspaceFolder.Name };
                        aliases.AddRange(argWorkspaceFolder.Aliases);
                        var newArgs = ForceOptionValue(args, workspaceRoot.FullName, aliases.ToArray());
                        parseResult = rootCommand.Parse(newArgs);

                        return parseResult.Invoke();
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

        private static List<ISubCommand> GetCommandRegistry(IServiceCollection serviceCollection)
        {
            var commands = new List<ISubCommand>();

            commands.Add(new ProviderCommand(serviceCollection));
            commands.Add(new MCPServerCommand(serviceCollection));


            return commands;
        }

        private static string[] ForceOptionValue(string[] args, string forcedValue, string[] optionAliases)
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

        private static DirectoryInfo ResolveWorkspaceRoot(string[] args)
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
            var argDictory=new DirectoryInfo(value);

            var workspaceRoot = AppSageWorkspaceManager.ResolveWorkspaceRootFolder(argDictory);

            return workspaceRoot;
        }

        private static IServiceCollection InitializeCoreServices(IServiceCollection services, IAppSageWorkspace workspace)
        {
            var logger = new LoggerConfiguration()
           .WriteTo.Sink(_countingSink)
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
