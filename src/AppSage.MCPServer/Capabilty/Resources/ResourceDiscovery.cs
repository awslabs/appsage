using AppSage.McpServer.Support;
using AppSage.MCPServer.Support;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace AppSage.MCPServer.Capabilty.Resources;

[McpServerResourceType]
[CapabilityRegistration("CodeGraph", @"Resources\CodeGraph")]
public class ResourceDiscovery
{
private static readonly List<string> DocumentationFiles = new()
    {
        @"C:\Dev\GitHub\appsage\src\BuiltInExtensions\AppSage.Providers.DotNet\DependencyAnalysis\Guides\GraphDescription.md",
        @"C:\Dev\GitHub\appsage\src\BuiltInExtensions\AppSage.Providers.DotNet\DependencyAnalysis\Guides\GraphReturnQueryExample.md"
    };

    public static ResourcesCapability CreateResourcesCapability()
    {
        return new ResourcesCapability
        {
            ListResourcesHandler = ListResources,
    ReadResourceHandler = ReadResource
   };
    }

    private static ValueTask<ListResourcesResult> ListResources(
    RequestContext<ListResourcesRequestParams> context,
    CancellationToken cancellationToken)
    {
        var resources = new List<Resource>();

      foreach (var filePath in DocumentationFiles)
      {
            if (File.Exists(filePath))
            {
      var fileName = Path.GetFileName(filePath);
                var fileInfo = new FileInfo(filePath);
       
    resources.Add(new Resource
     {
  Uri = $"resource://codegraph-docs/{fileName}",
   Name = fileName,
        Description = $"Documentation file: {fileName}",
        MimeType = GetMimeType(Path.GetExtension(filePath)),
        Size = fileInfo.Length
       });
     }
        }

        var result = new ListResourcesResult { Resources = resources.ToArray() };
        return ValueTask.FromResult(result);
    }

  private static ValueTask<ReadResourceResult> ReadResource(
        RequestContext<ReadResourceRequestParams> context,
        CancellationToken cancellationToken)
    {
        if (context.Params is null || string.IsNullOrWhiteSpace(context.Params.Uri))
       throw new McpException("Missing resource uri.");

        const string prefix = "resource://codegraph-docs/";
    if (!context.Params.Uri.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
      throw new McpException("Unknown resource scheme.");

   var fileName = context.Params.Uri.Substring(prefix.Length);
        var filePath = DocumentationFiles.FirstOrDefault(f => Path.GetFileName(f).Equals(fileName, StringComparison.OrdinalIgnoreCase));

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

    private static string GetMimeType(string extension)
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

