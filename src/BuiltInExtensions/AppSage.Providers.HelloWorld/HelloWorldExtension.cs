using AppSage.Extension;

namespace AppSage.Providers.HelloWorld
{
    public class HelloWorldExtension : IExtension
    {
        public string ExtensionId => this.GetType().FullName;
        public string DisplayName => "AppSage Hello World Extension";
        public string Version => "1.0.0";
        public string Description => "A simple AppSage extension that demonstrates basic metrics collection with minimal dependencies";

        public Task InitializeAsync(IExtensionContext context)
        {
            context.Logger.LogInformation("Initializing Hello World Extension");
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