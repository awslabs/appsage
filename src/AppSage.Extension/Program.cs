using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using AppSage.Extension;
using AppSage.Infrastructure.Metric;
using AppSage.Infrastructure.Workspace;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;
using Newtonsoft.Json;
using System.Text.Json;

namespace AppSage.Extension
{

 
        internal class Program
    {

        static async Task<int> Main(string[] args)
        {
            
  

            args = new string[] { "C:\\Temp\\MyAppSageWorkspace" };
            // Initialize logging
            var logger = ConfigureLogging();

            try
            {
                string workspacePath = args[0];
                string extensionPath = args.Length > 1 ? args[1] : Path.Combine(workspacePath, "Extensions");

                // Validate workspace path
                if (!Directory.Exists(workspacePath))
                {
                    logger.LogError("Workspace directory not found: {WorkspacePath}", workspacePath);
                    return -1;
                }
                logger.LogInformation("Workspace: {WorkspacePath}", workspacePath);


                // Setup services and extension system
                var services = SetupServices(logger, workspacePath, extensionPath);
                var serviceProvider = services.BuildServiceProvider();

                // Get the extension manager
                //var extensionManager = serviceProvider.GetRequiredService<IExtensionManagerV2>();

                //extensionManager.InstallExtension("AppSage.Providers.HelloWorld",true);
                //extensionManager.InstallExtension("AppSage.Providers.DotNet", true);

                // Load and execute extensions
                //var extensionManager = serviceProvider.GetRequiredService<IExtensionManager>();
                //await LoadAndExecuteExtensions(extensionManager, serviceProvider, logger, extensionPath);

                return 0;
            }
            catch (Exception ex)
            {
                logger.LogError("Extension execution failed", ex);
                return -1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IAppSageLogger ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithMachineName()
                .WriteTo.Console()
                .CreateLogger();

            return new AppSageLogger(Log.Logger);
        }

        private static IServiceCollection SetupServices(IAppSageLogger logger, string workspacePath, string extensionPath)
        {
            var services = new ServiceCollection();

            // Pre-load all host-provided dependencies
            //PreloadHostAssemblies(logger);

            // Register core services
            services.AddSingleton<IAppSageLogger>(logger);

            // Setup workspace
            var workspace = new AppSageWorkspaceManager(new DirectoryInfo(workspacePath), logger);
            services.AddSingleton<IAppSageWorkspace>(workspace);

            // Setup configuration
            var configuration = new AppSageConfiguration(((IAppSageWorkspace)workspace).AppSageConfigFilePath);
            services.AddSingleton<IAppSageConfiguration>(configuration);

            // Setup extension system
            var packageCacheDirectory = Path.Combine(workspacePath, "ExtensionCache");
            
            // Determine extension directory based on input
            string extensionDirectory;
            if (File.Exists(extensionPath) && extensionPath.EndsWith(".dll"))
            {
                // If it's a single DLL, use its directory
                extensionDirectory = Path.GetDirectoryName(extensionPath) ?? Path.Combine(workspacePath, "Extensions");
            }
            else
            {
                // It's a directory
                extensionDirectory = extensionPath;
            }
            
            Directory.CreateDirectory(extensionDirectory);
            Directory.CreateDirectory(packageCacheDirectory);

            // Register extension services
            //services.AddSingleton<IExtensionDependencyResolver>(provider =>
            //    new ExtensionDependencyResolver(
            //        provider.GetRequiredService<IAppSageLogger>(),
            //        packageCacheDirectory));

            //services.AddSingleton<IExtensionManager>(provider =>
            //    new ExtensionManager(
            //        provider.GetRequiredService<IAppSageLogger>(),
            //        provider.GetRequiredService<IExtensionDependencyResolver>(),
            //        provider.GetRequiredService<IAppSageConfiguration>(),
            //        provider.GetRequiredService<IAppSageWorkspace>(),
            //        provider,
            //        extensionDirectory));

            //services.AddSingleton<IExtensionManagerV2, ExtensionManagerV2>();

            return services;
        }



     
    }
}
