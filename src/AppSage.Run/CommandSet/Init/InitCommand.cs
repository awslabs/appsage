using AppSage.Core.Logging;
using AppSage.Infrastructure.Workspace;
using AppSage.Run.CommandSet.Root;
using System.CommandLine;
namespace AppSage.Run.CommandSet.Init
{
    public record InitOptions
    {
        public string WorkspaceFolder { get; set; }
    }
    public sealed class InitCommand : ISubCommand<InitOptions>
    {
        IAppSageLogger _logger;
        public InitCommand( IAppSageLogger logger)
        {
            _logger = logger;
        }

        public string Name => "init";
        public string Description => "Create an initialize an AppSage workspace folder";

        public Command Build()
        {

            var cmd = new Command(this.Name, this.Description);

            var argWorkspaceFolder=AppSageRootCommand.GetWorkspaceArgument();
            cmd.Add(argWorkspaceFolder);


            cmd.SetAction(pr =>
            {
                InitOptions options = new InitOptions()
                {
                    WorkspaceFolder = pr.GetValue<string>(argWorkspaceFolder),
                };
                return this.Execute(options);

            });

            return cmd;
        }
        public int Execute(InitOptions opt)
        {
            string resultPath = string.Empty;

            DirectoryInfo di = null;
            if (!string.IsNullOrWhiteSpace(opt.WorkspaceFolder))
            {
                _logger.LogInformation($"Using the specified workspace folder [{opt.WorkspaceFolder}]");
                di = new DirectoryInfo(opt.WorkspaceFolder);
            }
            else
            {
                _logger.LogInformation($"No workspace folder specified. Using the current working directory [{Environment.CurrentDirectory}] as the workspace folder.");
                di = new DirectoryInfo(Environment.CurrentDirectory);
            }

            return AppSageWorkspaceManager.Initialize(di, _logger);
        }


    }
}
