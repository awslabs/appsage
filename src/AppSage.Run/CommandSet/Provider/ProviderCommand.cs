using AppSage.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace AppSage.Run.CommandSet.Provider
{
    public record ProviderOptions
    {
        public Command ProviderCommand { get; set; }
    }
    public sealed class ProviderCommand : ISubCommand<ProviderOptions>
    {
        IServiceCollection _serviceCollection;
        IAppSageLogger _logger;
        public ProviderCommand(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
            ServiceProvider provider = serviceCollection.BuildServiceProvider();
            _logger = provider.GetService<IAppSageLogger>();
        }

        public string Name => "provider";
        public string Description => "Perform provider related tasks";

        public Command Build()
        {

            var cmd = new Command(this.Name, this.Description);
            cmd.TreatUnmatchedTokensAsErrors = false;

            var subCommandRegistry = new List<ISubCommand>();
            subCommandRegistry.Add(new ProviderListCommand(_serviceCollection));
            subCommandRegistry.Add(new ProviderInstallCommand(_serviceCollection));
            subCommandRegistry.Add(new ProviderUninstallCommand(_serviceCollection));
            subCommandRegistry.Add(new ProviderRunCommand(_serviceCollection));

            subCommandRegistry.ForEach(c =>
            {
                cmd.Add(c.Build());
            });

            cmd.SetAction(pr =>
            {
                var cmd = pr.CommandResult.Command;
                cmd.SetAction(pr =>
                {
                    ProviderOptions options = new ProviderOptions();
                    options.ProviderCommand = pr.CommandResult.Command;
                    this.Execute(options);
                });
            });
            return cmd;
        }
        public int Execute(ProviderOptions opt)
        {
             var cmd = opt.ProviderCommand;
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
