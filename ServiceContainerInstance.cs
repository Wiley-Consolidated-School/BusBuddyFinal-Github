using Microsoft.Extensions.DependencyInjection;
using BusBuddy.UI.Services;
using BusBuddy.Business;
using BusBuddy.Data;
using System;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using MSServiceProvider = Microsoft.Extensions.DependencyInjection.ServiceProvider;

namespace BusBuddy.DependencyInjection
{
    /// <summary>
    /// Singleton service container for dependency injection throughout BusBuddy application
    /// Provides centralized service registration and resolution
    /// </summary>
    public class ServiceContainerInstance
    {
        private static readonly Lazy<ServiceContainerInstance> _instance = new(() => new ServiceContainerInstance());
        private readonly Microsoft.Extensions.DependencyInjection.ServiceProvider _serviceProvider;

        /// <summary>
        /// Gets the singleton service container instance
        /// </summary>
        public static ServiceContainerInstance Instance => _instance.Value;

        private ServiceContainerInstance()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Configures services and dependencies for the application
        /// </summary>
        private void ConfigureServices(ServiceCollection services)
        {
            try
            {
                // Register EF Core DbContext - ONLY ONCE to avoid configuration conflicts
                // Get connection string from configuration with error handling
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;

                if (string.IsNullOrEmpty(connectionString))
                {
                    // Fallback to hardcoded connection string if configuration fails
                    connectionString = "Server=.\\SQLEXPRESS01;Database=BusBuddy;Trusted_Connection=True;TrustServerCertificate=True;";
                    Console.WriteLine("⚠️ Using fallback connection string");
                }

                Console.WriteLine($"🔗 Using connection string: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");

                services.AddDbContext<BusBuddyContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                    options.EnableSensitiveDataLogging(false); // Disable in production
                    options.EnableServiceProviderCaching(true);
                }, ServiceLifetime.Scoped);

                // Register services with proper error handling
                RegisterUIServices(services);
                RegisterBusinessServices(services);
                RegisterDataServices(services);

                Console.WriteLine("✅ All services registered successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Critical error configuring services: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to prevent silent failures
            }
        }

        private void RegisterUIServices(ServiceCollection services)
        {
            // Register UI services
            services.AddSingleton<BusBuddy.UI.Services.IFormFactory, BusBuddy.UI.Services.ServiceContainer>();
            services.AddSingleton<BusBuddy.UI.Services.INavigationService>(provider =>
                new BusBuddy.UI.Services.NavigationService((BusBuddy.UI.Services.IFormFactory)provider.GetService(typeof(BusBuddy.UI.Services.IFormFactory))));
            services.AddScoped<BusBuddy.UI.Services.IDatabaseHelperService, BusBuddy.UI.Services.DatabaseHelperService>();

            // Task 5: Register Report Service for CDE-40 reporting
            services.AddSingleton<HttpClient>(); // Register HttpClient
            services.AddScoped<BusBuddy.UI.Services.IReportService>(provider =>
                new BusBuddy.UI.Services.ReportService(
                    (BusBuddy.UI.Services.IDatabaseHelperService)provider.GetService(typeof(BusBuddy.UI.Services.IDatabaseHelperService)),
                    (HttpClient)provider.GetService(typeof(HttpClient))));

            // Task 6: Register Analytics Service for CDE-40 reporting and driver pay
            services.AddScoped<BusBuddy.UI.Services.IAnalyticsService>(provider =>
                new BusBuddy.UI.Services.AnalyticsService(
                    (BusBuddy.Business.IDatabaseHelperService)provider.GetService(typeof(BusBuddy.Business.IDatabaseHelperService)),
                    (BusBuddy.Business.IRouteAnalyticsService)provider.GetService(typeof(BusBuddy.Business.IRouteAnalyticsService))));

            // Task 7: Register Error Handler Service
            services.AddScoped<BusBuddy.UI.Services.IErrorHandlerService, BusBuddy.UI.Services.ErrorHandlerService>();
        }

        private void RegisterBusinessServices(ServiceCollection services)
        {
            services.AddScoped<BusBuddy.Business.IDatabaseHelperService, BusBuddy.Business.DatabaseHelperService>();

            // Fix: Register interfaces properly - use fully qualified names to avoid ambiguity
            services.AddScoped<BusBuddy.Business.IRouteAnalyticsService, BusBuddy.Business.RouteAnalyticsService>();
            services.AddScoped<BusBuddy.Business.IPredictiveMaintenanceService, BusBuddy.Business.PredictiveMaintenanceService>();
        }

        private void RegisterDataServices(ServiceCollection services)
        {
            // Register Data repositories
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IDriverRepository, DriverRepository>();
            services.AddScoped<IRouteRepository, RouteRepository>();
            services.AddScoped<IFuelRepository, FuelRepository>();
            services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddScoped<IActivityScheduleRepository, ActivityScheduleRepository>();
            services.AddScoped<ISchoolCalendarRepository, SchoolCalendarRepository>();
        }

        /// <summary>
        /// Gets a service of the specified type
        /// </summary>
        public T GetService<T>()
        {
            return _serviceProvider.GetService<T>();
        }

        /// <summary>
        /// Resets the service container (useful for testing or application shutdown)
        /// </summary>
        public void Reset()
        {
            // Dispose of current service provider and recreate
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
