using AppSage.Core.Logging;
using AppSage.Core.Configuration;
using AppSage.Core.Workspace;

namespace AppSage.Extension
{
    /// <summary>
    /// Base interface for all AppSage extensions
    /// </summary>
    public interface IExtension
    {
        /// <summary>
        /// Unique identifier for this extension
        /// </summary>
        string ExtensionId { get; }

        /// <summary>
        /// Display name for this extension
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Version of this extension
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Description of what this extension does
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Initialize the extension with required services
        /// </summary>
        Task InitializeAsync(IExtensionContext context);

        /// <summary>
        /// Start the extension
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Stop the extension gracefully
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// Dispose resources used by the extension
        /// </summary>
        Task DisposeAsync();
    }

    /// <summary>
    /// Context provided to extensions during initialization
    /// </summary>
    public interface IExtensionContext
    {
        IAppSageLogger Logger { get; }
        IAppSageConfiguration Configuration { get; }
        IAppSageWorkspace Workspace { get; }
        IServiceProvider ServiceProvider { get; }
        ExtensionManifest? Manifest { get; }
    }
}