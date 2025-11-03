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

        Dictionary<string, string>? GetProviderDescriptions { get; } 
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