using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Run.CommandSet.Root;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace AppSage.Run.CommandSet.MCP
{

    public sealed class MCPServerListCommand : ISubCommandWithNoOptions
    {
        private IMetricProvider[] _providers;
        private readonly IAppSageLogger _logger;
        public MCPServerListCommand(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();

            _providers= serviceProvider.GetRequiredService<IMetricProvider[]>();
            _logger = serviceProvider.GetRequiredService<IAppSageLogger>();
        }

        public string Name => "list";
        public string Description => "List the availble mcp capabilities";

        public Command Build()
        {
            var cmd = new Command(this.Name, this.Description);
            cmd.Aliases.Add("ls");

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
            _logger.LogInformation("Not implemented yet.");
             

            return 0;
        }


    }
}
