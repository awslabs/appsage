using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Template;
using AppSage.Core.Workspace;

namespace AppSage.Infrastructure.Template
{
    public class TemplateManager : ITemplateManager
    {
        IAppSageLogger _logger;
        IAppSageWorkspace _workspace;
        IAppSageConfiguration _config;
        public TemplateManager(IAppSageLogger logger, IAppSageWorkspace workspace, IAppSageConfiguration config)
        {
            _logger = logger;
            _workspace = workspace;
            _config = config;
        }

        public IEnumerable<ITemplateMetadata> GetTemplateMetadata()
        {
            var results = new List<ITemplateMetadata>();
            var groupDirs = Directory.GetDirectories(_workspace.TemplateFolder, "*", SearchOption.TopDirectoryOnly).Select(d => new DirectoryInfo(d));
            foreach (var groupDir in groupDirs)
            {
                string groupMetadataFile = Path.Combine(groupDir.FullName, $"{groupDir.Name}.metadata");
                string groupDescription = String.Empty;
                if (File.Exists(groupMetadataFile))
                {
                    groupDescription = File.ReadAllText(groupMetadataFile);
                }
                var templateGroup = new TemplateMetadata
                {
                    TemplateType = TemplateType.GroupQuery,
                    TemplateId = groupDir.Name,
                    Description = groupDescription
                };
                results.Add(templateGroup);

                var templateFiles = Directory.GetFiles(groupDir.FullName, "*.cs", SearchOption.TopDirectoryOnly);
                foreach (var templateFile in templateFiles)
                {

                    var templateId = templateFile.Replace(_workspace.TemplateFolder + Path.DirectorySeparatorChar, String.Empty);
                    var metadataFile = templateFile + ".metadata";
                    var description = String.Empty;
                    if (File.Exists(metadataFile))
                    {

                        description = File.ReadAllText(metadataFile);
                    }

                    var templateInfo = new TemplateMetadata
                    {
                        TemplateType = TemplateType.SingleQuery,
                        TemplateId = templateId,
                        Description = description
                    };
                    results.Add(templateInfo);
                }
            }
            return results;
        }

        public IEnumerable<ITemplate> GetTemplates(string Id)
        {
            var workspacePaths = (IAppSageWorkspacePaths)_workspace;
            var results = new List<ITemplate>();

            if (!string.IsNullOrEmpty(Id) && Id.EndsWith(".cs"))
            {
                // Single template
                string templateFilePath = Path.Combine(workspacePaths.TemplateFolder, Id);

                if (File.Exists(templateFilePath))
                {
                    string content = File.ReadAllText(templateFilePath);
                    var template = new TemplateBody
                    {
                        TemplateId = Id,
                        TemplateType = TemplateType.SingleQuery,
                        Content = content
                    };
                    string metadataFilePath = templateFilePath + ".metadata";
                    if (File.Exists(metadataFilePath))
                    {
                        template.Description = File.ReadAllText(metadataFilePath);
                    }
                    results.Add(template);
                }
                else
                {
                    _logger.LogError($"No matching template is found for Templat Id: {Id}. Template file '{templateFilePath}' does not exist");
                }
            }
            else
            {
                string groupDirPath = workspacePaths.TemplateFolder;
                var templateFiles = new List<string>();

                if (!string.IsNullOrEmpty(Id))
                {
                    groupDirPath = Path.Combine(workspacePaths.TemplateFolder, Id);
                    templateFiles.AddRange(Directory.GetFiles(groupDirPath, "*.cs", SearchOption.TopDirectoryOnly));
                }
                else
                {
                    var groupDirs = Directory.GetDirectories(workspacePaths.TemplateFolder, "*", SearchOption.TopDirectoryOnly);
                    foreach (var groupDir in groupDirs)
                    {
                        templateFiles.AddRange(Directory.GetFiles(groupDir, "*.cs", SearchOption.TopDirectoryOnly));
                    }
                }


                foreach (var templateFile in templateFiles)
                {
                    string content = File.ReadAllText(templateFile);
                    var relativeTemplateId = templateFile.Replace(workspacePaths.TemplateFolder + Path.DirectorySeparatorChar, String.Empty);
                    var template = new TemplateBody
                    {
                        TemplateId = relativeTemplateId,
                        TemplateType = TemplateType.SingleQuery,
                        Content = content
                    };
                    string metadataFilePath = templateFile + ".metadata";
                    if (File.Exists(metadataFilePath))
                    {
                        template.Description = File.ReadAllText(metadataFilePath);
                    }
                    results.Add(template);
                }

            }


            return results;
        }
    }
}
