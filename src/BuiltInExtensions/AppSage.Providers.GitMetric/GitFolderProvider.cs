//An implementation of IGitRepoProvider
using AppSage.Core.Logging;
using AppSage.Core.Resource;
using AppSage.Core.Workspace;
namespace AppSage.Providers.GitMetric
{
    internal class GitFolderProvider : IResourceProvider
    {
        IAppSageLogger _logger;
        IAppSageWorkspace _workspace;

        public GitFolderProvider(IAppSageLogger logger, IAppSageWorkspace workspace)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        }

        IEnumerable<IResource> IResourceProvider.GetResources()
        {
            List<IResource> result = new List<IResource>();
            string[] gitFolderList = Directory.GetDirectories(_workspace.RepositoryFolder);


            foreach (string gitFolder in gitFolderList.Where(folder => Directory.Exists(Path.Combine(folder, ".git"))))
            {
                //get the relative path without the root folder
                string scope = _workspace.GetResourceName(gitFolder);
                result.Add(new FileStorageResource(scope, gitFolder));
            }

            return result;
        }
    }
}
