using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Core.Metric;
using AppSage.Core.Workspace;
using AppSage.Infrastructure.Metric;
using AppSage.McpServer.Capability.Tools;
using AppSage.MCPServer.CapabilityBuilder;
using AppSage.MCPServer.Capabilty.Tools.CodeGraph;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Reflection;

namespace AppSage.MCPServer
{
    public class Runner
    {
        public Runner(IServiceCollection services)
        {
            ConfigureServices(services);

        }

        public async Task Run(WebApplicationBuilder builder) {
            var serviceCollection = builder.Services.BuildServiceProvider();
            var logger = serviceCollection.GetService<IAppSageLogger>();
            var config = serviceCollection.GetService<IAppSageConfiguration>();
            var workspace = serviceCollection.GetService<IAppSageWorkspace>();

            logger.LogInformation("Starting AppSage MCP Server for the AppSage workspace[{WorkspaceFolder}]", workspace.RootFolder);

            string listeningUrl = config.Get<string>("AppSage.MCPServer.Runner:ListeningUrl");
            builder.WebHost.UseUrls(listeningUrl);

            var serverCapabilities = serviceCollection.GetService<ServerDiscovery>().CreateServerCapabilities();
 

            builder.Services
                .AddMcpServer(options =>
                {
                    options.Capabilities = serverCapabilities;
                    options.ServerInfo = new Implementation()
                    {
                        Name = "AppSage MCP Server",
                        Version = Assembly.GetExecutingAssembly().GetName().Version.ToString()
                    };
                })
                .WithHttpTransport();

            var app = builder.Build();


            // Streamable HTTP endpoint at /mcp
            app.MapMcp("/mcp");

            // Helpful startup messages - now safe because Console is redirected to stderr
            app.Lifetime.ApplicationStarted.Register(() =>
            {
                logger.LogInformation("MCP server started for the workspace[{WorkspaceFolder}]", workspace.RootFolder);
                logger.LogInformation("[MCP] HTTP endpoint: {Endpoints}", string.Join(", ", app.Urls.Select(url => $"{url.TrimEnd('/')}/mcp")));
            });

            await app.RunAsync();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMetricReader, MetricReader>();
            // Register the ServerCapabilities singleton
            services.AddSingleton<ToolDiscovery>();
            services.AddSingleton<ServerDiscovery>();
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var toolTypes = assembly
                .GetTypes()
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    t.GetCustomAttribute<McpServerToolTypeAttribute>() is not null);

          

            foreach (var type in toolTypes)
            {
                // If already registered, skip (avoid duplicates on multi-calls)
                if (services.Any(d => d.ServiceType == type && d.ImplementationType == type))
                    continue;

                var descriptor = new ServiceDescriptor(type, type, ServiceLifetime.Transient);
                services.Add(descriptor);
            }

            services.AddSingleton<CodeGraphGenerator>();
            services.AddTransient<IDynamicCompiler, DynamicCompiler>();
            
        }
    }
}
