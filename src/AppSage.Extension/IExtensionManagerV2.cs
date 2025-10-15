using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.Extension
{
    internal interface IExtensionManagerV2
    {
        bool InstallExtension(string packageId);
    }
}
