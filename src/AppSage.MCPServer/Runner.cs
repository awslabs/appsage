using AppSage.Core.Configuration;
using AppSage.McpServer.Capability.Tools;
using AppSage.MCPServer.CapabilityBuilder;
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
            var appSageConfig = serviceCollection.GetService<IAppSageConfiguration>();

            string listeningUrl = appSageConfig.Get<string>("AppSage.MCPServer.Runner:ListeningUrl");
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
                Console.WriteLine($"[MCP] HTTP endpoint: {string.Join(", ", app.Urls.Select(url => $"{url.TrimEnd('/')}/mcp"))}");
            });
            await app.RunAsync();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
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

            services.AddTransient<IDynamicCompiler, DynamicCompiler>();
        }
    }
}
