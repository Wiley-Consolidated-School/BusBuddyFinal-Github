using Microsoft.Extensions.DependencyInjection;
using BusBuddy.UI.Services;
using BusBuddy.Business;
using BusBuddy.TimeCard.Services.Services;
using BusBuddy.Data;
using System;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
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
            // Register EF Core DbContext
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString
                ?? "Server=.\\SQLEXPRESS01;Database=BusBuddy;Trusted_Connection=True;TrustServerCertificate=True;";

            services.AddDbContext<BusBuddyContext>(options =>
                options.UseSqlServer(connectionString));

            // Register services
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddScoped<BusBuddy.Business.IDatabaseHelperService, BusBuddy.Business.DatabaseHelperService>();
            services.AddScoped<BusBuddy.UI.Services.IDatabaseHelperService, BusBuddy.UI.Services.DatabaseHelperService>();
            services.AddScoped<RouteAnalyticsService>();
            services.AddScoped<PredictiveMaintenanceService>();
            services.AddScoped<TimeEntryValidationService>();
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
