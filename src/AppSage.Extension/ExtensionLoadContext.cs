using AppSage.Core.Logging;
using System.Reflection;
using System.Runtime.Loader;

namespace AppSage.Extension
{
    public class ExtensionLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;
        private readonly ExtensionManifest _manifest;
        private readonly IExtensionDependencyResolver _dependencyResolver;
        private readonly string _extensionPath;
        private readonly Dictionary<string, Assembly> _loadedAssemblies;
        private readonly IAppSageLogger _logger;

        public ExtensionLoadContext(string extensionPath, ExtensionManifest manifest, 
            IExtensionDependencyResolver dependencyResolver, IAppSageLogger logger) 
            : base($"Extension_{manifest.ExtensionId}", isCollectible: true)
        {
            _extensionPath = extensionPath;
            _manifest = manifest;
            _dependencyResolver = dependencyResolver;
            _logger = logger;
            _resolver = new AssemblyDependencyResolver(extensionPath);
            _loadedAssemblies = new Dictionary<string, Assembly>();
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            try
            {
                // Check if already loaded
                if (_loadedAssemblies.TryGetValue(assemblyName.FullName, out var cachedAssembly))
                {
                    return cachedAssembly;
                }

                _logger.LogDebug("Loading assembly: {AssemblyName} for extension: {ExtensionId}", 
                    assemblyName.Name, _manifest.ExtensionId);

                // 1. Check if this is a host-provided dependency
                var hostDependency = _manifest.Dependencies.HostProvided
                    .FirstOrDefault(d => d.Name.Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase));
                
                if (hostDependency != null)
                {
                    var hostAssembly = _dependencyResolver.ResolveHostDependency(assemblyName, hostDependency);
                    if (hostAssembly != null)
                    {
                        _loadedAssemblies[assemblyName.FullName] = hostAssembly;
                        _logger.LogDebug("Resolved {AssemblyName} from host", assemblyName.Name);
                        return hostAssembly;
                    }
                }

                // 2. Check if this is a bundled dependency
                var bundledDependency = _manifest.Dependencies.Bundled
                    .FirstOrDefault(d => d.Assemblies.Any(a => 
                        Path.GetFileNameWithoutExtension(a).Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase)));

                if (bundledDependency != null)
                {
                    var bundledPath = Path.Combine(_extensionPath, "Dependencies", $"{assemblyName.Name}.dll");
                    if (File.Exists(bundledPath))
                    {
                        var assembly = LoadFromAssemblyPath(bundledPath);
                        _loadedAssemblies[assemblyName.FullName] = assembly;
                        _logger.LogDebug("Resolved {AssemblyName} from bundled dependencies", assemblyName.Name);
                        return assembly;
                    }
                }

                // 3. Use default resolver (for system assemblies and co-located assemblies)
                string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
                if (assemblyPath != null)
                {
                    var assembly = LoadFromAssemblyPath(assemblyPath);
                    _loadedAssemblies[assemblyName.FullName] = assembly;
                    _logger.LogDebug("Resolved {AssemblyName} using default resolver", assemblyName.Name);
                    return assembly;
                }

                _logger.LogWarning("Failed to resolve assembly: {AssemblyName} for extension: {ExtensionId}", 
                    assemblyName.Name, _manifest.ExtensionId);

                // 4. Fall back to default load context
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error loading assembly {AssemblyName} for extension {ExtensionId}", ex, assemblyName.Name, _manifest.ExtensionId);
                return null;
            }
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}