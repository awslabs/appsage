using AppSage.Extension;

namespace AppSage.Providers.HelloWorld
{
    public class HelloWorldExtension : IExtension
    {
        public string ExtensionId => this.GetType().FullName;
        public string DisplayName => "AppSage Hello World Extension";
        public string Version => "1.0.0";
        public string Description => "A simple AppSage extension that demonstrates basic metrics collection with minimal dependencies";

    }
}