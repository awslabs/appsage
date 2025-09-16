//using AppSage.McpServer.Support;
//using AppSage.MCPServer.Support;
//using ModelContextProtocol;
//using ModelContextProtocol.Protocol;
//using ModelContextProtocol.Server;

//namespace AppSage.McpServer.Capability.Resources;

//public static class ResourceDiscovery
//{
//    // Expose files under CapabilityGuide\Resources recursively as:
//    //   resource://resources/<relative-path>

//    public static ResourcesCapability CreateResourcesCapability()
//    {
//        return new ResourcesCapability
//        {
//            ListResourcesHandler = ListResources,
//            ReadResourceHandler = ReadResource
//        };
//    }

//    private static ValueTask<ListResourcesResult> ListResources(
//        RequestContext<ListResourcesRequestParams> context,
//        System.Threading.CancellationToken cancellationToken)
//    {
//        var result = new ListResourcesResult { Resources = Array.Empty<Resource>() };

//        var root = PathManager.RootResources;
//        if (root is null || !Directory.Exists(root))
//            return ValueTask.FromResult(result);

//        var items = new List<Resource>();

//        foreach (var full in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
//        {
//            var rel = Path.GetRelativePath(root, full).Replace('\\', '/');
//            var uri = $"resource://resources/{rel}";
//            var info = new FileInfo(full);

//            items.Add(new Resource
//            {
//                Uri = uri,
//                Name = Path.GetFileName(full),
//                MimeType = Mime.FromExtension(Path.GetExtension(full)),
//                Size = info.Length
//            });
//        }

//        result.Resources = items.ToArray();
//        return ValueTask.FromResult(result);
//    }

//    private static ValueTask<ReadResourceResult> ReadResource(
//        RequestContext<ReadResourceRequestParams> context,
//        System.Threading.CancellationToken cancellationToken)
//    {
//        if (context.Params is null || string.IsNullOrWhiteSpace(context.Params.Uri))
//            throw new McpException("Missing resource uri.");

//        const string prefix = "resource://resources/";
//        if (!context.Params.Uri.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
//            throw new McpException("Unknown resource scheme.");

//        var rel = context.Params.Uri.Substring(prefix.Length).Replace('/', Path.DirectorySeparatorChar);

//        var root = PathManager.RootResources ?? throw new McpException("Resources root not found.");
//        var full = Path.GetFullPath(Path.Combine(root, rel));

//        // Prevent directory traversal
//        var rootFull = Path.GetFullPath(root);
//        if (!full.StartsWith(rootFull, StringComparison.Ordinal))
//            throw new McpException("Resource path outside root.");

//        if (!File.Exists(full))
//            throw new McpException("Resource not found.");

//        var bytes = File.ReadAllBytes(full);

//        var contents = new ResourceContents[]
//        {
//            new BlobResourceContents
//            {
//                Uri = context.Params.Uri,
//                MimeType = Mime.FromExtension(Path.GetExtension(full)),
//                Blob = Convert.ToBase64String(bytes)
//            }
//        };

//        var result = new ReadResourceResult { Contents = contents };
//        return ValueTask.FromResult(result);
//    }
//}
