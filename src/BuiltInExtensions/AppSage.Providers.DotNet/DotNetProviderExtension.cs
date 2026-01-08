using AppSage.Extension;

namespace AppSage.Providers.DotNet
{
    public class DotNetProviderExtension : IExtension
    {
        public string ExtensionId => "AppSage.Providers.DotNet";
        public string DisplayName => ".NET Code Analysis Provider";
        public string Version => "1.0.0";
        public string Description => "Comprehensive .NET code analysis and metrics collection extension";

  
    }
}