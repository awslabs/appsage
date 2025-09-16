using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Workspace;

namespace AppSage.Infrastructure.Caching
{
    public class FileSystemCache:IAppSageCache
    {
        string _cachRootFolder;
        IAppSageLogger _logger;
        IAppSageConfiguration _configuration;
        public FileSystemCache(IAppSageWorkspace workspace,IAppSageLogger logger,IAppSageConfiguration configuration) { 
           _cachRootFolder=workspace.CacheFolder;
            _logger = logger;
            _configuration = configuration;
            EnsureCachFolder();
        }

        private bool _ignoreCache => _configuration.Get<bool>("AppSage.Infrastructure.Caching.FileSystemCache:IgnoreCache");

        private void EnsureCachFolder()
        {
            if (!Directory.Exists(_cachRootFolder))
            {
                Directory.CreateDirectory(_cachRootFolder);
                File.SetAttributes(_cachRootFolder, File.GetAttributes(_cachRootFolder) | FileAttributes.Hidden);
                _logger.LogInformation($"Hidden cache folder created at {_cachRootFolder}");
            }
        }
        private string GetCacheFilePath(string key)
        {
            return Path.Combine(_cachRootFolder, $"{key}.cache");
        }

        void IAppSageCache.Set(string key, string value)
        {
            EnsureCachFolder();
            var filePath = GetCacheFilePath(key);
            try
            {
                File.WriteAllText(filePath, value);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to set cache for key {key}: {ex.Message}");
            }
        }

        string IAppSageCache.Get(string key)
        {
            if (!_ignoreCache)
            {
                var path = GetCacheFilePath(key);
                if (File.Exists(path))
                {
                    return File.ReadAllText(path);
                }
            }
            return null;
        }

        void IAppSageCache.Remove(string key)
        {
            var path = GetCacheFilePath(key);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

        }

        bool IAppSageCache.ContainsKey(string key)
        {
            if (!_ignoreCache)
            { 
                var path = GetCacheFilePath(key);
                return File.Exists(path);
            }
            return false;
        }

        void IAppSageCache.Clear()
        {
            if (Directory.Exists(_cachRootFolder))
            {
                try
                {
                    Directory.Delete(_cachRootFolder, true);
                    _logger.LogInformation($"Cache folder cleared at {_cachRootFolder}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to clear cache: {ex.Message}");
                }
            }
            EnsureCachFolder();
        }
    }
}
