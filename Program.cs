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
            // Set up Windows Forms rendering BEFORE any form/control/DI is created
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

            try
            {
                // Syncfusion license registration
                SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXlcdHRdRGNcWENxXkZWYUA=");
                _logger?.LogInformation("Syncfusion license registered successfully.");

                // Ensure database is online
                EnsureDatabaseOnline();

                // Configure host
                var host = Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                        var connectionString = context.Configuration.GetConnectionString("BusBuddyDB");
                        Console.WriteLine($"Connection string: {connectionString}");
                        _logger?.LogDebug("Configuration loaded with connection string.");
                    })
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Debug);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        // Register configuration and logger
                        services.AddSingleton<IConfiguration>(context.Configuration);
                        services.AddLogging();

                        // Register repositories
                        services.AddScoped<IBusRepository, BusRepository>();
                        services.AddScoped<IRouteRepository, RouteRepository>();
                        services.AddScoped<IDriverRepository, DriverRepository>();
                        services.AddScoped<IActivityRepository, ActivityRepository>();
                        services.AddScoped<IFuelRepository, FuelRepository>();
                        services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
                        services.AddScoped<IActivityScheduleRepository, ActivityScheduleRepository>();

                        // Register UI messaging
                        services.AddScoped<IMessageService, MessageBoxService>();

                        // Register UI forms (use only existing forms)
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
                    })
                    .Build();

                // Get logger
                using (var scope = host.Services.CreateScope())
                {
                    var provider = scope.ServiceProvider;
                    // In the DI scope, get logger factory and create logger for Program
                    var loggerFactory = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ILoggerFactory>(provider);
                    _logger = loggerFactory.CreateLogger("Program");

                    // Validate DI
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

                    // Start main form (use RouteManagementFormSyncfusion as main form)
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

        private static void EnsureDatabaseOnline()
        {
            var connectionString = "Server=localhost\\SQLEXPRESS;Database=master;Trusted_Connection=True;";
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            try
            {
                connection.Open();
                using var command = new Microsoft.Data.SqlClient.SqlCommand(
                    "IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'BusBuddy' AND state_desc = 'OFFLINE') " +
                    "ALTER DATABASE [BusBuddy] SET ONLINE;", connection);
                command.ExecuteNonQuery();
                _logger?.LogInformation("Database online check completed successfully.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to ensure database is online");
                throw;
            }
        }
    }
}
