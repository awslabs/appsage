namespace AppSage.Web
{
    using AppSage.Core.Configuration;
    using AppSage.Core.Localization;
    using AppSage.Core.Logging;
    using AppSage.Core.Workspace;
    using AppSage.Infrastructure.Workspace;
    using AppSage.Web.Components.DataExport;
    using AppSage.Web.Components.Filter.Table;
    using Serilog;
    using Serilog.Events;
    using Serilog.Formatting.Display;
    using System.Globalization;
    using System.Xml.Serialization;

    public class Program
    {
        public static void Main(string[] args)
        {
           

            var builder = WebApplication.CreateBuilder(args);
            
            var services=InitializeCoreServices(builder.Services, null);


            builder.Host.UseSerilog();

            // Add services to the container.
            services.AddRazorPages();
            services.AddControllersWithViews();
            services.AddScoped<Controllers.SampleController>();
            services.AddScoped<DataExportController>();
            services.AddSingleton<Dictionary<string, TableModel>>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();            
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();
            app.MapControllers(); // Add this line to map controller routes

            app.Run();
        }


        private static IServiceCollection InitializeCoreServices(IServiceCollection services, IAppSageWorkspace workspace)
        {
            var logger = new LoggerConfiguration()
           .MinimumLevel.Debug()
           .Enrich.FromLogContext()
           .Enrich.WithThreadId()
           .Enrich.WithMachineName();

            //We will always log to the console with the look and feel of a Console.WriteLine for Information level messages. 
            logger.WriteTo.Logger(lc => lc
                        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
                        .WriteTo.Console(formatter: new MessageTemplateTextFormatter("{Message:lj}{NewLine}", null)
            ));
            // Console sink for all *non-Information* levels (default format)
            logger.WriteTo.Logger(lc => lc
                .Filter.ByExcluding(e => e.Level == LogEventLevel.Information)
                .WriteTo.Console() // default Serilog console theme/formatter
            );

            if (workspace != null && !String.IsNullOrEmpty(workspace.LogsFolder) && Directory.Exists(workspace.LogsFolder))
            {
                logger.WriteTo.File(Path.Combine(workspace.LogsFolder, "appSage-.log"), rollingInterval: RollingInterval.Day);
            }
            // Create Serilog logger
            Log.Logger = logger.CreateLogger();


            // Add Serilog
            services.AddSingleton(Log.Logger);
            // Register the logger as a singleton (one instance for the entire application)
            services.AddSingleton<IAppSageLogger, AppSageLogger>();


            IAppSageConfiguration appSageConfiguration = new AppSageConfiguration(AppSageConfiguration.GetDefaultConfigTemplateFilePath());
            services.AddSingleton<IAppSageConfiguration>(appSageConfiguration);

            services.AddSingleton<IAppSageWorkspace>(sp =>
            {
                IAppSageConfiguration config = sp.GetRequiredService<IAppSageConfiguration>();
                string workspaceFolder = config.Get<string>("AppSage.Web:WorkspaceRootFolder");

                return new AppSageWorkspaceManager(new DirectoryInfo(workspaceFolder), sp.GetRequiredService<IAppSageLogger>());
            });

            //Initialize the localization
            var cultureName = Environment.GetEnvironmentVariable("APPSAGE_CULTURE") ?? "en-US";
            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureName);
            // Initialize all LocalizationManager derived classes automatically
            LocalizationManager.InitializeAll();

            return services;
        }

    }
}
