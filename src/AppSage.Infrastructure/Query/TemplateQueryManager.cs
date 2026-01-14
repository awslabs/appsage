using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Query;
using AppSage.Core.Workspace;

namespace AppSage.Infrastructure.Query
{
    public class TemplateQueryManager:ITemplateQueryManager
    {
        IAppSageLogger _logger;
        IAppSageWorkspace _workspace;
        IAppSageConfiguration _config;
        public TemplateQueryManager(IAppSageLogger logger, IAppSageWorkspace workspace, IAppSageConfiguration config)
        {
            _logger = logger;
            _workspace = workspace;
            _config = config;
        }

        public IEnumerable<string> GetTemplateGroups()
        {
            return Directory.GetDirectories(_workspace.TemplateFolder, "*", SearchOption.TopDirectoryOnly).Select(d => new DirectoryInfo(d).Name);
        }

        public IEnumerable<string> GetTemplateNames()
        {
            return Directory.GetFiles(_workspace.TemplateFolder, "*.cs", SearchOption.AllDirectories).Select(f => f.Replace(_workspace.TemplateFolder, String.Empty));
        }


    }
}
