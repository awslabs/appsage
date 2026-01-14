using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using AppSage.Run.CommandSet.Root;
using AppSage.Run.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;
using System.CommandLine;

namespace AppSage.Run.CommandSet.Template
{
    public sealed class TemplateListCommand : ISubCommandWithNoOptions
    {
 
        private readonly IAppSageLogger _logger;
        private IServiceCollection _services; 
        public TemplateListCommand(IServiceCollection services)
        {
            _services = services;

            ServiceProvider provider = services.BuildServiceProvider();
       
            

        }

        public string Name => "list";
        public string Description => "List the availble extensions";

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

            var table = new Table();
            table.Border(TableBorder.Square);

            table.BorderColor(Color.Grey);
            table.AddColumn("[bold]ID[/]");
            table.AddColumn("[bold]Description[/]");
            table.ShowRowSeparators = true;

        

            // Render to ANSI and log via Serilog:
            var ansi = SpectreRender.ToAnsi(table);
            _logger.LogInformation("\n{Table}", ansi);

            return 0;
        }


    }
}
