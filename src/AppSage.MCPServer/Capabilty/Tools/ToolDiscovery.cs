using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.McpServer.Support;
using AppSage.MCPServer.Capabilty;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AppSage.McpServer.Capability.Tools;

public class ToolDiscovery
{
    private static readonly Dictionary<string, MethodInfo> Registry = new(StringComparer.OrdinalIgnoreCase);

    // materialized tools list (with description + schema)
    private static readonly List<Tool> Tools = new();


    private IAppSageLogger _logger;
    private IAppSageConfiguration _config;
    private IServiceProvider _services;
    ResultBuilder _utility;
    public ToolDiscovery(IAppSageLogger logger, IServiceProvider services,IAppSageConfiguration config)
    {
        _logger = logger;
        _services = services;
        _config = config;
        _utility = new ResultBuilder(_logger, _config);
        Init();
    }

    private void Init()
    {
        Registry.Clear();

        int classCount = 0;
        int methodCount = 0;

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.GetCustomAttribute<CapabilityRegistrationAttribute>() is not null)
            {
                var toolsToIgnore= _config.Get<string[]>("AppSage.McpServer.Capability.Tools.ToolDiscovery:ToolNamesToIgnore");
                if (toolsToIgnore != null && toolsToIgnore.Contains(type.GetCustomAttribute<CapabilityRegistrationAttribute>().Name, StringComparer.OrdinalIgnoreCase))
                {
                    _logger.LogInformation($"Skipping registration of capability '{type.GetCustomAttribute<CapabilityRegistrationAttribute>().Name}' as it is present in the ignore list.");
                    continue;
                }
                var capabilityRegistration = type.GetCustomAttribute<CapabilityRegistrationAttribute>();
                classCount++;
                //we will only register public methods
                //we will only register instance and static methods
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (method.GetCustomAttribute<McpServerToolAttribute>() is not null)
                    {
                        methodCount++;
                        //MCP requires unique tool names. Tool name can't contain '.' or ':' characters.
                        string key = capabilityRegistration.GetCapabiltyKey(method);
                        Registry[key] = method;
                    }
                }
            }
        }
        _logger.LogDebug($"Discovered {classCount} class(es) with {typeof(CapabilityRegistrationAttribute).Name} defined along with {methodCount} method(s) with {typeof(McpServerToolAttribute).Name} defined.");
        BuildToolListFromGuides();
    }

    // Build tool list from CapabilityGuide\Tools\[Folder name] as given by CapabilityRegistrationAttribute' relative path with fallback to [Description] 
    private void BuildToolListFromGuides()
    {
        Tools.Clear();

        foreach (var (toolName, method) in Registry)
        {

            var (methodDescription, parameterDescription) = GetGuide(toolName, method);
            // Build JSON schema from parameters
            var schemaObj = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject()
                };
            var props = (JsonObject)schemaObj["properties"]!;
            var required = new JsonArray();

            foreach (var p in method.GetParameters())
            {
                var node = new JsonObject
                {
                    ["type"] = ArgBinder.MapTypeToJsonType(p.ParameterType)
                };

                var paramDesc = parameterDescription[p.Name];

                if (!string.IsNullOrWhiteSpace(paramDesc))
                    node["description"] = paramDesc;

                if (!p.IsOptional)
                    required.Add(p.Name);

                props[p.Name] = node;
            }

            if (required.Count > 0)
                schemaObj["required"] = required;

            var schema = JsonSerializer.Deserialize<JsonElement>(schemaObj.ToJsonString());

            Tools.Add(new Tool
            {
                Name = toolName,
                Description = methodDescription,
                InputSchema = schema
            });


        }
    }

    private (string MethodDeescription, Dictionary<string, string> parameterDescription) GetGuide(string toolName, MethodInfo method) {
        var guideAttr = method.DeclaringType.GetCustomAttribute<CapabilityRegistrationAttribute>();

        Dictionary<string, string> parameterDescription = new();
        StringBuilder methodDescription = new StringBuilder();
        if (guideAttr is not null)
        {
            var capabilityDir = PathManager.GetFullDirectoryPath(guideAttr.GuideFolderPath);
            if (!String.IsNullOrEmpty(capabilityDir))
            {
                // Read method common description from the root of the given tool directory
                var commonDescriptionGuideFile = Path.Join(capabilityDir, "description.md");
                if (File.Exists(commonDescriptionGuideFile))
                {
                    methodDescription.AppendLine(File.ReadAllText(commonDescriptionGuideFile));
                }
                var methodDescriptionGuideFile = Path.Join(capabilityDir, method.Name, $"description.md");
                if (File.Exists(methodDescriptionGuideFile))
                {
                    methodDescription.AppendLine(File.ReadAllText(methodDescriptionGuideFile));
                }
                else if (method.GetCustomAttribute<DescriptionAttribute>() is not null)
                {
                    methodDescription.AppendLine(method.GetCustomAttribute<DescriptionAttribute>().Description);
                }
                else
                {
                    throw new Exception($"No description found for tool {toolName}. Please add a description.md file in {Path.Join(capabilityDir, method.Name)} or add a [Description] attribute to the method.");
                }
                var paramDir = Path.Join(capabilityDir, method.Name, "Params");

                foreach (var parameter in method.GetParameters())
                {
                    var paramDescFile = Path.Join(paramDir, $"{parameter.Name}.md");
                    if (File.Exists(paramDescFile))
                    {
                        var paramDesc = File.ReadAllText(paramDescFile);
                        if (!string.IsNullOrWhiteSpace(paramDesc))
                        {
                            parameterDescription.Add(parameter.Name, paramDesc);
                        }
                    }
                    else if (parameter.GetCustomAttribute<DescriptionAttribute>() is not null)
                    {
                        var paramDesc = parameter.GetCustomAttribute<DescriptionAttribute>().Description;
                        parameterDescription.Add(parameter.Name, paramDesc);
                    }
                    else
                    {
                        throw new Exception($"No description found for parameter '{parameter.Name}' of tool '{toolName}'. Please add a '{parameter.Name}.md' file in {paramDir} or add a [Description] attribute to the parameter.");
                    }
                }
            }
            else
            {
                //read from Description from attributes
                if (method.GetCustomAttribute<DescriptionAttribute>() is not null)
                {
                    methodDescription.AppendLine(method.GetCustomAttribute<DescriptionAttribute>().Description);
                }
                else
                {
                    throw new Exception($"No description found for tool {toolName}. Please add a [Description] attribute to the method.");
                }
                foreach (var parameter in method.GetParameters())
                {
                    if (parameter.GetCustomAttribute<DescriptionAttribute>() is not null)
                    {
                        var paramDesc = parameter.GetCustomAttribute<DescriptionAttribute>().Description;
                        parameterDescription.Add(parameter.Name, paramDesc);
                    }
                    else
                    {
                        throw new Exception($"No description found for parameter '{parameter.Name}' of tool '{toolName}'. Please add a [Description] attribute to the parameter.");
                    }
                }
            }
        }
      
        return (methodDescription.ToString(), parameterDescription);
    }
    public ToolsCapability CreateToolsCapability()
    {
        ToolsCapability toolsCapability = new ToolsCapability();
        toolsCapability.ListToolsHandler = ListTools;
        toolsCapability.CallToolHandler = CallTool;

        return toolsCapability;
    }

    private ValueTask<ListToolsResult> ListTools(
        RequestContext<ListToolsRequestParams> context,
        CancellationToken cancellationToken)
    {
        var result = new ListToolsResult { Tools = [.. Tools] };
        return ValueTask.FromResult(result);
    }

    private ValueTask<CallToolResult> CallTool(
        RequestContext<CallToolRequestParams> context,
        CancellationToken cancellationToken)
    {
        if (context.Params is null || string.IsNullOrEmpty(context.Params.Name))
            throw new McpException("Missing tool name.");

        if (!Registry.TryGetValue(context.Params.Name, out var method))
            throw new McpException($"Unknown tool: {context.Params.Name}");

        var argsDict = context.Params.Arguments ?? new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

        var parameters = method.GetParameters();
        var argv = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];

            if (!argsDict.TryGetValue(p.Name!, out var raw))
            {
                if (p.IsOptional) { argv[i] = p.DefaultValue; continue; }
                throw new McpException($"Missing required argument '{p.Name}'.");
            }

            argv[i] = ArgBinder.ConvertArg(raw, p.ParameterType);
        }

        object? result;
        try
        {
            if (method.IsStatic)
            {
                result = method.Invoke(null, argv);
            }
            else
            {
                var declaringInstance = _services.GetService(method.DeclaringType);
                result = method.Invoke(declaringInstance, argv);
            }
        }
        catch (McpException ex)
        {
            _logger.LogError($"Error invoking tool", ex);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error invoking tool",ex);

            // Unwrap invocation exceptions
            throw new McpException($"Error invoking tool:{ex.ToString()}");
        }
        ResultBuilder _utility = new ResultBuilder(_logger, _config);
        // Use the static method to create the result
        var wrapped = _utility.CreateCallToolResult(result);
        return ValueTask.FromResult(wrapped);
    }


}
