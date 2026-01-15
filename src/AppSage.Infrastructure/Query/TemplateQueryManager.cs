using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Query;
using AppSage.Core.Workspace;
using System.Net.Http.Headers;

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

        public IEnumerable<TemplateInfo> GetTemplates()
        {
            var results = new List<TemplateInfo>();
            var groupDirs= Directory.GetDirectories(_workspace.TemplateFolder, "*", SearchOption.TopDirectoryOnly).Select(d => new DirectoryInfo(d));
            foreach(var groupDir in groupDirs)
            {
                var templateGroup=new TemplateInfo
                {
                    TemplateType=TemplateType.Group,
                    TemplateId=groupDir.Name,
                    Description=$"Template group for {groupDir.Name}"
                };
                results.Add(templateGroup);

                var templateFiles = Directory.GetFiles(groupDir.FullName, "*.cs", SearchOption.TopDirectoryOnly);
                foreach(var templateFile in templateFiles)
                {
                    var templateId = templateFile.Replace(groupDir.FullName + Path.DirectorySeparatorChar, String.Empty);
                    var templateInfo = new TemplateInfo
                    {
                        TemplateType = TemplateType.Single,
                        TemplateId = templateId,
                        Description = $"Template for {templateId} in group {groupDir.Name}"
                    };
                    results.Add(templateInfo);
                }
            }
            return results;
        }


    }
}
