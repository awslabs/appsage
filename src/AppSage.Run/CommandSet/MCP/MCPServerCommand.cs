using AppSage.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace AppSage.Run.CommandSet.MCP
{
    public record MCPServerOptions
    {
        public Command MCPCommand { get; set; }
    }
    public sealed class MCPServerCommand : ISubCommand<MCPServerOptions>
    {
        IServiceCollection _serviceCollection;
        IAppSageLogger _logger;
        public MCPServerCommand(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            ServiceProvider provider = serviceCollection.BuildServiceProvider();
            _logger = provider.GetService<IAppSageLogger>();
        }

        public string Name => "mcpserver";
        public string Description => "AppSage support querying it's data using MCP (Model Context Protocol). THis perform MCP server  related tasks such as starting it.";

        public Command Build()
        {

            var cmd = new Command(this.Name, this.Description);
            cmd.TreatUnmatchedTokensAsErrors = false;

            var subCommandRegistry = new List<ISubCommand>();
            subCommandRegistry.Add(new MCPServerListCommand(_serviceCollection));
            subCommandRegistry.Add(new MCPServerRunCommand(_serviceCollection));

            subCommandRegistry.ForEach(c =>
            {
                cmd.Add(c.Build());
            });

            cmd.SetAction(pr =>
            {
                var cmd = pr.CommandResult.Command;
                cmd.SetAction(pr =>
                {
                    MCPServerOptions options = new MCPServerOptions();
                    options.MCPCommand = pr.CommandResult.Command;
                    this.Execute(options);
                });
            });
            return cmd;
        }
        public int Execute(MCPServerOptions opt)
        {
             var cmd = opt.MCPCommand;
            _logger.LogInformation("Select the sub command of '{CommandName}'", cmd.Name);
            string message = $"""
                    {cmd.Name} : {cmd.Description}
                    No sub command specified for '{cmd.Name}'
                    Select the sub command of '{cmd.Name}'
                    Use '--help' to see available options.
                    """;
            _logger.LogInformation("{Message}", message);
            cmd.Children.ToList().ForEach(c =>
            {
                if (c is Command subCmd)
                {
                    _logger.LogInformation("\t{Name} : {Description}", subCmd.Name, subCmd.Description);
                }
            });

            return 0;
        }


    }
}
