using AppSage.Core.Logging;
using AppSage.Core.Resource;
using AppSage.Core.Workspace;


namespace AppSage.Providers.DotNet
{
    internal class ProjectFileProvider : IResourceProvider
    {
        private readonly IAppSageLogger _logger;
        private readonly IAppSageWorkspace _workspace;
        public ProjectFileProvider(IAppSageLogger logger, IAppSageWorkspace workspace)
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
                List<string> projFiles=new List<string>();
                //for the moment we will prioritize C# projects only
                projFiles.AddRange(Directory.GetFiles(reposPath, "*.csproj", SearchOption.AllDirectories));
                //projFiles.AddRange(Directory.GetFiles(reposPath, "*.fsproj", SearchOption.AllDirectories));
                //projFiles.AddRange(Directory.GetFiles(reposPath, "*.vbproj", SearchOption.AllDirectories));
                foreach (string projFile in projFiles)
                {
                    string scope = _workspace.GetResourceName(projFile);
                    result.Add(new FileStorageResource(scope, projFile));
                }
            }

            return result;
        }
    }
}
