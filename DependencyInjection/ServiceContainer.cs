using System;
using Microsoft.Extensions.DependencyInjection;
using BusBuddy.Data;
using BusBuddy.Business;

namespace BusBuddy.DependencyInjection
{
    public static class ServiceContainer
    {
        private static IServiceProvider? _serviceProvider;

        public static void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register repositories
            services.AddTransient<IActivityRepository, ActivityRepository>();
            services.AddTransient<IActivityScheduleRepository, ActivityScheduleRepository>();
            services.AddTransient<IDriverRepository, DriverRepository>();
            services.AddTransient<IFuelRepository, FuelRepository>();
            services.AddTransient<IMaintenanceRepository, MaintenanceRepository>();
            services.AddTransient<IRouteRepository, RouteRepository>();
            services.AddTransient<ISchoolCalendarRepository, SchoolCalendarRepository>();
            services.AddTransient<IVehicleRepository, VehicleRepository>();

            // Register business services
            services.AddTransient<IVehicleService, VehicleService>();
            services.AddTransient<IDatabaseHelperService, DatabaseHelperService>();

            _serviceProvider = services.BuildServiceProvider();
        }

        public static T GetService<T>() where T : class
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("Services have not been configured. Call ConfigureServices() first.");
            }

            return _serviceProvider.GetRequiredService<T>();
        }

        public static object GetService(Type serviceType)
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("Services have not been configured. Call ConfigureServices() first.");
            }

            return _serviceProvider.GetRequiredService(serviceType);
        }

        public static void Dispose()
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
