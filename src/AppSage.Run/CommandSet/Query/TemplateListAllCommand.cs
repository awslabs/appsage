using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using AppSage.Run.CommandSet.Root;
using AppSage.Run.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;
using System.CommandLine;
using AppSage.Infrastructure.Template;
using AppSage.Core.Template;

namespace AppSage.Run.CommandSet.Query
{
    public sealed class TemplateListAllCommand : ISubCommandWithNoOptions
    {
 
        IServiceCollection _serviceCollection;
        public TemplateListAllCommand(IServiceCollection services)
        {
            _serviceCollection = services;
        }

        public string Name => "list-templates";
        public string Description => "List all the available qeury templates";

        public Command Build()
        {
           

            var cmd = new Command(this.Name, this.Description);
            cmd.Aliases.Add("lt");
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
            _serviceCollection.AddTransient<ITemplateManager, TemplateManager>();
            var provider= _serviceCollection.BuildServiceProvider();
            var logger= provider.GetRequiredService<IAppSageLogger>();
       

            var templateManager=provider.GetRequiredService<ITemplateManager>();


            var table = new Table();
            table.Border(TableBorder.Square);

            table.BorderColor(Color.Grey);
            table.AddColumn("[bold]ID[/]");
            table.AddColumn("[bold]Description[/]");
            table.ShowRowSeparators = true;

            var individualTemplates = templateManager.GetTemplateMetadata().Where(t=>t.TemplateType==TemplateType.SingleQuery).OrderBy(g=>g.TemplateId);

            foreach (var template in individualTemplates)
            {
                table.AddRow(new Markup(template.TemplateId), new Markup(template.Description));
            }

            // Render to ANSI and log via Serilog:
            var ansi = SpectreRender.ToAnsi(table);
            logger.LogInformation("\n{Table}", ansi);

            return 0;
        }


    }
}
