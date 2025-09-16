using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Infrastructure;
using AppSage.Infrastructure.Workspace;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Security.Cryptography;
namespace AppSage.Run.CommandSet.Init
{
    public record InitOptions
    {
        public string WorkspaceFolder { get; set; }
        public bool UpdateGlobalconfig { get; set; } = false;
    }
    public sealed class InitCommand : ISubCommand<InitOptions>
    {
        IAppSageConfiguration _config;
        IAppSageLogger _logger;
        public InitCommand(IAppSageConfiguration config, IAppSageLogger logger)
        {
            _config = config;
            _logger = logger;
        }

        public string Name => "init";
        public string Description => "Create an initialize an AppSage workspace folder";

        public Command Build()
        {

            var cmd = new Command(this.Name, this.Description);

            var argWorkspaceFolder = new Option<string>(
                name: "--workspace-folder",
                aliases: new string[] { "-ws" }
            );
            argWorkspaceFolder.Description = $"The workspace folder path to initialize. This folder should be non existing or if exists should be empty.  If not specified, defaults to the current working directory. Which is as of now is [{Environment.CurrentDirectory}].";

            var argUpdateGlobalConfig = new Option<bool>(
                name: "--update-global-config"
            );
            argUpdateGlobalConfig.Description = "If specified, updates the AppSage's configuration file to set the workspace folder to the specified or default value. If not specified, the global configuration file is not updated.";

            cmd.Add(argWorkspaceFolder);
            cmd.Add(argUpdateGlobalConfig);
            cmd.SetAction(pr =>
            {
                InitOptions options = new InitOptions()
                {
                    WorkspaceFolder = pr.GetValue<string>(argWorkspaceFolder),
                    UpdateGlobalconfig = pr.GetValue<bool>(argUpdateGlobalConfig)
                };
                return this.Execute(options);

            });

            return cmd;
        }
        public int Execute(InitOptions opt)
        {
            // Ensure WorkspaceFolder has a value, defaulting to configuration value and if not to current directory if null/empty
            string resultPath = string.Empty;

            DirectoryInfo di = null;
            if (!string.IsNullOrWhiteSpace(opt.WorkspaceFolder))
            {
                _logger.LogInformation($"Using the specified workspace folder [{opt.WorkspaceFolder}]");
                di = new DirectoryInfo(opt.WorkspaceFolder);
            }
            else
            {
                _logger.LogInformation($"No workspace folder specified. Using the current working directory as the workspace folder.");
                di = new DirectoryInfo(Environment.CurrentDirectory);
            }


            if (di.Exists && (di.EnumerateDirectories().Any() || di.GetFiles().Any()))
            {
                _logger.LogError($"The specified workspace folder [{di.FullName}] already exists and is not empty. Please specify a non-existing or empty folder.");
                return -1;
            }
            else
            {
                AppSageWorkspaceManager wsManager = new AppSageWorkspaceManager(di.FullName, _logger);
                wsManager.Initialize();
                return 0;
            }
        }


    }
}
