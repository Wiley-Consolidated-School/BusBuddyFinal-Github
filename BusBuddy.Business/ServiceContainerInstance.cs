using Microsoft.Extensions.DependencyInjection;
using BusBuddy.Data;
using System;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using MSServiceProvider = Microsoft.Extensions.DependencyInjection.ServiceProvider;

namespace BusBuddy.Business
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
            // Register EF Core DbContext - ONLY ONCE to avoid configuration conflicts
            // Temporarily hardcode connection string to bypass duplicate config issue
            var connectionString = "Server=.\\SQLEXPRESS01;Database=BusBuddy;Trusted_Connection=True;TrustServerCertificate=True;";

            services.AddDbContext<BusBuddyContext>(options =>
            {
                options.UseSqlServer(connectionString);
            }, ServiceLifetime.Scoped);

            // Register Business services
            services.AddScoped<IDatabaseHelperService, DatabaseHelperService>();
            services.AddScoped<IRouteAnalyticsService, RouteAnalyticsService>();
            services.AddScoped<IPredictiveMaintenanceService, PredictiveMaintenanceService>();
            services.AddScoped<IVehicleService, VehicleService>();
            // Note: ValidationService removed until it properly implements IValidationService

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
        /// Gets a required service of the specified type (throws if not found)
        /// </summary>
        public T GetRequiredService<T>() where T : notnull
        {
            return _serviceProvider.GetRequiredService<T>();
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
