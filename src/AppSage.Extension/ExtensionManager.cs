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

        private readonly IAppSageConfiguration _configuration;
        private readonly IAppSageWorkspace _workspace;
        private readonly IServiceProvider _serviceProvider;

        public ExtensionManager(
            IAppSageLogger logger, 

            IAppSageConfiguration configuration,
            IAppSageWorkspace workspace,
            IServiceProvider serviceProvider,
            string extensionDirectory)
        {
            _logger = logger;
            _configuration = configuration;
            _workspace = workspace;
            _serviceProvider = serviceProvider;
        }

    }



}