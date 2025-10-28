using AppSage.Core.Logging;
using AppSage.Infrastructure.Workspace;
using AppSage.Run.CommandSet.Root;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Reflection;
namespace AppSage.Run.CommandSet.Version
{
    public sealed class VersionCommnad : ISubCommand
    {
        IAppSageLogger _logger;
 
        public VersionCommnad(IServiceCollection serviceCollection)
        {
            ServiceProvider provider = serviceCollection.BuildServiceProvider();
            _logger = provider.GetService<IAppSageLogger>();
        }
        

        public string Name => "version";
        public string Description => "Show the current version of AppSage";

        public Command Build()
        {

            var cmd = new Command(this.Name, this.Description);
            cmd.SetAction(pr =>
            {

                return this.Execute();

            });

            return cmd;
        }
        public int Execute()
        {
            var meta = Assembly.GetExecutingAssembly().GetCustomAttributes<AssemblyMetadataAttribute>()
    .FirstOrDefault(a => string.Equals(a.Key, "BuildTimestamp", StringComparison.Ordinal));
            if (meta != null)
            {
                _logger.LogInformation("AppSage Build Timestamp: {BuildTimestamp}", meta.Value);
            }
            else
            {
                _logger.LogWarning("Build timestamp metadata not found.");
            }
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            _logger.LogInformation("AppSage Version: {Version}", version?.ToString() ?? "Unknown");
            return 0;
        }


    }
}
