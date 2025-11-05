using AppSage.Core.Logging;
using System.Reflection;

namespace AppSage.Extension
{
    public class ExtensionDependencyResolver : IExtensionDependencyResolver
    {
        private readonly IAppSageLogger _logger;
        private readonly Dictionary<string, Assembly> _hostAssemblies;
        private readonly string _packageCacheDirectory;

        public ExtensionDependencyResolver(IAppSageLogger logger, string packageCacheDirectory)
        {
            _logger = logger;
            _packageCacheDirectory = packageCacheDirectory;
            _hostAssemblies = LoadHostAssemblies();
        }

        public Assembly? ResolveHostDependency(AssemblyName assemblyName, HostProvidedDependency hostDependency)
        {
            try
            {
                // Check if assembly is already loaded in host
                if (_hostAssemblies.TryGetValue(assemblyName.Name!, out var assembly))
                {
                    // Validate version compatibility
                    if (IsVersionCompatible(assembly.GetName().Version, hostDependency.Version))
                    {
                        _logger.LogDebug("Host dependency resolved: {AssemblyName} v{Version}", 
                            assemblyName.Name, assembly.GetName().Version);
                        return assembly;
                    }
                    else
                    {
                        _logger.LogWarning("Host assembly {AssemblyName} version {ActualVersion} does not meet minimum requirement {Version}", 
                            assemblyName.Name, assembly.GetName().Version, hostDependency.Version);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error resolving host dependency: {AssemblyName}", ex, assemblyName.Name);
                return null;
            }
        }

        public async Task<Assembly?> ResolveExternalDependencyAsync(AssemblyName assemblyName, ExternalDependency externalDependency)
        {
            try
            {
                // Check local cache first
                var cachedPath = Path.Combine(_packageCacheDirectory, 
                    $"{externalDependency.Name}.{externalDependency.Version}", 
                    "lib", "net8.0", $"{assemblyName.Name}.dll");

                if (File.Exists(cachedPath))
                {
                    _logger.LogDebug("Loading external dependency from cache: {AssemblyPath}", cachedPath);
                    return Assembly.LoadFrom(cachedPath);
                }

                // For this implementation, we'll just log that NuGet download isn't implemented
                _logger.LogWarning("External dependency resolution not fully implemented: {AssemblyName}", assemblyName.Name);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to resolve external dependency: {AssemblyName}", ex, assemblyName.Name);
                if (!externalDependency.Optional)
                {
                    throw;
                }
                return null;
            }
        }

        public async Task<ValidationResult> ValidateDependenciesAsync(ExtensionManifest manifest)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                // Validate host provided dependencies
                foreach (var hostDep in manifest.Dependencies.HostProvided)
                {
                    if (!_hostAssemblies.ContainsKey(hostDep.Name))
                    {
                        result.Errors.Add($"Required host dependency not found: {hostDep.Name}");
                        result.IsValid = false;
                    }
                    else
                    {
                        var assembly = _hostAssemblies[hostDep.Name];
                        if (!IsVersionCompatible(assembly.GetName().Version, hostDep.Version))
                        {
                            result.Errors.Add($"Host dependency {hostDep.Name} version {assembly.GetName().Version} does not meet minimum requirement {hostDep.Version}");
                            result.IsValid = false;
                        }
                    }
                }

                // For this implementation, we'll assume external dependencies are available
                // In a real implementation, you would check NuGet API
                await Task.Delay(1); // Placeholder for async operation
            }
            catch (Exception ex)
            {
                _logger.LogError("Error validating dependencies for extension: {ExtensionId}", ex, manifest.ExtensionId);
                result.Errors.Add($"Validation error: {ex.Message}");
                result.IsValid = false;
            }

            return result;
        }

        private Dictionary<string, Assembly> LoadHostAssemblies()
        {
            var assemblies = new Dictionary<string, Assembly>();
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name != null)
                {
                    assemblies[assembly.GetName().Name!] = assembly;
                }
            }

            _logger.LogDebug("Loaded {Count} host assemblies for dependency resolution", assemblies.Count);
            return assemblies;
        }

        private bool IsVersionCompatible(Version? assemblyVersion, string versionSpec)
        {
            if (assemblyVersion == null || string.IsNullOrWhiteSpace(versionSpec))
                return false;

            versionSpec = versionSpec.Trim();

            // Exact version (e.g. 1.2.3)
            if (!versionSpec.StartsWith("[") && !versionSpec.StartsWith("(") && !versionSpec.Contains(","))
            {
                if (Version.TryParse(versionSpec, out var exactVersion))
                    return assemblyVersion == exactVersion;
                return false;
            }

            // Range (e.g. [1.2.3,2.0.0), [1.2.3,), (1.2.3,2.0.0], etc.)
            Version? minVersion = null;
            Version? maxVersion = null;
            bool minInclusive = false, maxInclusive = false;

            // Parse range
            if ((versionSpec.StartsWith("[") || versionSpec.StartsWith("(")) && versionSpec.Contains(","))
            {
                minInclusive = versionSpec.StartsWith("[");
                maxInclusive = versionSpec.EndsWith("]");
                var range = versionSpec.Substring(1, versionSpec.Length - 2); // remove [ ] or ( )
                var parts = range.Split(',');
                if (parts.Length == 2)
                {
                    if (!string.IsNullOrWhiteSpace(parts[0]))
                        Version.TryParse(parts[0], out minVersion);
                    if (!string.IsNullOrWhiteSpace(parts[1]))
                        Version.TryParse(parts[1], out maxVersion);
                }
            }

            // Check min
            if (minVersion != null)
            {
                int cmp = assemblyVersion.CompareTo(minVersion);
                if (cmp < 0 || (cmp == 0 && !minInclusive))
                    return false;
            }
            // Check max
            if (maxVersion != null)
            {
                int cmp = assemblyVersion.CompareTo(maxVersion);
                if (cmp > 0 || (cmp == 0 && !maxInclusive))
                    return false;
            }
            // If only min or max is specified, and passed above, it's compatible
            return true;
        }
    }
}