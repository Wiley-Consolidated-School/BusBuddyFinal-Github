using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusBuddy.Data;
using BusBuddy.UI.Services;
using BusBuddy.UI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Syncfusion.Licensing;
using Syncfusion.Windows.Forms;

namespace BusBuddy
{
    internal static class Program
    {
        private static ILogger _logger;

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

            var initialLoggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            _logger = initialLoggerFactory.CreateLogger(typeof(Program));
            _logger.LogInformation("Starting BusBuddy application at {Time}", DateTime.Now);

            try
            {
                var syncfusionKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
                if (!string.IsNullOrWhiteSpace(syncfusionKey))
                {
                    SyncfusionLicenseProvider.RegisterLicense(syncfusionKey);
                    _logger.LogInformation("Syncfusion license registered from environment variable.");
                }
                else
                {
                    _logger.LogWarning("Syncfusion license key not found in environment variables.");
                }

                var host = Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                        DatabaseConfiguration.Initialize(config.Build());
                        var builtConfig = config.Build();
                        var connectionString = builtConfig.GetConnectionString("BusBuddyDB");
                        _logger.LogDebug("Configuration loaded with connection string: {ConnectionString}", connectionString ?? "Not found");
                    })
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Debug);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        services.AddSingleton<IConfiguration>(context.Configuration);
                        services.AddLogging();
                        services.AddScoped<IBusRepository, BusRepository>();
                        services.AddScoped<IRouteRepository, RouteRepository>();
                        services.AddScoped<IDriverRepository, DriverRepository>();
                        services.AddScoped<IActivityRepository, ActivityRepository>();
                        services.AddScoped<IFuelRepository, FuelRepository>();
                        services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
                        services.AddScoped<IActivityScheduleRepository, ActivityScheduleRepository>();
                        services.AddScoped<IMessageService, MessageBoxService>();
                        services.AddScoped<RouteManagementFormSyncfusion>(provider =>
                        {
                            var sp = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IServiceProvider>(provider);
                            var routeRepo = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IRouteRepository>(provider);
                            var busRepo = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IBusRepository>(provider);
                            var driverRepo = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IDriverRepository>(provider);
                            var msgService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IMessageService>(provider);
                            return new RouteManagementFormSyncfusion(sp, routeRepo, busRepo, driverRepo, msgService);
                        });
                        services.AddScoped<DriverManagementFormSyncfusion>(provider =>
                        {
                            var sp = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IServiceProvider>(provider);
                            var driverRepo = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IDriverRepository>(provider);
                            var msgService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IMessageService>(provider);
                            return new DriverManagementFormSyncfusion(sp, driverRepo, msgService);
                        });
                        services.AddScoped<RouteFormSyncfusion>(provider =>
                        {
                            var logger = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ILogger<RouteFormSyncfusion>>(provider);
                            var sp = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IServiceProvider>(provider);
                            return new RouteFormSyncfusion(sp, logger);
                        });
                        services.AddScoped<BusBuddyContext>(provider => DatabaseConfiguration.CreateContext());
                    })
                    .Build();

                using (var scope = host.Services.CreateScope())
                {
                    var provider = scope.ServiceProvider;
                    var loggerFactory = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ILoggerFactory>(provider);
                    _logger = loggerFactory.CreateLogger(typeof(Program));
                    _logger.LogInformation("Host services initialized.");

                    try
                    {
                        Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IConfiguration>(provider);
                        Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IBusRepository>(provider);
                        _logger.LogInformation("DI validation successful");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "DI validation failed");
                        throw;
                    }

                    var mainForm = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<RouteManagementFormSyncfusion>(provider);
                    Application.Run(mainForm);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogCritical(ex, "Application failed to start");
                MessageBox.Show($"Application failed to start: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

