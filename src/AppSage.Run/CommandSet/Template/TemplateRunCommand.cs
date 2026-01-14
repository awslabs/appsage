using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.MCPServer;
using AppSage.Run.CommandSet.Root;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace AppSage.Run.CommandSet.Template
{
    public sealed class TemplateRunCommand : ISubCommand<MCPServerRunOptions>
    {
        IServiceCollection _services;
        public TemplateRunCommand(IServiceCollection services)
        {
            _services = services;
        }

        public string Name => "run";
        public string Description => "Run the set of AppSage providers";

        public Command Build()
        {
            var cmd = new Command(this.Name, this.Description);
            var argWorkspaceFolder = AppSageRootCommand.GetWorkspaceArgument();
            cmd.Add(argWorkspaceFolder);
            cmd.SetAction(pr =>
            {
                MCPServerRunOptions options = new MCPServerRunOptions();
                return this.Execute(options);
            });
            return cmd;
        }
        public int Execute(MCPServerRunOptions opt)
        {
            var builder = WebApplication.CreateBuilder();
            //add prebuild services to the builder
            foreach (var svc in _services)
            {
                builder.Services.Add(svc);
            }
            Runner mcpServerRunner = new Runner(builder.Services);
            mcpServerRunner.Run(builder, opt).GetAwaiter().GetResult();
            return 0;
        }
    }
}
