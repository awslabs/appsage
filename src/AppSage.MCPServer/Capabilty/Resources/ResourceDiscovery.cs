using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Workspace;
using AppSage.McpServer.Support;
using AppSage.MCPServer.Support;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace AppSage.MCPServer.Capabilty.Resources;


public class ResourceDiscovery
{
    IAppSageLogger _logger;
    IServiceProvider _services;
    IAppSageConfiguration _config;
    IAppSageWorkspace _workspace;
    ResultBuilder _utility;
    public ResourceDiscovery(IAppSageLogger logger, IServiceProvider services, IAppSageConfiguration config, IAppSageWorkspace workspace)
    {
        _services = services;
        _logger = logger;
        _config = config;
        _workspace = workspace;
        _utility = new ResultBuilder(_logger, _config, workspace);

    }



    public ResourcesCapability CreateResourcesCapability()
    {
        return new ResourcesCapability
        {
            ListResourcesHandler = ListResources,
            ReadResourceHandler = ReadResource
        };
    }



    private ValueTask<ListResourcesResult> ListResources(
        RequestContext<ListResourcesRequestParams> context,
          CancellationToken cancellationToken)
    {
        var resources = new List<Resource>();

        var extensionList = Directory.GetDirectories(_workspace.ExtensionInstallFolder, "*", SearchOption.TopDirectoryOnly);
        foreach (var extension in extensionList.Select(e => new DirectoryInfo(e)))
        {
            var extensionDocFolder = _workspace.GetExtensionDocumentationFolder(extension.Name);
            var files = Directory.GetFiles(extensionDocFolder, "*.md", SearchOption.TopDirectoryOnly).Select(f => new FileInfo(f));

            foreach (var file in files)
            {
                if (file.Exists)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file.Name);
                    var descriptionFile = Path.Combine(file.DirectoryName, fileName, ".description");

                    var descriptionText = $"Description for {fileName}";
                    if (File.Exists(descriptionFile))
                    {
                        descriptionText = File.ReadAllText(descriptionFile);
                    }

                    resources.Add(new Resource
                    {
                        Uri = $"resource://appsage/extension/{extension.Name}/{fileName}",
                        Name = fileName,
                        Description = descriptionText,
                        MimeType = GetMimeType(Path.GetExtension(file.FullName)),
                        Size = file.Length
                    });
                }
            }
        }






        var result = new ListResourcesResult { Resources = resources.ToArray() };
        return ValueTask.FromResult(result);
    }

    private ValueTask<ReadResourceResult> ReadResource(
        RequestContext<ReadResourceRequestParams> context,
        CancellationToken cancellationToken)
    {
        if (context.Params is null || string.IsNullOrWhiteSpace(context.Params.Uri))
            throw new McpException("Missing resource uri.");

        const string prefix = "resource://appsage/extension/";
        if (!context.Params.Uri.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new McpException("Unknown resource scheme.");

        var fileName = context.Params.Uri.Substring(prefix.Length);

        var filePath = Path.Combine(@"C:\Dev\GitHub\appsage\src\BuiltInExtensions\AppSage.Providers.DotNet\DependencyAnalysis\Guides\", fileName + ".md");

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            throw new McpException("Resource not found.");

        var fileContent = File.ReadAllText(filePath);

        var contents = new ResourceContents[]
        {
            new TextResourceContents
{
                Uri = context.Params.Uri,
                MimeType = GetMimeType(Path.GetExtension(filePath)),
                Text = fileContent
            }
      };

        var result = new ReadResourceResult { Contents = contents };
        return ValueTask.FromResult(result);
    }

    private string GetMimeType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".md" => "text/markdown",
            ".txt" => "text/plain",
            ".json" => "application/json",
            ".xml" => "application/xml",
            _ => "text/plain"
        };
    }
}

