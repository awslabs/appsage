using AppSage.Core.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
namespace AppSage.MCPServer.Capabilty.Tools.Echo1
{
    [McpServerToolType]
    [CapabilityRegistration("Echo1", @"Tools\Echo1")]
    public class EchoGenerator
    {
        IAppSageLogger _logger;
        public EchoGenerator(IAppSageLogger logger)
        {
            _logger = logger;
        }

        [McpServerTool, Description("Echo1Method1 fallback description if guide files are missing.")]
        public string Method1(
            [Description("Message to echo (fallback if params/message.md missing).")] string message)
        {
            _logger.LogInformation("Echo1.Method1 called");
            return   $"echo1.method1: {message}";
        }

        [McpServerTool, Description("Echo1Method2 fallback description if guide files are missing.")]
        public static CallToolResult Method2(
            [Description("Message to echo (fallback).")] string message)
        { 
           var result= new CallToolResult
               {
                   Content = [new TextContentBlock { Text = $"echo1.method2: {message}" }]
               };
           return result;
        }
    }
}
