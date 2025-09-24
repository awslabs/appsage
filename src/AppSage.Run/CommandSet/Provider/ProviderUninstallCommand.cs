using AppSage.Core.Logging;
using AppSage.Run.CommandSet.Root;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace AppSage.Run.CommandSet.Provider
{

    public sealed class ProviderUninstallCommand : ISubCommandWithNoOptions
    {
        private readonly IAppSageLogger _logger;
        public ProviderUninstallCommand(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            _logger = serviceProvider.GetRequiredService<IAppSageLogger>();
        }

        public string Name => "uninstall";
        public string Description => "uninstall providers";

        public Command Build()
        {
            var cmd = new Command(this.Name, this.Description);
            var argWorkspaceFolder = AppSageRootCommand.GetWorkspaceArgument();
            cmd.Add(argWorkspaceFolder);
            cmd.SetAction(pr =>
            {
                return this.Execute();
            });
            return cmd;
        }
        public int Execute()
        {
            _logger.LogInformation("Nothing happens. Uninstallation of providers is not yet implemented.");
            return 0;
        }
    }
}
