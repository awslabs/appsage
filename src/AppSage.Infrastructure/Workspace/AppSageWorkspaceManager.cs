using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Workspace;
using System.Reflection;

namespace AppSage.Infrastructure.Workspace
{
    public class AppSageWorkspaceManager : IAppSageWorkspace
    {
        private class AppSageWorkspacePathProvider : IAppSageWorkspacePaths
        {
            private readonly string _rootFolder;
            public AppSageWorkspacePathProvider(string rootFolder)
            {
                _rootFolder = rootFolder;
            }
            string IAppSageWorkspacePaths.RootFolder => _rootFolder;
        }

      

        private readonly DirectoryInfo _rootFolder;
        private readonly IAppSageLogger _logger;
        public AppSageWorkspaceManager(DirectoryInfo rootFolder, IAppSageLogger logger)
        {
            var resolvedFolder = ResolveWorkspaceRootFolder(rootFolder, logger);
            if(resolvedFolder==null)
            {
                throw new ArgumentException("The specified folder [{RootFolder}] is not part of an existing AppSage workspace. Please initialize the workspace first by running 'appsage init -ws <folder>'", nameof(rootFolder));
            }

            _rootFolder = resolvedFolder;
            _logger = logger;
        }
        public string RootFolder => _rootFolder.FullName;




        public static int Initialize(DirectoryInfo rootFolder, IAppSageLogger logger)
        {
            DirectoryInfo di = rootFolder;
            var resolvedFolder = ResolveWorkspaceRootFolder(rootFolder, logger);
            string messagePrefix = "Creating";

            if (resolvedFolder==null)
            {
                logger.LogInformation("The specified folder [{FolderPath}] is not part of an existing AppSage workspace.", di.FullName);
            }
            else
            {
                logger.LogInformation("The specified folder [{FolderPath}] is already a part of an existing AppSage workspace at [{ResolvedFolderPath}].", di.FullName, resolvedFolder.FullName);
                logger.LogInformation("You can't have nested AppSage workspaces. We will use [{ResolvedFolderPath}] as the workspace. If it is corrupted, we will try to repair it.", resolvedFolder.FullName);
                di= resolvedFolder;
                messagePrefix = "Repairing";
            }

            if (di.Parent == null)
            {
                logger.LogError("The specified folder [{FolderPath}] is not valid. You can't create an AppSage workspace at the root of your driver/mount. Create the workspace inside a folder", di.FullName);
                return -1;
            }

            if (di.Exists && resolvedFolder==null && (di.EnumerateDirectories().Any() || di.GetFiles().Any()))
            {
                logger.LogError("The specified workspace folder [{FolderPath}] already exists and is not empty. Please specify a non-existing or empty folder.", di.FullName);
                return -1;
            }

            logger.LogInformation("Initializing workspace");

            try
            {
                IAppSageWorkspacePaths ws = new AppSageWorkspacePathProvider(di.FullName);


                logger.LogInformation("{MessagePrefix} [{RootFolder}]. This is the root of your workspace", messagePrefix, ws.RootFolder);
                Directory.CreateDirectory(ws.RootFolder);
                logger.LogInformation("{MessagePrefix} [{RepositoryFolder}]. This is where you put all your code repositories. You have to keep seperate folders for each repository.", messagePrefix, ws.RepositoryFolder);
                Directory.CreateDirectory(ws.RepositoryFolder);
                logger.LogInformation("{MessagePrefix} [{DatabaseSchemaFolder}]. This is where you keep your database schemas. You have to keep seperate folders for each database.", messagePrefix, ws.DatabaseSchemaFolder);
                Directory.CreateDirectory(ws.DatabaseSchemaFolder);

                logger.LogInformation("{MessagePrefix} [{ProviderOutputFolder}]. This is where all tooling output after code scan will be saved.", messagePrefix, ws.ProviderOutputFolder);
                Directory.CreateDirectory(ws.ProviderOutputFolder);

                logger.LogInformation("{MessagePrefix} [{MCPServerOutputFolder}]. If a query to AppSage MCP server generates a file/files, this is where those files will be saved.", messagePrefix, ws.MCPServerOutputFolder);
                Directory.CreateDirectory(ws.MCPServerOutputFolder);

                logger.LogInformation("{MessagePrefix} [{LogsFolder}]. This is where all logs will be saved.", messagePrefix, ws.LogsFolder);
                Directory.CreateDirectory(ws.LogsFolder);

                logger.LogInformation("{MessagePrefix} [{ProviderFolder}]. This is where all provider plugins should be placed. One folder for each plugin.", messagePrefix, ws.ProviderFolder);
                Directory.CreateDirectory(ws.ProviderFolder);

                logger.LogInformation("{MessagePrefix} [{DocsFolder}]. This is where documents will be kept.", messagePrefix, ws.DocsFolder);
                Directory.CreateDirectory(ws.DocsFolder);

                string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string docsDirectory = Path.Combine(directory,"Workspace","Docs");
                CopyDirectoryIterative(docsDirectory, ws.DocsFolder);


                logger.LogInformation("{MessagePrefix} hidden AppSage config folder folder [{AppSageConfigFolder}]. Used by AppSage to identify configuration.", messagePrefix, ws.AppSageConfigFolder);
                Directory.CreateDirectory(ws.AppSageConfigFolder);
                File.SetAttributes(ws.AppSageConfigFolder, File.GetAttributes(ws.AppSageConfigFolder) | FileAttributes.Hidden);

                //Copy the default config file
                string defaultConfigFile =AppSageConfiguration.GetDefaultConfigTemplateFilePath();
                string destinationConfigFile = ws.AppSageConfigFilePath;
                logger.LogInformation("{MessagePrefix} the AppSage config file [{DestinationConfigFile}]. This is where workspace related AppSage configurations are kept.", messagePrefix, destinationConfigFile);
                File.Copy(defaultConfigFile, destinationConfigFile, true);
                IAppSageConfigurationWriter writer= new AppSageConfiguration(destinationConfigFile);
                writer.Set("AppSage.Core:IsAppSageWorkspace", true);
                writer.Set("AppSage.Core:CreatedDateTime", DateTime.Now.ToUniversalTime().ToString());
                writer.Set("AppSage.Core:CreatedBy", Environment.UserName);

                logger.LogInformation("{MessagePrefix} hidden cache folder [{CacheFolder}]. Used by AppSage for internal caching.", messagePrefix, ws.CacheFolder);
                Directory.CreateDirectory(ws.CacheFolder);
                File.SetAttributes(ws.CacheFolder, File.GetAttributes(ws.CacheFolder) | FileAttributes.Hidden);
                logger.LogInformation("AppSage workspace succefully initialized at [{RootFolder}]", ws.RootFolder);
            }
            catch (Exception ex)
            {
                logger.LogError("Error initializing workspace", ex);
                return -1;
            }
            return 0;
        }


        static void CopyDirectoryIterative(string sourceDir, string destinationDir)
        {
            var sourceRoot = new DirectoryInfo(sourceDir);

            if (!sourceRoot.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {sourceRoot.FullName}");
            }

            // Stack for DFS traversal
            Stack<DirectoryInfo> dirs = new Stack<DirectoryInfo>();
            dirs.Push(sourceRoot);

            while (dirs.Count > 0)
            {
                var currentDir = dirs.Pop();
                string relativePath = Path.GetRelativePath(sourceRoot.FullName, currentDir.FullName);
                string targetDir = Path.Combine(destinationDir, relativePath);

                Directory.CreateDirectory(targetDir);

                // Copy files in the current directory
                foreach (FileInfo file in currentDir.GetFiles())
                {
                    string targetFilePath = Path.Combine(targetDir, file.Name);
                    file.CopyTo(targetFilePath, overwrite: true);
                }

                // Push subdirectories to stack
                foreach (DirectoryInfo subDir in currentDir.GetDirectories())
                {
                    dirs.Push(subDir);
                }
            }
        }

        public static  DirectoryInfo ResolveWorkspaceRootFolder(DirectoryInfo folder, IAppSageLogger logger=null)
        {
            DirectoryInfo resolvedWorkspaceRootFolder = null; 
            //we check for the presence of the AppSageConfig.json file to determine if this is an existing workspace
            Stack<DirectoryInfo> foldersToCheck = new Stack<DirectoryInfo>();
            foldersToCheck.Push(folder);
            while (foldersToCheck.Count > 0) { 
                DirectoryInfo currentDi = foldersToCheck.Pop();
                string appsageConfigFile = Path.Combine(currentDi.FullName, IAppSageWorkspace.APPSAGE_CONFIG_ROOT_FOLDER_NAME, IAppSageWorkspace.APPSAGE_CONFIG_FILENAME);
                if (File.Exists(appsageConfigFile))
                {
                    var config= new AppSageConfiguration(appsageConfigFile);
                    string fingerPrintKey = "AppSage.Core:IsAppSageWorkspace";
                    if (config.KeyExist(fingerPrintKey))
                    {
                        bool isAppSageWorkspace = config.Get<bool>(fingerPrintKey);
                        if(isAppSageWorkspace && currentDi.Parent!=null)
                        {
                            resolvedWorkspaceRootFolder = currentDi;
                            break;
                        }
                        else
                        {
                            logger?.LogWarning("The folder [{FolderPath}] contains an AppSage configuration file but it is not marked as an AppSage workspace. Continuing to look in parent folders.", currentDi.FullName);
                        }
                    }
                }
                else
                {
                    if (currentDi.Parent != null)
                    {
                        foldersToCheck.Push(currentDi.Parent);
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
