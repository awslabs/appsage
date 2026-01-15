using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Workspace;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.CodeAnalysis;

namespace AppSage.MCPServer
{
    internal class TemplateGenerator
    {
        IAppSageLogger _logger;
        IAppSageConfiguration _config;
        IAppSageWorkspace _workspace;

        public TemplateGenerator(IAppSageLogger logger, IAppSageWorkspace workspace,IAppSageConfiguration config) {
            _logger = logger;
            _workspace = workspace;
            _config = config;
        }
        internal void SaveAsTemplate(string code,string returnType, string comment)
        {
            var saveQueryAsTemplate = _config.Get<bool>("AppSage.MCPServer.TemplateGenerator:SaveQueryAsTemplateGroup");

            if (saveQueryAsTemplate)
            {
                var templateGroupName = _config.Get<string>("AppSage.MCPServer.TemplateGenerator:TemplateGroupNameForSaving");
                var templateFolderPath = Path.Combine(_workspace.TemplateFolder, templateGroupName);
                if (!Directory.Exists(templateFolderPath))
                {
                    Directory.CreateDirectory(templateFolderPath);
                }

                string templateName = $"Template_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}_{Guid.NewGuid().ToString().Take(6).ToString()}.cs";

                string filieName = Path.Combine(templateFolderPath, templateName);
                File.WriteAllText(filieName, code);

                string metaData = Path.Combine(templateFolderPath, templateName + ".metadata");
                var content = new List<string>();
                content.Add($"ReturnType:{returnType}");
                content.Add(comment);
                File.AppendAllLines(metaData, content.ToArray());

                _logger.LogInformation($"Template saved to {filieName}");
            }
        }
    }
}
