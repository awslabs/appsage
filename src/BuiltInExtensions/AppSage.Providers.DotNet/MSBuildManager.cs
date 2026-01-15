using AppSage.Core.Logging;
using Microsoft.Build.Locator;

namespace AppSage.Providers.DotNet
{
    internal class MSBuildManager
    {
        private static readonly object _lock = new object();

        /// <summary>
        /// Initializes MSBuild environment with automatic discovery or user-provided path.
        /// This method is thread-safe and should be called before any MSBuildWorkspace operations.
        /// </summary>
        /// <param name="userProvidedMSBuildPath">Optional MSBuild path from configuration. If null or empty, auto-discovery is used.</param>
        /// <param name="logger">Logger for diagnostic messages.</param>
        public static void InitializeMSBuild(string? userProvidedMSBuildPath, IAppSageLogger? logger = null)
        {
            lock (_lock)
            {
                if (MSBuildLocator.IsRegistered)
                {
                    logger?.LogDebug("MSBuild is already registered. Skipping initialization.");
                    return;
                }

                try
                {
                    // Check if user provided a custom path
                    if (!string.IsNullOrWhiteSpace(userProvidedMSBuildPath) && Directory.Exists(userProvidedMSBuildPath))
                    {
                        logger?.LogInformation("Using configured MSBuild path: {MSBuildPath}", userProvidedMSBuildPath);
                        MSBuildLocator.RegisterMSBuildPath(userProvidedMSBuildPath);
                    }
                    else
                    {
                        // Auto-discover MSBuild
                        logger?.LogInformation("Auto-discovering MSBuild installation...");
                        var instances = MSBuildLocator.QueryVisualStudioInstances()
                            .OrderByDescending(i => i.Version)
                            .ToList();

                        if (instances.Count == 0)
                        {
                            var errorMessage = """
                                No MSBuild installation found. AppSage needs MSBuild to do Rosylyn compiler based code analysis.  
                                You can fix this in two ways. 
                                1) Install Visual Studio
                                The easiest way to fix this issue is to install Visual Studio latest version. 
                                Visual Studio Community 2022 or 2026 edition will work. AppSage will then automatically detect the MSBuild installation.
                                2) Manually configure MSBuild path
                                Alternatively you can set the AppSage configuration parameter 'AppSage.Providers.DotNet.SHARED->MSBuildPath' to point to an existing MSBuild installation.
                                The configuraiton file is available in a hidden AppSage workspace folder under [AppSage workspace folder]\.AppSageConfig\AppSageConfig.json
                                For exmple when you install Visual Studio 2022 the MSBuild path is: 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin'
                                """;
                                
                            if (!string.IsNullOrWhiteSpace(userProvidedMSBuildPath))
                            {
                                errorMessage += $" The configured path '{userProvidedMSBuildPath}' does not exist.";
                            }
                            logger?.LogError(errorMessage);
                            throw new InvalidOperationException(errorMessage);
                        }

                        var selectedInstance = instances.First();
                        logger?.LogInformation(
                            "Found MSBuild: {MSBuildName} {MSBuildVersion} at {MSBuildPath}",
                            selectedInstance.Name,
                            selectedInstance.Version,
                            selectedInstance.MSBuildPath
                        );

                        MSBuildLocator.RegisterInstance(selectedInstance);
                    }

                    logger?.LogInformation("MSBuild successfully registered.");
                }
                catch (Exception ex)
                {
                    logger?.LogError("Failed to initialize MSBuild: {ErrorMessage}", ex.Message);
                    throw;
                }
            }
        }
    }
}
