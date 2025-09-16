using AppSage.Core.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace AppSage.MCPServer.Capabilty.Tools.Echo2
{
    [McpServerToolType]
    [CapabilityRegistration("Echo2", @"Tools\Echo2")]
    public class EchoGenerator
    {
        IAppSageLogger _logger;
        public EchoGenerator(IAppSageLogger logger) { 
        
            _logger = logger; 
        }
        [McpServerTool, Description("Echo2Method1 fallback description.")]
        public string Method1(
            [Description("Message to echo (fallback).")] string message)
        { 
            _logger.LogInformation("Echo2.Method1 called");
            return $"echo2.method1: {message}"; 
        }

        [McpServerTool, Description("Echo2Method2 fallback description.")]
        public string Method2(
            [Description("Message to echo (fallback).")] string message = "default")
        { 
            _logger.LogInformation("Echo2.Method2 called");
            return $"echo2.method2: {message}";
        }
    }
}
