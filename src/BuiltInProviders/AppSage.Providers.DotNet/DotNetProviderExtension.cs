using AppSage.Extension;

namespace AppSage.Providers.DotNet
{
    public class DotNetProviderExtension : IExtension
    {
        public string ExtensionId => "AppSage.Providers.DotNet";
        public string DisplayName => ".NET Code Analysis Provider";
        public string Version => "1.0.0";
        public string Description => "Comprehensive .NET code analysis and metrics collection extension";

        public Task InitializeAsync(IExtensionContext context)
        {
            context.Logger.LogInformation("Initializing .NET Provider Extension");
            return Task.CompletedTask;
        }

        public Task StartAsync()
        {
            // Extension startup logic here if needed
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            // Extension shutdown logic here if needed
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            // Clean up resources here if needed
            return Task.CompletedTask;
        }
    }
}