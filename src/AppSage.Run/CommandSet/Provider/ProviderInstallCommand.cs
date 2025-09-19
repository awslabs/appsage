using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Run.CommandSet.Root;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.Run.CommandSet.Provider
{
    public sealed class ProviderInstallCommand : ISubCommandWithNoOptions
    {
        private readonly IAppSageLogger _logger;
        public ProviderInstallCommand(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            _logger = serviceProvider.GetRequiredService<IAppSageLogger>();
        }

        public string Name => "install";
        public string Description => "install providers";

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
            _logger.LogInformation($"Nothing happens. Installation of providers is not yet implemented.");
            return 0;
        }
    }
}
