using AppSage.Core.Logging;
using AppSage.Core.Resource;
using AppSage.Core.Workspace;

namespace AppSage.Providers.DotNet
{
    internal class SolutionFileProvider: IResourceProvider
    {
        private readonly IAppSageLogger _logger;
        private readonly IAppSageWorkspace _workspace;
        public SolutionFileProvider(IAppSageLogger logger, IAppSageWorkspace workspace)
        {
            _logger = logger;
            _workspace = workspace;
        }

        IEnumerable<IResource> IResourceProvider.GetResources()
        {
            List<IResource> result = new List<IResource>();
            string[] repos = Directory.GetDirectories(_workspace.RepositoryFolder);
            foreach (string reposPath in repos)
            {
                string[] slnFiles = Directory.GetFiles(reposPath, "*.sln", SearchOption.AllDirectories);
                foreach (string slnFile in slnFiles)
                {
                    string scope = _workspace.GetResourceName(slnFile);
                    result.Add(new FileStorageResource(scope, slnFile));
                }
            }

            return result;
        }
    }
   
}
