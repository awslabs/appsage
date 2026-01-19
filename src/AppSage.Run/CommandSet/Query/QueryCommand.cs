using AppSage.Core.Logging;
using AppSage.Run.CommandSet.MCP;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace AppSage.Run.CommandSet.Query
{

    public record QueryOptions
    {
        public Command TemplateCommand { get; set; }
    }
    public sealed class QueryCommand : ISubCommand<QueryOptions>
    {
        IServiceCollection _serviceCollection;
        IAppSageLogger _logger;
        public QueryCommand(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            ServiceProvider provider = serviceCollection.BuildServiceProvider();
            _logger = provider.GetService<IAppSageLogger>();
        }

        public string Name => "query";
        public string Description => "Allows you to directly query AppSage data. You can use a query templates with pre-defined set of analysis to run.";

        public Command Build()
        {

            var cmd = new Command(this.Name, this.Description);
            cmd.Aliases.Add("q");
            cmd.TreatUnmatchedTokensAsErrors = false;

            var subCommandRegistry = new List<ISubCommand>();
            subCommandRegistry.Add(new TemplateListGroupCommand(_serviceCollection));
            subCommandRegistry.Add(new TemplateListAllCommand(_serviceCollection));
            subCommandRegistry.Add(new QueryRunCommand(_serviceCollection));
            
            subCommandRegistry.ForEach(c =>
            {
                cmd.Add(c.Build());
            });

            cmd.SetAction(pr =>
            {
                var options = new QueryOptions();
                options.TemplateCommand = pr.CommandResult.Command;
                this.Execute(options);
            });
            return cmd;
        }
        public int Execute(QueryOptions opt)
        {
            var cmd = opt.TemplateCommand;
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
