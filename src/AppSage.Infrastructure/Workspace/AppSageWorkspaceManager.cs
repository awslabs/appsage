using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Workspace;

namespace AppSage.Infrastructure.Workspace
{
    public class AppSageWorkspaceManager : IAppSageWorkspace
    {
 
        private const string _APPSAGE_CONFIG_FILENAME = "AppSageConfig.json";

        private readonly string _rootFolder;
        private readonly IAppSageLogger _logger;
        public AppSageWorkspaceManager(string rootFolder, IAppSageLogger logger)
        {
            _rootFolder = rootFolder;
            _logger = logger;
        }
        public string RootFolder => _rootFolder;




        public int Initialize()
        {
            DirectoryInfo di = new DirectoryInfo(_rootFolder);
            
            string messagePrefix = "Creating";
            
            if (di.Exists && !IsExistingAppSageWorkspace() && (di.EnumerateDirectories().Any() || di.GetFiles().Any()))
            {
                _logger.LogError($"The specified workspace folder [{di.FullName}] already exists and is not empty. Please specify a non-existing or empty folder.");
                return -1;
            }
            else if(IsExistingAppSageWorkspace())
            {
                _logger.LogInformation($"The specified workspace folder [{di.FullName}] is already an AppSage workspace. We will repair it, if it is broken.");
                messagePrefix = "Repairing";
            }

            _logger.LogInformation("Initializing workspace");

            try
            {
                IAppSageWorkspace ws = this as IAppSageWorkspace;

                _logger.LogInformation($"{messagePrefix} [{ws.RootFolder}]. This is the root of your workspace");
                Directory.CreateDirectory(RootFolder);
                _logger.LogInformation($"{messagePrefix} [{ws.RepositoryFolder}]. This is where you put all your code repositories. You have to keep seperate folders for each repository.");
                Directory.CreateDirectory(ws.RepositoryFolder);
                _logger.LogInformation($"{messagePrefix} [{ws.DatabaseSchemaFolder}]. This is where you keep your database schemas. You have to keep seperate folders for each database.");
                Directory.CreateDirectory(ws.DatabaseSchemaFolder);

                _logger.LogInformation($"{messagePrefix} [{ws.ProviderOutputFolder}]. This is where all tooling output after code scan will be saved.");
                Directory.CreateDirectory(ws.ProviderOutputFolder);

                _logger.LogInformation($"{messagePrefix} [{ws.MCPServerOutputFolder}]. If a query to AppSage MCP server generates a file/files, this is where those files will be saved.");
                Directory.CreateDirectory(ws.MCPServerOutputFolder);

                _logger.LogInformation($"{messagePrefix} [{ws.LogsFolder}]. This is where all logs will be saved.");
                Directory.CreateDirectory(ws.LogsFolder);

                _logger.LogInformation($"{messagePrefix} [{ws.ProviderFolder}]. This is where all provider plugins should be placed. One folder for each plugin.");
                Directory.CreateDirectory(ws.ProviderFolder);


                _logger.LogInformation($"{messagePrefix} hidden AppSage config folder folder [{ws.AppSageConfigFolder}]. Used by AppSage to identify configuration.");
                Directory.CreateDirectory(ws.AppSageConfigFolder);
                File.SetAttributes(ws.AppSageConfigFolder, File.GetAttributes(ws.AppSageConfigFolder) | FileAttributes.Hidden);

                //Copyt the default config file
                string defaultConfigFile =AppSageConfiguration.GetDefaultConfigFilePath();
                string destinationConfigFile = Path.Combine(ws.AppSageConfigFolder, _APPSAGE_CONFIG_FILENAME);
                File.Copy(defaultConfigFile, destinationConfigFile, true);
                IAppSageConfigurationWriter writer= new AppSageConfiguration(destinationConfigFile);
                writer.Set("AppSage.Core:WorkspaceRootFolder", ws.RootFolder);
                writer.Set("AppSage.Core:LogFolder", ws.LogsFolder);

                _logger.LogInformation($"{messagePrefix} hidden cache folder [{ws.CacheFolder}]. Used by AppSage for internal caching.");
                Directory.CreateDirectory(ws.CacheFolder);
                File.SetAttributes(ws.CacheFolder, File.GetAttributes(ws.CacheFolder) | FileAttributes.Hidden);
                _logger.LogInformation($"AppSage workspace succefully initialized at [{RootFolder}]");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error initializing workspace", ex);
                return -1;
            }
            return 0;
        }

        private bool IsExistingAppSageWorkspace()
        {
            //we check for the presence of the AppSageConfig.json file to determine if this is an existing workspace
            IAppSageWorkspace ws = this as IAppSageWorkspace;
            string appsageConfigFile = Path.Combine(ws.AppSageConfigFolder, _APPSAGE_CONFIG_FILENAME);
            return File.Exists(appsageConfigFile);
        }

        public static  string ResolveWorkspaceRootFolder(string folder)
        {
            string resolvedWorkspaceRootFolder = null; 
            //we check for the presence of the AppSageConfig.json file to determine if this is an existing workspace
            Stack<string> foldersToCheck = new Stack<string>();
            foldersToCheck.Push(folder);
            while (foldersToCheck.Count > 0) { 
                string currentFolder = foldersToCheck.Pop();
                DirectoryInfo currentDi = new DirectoryInfo(currentFolder);
                string appsageConfigFile = Path.Combine(currentDi.FullName, IAppSageWorkspace.APPSAGE_CONFIG_ROOT_FOLDER_NAME, _APPSAGE_CONFIG_FILENAME);
                if (File.Exists(appsageConfigFile))
                {
                    IAppSageConfiguration config= new AppSageConfiguration(appsageConfigFile);
                    string rootFolderKey = "AppSage.Core:WorkspaceRootFolder";
                    if (config.KeyExist(rootFolderKey))
                    {
                        string configuredRootFolder = config.Get<string>(rootFolderKey);
                        DirectoryInfo configValue = new DirectoryInfo(configuredRootFolder);
                        if (configValue.FullName != currentDi.FullName)
                        {
                            throw new InvalidOperationException($"The configuration file [{appsageConfigFile}]'s key {rootFolderKey} indicates that the workspace root folder is [{configuredRootFolder}], but it was found in [{currentFolder}]. This is an invalid state. Please fix the configuration file or move it to the correct location.");
                        }
                        return configuredRootFolder;
                    }
                   
                    resolvedWorkspaceRootFolder = currentFolder;
                    break;
                }
                else
                {
                    DirectoryInfo di = new DirectoryInfo(currentFolder);
                    if (di.Parent != null)
                    {
                        foldersToCheck.Push(di.Parent.FullName);
                    }
                }
            }

            return resolvedWorkspaceRootFolder;
        }

        /// <summary>
        /// Get a relative path relative to the namespace. This can be used as a resource name.
        /// </summary>
        /// <param name="path">File path of the resource</param>
        /// <returns>Relative path relative to the workspace</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public string GetResourceName(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }else if(!path.StartsWith(RootFolder,StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Path {path} is not in the workspace {RootFolder}.{typeof(AppSageWorkspaceManager).FullName} can provide scopes only for files and folders under it's workspace.");
            }
            IAppSageWorkspace ws = this as IAppSageWorkspace;

            if (path.StartsWith(ws.RepositoryFolder, StringComparison.OrdinalIgnoreCase))
            {
                string relativePath = path.Substring(ws.RepositoryFolder.Length).TrimStart('\\').Replace("\\", "/");
                return $"/{IAppSageWorkspace.REPOSITORIES_ROOT_FOLDER_NAME}/{relativePath}";
            }
            else if (path.StartsWith(ws.DatabaseSchemaFolder, StringComparison.OrdinalIgnoreCase))
            {
                string relativePath = path.Substring(ws.DatabaseSchemaFolder.Length).TrimStart('\\').Replace("\\", "/");
                return $"/{IAppSageWorkspace.DATABASE_SCHEMA_ROOT_FOLDER_NAME}/{relativePath}";
            }
            throw new ArgumentException($"Path {path} has unknown workspace folder type.");
        }

        public string GetRepositoryName(string path)
        {
            IAppSageWorkspace ws = this as IAppSageWorkspace;
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (!path.StartsWith(RootFolder, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Path {path} is not in the workspace {RootFolder}.{typeof(AppSageWorkspaceManager).FullName} can provide scopes only for files and folders under it's workspace.");
            }
            if (path.StartsWith(ws.RepositoryFolder, StringComparison.OrdinalIgnoreCase))
            {
                string relativePath = path.Substring(ws.RepositoryFolder.Length).TrimStart('\\').Replace("\\", "/");
                if(relativePath.Contains("/"))
                {
                    return relativePath.Split('/')[0];
                }
                else
                {
                    return relativePath;
                }

            }
            throw new ArgumentException($"Path {path} has unknown workspace folder type.");
        }



    
    }
}
