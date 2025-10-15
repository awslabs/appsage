using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Workspace;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reflection;

namespace AppSage.Extension
{
    internal class ExtensionManagerV2:IExtensionManagerV2
    {
        private readonly IAppSageLogger _logger;
        private readonly IAppSageWorkspace _workspace;
        private readonly NuGetFramework _hostFramework;
        private readonly ISettings _nugetSettings;
        private readonly List<PackageSource> _packageSources;

        public ExtensionManagerV2(IAppSageLogger logger,IAppSageWorkspace workspace)
        {
            _logger = logger;
            _workspace = workspace;
            _hostFramework = GetCurrentHostFramework();
            
            // Initialize NuGet settings with configurable sources
            _nugetSettings = Settings.LoadDefaultSettings(root: null);
            _packageSources = GetConfiguredPackageSources();
            
            _logger.LogInformation("Host framework detected: {Framework}", _hostFramework.GetShortFolderName());
        }

        public bool InstallExtension(string packageId,bool force)
        {
            try
            {
                _logger.LogInformation("Installing extension package: {PackageId}", packageId);
                
                // Parse package ID and version
                var (resolvedPackageId, version) = ParsePackageIdAndVersion(packageId);
                
                // Find the package file
                string resolvedPackage = FindPackageFile(resolvedPackageId, version);
                if (string.IsNullOrEmpty(resolvedPackage))
                {
                    _logger.LogError("Package file not found for: {PackageId}", packageId);
                    return false;
                }

                _logger.LogInformation("Found package file: {PackageFile}", resolvedPackage);

                // If no version was specified in the input, extract the actual version from the resolved package
                if (version == null)
                {
                    version = ExtractVersionFromPackageFile(resolvedPackage);
                    if (version == null)
                    {
                        _logger.LogError("Could not extract version from package file: {PackageFile}", resolvedPackage);
                        return false;
                    }
                }

                //Ensure extension install folder exists
                if (!Directory.Exists(_workspace.ExtensionInstallFolder))
                {
                    Directory.CreateDirectory(_workspace.ExtensionInstallFolder);
                }

                // Create installation directory without version in path
                string installFolder = Path.Combine(_workspace.ExtensionInstallFolder, resolvedPackageId);
                
                if (Directory.Exists(installFolder) && !force)
                {
                    _logger.LogInformation("Package {PackageId} is already installed. Use Force to reinstall it", resolvedPackageId);
                    return true;
                }
                else if (Directory.Exists(installFolder) && force)
                {
                    _logger.LogInformation("Force installing package {PackageId}", resolvedPackageId);
                    Directory.Delete(installFolder, true);
                }

                Directory.CreateDirectory(installFolder);
                _logger.LogInformation("Created installation folder: {InstallFolder}", installFolder);

                // Extract the main package with proper TFM selection
                if (!ExtractMainPackage(resolvedPackage, installFolder, resolvedPackageId))
                {
                    _logger.LogError("Failed to extract main package: {PackageFile}", resolvedPackage);
                    return false;
                }

                _logger.LogInformation("Extracted main package to: {InstallFolder}", installFolder);

                // Resolve and install dependencies recursively to the same folder
                var processedDependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (!InstallDependenciesRecursively(resolvedPackage, installFolder, processedDependencies))
                {
                    _logger.LogWarning("Some dependencies could not be installed for: {PackageId}", resolvedPackageId);
                    // Don't fail the installation for missing optional dependencies
                }

                _logger.LogInformation("Successfully installed extension: {PackageId}", packageId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to install extension {PackageId}", ex, packageId);
                return false;
            }
        }

        private NuGetFramework GetCurrentHostFramework()
        {
            try
            {
                // Get the current runtime framework dynamically
                var frameworkName = Assembly.GetEntryAssembly()?.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()?.FrameworkName;
                
                if (!string.IsNullOrEmpty(frameworkName))
                {
                    var framework = NuGetFramework.ParseFrameworkName(frameworkName, new DefaultFrameworkNameProvider());
                    _logger.LogInformation("Detected host framework from entry assembly: {Framework}", framework.GetShortFolderName());
                    return framework;
                }

                // Fallback to runtime information
                var runtimeVersion = Environment.Version;
                if (runtimeVersion.Major >= 8)
                {
                    return NuGetFramework.ParseFolder("net8.0");
                }
                else if (runtimeVersion.Major >= 6)
                {
                    return NuGetFramework.ParseFolder("net6.0");
                }
                else
                {
                    return NuGetFramework.ParseFolder("netcoreapp3.1");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to detect host framework, defaulting to net8.0: {Error}", ex.Message);
                return NuGetFramework.ParseFolder("net8.0");
            }
        }

        private List<PackageSource> GetConfiguredPackageSources()
        {
            var sources = new List<PackageSource>();
            
            try
            {
                // Get configured sources from NuGet settings
                var packageSources = SettingsUtility.GetEnabledSources(_nugetSettings);
                sources.AddRange(packageSources);
                
                // Ensure nuget.org is included as fallback
                var nugetOrgExists = sources.Any(s => s.SourceUri.Host.Equals("api.nuget.org", StringComparison.OrdinalIgnoreCase));
                if (!nugetOrgExists)
                {
                    sources.Add(new PackageSource("https://api.nuget.org/v3/index.json", "nuget.org"));
                }

                _logger.LogInformation("Configured package sources: {Sources}", 
                    string.Join(", ", sources.Select(s => s.Name)));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to load configured package sources, using defaults: {Error}", ex.Message);
                sources.Add(new PackageSource("https://api.nuget.org/v3/index.json", "nuget.org"));
            }

            return sources;
        }

        private (string packageId, NuGetVersion version) ParsePackageIdAndVersion(string packageIdInput)
        {
            // Check if version is specified in the packageId (e.g., "AppSage.Providers.DotNet.1.0.0")
            var parts = packageIdInput.Split('.');
            
            // Try to find version pattern starting from the longest possible version
            // and working down to shorter versions
            // We need at least one part for package ID, so start from index 1
            for (int versionStartIndex = 1; versionStartIndex < parts.Length; versionStartIndex++)
            {
                var potentialVersionParts = parts.Skip(versionStartIndex).ToArray();
                var potentialVersionString = string.Join(".", potentialVersionParts);
                
                if (NuGetVersion.TryParse(potentialVersionString, out var version))
                {
                    var packageId = string.Join(".", parts.Take(versionStartIndex));
                    return (packageId, version);
                }
            }
            
            // No version specified, will find latest
            return (packageIdInput, null);
        }

        private string FindPackageFile(string packageId, NuGetVersion specificVersion)
        {
            try
            {
                var pattern = specificVersion != null 
                    ? $"{packageId}.{specificVersion}.nupkg"
                    : $"{packageId}.*.nupkg";

                var matchingFiles = Directory.GetFiles(_workspace.ExtensionPackagesFolder, pattern);

                if (specificVersion != null)
                {
                    return matchingFiles.FirstOrDefault() ?? string.Empty;
                }

                // Find the latest version
                NuGetVersion latestVersion = null;
                string latestFile = string.Empty;

                foreach (var file in matchingFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var versionStart = fileName.LastIndexOf(packageId + ".", StringComparison.OrdinalIgnoreCase);
                    
                    if (versionStart >= 0)
                    {
                        var versionString = fileName.Substring(versionStart + packageId.Length + 1);
                        if (NuGetVersion.TryParse(versionString, out var version))
                        {
                            if (latestVersion == null || version > latestVersion)
                            {
                                latestVersion = version;
                                latestFile = file;
                            }
                        }
                    }
                }

                return latestFile;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error finding package file for {PackageId}", ex, packageId);
                return string.Empty;
            }
        }

        private bool ExtractMainPackage(string packageFile, string extractPath, string packageId)
        {
            try
            {
                using var packageReader = new PackageArchiveReader(packageFile);
                
                // Get all files and filter lib files
                var allFiles = packageReader.GetFiles();
                var libFiles = allFiles.Where(f => f.StartsWith("lib/", StringComparison.OrdinalIgnoreCase));
                
                var bestTfm = SelectBestTargetFramework(libFiles);
                
                if (bestTfm != null)
                {
                    var libPattern = $"lib/{bestTfm}/";
                    var selectedLibFiles = libFiles.Where(f => f.StartsWith(libPattern, StringComparison.OrdinalIgnoreCase));
                    
                    foreach (var file in selectedLibFiles)
                    {
                        // Extract directly to root, removing lib/tfm/ prefix
                        var relativePath = file.Substring(libPattern.Length);
                        var destinationPath = Path.Combine(extractPath, relativePath);
                        ExtractFile(packageReader, file, destinationPath);
                    }
                    
                    _logger.LogInformation("Extracted lib files for TFM: {TFM}", bestTfm);
                }
                else
                {
                    _logger.LogWarning("No compatible lib files found for package: {PackageId}", packageId);
                }

                // Extract README file
                var readmeFiles = packageReader.GetFiles().Where(f => 
                    Path.GetFileNameWithoutExtension(f).Equals("README", StringComparison.OrdinalIgnoreCase));
                
                foreach (var readmeFile in readmeFiles)
                {
                    var fileName = Path.GetFileName(readmeFile);
                    var destinationPath = Path.Combine(extractPath, fileName);
                    ExtractFile(packageReader, readmeFile, destinationPath);
                }

                // Extract nuspec file
                var nuspecFile = packageReader.GetNuspecFile();
                if (!string.IsNullOrEmpty(nuspecFile))
                {
                    var nuspecDestination = Path.Combine(extractPath, $"{packageId}.nuspec");
                    ExtractFile(packageReader, nuspecFile, nuspecDestination);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error extracting main package: {PackageFile}", ex, packageFile);
                return false;
            }
        }

        private string SelectBestTargetFramework(IEnumerable<string> libFiles)
        {
            // Extract unique TFMs from lib files
            var availableTfmStrings = libFiles
                .Where(f => f.StartsWith("lib/", StringComparison.OrdinalIgnoreCase))
                .Select(f => f.Split('/')[1])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!availableTfmStrings.Any())
            {
                return null;
            }

            var availableTfms = availableTfmStrings
                .Select(tfm => 
                {
                    try 
                    { 
                        return NuGetFramework.ParseFolder(tfm); 
                    } 
                    catch 
                    { 
                        return null; 
                    }
                })
                .Where(tfm => tfm != null)
                .ToList();

            if (!availableTfms.Any())
            {
                return availableTfmStrings.First(); // fallback to first available
            }

            // Create framework specific wrappers for NuGetFrameworkUtility
            var frameworkSpecifics = availableTfms.Select(tfm => new FrameworkSpecificGroup(tfm, Enumerable.Empty<string>())).ToList();
            
            // Find the best match for the host framework
            var bestMatch = NuGetFrameworkUtility.GetNearest(frameworkSpecifics, _hostFramework, x => x.TargetFramework);
            
            if (bestMatch == null)
            {
                _logger.LogWarning("No compatible TFM found for host framework {HostFramework}, using first available: {AvailableTfm}", 
                    _hostFramework.GetShortFolderName(), availableTfms.First().GetShortFolderName());
                return availableTfms.First().GetShortFolderName();
            }
            else if (!bestMatch.TargetFramework.Equals(_hostFramework))
            {
                _logger.LogWarning("Exact TFM match not found. Host: {HostFramework}, Using: {SelectedTfm}", 
                    _hostFramework.GetShortFolderName(), bestMatch.TargetFramework.GetShortFolderName());
            }

            return bestMatch.TargetFramework.GetShortFolderName();
        }

        private void ExtractFile(PackageArchiveReader packageReader, string sourceFile, string destinationPath)
        {
            var destinationDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            using var sourceStream = packageReader.GetStream(sourceFile);
            using var destinationStream = File.Create(destinationPath);
            sourceStream.CopyTo(destinationStream);
        }

        private bool InstallDependenciesRecursively(string packageFile, string installFolder, HashSet<string> processedDependencies)
        {
            try
            {
                var dependencies = ResolveDependencies(packageFile);
                bool allSuccessful = true;

                foreach (var dependency in dependencies)
                {
                    if (dependency.Id == "AppSage.Core" || dependency.Id == "AppSage.Extension")
                    {
                        // Skip core dependencies
                        continue;
                    }

                    if (processedDependencies.Contains(dependency.Id))
                    {
                        // Already processed this dependency
                        continue;
                    }

                    processedDependencies.Add(dependency.Id);

                    var dependencyPackageFile = FindDependencyPackage(dependency);
                    if (string.IsNullOrEmpty(dependencyPackageFile))
                    {
                        _logger.LogWarning("Could not resolve dependency: {PackageId} {VersionRange}", 
                            dependency.Id, dependency.VersionRange);
                        allSuccessful = false;
                        continue;
                    }

                    // Extract dependency lib files to the same install folder
                    if (ExtractDependencyLibFiles(dependencyPackageFile, installFolder, dependency.Id))
                    {
                        _logger.LogInformation("Installed dependency: {PackageId}", dependency.Id);
                        
                        // Recursively process dependencies of this dependency
                        InstallDependenciesRecursively(dependencyPackageFile, installFolder, processedDependencies);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to extract dependency: {PackageId}", dependency.Id);
                        allSuccessful = false;
                    }
                }

                return allSuccessful;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error installing dependencies: {Error}", ex.Message);
                return false;
            }
        }

        private bool ExtractDependencyLibFiles(string packageFile, string installFolder, string packageId)
        {
            try
            {
                using var packageReader = new PackageArchiveReader(packageFile);
                
                // Get all files and filter lib files
                var allFiles = packageReader.GetFiles();
                var libFiles = allFiles.Where(f => f.StartsWith("lib/", StringComparison.OrdinalIgnoreCase));
                
                var bestTfm = SelectBestTargetFramework(libFiles);
                
                if (bestTfm != null)
                {
                    var libPattern = $"lib/{bestTfm}/";
                    var selectedLibFiles = libFiles.Where(f => f.StartsWith(libPattern, StringComparison.OrdinalIgnoreCase));
                    
                    foreach (var file in selectedLibFiles)
                    {
                        // Extract directly to install folder root, removing lib/tfm/ prefix
                        var relativePath = file.Substring(libPattern.Length);
                        var destinationPath = Path.Combine(installFolder, relativePath);
                        
                        // Skip if file already exists (avoid overwriting main extension files)
                        if (!File.Exists(destinationPath))
                        {
                            ExtractFile(packageReader, file, destinationPath);
                        }
                    }
                    
                    return true;
                }
                else
                {
                    _logger.LogWarning("No compatible lib files found for dependency: {PackageId}", packageId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error extracting dependency lib files: {PackageId}", ex, packageId);
                return false;
            }
        }

        private List<PackageDependency> ResolveDependencies(string packageFile)
        {
            var dependencies = new List<PackageDependency>();

            try
            {
                using var packageReader = new PackageArchiveReader(packageFile);
                var nuspecReader = packageReader.NuspecReader;
                
                // Get dependency groups for the host framework
                var dependencyGroups = nuspecReader.GetDependencyGroups();
                var applicableGroup = NuGetFrameworkUtility.GetNearest(
                    dependencyGroups,
                    _hostFramework,
                    group => group.TargetFramework);

                if (applicableGroup != null)
                {
                    dependencies.AddRange(applicableGroup.Packages);
                }

                _logger.LogInformation("Found {Count} dependencies for target framework {Framework}", 
                    dependencies.Count, _hostFramework.GetShortFolderName());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error resolving dependencies for package: {PackageFile}", ex, packageFile);
            }

            return dependencies;
        }

        private string FindDependencyPackage(PackageDependency dependency)
        {
            // Try to find in local packages first
            var localPackage = FindLocalDependencyPackage(dependency);
            if (!string.IsNullOrEmpty(localPackage))
            {
                return localPackage;
            }

            // Try to find in global NuGet cache
            var globalPackage = FindInGlobalCache(dependency);
            if (!string.IsNullOrEmpty(globalPackage))
            {
                return globalPackage;
            }

            // Try to download from online sources
            var onlinePackage = DownloadFromOnlineSources(dependency);
            if (!string.IsNullOrEmpty(onlinePackage))
            {
                return onlinePackage;
            }

            return string.Empty;
        }

        private string FindLocalDependencyPackage(PackageDependency dependency)
        {
            try
            {
                var pattern = $"{dependency.Id}.*.nupkg";
                var matchingFiles = Directory.GetFiles(_workspace.ExtensionPackagesFolder, pattern);
                
                foreach (var file in matchingFiles)
                {
                    var version = ExtractVersionFromPackageFile(file);
                    if (version != null && dependency.VersionRange.Satisfies(version))
                    {
                        return file;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error finding local dependency package: {PackageId}", ex, dependency.Id);
            }

            return string.Empty;
        }

        private string FindInGlobalCache(PackageDependency dependency)
        {
            try
            {
                var globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(_nugetSettings);
                if (string.IsNullOrEmpty(globalPackagesFolder) || !Directory.Exists(globalPackagesFolder))
                {
                    return string.Empty;
                }

                var packageFolder = Path.Combine(globalPackagesFolder, dependency.Id.ToLowerInvariant());
                if (!Directory.Exists(packageFolder))
                {
                    return string.Empty;
                }

                // Find the best matching version
                var versionDirectories = Directory.GetDirectories(packageFolder);
                NuGetVersion bestVersion = null;
                string bestPackagePath = string.Empty;

                foreach (var versionDir in versionDirectories)
                {
                    var versionString = Path.GetFileName(versionDir);
                    if (NuGetVersion.TryParse(versionString, out var version) && 
                        dependency.VersionRange.Satisfies(version))
                    {
                        if (bestVersion == null || version > bestVersion)
                        {
                            bestVersion = version;
                            var nupkgPath = Path.Combine(versionDir, $"{dependency.Id.ToLowerInvariant()}.{version}.nupkg");
                            if (File.Exists(nupkgPath))
                            {
                                bestPackagePath = nupkgPath;
                            }
                        }
                    }
                }

                return bestPackagePath;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error finding package in global cache: {PackageId}", ex, dependency.Id);
                return string.Empty;
            }
        }

        private string DownloadFromOnlineSources(PackageDependency dependency)
        {
            try
            {
                // This is a simplified implementation
                // In a production system, you would use NuGet's DownloadResource to download packages
                _logger.LogWarning("Online package download not fully implemented for: {PackageId}", dependency.Id);
                
                // For now, we'll just log the attempt
                // Future implementation would:
                // 1. Create SourceRepository instances from _packageSources
                // 2. Use FindPackageByIdResource to search for the package
                // 3. Use DownloadResource to download the package
                // 4. Cache it locally for future use
                
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error downloading package from online sources: {PackageId}", ex, dependency.Id);
                return string.Empty;
            }
        }

        private NuGetVersion ExtractVersionFromPackageFile(string packageFile)
        {
            try
            {
                using var packageReader = new PackageArchiveReader(packageFile);
                return packageReader.NuspecReader.GetVersion();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error extracting version from package file: {PackageFile}", ex, packageFile);
                return null;
            }
        }
    }
}
