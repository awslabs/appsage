using AppSage.Core.Logging;
using AppSage.McpServer.Capability.Tools;
using AppSage.MCPServer.Capabilty;
using AppSage.MCPServer.Capabilty.Resources;
using ModelContextProtocol.Protocol;
using System.Reflection;

namespace AppSage.MCPServer.CapabilityBuilder
{
    internal class ServerDiscovery
    {
        IAppSageLogger _logger;

        ToolDiscovery _toolDiscovery;
        ResourceDiscovery _resourceDiscovery;
        public ServerDiscovery(IAppSageLogger logger, ToolDiscovery toolDiscovery, ResourceDiscovery resourceDiscovery)
        {
            _logger = logger;
            _toolDiscovery = toolDiscovery;
            _resourceDiscovery = resourceDiscovery;
        }
        public ServerCapabilities CreateServerCapabilities()
        {
            CheckCapabilityNameCollision();
            ServerCapabilities serverCapabilities = new ServerCapabilities();


            serverCapabilities.Tools= _toolDiscovery.CreateToolsCapability();
            //serverCapabilities.Prompts = PromptDiscovery.CreatePromptsCapability();
            
            //serverCapabilities.Resources = _resourceDiscovery.CreateResourcesCapability();
 
            return serverCapabilities;
        }

        private void CheckCapabilityNameCollision() {
            
            Dictionary<string,int> capabilityNameCount = new(StringComparer.OrdinalIgnoreCase);

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.GetCustomAttribute<CapabilityRegistrationAttribute>() is not null)
                {
                    var capabilityName = type.GetCustomAttribute<CapabilityRegistrationAttribute>().Name;
                    if(capabilityNameCount.ContainsKey(capabilityName))
                    {
                        capabilityNameCount[capabilityName]++;
                    }
                    else
                    {
                        capabilityNameCount[capabilityName] = 1;
                    }
                }
            }
            bool hasCollision = false;
            foreach (var kvp in capabilityNameCount)
            {
                if (kvp.Value > 1)
                {
                    _logger.LogError("Capability name '{CapabilityName}' is registered {Count} times. Please ensure each capability has a unique name.", kvp.Key, kvp.Value);
                    hasCollision = true;
                }
            }
            if(hasCollision)
            {
                throw new InvalidOperationException("Capability name collision detected. Please check the logs for details.");
            }

        }
    }
}
