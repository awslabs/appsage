using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Bookstore.Web
{
    /// <summary>
    /// Logging configuration setup for the Bookstore web application
    /// </summary>
    public class LoggingSetup
    {
        /// <summary>
        /// Configure logging services for the application
        /// </summary>
        /// <param name="services">The service collection to configure</param>
        /// <param name="configuration">The application configuration</param>
        public static void ConfigureLogging(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(builder =>
            {
                // Clear default providers
                builder.ClearProviders();

                // Add console logging
                builder.AddConsole(options =>
                {
                    options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
                });

                // Add debug logging in development
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    builder.AddDebug();
                }

                // Add file logging
                var logPath = configuration.GetValue<string>("Logging:FilePath") ?? "logs/bookstore.log";
                var logDirectory = Path.GetDirectoryName(logPath);
                
                if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // Set logging levels from configuration
                builder.AddConfiguration(configuration.GetSection("Logging"));
            });
        }
    }
}
