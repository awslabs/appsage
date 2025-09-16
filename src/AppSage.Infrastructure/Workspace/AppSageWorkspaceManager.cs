using AppSage.Core.Logging;
using AppSage.Core.Workspace;

namespace AppSage.Infrastructure.Workspace
{
    public class AppSageWorkspaceManager : IAppSageWorkspace
    {
        private readonly string _rootFolder;
        private readonly IAppSageLogger _logger;
        public AppSageWorkspaceManager(string rootFolder, IAppSageLogger logger)
        {
            _rootFolder = rootFolder;
            _logger = logger;
        }
        public string RootFolder => _rootFolder;

        public string RepositoryFolder => Path.Combine(RootFolder, ScopeNames.REPOSITORIES_ROOT);

        public string DatabaseSchemaFolder => Path.Combine(RootFolder, ScopeNames.DATABASE_SCHEMA_ROOT);

        public string CacheFolder => Path.Combine(RootFolder, ScopeNames.CACHE_ROOT);

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

            if(path.StartsWith(RepositoryFolder, StringComparison.OrdinalIgnoreCase))
            {
                string relativePath = path.Substring(RepositoryFolder.Length).TrimStart('\\').Replace("\\", "/");
                return $"/{ScopeNames.REPOSITORIES_ROOT}/{relativePath}";
            }
            else if (path.StartsWith(DatabaseSchemaFolder, StringComparison.OrdinalIgnoreCase))
            {
                string relativePath = path.Substring(DatabaseSchemaFolder.Length).TrimStart('\\').Replace("\\", "/");
                return $"/{ScopeNames.DATABASE_SCHEMA_ROOT}/{relativePath}";
            }
            throw new ArgumentException($"Path {path} has unknown workspace folder type.");
        }

        public string GetRepositoryName(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (!path.StartsWith(RootFolder, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Path {path} is not in the workspace {RootFolder}.{typeof(AppSageWorkspaceManager).FullName} can provide scopes only for files and folders under it's workspace.");
            }
            if (path.StartsWith(RepositoryFolder, StringComparison.OrdinalIgnoreCase))
            {
                string relativePath = path.Substring(RepositoryFolder.Length).TrimStart('\\').Replace("\\", "/");
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

        public void Initialize()
        {
            _logger.LogInformation("Initializing workspace");
            try
            {
                _logger.LogInformation($"Creating {RootFolder}. This is the root of your workspace");
                Directory.CreateDirectory(RootFolder);
                _logger.LogInformation($"Creating {RepositoryFolder}. This is where you put all your code repositories. You have to keep seperate folders for each repository.");
                Directory.CreateDirectory(RepositoryFolder);
                _logger.LogInformation($"Creating {DatabaseSchemaFolder}. This is where you keep your database schemas. You have to keep seperate folders for each database.");
                Directory.CreateDirectory(DatabaseSchemaFolder);
                _logger.LogInformation($"Creating hidden cache folder {CacheFolder}. Used by AppSage for internal caching.");
                Directory.CreateDirectory(CacheFolder);
                File.SetAttributes(CacheFolder, File.GetAttributes(CacheFolder) | FileAttributes.Hidden);
                _logger.LogInformation($"AppSage workspace succefully initialized at {RootFolder}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error initializing workspace", ex);
            }
        }

    
    }
}
