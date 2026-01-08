using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Workspace;

namespace AppSage.Extension
{
    /// <summary>
    /// Context provided to extensions during initialization
    /// </summary>
    public interface IExtensionContext
    {
        IAppSageLogger Logger { get; }
        IAppSageConfiguration Configuration { get; }
        IAppSageWorkspace Workspace { get; }
        IServiceProvider ServiceProvider { get; }
    }
}
