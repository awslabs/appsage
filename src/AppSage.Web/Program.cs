namespace AppSage.Web
{
    using AppSage.Core.Logging;
    using AppSage.Web.Components.DataExport;
    using AppSage.Web.Components.Filter.Table;
    using Serilog;

    public class Program
    {
        public static void Main(string[] args)
        {
           

            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog
            ConfigureSerilog();

            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<Controllers.SampleController>();
            builder.Services.AddScoped<DataExportController>();

            // Register AppSageLogger
            builder.Services.AddSingleton<ILogger>(Log.Logger);
            builder.Services.AddSingleton<IAppSageLogger, AppSageLogger>();
            // Register table services
            builder.Services.AddSingleton<Dictionary<string, TableModel>>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();
            app.MapControllers(); // Add this line to map controller routes

            app.Run();
        }


        private static void ConfigureSerilog()
        {
            string logFolder = @"C:\temp\logs";
            // Ensure logs directory exists
            Directory.CreateDirectory(logFolder);

            // Create Serilog logger
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(logFolder, "appSage-.log"), rollingInterval: RollingInterval.Day)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithMachineName()
                .CreateLogger();
        }
    }
}
