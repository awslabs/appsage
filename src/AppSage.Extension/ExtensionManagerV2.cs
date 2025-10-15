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

namespace AppSage.Extension
{
    internal class ExtensionManagerV2:IExtensionManagerV2
    {
        private readonly IAppSageLogger _logger;
        private readonly IAppSageWorkspace _workspace;
        private readonly NuGetFramework _targetFramework;
        private readonly ISettings _nugetSettings;

        public ExtensionManagerV2(IAppSageLogger logger,IAppSageWorkspace workspace)
        {
            _logger = logger;
            _workspace = workspace;
            _targetFramework = NuGetFramework.ParseFolder("net8.0");
            
            // Initialize NuGet settings with global cache support
            _nugetSettings = Settings.LoadDefaultSettings(root: null);
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

                // Create installation directory
                string versionString = version.ToString();
                string installFolder = Path.Combine(_workspace.ExtensionInstallFolder, resolvedPackageId, versionString);
                
                if (Directory.Exists(installFolder) && !force)
                {
                    _logger.LogInformation("Package {PackageId} version {Version} is already installed. Use Force to force install it", resolvedPackageId, versionString);
                    return true;
                }else if (Directory.Exists(installFolder) && force)
                {
                    _logger.LogInformation("Force installing package {PackageId} version {Version}", resolvedPackageId, versionString);
                    Directory.Delete(installFolder, true);
                }

                Directory.CreateDirectory(installFolder);
                _logger.LogInformation("Created installation folder: {InstallFolder}", installFolder);

                // Extract the main package
                ExtractPackage(resolvedPackage, installFolder);
                _logger.LogInformation("Extracted main package to: {InstallFolder}", installFolder);

                // Resolve and install dependencies
                var dependencies = ResolveDependencies(resolvedPackage);
                foreach (var dependency in dependencies)
                {
                    if(dependency.Id=="AppSage.Core" || dependency.Id=="AppSage.Extension")
                    {
                        // Skip core dependencies
                        continue;
                    }
                    InstallDependency(dependency);
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

        private void ExtractPackage(string packageFile, string extractPath)
        {
            using var packageReader = new PackageArchiveReader(packageFile);
            
            // Extract all files
            var files = packageReader.GetFiles();
            
            foreach (var file in files)
            {
                var destinationPath = Path.Combine(extractPath, file);
                var destinationDir = Path.GetDirectoryName(destinationPath);
                
                if (!string.IsNullOrEmpty(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }

                using var fileStream = packageReader.GetStream(file);
                using var outputStream = File.Create(destinationPath);
                fileStream.CopyTo(outputStream);
            }
        }

        private List<PackageDependency> ResolveDependencies(string packageFile)
        {
            var dependencies = new List<PackageDependency>();

            try
            {
                using var packageReader = new PackageArchiveReader(packageFile);
                var nuspecReader = packageReader.NuspecReader;
                
                // Get dependency groups for the target framework
                var dependencyGroups = nuspecReader.GetDependencyGroups();
                var applicableGroup = NuGetFrameworkUtility.GetNearest(
                    dependencyGroups,
                    _targetFramework,
                    group => group.TargetFramework);

                if (applicableGroup != null)
                {
                    dependencies.AddRange(applicableGroup.Packages);
                }

                _logger.LogInformation("Found {Count} dependencies for target framework {Framework}", 
                    dependencies.Count, _targetFramework.GetShortFolderName());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error resolving dependencies for package: {PackageFile}", ex, packageFile);
            }

            return dependencies;
        }

        private void InstallDependency(PackageDependency dependency)
        {
            try
            {
                _logger.LogInformation("Installing dependency: {PackageId} {VersionRange}", 
                    dependency.Id, dependency.VersionRange);

                // Check if dependency is already installed
                var dependencyInstallPath = Path.Combine(_workspace.ExtensionInstallFolder, dependency.Id);
                if (Directory.Exists(dependencyInstallPath))
                {
                    // Check if any installed version satisfies the requirement
                    var installedVersions = Directory.GetDirectories(dependencyInstallPath)
                        .Select(d => Path.GetFileName(d))
                        .Where(v => NuGetVersion.TryParse(v, out _))
                        .Select(v => NuGetVersion.Parse(v))
                        .ToList();

                    if (installedVersions.Any(v => dependency.VersionRange.Satisfies(v)))
                    {
                        _logger.LogInformation("Dependency {PackageId} is already satisfied", dependency.Id);
                        return;
                    }
                }

                // Try to find in local packages first
                var localPackage = FindLocalDependencyPackage(dependency);
                if (!string.IsNullOrEmpty(localPackage))
                {
                    var version = ExtractVersionFromPackageFile(localPackage);
                    var installFolder = Path.Combine(_workspace.ExtensionInstallFolder, dependency.Id, version.ToString());
                    
                    if (!Directory.Exists(installFolder))
                    {
                        Directory.CreateDirectory(installFolder);
                        ExtractPackage(localPackage, installFolder);
                        _logger.LogInformation("Installed local dependency: {PackageId} {Version}", dependency.Id, version);
                    }
                    return;
                }

                // Try to find in global NuGet cache
                var globalPackage = FindInGlobalCache(dependency);
                if (!string.IsNullOrEmpty(globalPackage))
                {
                    var version = ExtractVersionFromPackageFile(globalPackage);
                    var installFolder = Path.Combine(_workspace.ExtensionInstallFolder, dependency.Id, version.ToString());
                    
                    if (!Directory.Exists(installFolder))
                    {
                        Directory.CreateDirectory(installFolder);
                        ExtractPackage(globalPackage, installFolder);
                        _logger.LogInformation("Installed global cache dependency: {PackageId} {Version}", dependency.Id, version);
                    }
                    return;
                }

                _logger.LogWarning("Could not resolve dependency: {PackageId} {VersionRange}", 
                    dependency.Id, dependency.VersionRange);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error installing dependency: {PackageId}", ex, dependency.Id);
            }
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
