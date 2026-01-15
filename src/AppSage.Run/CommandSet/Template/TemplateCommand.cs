using AppSage.Core.Logging;
using AppSage.Run.CommandSet.MCP;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace AppSage.Run.CommandSet.Template
{

    public record TemplateOptions
    {
        public Command TemplateCommand { get; set; }
    }
    public sealed class TemplateCommand : ISubCommand<TemplateOptions>
    {
        IServiceCollection _serviceCollection;
        IAppSageLogger _logger;
        public TemplateCommand(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            ServiceProvider provider = serviceCollection.BuildServiceProvider();
            _logger = provider.GetService<IAppSageLogger>();
        }

        public string Name => "template";
        public string Description => "Perform  template related tasks. Templates are pre-defined set of analysis you like to run on AppSage.";

        public Command Build()
        {

            var cmd = new Command(this.Name, this.Description);
            cmd.TreatUnmatchedTokensAsErrors = false;

            var subCommandRegistry = new List<ISubCommand>();
            subCommandRegistry.Add(new TemplateListGroupCommand(_serviceCollection));
            subCommandRegistry.Add(new TemplateListAllCommand(_serviceCollection));
            subCommandRegistry.Add(new TemplateRunCommand(_serviceCollection));
            
            subCommandRegistry.ForEach(c =>
            {
                cmd.Add(c.Build());
            });

            cmd.SetAction(pr =>
            {
                var cmd = pr.CommandResult.Command;
                cmd.SetAction(pr =>
                {
                    var options = new TemplateOptions();
                    options.TemplateCommand = pr.CommandResult.Command;
                    this.Execute(options);
                });
            });
            return cmd;
        }
        public int Execute(TemplateOptions opt)
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
