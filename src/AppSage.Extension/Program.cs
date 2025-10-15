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
                var extensionManager = serviceProvider.GetRequiredService<IExtensionManagerV2>();

                extensionManager.InstallExtension("AppSage.Providers.HelloWorld");

                // Load and execute extensions
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
            PreloadHostAssemblies(logger);

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
            services.AddSingleton<IExtensionDependencyResolver>(provider =>
                new ExtensionDependencyResolver(
                    provider.GetRequiredService<IAppSageLogger>(),
                    packageCacheDirectory));

            //services.AddSingleton<IExtensionManager>(provider =>
            //    new ExtensionManager(
            //        provider.GetRequiredService<IAppSageLogger>(),
            //        provider.GetRequiredService<IExtensionDependencyResolver>(),
            //        provider.GetRequiredService<IAppSageConfiguration>(),
            //        provider.GetRequiredService<IAppSageWorkspace>(),
            //        provider,
            //        extensionDirectory));

            services.AddSingleton<IExtensionManagerV2, ExtensionManagerV2>();

            return services;
        }

        private static void PreloadHostAssemblies(IAppSageLogger logger)
        {
            logger.LogInformation("=== Pre-loading Host Dependencies ===");
            
            var hostDependencies = new[]
            {
                "Newtonsoft.Json",
                "System.Text.Json",
                "Microsoft.Extensions.DependencyInjection",
                "Microsoft.Extensions.Configuration",
                "Microsoft.Extensions.Configuration.Abstractions",
                "Microsoft.Extensions.DependencyInjection.Abstractions",
                "Serilog"
            };

            foreach (var dependency in hostDependencies)
            {
                try
                {
                    var assembly = Assembly.Load(dependency);
                    logger.LogInformation("✓ Pre-loaded host assembly: {AssemblyName} v{Version}", 
                        assembly.GetName().Name, assembly.GetName().Version);
                }
                catch (Exception ex)
                {
                    logger.LogWarning("⚠ Failed to pre-load assembly {AssemblyName}: {Error}", dependency, ex.Message);
                }
            }

            // Explicitly force Newtonsoft.Json types to be loaded
            try
            {
                logger.LogInformation("Pre-loading Newtonsoft.Json types...");
                var newtonsoftTypes = new[]
                {
                    typeof(Newtonsoft.Json.JsonSerializer),
                    typeof(Newtonsoft.Json.JsonConvert),
                    typeof(Newtonsoft.Json.Linq.JObject),
                    typeof(Newtonsoft.Json.Linq.JArray),
                    typeof(Newtonsoft.Json.Linq.JToken),
                    typeof(Newtonsoft.Json.JsonTextReader),
                    typeof(Newtonsoft.Json.JsonTextWriter)
                };

                foreach (var type in newtonsoftTypes)
                {
                    logger.LogDebug("✓ Pre-loaded Newtonsoft.Json type: {TypeName}", type.FullName);
                }

                // Create instances to ensure all dependent assemblies are loaded
                var serializer = new Newtonsoft.Json.JsonSerializer();
                var jobject = new Newtonsoft.Json.Linq.JObject();
                logger.LogInformation("✓ Newtonsoft.Json types and instances loaded successfully");
            }
            catch (Exception ex)
            {
                logger.LogError("✗ Failed to pre-load Newtonsoft.Json types", ex);
            }

            // Explicitly force System.Text.Json types to be loaded
            try
            {
                logger.LogInformation("Pre-loading System.Text.Json types...");
                var systemTextJsonTypes = new[]
                {
                    typeof(System.Text.Json.JsonSerializer),
                    typeof(System.Text.Json.JsonDocument),
                    typeof(System.Text.Json.JsonElement),
                    typeof(System.Text.Json.Utf8JsonReader),
                    typeof(System.Text.Json.Utf8JsonWriter),
                    typeof(System.Text.Json.JsonSerializerOptions)
                };

                foreach (var type in systemTextJsonTypes)
                {
                    logger.LogDebug("✓ Pre-loaded System.Text.Json type: {TypeName}", type.FullName);
                }

                // Create instances to ensure all dependent assemblies are loaded
                var options = new System.Text.Json.JsonSerializerOptions();
                logger.LogInformation("✓ System.Text.Json types and instances loaded successfully");
            }
            catch (Exception ex)
            {
                logger.LogError("✗ Failed to pre-load System.Text.Json types", ex);
            }

            // Force load AppSage assemblies that should be host-provided
            try
            {
                logger.LogInformation("Pre-loading AppSage assemblies...");
                var appSageAssemblies = new[]
                {
                    "AppSage.Core",
                    "AppSage.Infrastructure"
                };

                foreach (var assemblyName in appSageAssemblies)
                {
                    try
                    {
                        var assembly = Assembly.Load(assemblyName);
                        logger.LogInformation("✓ Pre-loaded AppSage assembly: {AssemblyName} v{Version}", 
                            assembly.GetName().Name, assembly.GetName().Version);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning("⚠ Failed to pre-load AppSage assembly {AssemblyName}: {Error}", 
                            assemblyName, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError("✗ Error during AppSage assemblies pre-loading", ex);
            }

            logger.LogInformation("=== Host Dependencies Pre-loading Completed ===");
            logger.LogInformation("");
        }

        private static async Task LoadAndExecuteExtensions(IExtensionManager extensionManager, IServiceProvider serviceProvider, IAppSageLogger logger, string extensionPath)
        {
            logger.LogInformation("=== Loading Extensions ===");

            IEnumerable<IExtension> extensions;

            // Check if we're loading a specific DLL or discovering from directory
            if (File.Exists(extensionPath) && extensionPath.EndsWith(".dll"))
            {
                logger.LogInformation("Loading specific extension: {ExtensionPath}", extensionPath);
                var extension = await extensionManager.LoadExtensionAsync(Path.GetDirectoryName(extensionPath)!);
                extensions = extension != null ? new[] { extension } : Array.Empty<IExtension>();
            }
            else
            {
                logger.LogInformation("Discovering extensions from directory: {ExtensionDirectory}", extensionPath);
                extensions = await extensionManager.LoadExtensionsAsync();
            }
            
            if (!extensions.Any())
            {
                logger.LogWarning("No extensions found to load");
                return;
            }

            logger.LogInformation("Loaded {Count} extensions", extensions.Count());
            logger.LogInformation("");

            // Start all loaded extensions
            logger.LogInformation("=== Starting Extensions ===");
            foreach (var extension in extensions)
            {
                try
                {
                    await extension.StartAsync();
                    logger.LogInformation("✓ Started extension: {ExtensionId} - {DisplayName}", 
                        extension.ExtensionId, extension.DisplayName);
                }
                catch (Exception ex)
                {
                    logger.LogError("✗ Failed to start extension: {ExtensionId}", ex, extension.ExtensionId);
                    continue;
                }
            }
            logger.LogInformation("");

            // Discover and execute IMetricProvider implementations in loaded extensions
            await DiscoverAndExecuteMetricProviders(extensionManager, serviceProvider, logger);

            // Stop all extensions gracefully
            logger.LogInformation("=== Stopping Extensions ===");
            foreach (var extension in extensions)
            {
                try
                {
                    await extension.StopAsync();
                    logger.LogInformation("✓ Stopped extension: {ExtensionId}", extension.ExtensionId);
                }
                catch (Exception ex)
                {
                    logger.LogError("✗ Error stopping extension: {ExtensionId}", ex, extension.ExtensionId);
                }
            }
        }

        private static async Task DiscoverAndExecuteMetricProviders(IExtensionManager extensionManager, IServiceProvider serviceProvider, IAppSageLogger logger)
        {
            var workspace = serviceProvider.GetRequiredService<IAppSageWorkspace>();
            var configuration = serviceProvider.GetRequiredService<IAppSageConfiguration>();

            logger.LogInformation("=== Discovering IMetricProvider Implementations ===");

            var loadedExtensions = extensionManager.GetExtensions().ToList();
            var totalProviders = 0;

            foreach (var extension in loadedExtensions)
            {
                try
                {
                    logger.LogInformation("Scanning extension: {ExtensionId} ({DisplayName})", 
                        extension.ExtensionId, extension.DisplayName);

                    // Get the extension's assembly
                    Assembly? extensionAssembly = null;
                    if (extensionManager is ExtensionManager manager)
                    {
                        extensionAssembly = manager.GetExtensionAssembly(extension.ExtensionId);
                    }

                    if (extensionAssembly == null)
                    {
                        // Fallback: get assembly from extension type
                        extensionAssembly = extension.GetType().Assembly;
                    }

                    // Find IMetricProvider implementations in the extension's assembly
                    var metricProviderTypes = extensionAssembly.GetTypes()
                        .Where(type => typeof(IMetricProvider).IsAssignableFrom(type) 
                                      && !type.IsInterface 
                                      && !type.IsAbstract)
                        .ToList();

                    if (!metricProviderTypes.Any())
                    {
                        logger.LogInformation("  ℹ No IMetricProvider implementations found");
                        continue;
                    }

                    logger.LogInformation("  ✓ Found {Count} IMetricProvider implementations:", metricProviderTypes.Count);
                    totalProviders += metricProviderTypes.Count;

                    // List the found providers
                    foreach (var providerType in metricProviderTypes)
                    {
                        logger.LogInformation("    - {ProviderType}", providerType.Name);
                    }

                    logger.LogInformation("");
                    logger.LogInformation("=== Executing IMetricProvider Implementations ===");

                    // Create and execute each metric provider
                    foreach (var providerType in metricProviderTypes)
                    {
                        await ExecuteMetricProvider(providerType, serviceProvider, logger, extension.ExtensionId);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError("✗ Error processing extension: {ExtensionId}", ex, extension.ExtensionId);
                }
            }

            logger.LogInformation("");
            logger.LogInformation("=== Summary ===");
            logger.LogInformation("Total extensions processed: {ExtensionCount}", loadedExtensions.Count);
            logger.LogInformation("Total IMetricProvider implementations executed: {ProviderCount}", totalProviders);
        }

        private static async Task ExecuteMetricProvider(Type providerType, IServiceProvider serviceProvider, IAppSageLogger logger, string extensionId)
        {
            var workspace = serviceProvider.GetRequiredService<IAppSageWorkspace>();
            var configuration = serviceProvider.GetRequiredService<IAppSageConfiguration>();

            try
            {
                logger.LogInformation("▶ Executing: {ProviderType}", providerType.FullName);

                // Create provider instance using dependency injection
                var provider = CreateProviderInstance(providerType, serviceProvider, logger);
                
                if (provider == null)
                {
                    logger.LogError("  ✗ Failed to create instance of provider: {ProviderType}", providerType.FullName);
                    return;
                }

                string providerVersion = providerType.Assembly.GetName().Version?.ToString() ?? "0.0.0.0";

                logger.LogInformation("  Provider: {ProviderName}", provider.FullQualifiedName);
                logger.LogInformation("  Description: {Description}", provider.Description);
                logger.LogInformation("  Version: {Version}", providerVersion);

                // Execute the provider with metric collection
                using var collector = new MetricCollector(
                    provider.FullQualifiedName, 
                    providerVersion, 
                    logger, 
                    workspace, 
                    configuration);

                var startTime = DateTime.UtcNow;

                // Run the provider
                provider.Run(collector);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                logger.LogInformation("  ✓ Execution completed in {Duration:mm\\:ss\\.fff}", duration);
                logger.LogInformation("  📊 Collected {MetricCount} metrics", collector.TotalCollectedMetricCount);
                logger.LogInformation("");
            }
            catch (Exception ex)
            {
                logger.LogError("  ✗ Error executing IMetricProvider: {ProviderType}", ex, providerType.FullName);
                logger.LogInformation("");
            }
        }

        private static IMetricProvider? CreateProviderInstance(Type providerType, IServiceProvider serviceProvider, IAppSageLogger logger)
        {
            // Get the constructors ordered by parameter count (prefer dependency injection)
            var constructors = providerType.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .ToArray();

            foreach (var constructor in constructors)
            {
                try
                {
                    var parameters = constructor.GetParameters();
                    var args = new object[parameters.Length];

                    // Try to resolve all parameters from the service provider
                    bool canResolveAll = true;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameterType = parameters[i].ParameterType;
                        var service = serviceProvider.GetService(parameterType);
                        
                        if (service != null)
                        {
                            args[i] = service;
                        }
                        else
                        {
                            logger.LogDebug("Cannot resolve parameter {ParameterName} of type {ParameterType}", 
                                parameters[i].Name, parameterType.Name);
                            canResolveAll = false;
                            break;
                        }
                    }

                    if (canResolveAll)
                    {
                        return (IMetricProvider)Activator.CreateInstance(providerType, args)!;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug("Constructor with {ParameterCount} parameters failed: {Error}", 
                        constructor.GetParameters().Length, ex.Message);
                    continue;
                }
            }

            // If no constructor worked with DI, try parameterless constructor
            try
            {
                return (IMetricProvider)Activator.CreateInstance(providerType)!;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to create instance using parameterless constructor", ex);
                return null;
            }
        }
    }
}
