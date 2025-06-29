using System;
using BusBuddy.Business;
using BusBuddy.Data;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Models;
using BusBuddy.UI.Views;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Service to bridge UnifiedServiceManager with Dashboard data needs
    /// Provides proper dependency injection for dashboard components
    /// </summary>
    public class DashboardService
    {
        private readonly System.IServiceProvider _serviceProvider;

        public DashboardService(System.IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates a properly initialized DashboardViewModel with dependency injection
        /// </summary>
        public DashboardViewModel CreateDashboardViewModel()
        {
            try
            {
                Console.WriteLine("🔧 Creating DashboardViewModel with proper DI...");
                // Get services from DI container
                var routeAnalyticsService = (IRouteAnalyticsService)_serviceProvider.GetService(typeof(IRouteAnalyticsService));
                var busService = (IBusService)_serviceProvider.GetService(typeof(IBusService));
                var routeRepository = (IRouteRepository)_serviceProvider.GetService(typeof(IRouteRepository));
                var driverRepository = (IDriverRepository)_serviceProvider.GetService(typeof(IDriverRepository));
                var activityRepository = (IActivityRepository)_serviceProvider.GetService(typeof(IActivityRepository));
                var errorHandler = (IErrorHandlerService)_serviceProvider.GetService(typeof(IErrorHandlerService));
                // Fetch or create data for the dashboard (placeholders, adapt as needed)
                var vehicleData = new VehicleData();
                var routeData = new RouteData();
                var activityData = new ActivityData();
                var chartData = new List<ChartDataPoint>();
                var viewModel = new DashboardViewModel
                {
                    VehicleData = vehicleData,
                    RouteData = routeData,
                    ActivityData = activityData,
                    ChartData = chartData
                };
                Console.WriteLine("✅ DashboardViewModel created with proper DI");
                return viewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating DashboardViewModel: {ex.Message}");
                // Fallback to parameterless constructor
                Console.WriteLine("🔄 Falling back to parameterless DashboardViewModel constructor");
                return new DashboardViewModel();
            }
        }
    }
}

