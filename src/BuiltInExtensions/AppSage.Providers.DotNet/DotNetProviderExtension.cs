using AppSage.Extension;
using AppSage.Providers.DotNet.DependencyAnalysis;

namespace AppSage.Providers.DotNet
{
    public class DotNetProviderExtension : IExtension
    {
        public string ExtensionId => "AppSage.Providers.DotNet";
        public string DisplayName => ".NET Code Analysis Provider";
        public string Version => "1.0.0";
        public string Description => "Provides .NET related code analysis including code statistics and dependencies.";

        public Dictionary<string, string>? GetProviderDescriptions
        {
            get
            {
                Dictionary<string, string> descriptions = new Dictionary<string, string>();
                //descriptions.Add(typeof(DotNetDependencyAnalysisProvider).FullName!, "Analyzes .NET project dependencies and generates a report.");)
                return descriptions;
            }
        }
    }
}