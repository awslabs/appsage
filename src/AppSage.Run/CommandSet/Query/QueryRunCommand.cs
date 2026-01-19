using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Query;
using AppSage.Core.Template;
using AppSage.Infrastructure.Metric;
using AppSage.Infrastructure.Query;
using AppSage.Infrastructure.Template;
using AppSage.MCPServer;
using AppSage.Query;
using AppSage.Run.CommandSet.Root;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace AppSage.Run.CommandSet.Query
{
    public record QueryRunOptions
    {
        public string TemplateId { get; set; } = string.Empty;
    }
    public sealed class QueryRunCommand : ISubCommand<QueryRunOptions>
    {
        IServiceCollection _services;
        public QueryRunCommand(IServiceCollection services)
        {
            _services = services;
        }

        public string Name => "run";
        public string Description => "Run the set of AppSage query templates and create the analysis";

        public Command Build()
        {
            var cmd = new Command(this.Name, this.Description);
            cmd.Aliases.Add("r");
            var argWorkspaceFolder = AppSageRootCommand.GetWorkspaceArgument();
            cmd.Add(argWorkspaceFolder);

            var templateId = new Option<string>(name: "--id", aliases: new string[] { "-i" });
            templateId.Description = $"""
                Template Id to run. If no id is provided all templates will be run.
                You can specify a group id or a single template id.
                """;
            cmd.Add(templateId);



            cmd.SetAction(pr =>
            {
                var id = pr.GetValue(templateId);
                QueryRunOptions options = new QueryRunOptions();
                options.TemplateId = id ;
                return this.Execute(options);
            });
            return cmd;
        }
        public int Execute(QueryRunOptions opt)
        {
            _services.AddTransient<ITemplateManager, TemplateManager>();
            _services.AddTransient<ITemplateQuery, TemplateQueryManager>();
            _services.AddTransient<IDynamicCompiler, DynamicCompiler>();
            _services.AddTransient<IMetricReader, MetricReader>();

            var serviceProvider = _services.BuildServiceProvider();
            var queryManager = serviceProvider.GetRequiredService<ITemplateQuery>();
            queryManager.Run(opt.TemplateId);

            return 0;
        }
    }
}
