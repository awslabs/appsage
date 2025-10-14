using AppSage.Core.Logging;
using AppSage.Core.Configuration;
using AppSage.Core.Workspace;
using System.Reflection;
using System.Text.Json;

namespace AppSage.Extension
{
    public class ExtensionManager : IExtensionManager
    {
        private readonly IAppSageLogger _logger;
        private readonly IExtensionDependencyResolver _dependencyResolver;
        private readonly IAppSageConfiguration _configuration;
        private readonly IAppSageWorkspace _workspace;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, ExtensionInstance> _loadedExtensions;
        private readonly string _extensionDirectory;

        public ExtensionManager(
            IAppSageLogger logger, 
            IExtensionDependencyResolver dependencyResolver,
            IAppSageConfiguration configuration,
            IAppSageWorkspace workspace,
            IServiceProvider serviceProvider,
            string extensionDirectory)
        {
            _logger = logger;
            _dependencyResolver = dependencyResolver;
            _configuration = configuration;
            _workspace = workspace;
            _serviceProvider = serviceProvider;
            _extensionDirectory = extensionDirectory;
            _loadedExtensions = new Dictionary<string, ExtensionInstance>();
        }

        public async Task<IEnumerable<IExtension>> LoadExtensionsAsync()
        {
            var extensions = new List<IExtension>();

            try
            {
                _logger.LogInformation("Loading extensions from directory: {ExtensionDirectory}", _extensionDirectory);

                // Discover extension packages
                var extensionPackages = DiscoverExtensionPackages();

                foreach (var package in extensionPackages)
                {
                    try
                    {
                        var extension = await LoadExtensionAsync(package);
                        if (extension != null)
                        {
                            extensions.Add(extension);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to load extension from package: {PackagePath}", ex, package.Path);
                    }
                }

                _logger.LogInformation("Successfully loaded {Count} extensions", extensions.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during extension loading process", ex);
            }

            return extensions;
        }

        public async Task<IExtension?> LoadExtensionAsync(string extensionPath)
        {
            var package = new ExtensionPackage
            {
                Path = extensionPath,
                ManifestPath = Path.Combine(extensionPath, "extension.manifest.json"),
                EntryAssemblyPath = ""
            };

            return await LoadExtensionAsync(package);
        }

        private async Task<IExtension?> LoadExtensionAsync(ExtensionPackage package)
        {
            try
            {
                // Load manifest (optional for direct assembly loading)
                ExtensionManifest? manifest = null;
                if (File.Exists(package.ManifestPath))
                {
                    manifest = LoadManifest(package.ManifestPath);
                }

                // Find entry assembly
                if (!string.IsNullOrEmpty(package.EntryAssemblyPath) && File.Exists(package.EntryAssemblyPath))
                {
                    // Use provided entry assembly
                }
                else if (manifest != null && !string.IsNullOrEmpty(manifest.EntryAssembly))
                {
                    package.EntryAssemblyPath = Path.Combine(package.Path, manifest.EntryAssembly);
                }
                else
                {
                    // Look for DLL files in the directory
                    var dllFiles = Directory.GetFiles(package.Path, "*.dll");
                    package.EntryAssemblyPath = dllFiles.FirstOrDefault() ?? "";
                }

                if (string.IsNullOrEmpty(package.EntryAssemblyPath) || !File.Exists(package.EntryAssemblyPath))
                {
                    _logger.LogError("Entry assembly not found for extension package: {PackagePath}", package.Path);
                    return null;
                }

                // Create default manifest if none exists
                if (manifest == null)
                {
                    manifest = CreateDefaultManifest(package.EntryAssemblyPath);
                }

                // Check if already loaded
                if (_loadedExtensions.ContainsKey(manifest.ExtensionId))
                {
                    _logger.LogWarning("Extension {ExtensionId} is already loaded", manifest.ExtensionId);
                    return _loadedExtensions[manifest.ExtensionId].Extension;
                }

                // Validate dependencies (only if manifest has dependencies)
                if (manifest.Dependencies.HostProvided.Any() || manifest.Dependencies.External.Any())
                {
                    var validation = await _dependencyResolver.ValidateDependenciesAsync(manifest);
                    if (!validation.IsValid)
                    {
                        _logger.LogError("Extension {ExtensionId} failed dependency validation: {Errors}", 
                            manifest.ExtensionId, string.Join(", ", validation.Errors));
                        return null;
                    }
                }

                // Create extension load context
                var loadContext = new ExtensionLoadContext(package.Path, manifest, _dependencyResolver, _logger);

                // Load main assembly
                var mainAssembly = loadContext.LoadFromAssemblyPath(package.EntryAssemblyPath);
                
                // Find extension types
                var extensionTypes = mainAssembly.GetTypes()
                    .Where(t => typeof(IExtension).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var extensionType in extensionTypes)
                {
                    var extension = (IExtension)Activator.CreateInstance(extensionType)!;
                    
                    // Create extension context
                    var context = new ExtensionContext(_logger, _configuration, _workspace, _serviceProvider, manifest);
                    
                    // Initialize extension
                    await extension.InitializeAsync(context);
                    
                    var extensionInstance = new ExtensionInstance(extension, loadContext, manifest, package, mainAssembly);
                    _loadedExtensions[manifest.ExtensionId] = extensionInstance;
                    
                    _logger.LogInformation("Successfully loaded extension: {ExtensionId} v{Version}", 
                        manifest.ExtensionId, manifest.Version);
                    
                    return extension;
                }

                _logger.LogWarning("No extension types found in assembly: {AssemblyPath}", package.EntryAssemblyPath);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load extension from package: {PackagePath}", ex, package.Path);
            }

            return null;
        }

        public async Task UnloadExtensionAsync(string extensionId)
        {
            if (_loadedExtensions.TryGetValue(extensionId, out var extensionInstance))
            {
                try
                {
                    await extensionInstance.Extension.StopAsync();
                    await extensionInstance.Extension.DisposeAsync();
                    extensionInstance.LoadContext.Unload();
                    _loadedExtensions.Remove(extensionId);
                    
                    _logger.LogInformation("Successfully unloaded extension: {ExtensionId}", extensionId);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error unloading extension: {ExtensionId}", ex, extensionId);
                }
            }
        }

        public IExtension? GetExtension(string extensionId)
        {
            return _loadedExtensions.TryGetValue(extensionId, out var instance) ? instance.Extension : null;
        }

        public IEnumerable<IExtension> GetExtensions()
        {
            return _loadedExtensions.Values.Select(i => i.Extension);
        }

        public Task<bool> InstallExtensionAsync(string packagePath)
        {
            // Implementation for installing extension packages
            throw new NotImplementedException("Extension installation not yet implemented");
        }

        public Task<bool> UninstallExtensionAsync(string extensionId)
        {
            // Implementation for uninstalling extensions
            throw new NotImplementedException("Extension uninstallation not yet implemented");
        }

        private IEnumerable<ExtensionPackage> DiscoverExtensionPackages()
        {
            var packages = new List<ExtensionPackage>();

            if (!Directory.Exists(_extensionDirectory))
            {
                _logger.LogWarning("Extension directory does not exist: {ExtensionDirectory}", _extensionDirectory);
                return packages;
            }

            // Look for manifest files first
            foreach (var directory in Directory.GetDirectories(_extensionDirectory))
            {
                var manifestPath = Path.Combine(directory, "extension.manifest.json");
                if (File.Exists(manifestPath))
                {
                    packages.Add(new ExtensionPackage
                    {
                        Path = directory,
                        ManifestPath = manifestPath,
                        EntryAssemblyPath = ""
                    });
                }
            }

            // Also look for standalone DLL files
            var dllFiles = Directory.GetFiles(_extensionDirectory, "*.dll");
            foreach (var dllFile in dllFiles)
            {
                packages.Add(new ExtensionPackage
                {
                    Path = Path.GetDirectoryName(dllFile) ?? _extensionDirectory,
                    ManifestPath = "",
                    EntryAssemblyPath = dllFile
                });
            }

            _logger.LogDebug("Discovered {Count} extension packages", packages.Count);
            return packages;
        }

        private ExtensionManifest? LoadManifest(string manifestPath)
        {
            try
            {
                var manifestJson = File.ReadAllText(manifestPath);
                return JsonSerializer.Deserialize<ExtensionManifest>(manifestJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load extension manifest: {ManifestPath}", ex, manifestPath);
                return null;
            }
        }

        private ExtensionManifest CreateDefaultManifest(string assemblyPath)
        {
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            return new ExtensionManifest
            {
                ExtensionId = assemblyName,
                DisplayName = assemblyName,
                Version = "1.0.0",
                Description = $"Auto-generated manifest for {assemblyName}",
                Author = "Unknown",
                EntryAssembly = Path.GetFileName(assemblyPath),
                TargetFramework = "net8.0",
                HostVersion = "1.0.0"
            };
        }

        // Get the assembly for a loaded extension
        public Assembly? GetExtensionAssembly(string extensionId)
        {
            return _loadedExtensions.TryGetValue(extensionId, out var instance) ? instance.Assembly : null;
        }
    }

    // Supporting classes
    internal class ExtensionInstance
    {
        public IExtension Extension { get; }
        public ExtensionLoadContext LoadContext { get; }
        public ExtensionManifest Manifest { get; }
        public ExtensionPackage Package { get; }
        public Assembly Assembly { get; }

        public ExtensionInstance(IExtension extension, ExtensionLoadContext loadContext, 
            ExtensionManifest manifest, ExtensionPackage package, Assembly assembly)
        {
            Extension = extension;
            LoadContext = loadContext;
            Manifest = manifest;
            Package = package;
            Assembly = assembly;
        }
    }

    internal class ExtensionPackage
    {
        public string Path { get; set; } = "";
        public string ManifestPath { get; set; } = "";
        public string EntryAssemblyPath { get; set; } = "";
    }

    internal class ExtensionContext : IExtensionContext
    {
        public IAppSageLogger Logger { get; }
        public IAppSageConfiguration Configuration { get; }
        public IAppSageWorkspace Workspace { get; }
        public IServiceProvider ServiceProvider { get; }
        public ExtensionManifest? Manifest { get; }

        public ExtensionContext(IAppSageLogger logger, IAppSageConfiguration configuration, 
            IAppSageWorkspace workspace, IServiceProvider serviceProvider, ExtensionManifest? manifest)
        {
            Logger = logger;
            Configuration = configuration;
            Workspace = workspace;
            ServiceProvider = serviceProvider;
            Manifest = manifest;
        }
    }
}