using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusBuddy.Data;
using BusBuddy.UI.Services;
using BusBuddy.UI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Syncfusion.Licensing;
using Syncfusion.Windows.Forms;

namespace BusBuddy
{
    internal static class Program
    {
        [STAThread]
        static async Task Main(string[] args)
        {
            // Syncfusion license registration
            SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXlcdHRdRGNcWENxXkZWYUA=");

            // Configure host
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    Console.WriteLine($"Connection string: {context.Configuration.GetConnectionString("BusBuddyDB")}");
                })
                .ConfigureServices((context, services) =>
                {
                    // Register configuration
                    services.AddSingleton<IConfiguration>(context.Configuration);

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

                    // Register UI forms
                    services.AddScoped<Dashboard>();
                    services.AddScoped<RouteManagementFormSyncfusion>(provider =>
                    {
                        var serviceProvider = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IServiceProvider>(provider);
                        var routeRepository = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IRouteRepository>(provider);
                        var busRepository = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IBusRepository>(provider);
                        var driverRepository = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IDriverRepository>(provider);
                        var messageService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IMessageService>(provider);
                        return new RouteManagementFormSyncfusion(serviceProvider, routeRepository, busRepository, driverRepository, messageService);
                    });
                    services.AddScoped<ActivityManagementForm>();
                    services.AddScoped<ActivityScheduleManagementForm>();
                    services.AddScoped<DriverManagementForm>();
                    services.AddScoped<DriverPayConfigForm>(provider =>
                    {
                        var serviceProvider = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IServiceProvider>(provider);
                        return new DriverPayConfigForm(serviceProvider);
                    });
                })
                .Build();

            // Validate DI
            using (var scope = host.Services.CreateScope())
            {
                var provider = scope.ServiceProvider;
                try
                {
                    Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IConfiguration>(provider);
                    Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IBusRepository>(provider);
                    Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<Dashboard>(provider);
                    Console.WriteLine("DI validation successful");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DI validation failed: {ex.Message}");
                    throw;
                }

                // Start main form
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
                var mainForm = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<Dashboard>(provider);
                Application.Run(mainForm);
            }

            await host.RunAsync();
        }
    }
}

