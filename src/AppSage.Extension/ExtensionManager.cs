using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.Extension
{
    public class ExtensionManager : IExtensionManager
    {
        public IEnumerable<IExtension> GetExtensions()
        {
            throw new NotImplementedException();
        }

        public bool InstallExtension(string extensionId)
        {
            throw new NotImplementedException();
        }

        public bool UninstallExtension(string extensionId)
        {
            throw new NotImplementedException();
        }
    }
}
