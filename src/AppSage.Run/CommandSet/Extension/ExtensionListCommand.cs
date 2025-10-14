using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Run.CommandSet.Root;
using AppSage.Run.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;
using System.CommandLine;

namespace AppSage.Run.CommandSet.Extension
{

    public sealed class ExtensionListCommand : ISubCommandWithNoOptions
    {
        private IMetricProvider[] _providers;
        private readonly IAppSageLogger _logger;
        public ExtensionListCommand(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            ExtensionRegistry.RegisterProviders(services);
            //tentatively registering all metric providers they will be added later based on configuration
            services.AddTransient<IMetricProvider[]>(sp => sp.GetServices<IMetricProvider>().ToArray());
            // Register the main runner service that will execute all providers
            serviceProvider = services.BuildServiceProvider();
            _providers= serviceProvider.GetRequiredService<IMetricProvider[]>();
            _logger = serviceProvider.GetRequiredService<IAppSageLogger>();
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
            table.AddColumn("[bold]Description[/]") ;
            table.ShowRowSeparators = true;

            _providers.ToList().ForEach(p=>
            {
                table.AddRow(p.FullQualifiedName, p.Description);
            });

            // Render to ANSI and log via Serilog:
            var ansi = SpectreRender.ToAnsi(table);
            _logger.LogInformation("\n{Table}", ansi);

            return 0;
        }


    }
}
