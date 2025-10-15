using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Workspace;
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

        public ExtensionManagerV2(IAppSageLogger logger,IAppSageWorkspace workspace)
        {
            _logger = logger;
            _workspace = workspace;
        }

        public bool InstallExtension(string packageId)
        {
 

            string resolvedPacakge=Directory.GetFiles(_workspace.ExtensionPackagesFolder, $"{packagId}.*.nupkg").FirstOrDefault()??string.Empty;
 
            string installFolder= Path.Combine(_workspace.ExtensionInstallFolder, packageId);
            throw new NotImplementedException();
        }
    }
}
