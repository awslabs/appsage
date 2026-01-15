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

namespace AppSage.Run.CommandSet.Template
{
    public sealed class TemplateListGroupCommand : ISubCommandWithNoOptions
    {
 
        IServiceCollection _serviceCollection;
        public TemplateListGroupCommand(IServiceCollection services)
        {
            _serviceCollection = services;
        }

        public string Name => "list-groups";
        public string Description => "List the available template groups";

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

            var groups = templateManager.GetTemplateMetadata().Where(t=>t.TemplateType==TemplateType.GroupQuery).OrderBy(g=>g.TemplateId);

            foreach (var template in groups)
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
