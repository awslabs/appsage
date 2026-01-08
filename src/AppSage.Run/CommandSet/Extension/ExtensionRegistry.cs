using AppSage.Core.Metric;
using AppSage.Infrastructure;
using AppSage.Infrastructure.AI;
using AppSage.Infrastructure.Caching;
using AppSage.Providers.DotNet.AIAnalysis;
using AppSage.Providers.DotNet.BasicCodeAnalysis;
using AppSage.Providers.DotNet.DependencyAnalysis;
using Microsoft.Extensions.DependencyInjection;
using AppSage.Infrastructure.AWS;
using AppSage.Providers.BasicRepositoryMetric;
using AppSage.Providers.GitMetric;
using AppSage.Core.Documentation;
using AppSage.Providers.HelloWorld;
namespace AppSage.Run.CommandSet.Extension
{
    public static class ExtensionRegistry
    {
        public static IServiceCollection RegisterProviders(IServiceCollection services)
        {
            //register infrastructure components
            //services.AddSingleton<IAWSCredentialProvider, AWSCredentialProvider>();
            //services.AddSingleton<IAIQuery, BedrockService>();
            //services.AddSingleton<IAIQuery, OllamaService>();
            services.AddSingleton<IAppSageCache, FileSystemCache>();

            // Register metric providers
            //services.AddTransient<IMetricProvider, RepositoryMetricProvider>();
            //services.AddTransient<IMetricProvider, GitMetricProvider>();
            //services.AddTransient<IMetricProvider, DotNetBasicCodeAnalysisProvider>();
            //services.AddTransient<IMetricProvider, HelloWorldProvider>();
            services.AddTransient<IMetricProvider, DotNetDependencyAnalysisProvider>();
            
            //services.AddTransient<IMetricProvider, DotNetAIAnalysisProvider>();
            //Add more providers as needed


            return services;
        }
    }
}
