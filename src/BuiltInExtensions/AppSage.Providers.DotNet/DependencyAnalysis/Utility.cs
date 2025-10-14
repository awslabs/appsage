using AppSage.Core.Logging;
using Microsoft.CodeAnalysis;
using AppSage.Core.Workspace;

namespace AppSage.Providers.DotNet.DependencyAnalysis
{
    internal class Utility
    {
        IAppSageLogger _logger;
        IAppSageWorkspace _workspace;

        internal Utility(IAppSageLogger logger, IAppSageWorkspace workspace)
        {
            _logger = logger;
            _workspace = workspace;
        }

        internal string GetNodeIdProject(Project project) { 

            string id= _workspace.GetResourceName(project.FilePath);
            return id;
        }

        internal string GetNodeIdSolution(Solution solution) { 
            string id = _workspace.GetResourceName(solution.FilePath);
            return id;
        }
    }
}
