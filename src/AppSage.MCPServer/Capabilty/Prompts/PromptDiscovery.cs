//using AppSage.McpServer.Support;
//using ModelContextProtocol;
//using ModelContextProtocol.Protocol;
//using ModelContextProtocol.Server;


//namespace AppSage.McpServer.Capability.Prompts;

//public static class PromptDiscovery
//{
//    // Reads prompts from: CapabilityGuide\Prompts\<PromptName>\template.md (+ description.md optional)

//    public static PromptsCapability CreatePromptsCapability()
//    {
//        return new PromptsCapability
//        {
//            ListPromptsHandler = ListPrompts,
//            GetPromptHandler = GetPrompt
//        };
//    }

//    private static ValueTask<ListPromptsResult> ListPrompts(
//        RequestContext<ListPromptsRequestParams> context,
//        CancellationToken cancellationToken)
//    {
//        var result = new ListPromptsResult { Prompts = Array.Empty<Prompt>() };

//        var root = PathManager.RootPrompts;
//        if (root is null || !Directory.Exists(root))
//            return ValueTask.FromResult(result);

//        var items = new List<Prompt>();

//        foreach (var dir in Directory.EnumerateDirectories(root))
//        {
//            var name = Path.GetFileName(dir);
//            var templatePath = PathManager.Join(dir, "template.md");
//            if (!File.Exists(templatePath))
//                continue;

//            var description = PathManager.ReadText(PathManager.Join(dir, "description.md"));

//            // New API: Prompt has Name/Description/Title/Arguments (no InputSchema)
//            var p = new Prompt
//            {
//                Name = name,
//                Description = description ?? $"Prompt '{name}'."
//                // You can optionally set Title/Arguments here
//            };

//            items.Add(p);
//        }

//        result.Prompts = items.ToArray();
//        return ValueTask.FromResult(result);
//    }

//    private static ValueTask<GetPromptResult> GetPrompt(
//        RequestContext<GetPromptRequestParams> context,
//        CancellationToken cancellationToken)
//    {
//        if (context.Params is null || string.IsNullOrWhiteSpace(context.Params.Name))
//            throw new McpException("Missing prompt name.");

//        var dir = PathManager.Join(PathManager.RootPrompts, context.Params.Name);
//        var templatePath = PathManager.Join(dir, "template.md");
//        if (!File.Exists(templatePath))
//            throw new McpException($"Prompt template not found: {context.Params.Name}");

//        var body = File.ReadAllText(templatePath);

//        var message = new PromptMessage
//        {
//            Role = Role.User,
//            // FIX: Content should be a single ContentBlock, not an array
//            Content = new TextContentBlock { Text = body }
//        };

//        var result = new GetPromptResult
//        {
//            // EXPLICIT array here as well (safe for IReadOnlyList<T>)
//            Messages = new PromptMessage[] { message }
//        };

//        return ValueTask.FromResult(result);
//    }
//}
