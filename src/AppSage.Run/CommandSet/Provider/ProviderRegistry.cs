using AppSage.Core.Metric;
using AppSage.Infrastructure;
using AppSage.Infrastructure.AI;
using AppSage.Providers.DotNet.BasicCodeAnalysis;
using AppSage.Providers.DotNet.DependencyAnalysis;
using AppSage.Providers.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace AppSage.Run.CommandSet.Provider
{
    public static class ProviderRegistry
    {
        public static IServiceCollection RegisterProviders(IServiceCollection services)
        {
            //register infrastructure components
            services.AddSingleton<IAWSCredentialProvider, AWSCredentialProvider>();
            services.AddSingleton<IAIQuery, BedrockService>();
            services.AddSingleton<IAIQuery, OllamaService>();


            // Register metric providers
            //services.AddTransient<IMetricProvider, RepositoryMetricProvider>();
            //services.AddTransient<IMetricProvider, GitMetricProvider>();
            //services.AddTransient<IMetricProvider, DotNetBasicCodeAnalysisProvider>();
            services.AddTransient<IMetricProvider, DotNetDependencyAnalysisProvider>();
            //services.AddTransient<IMetricProvider, DotNetAIAnalysisProvider>();

            
            // Add more providers as needed
            return services;
        }
    }
}
