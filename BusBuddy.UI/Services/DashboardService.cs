using System;
using System.Threading.Tasks;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Views;
using BusBuddy.Data;
using BusBuddy.Business;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Service to bridge UnifiedServiceManager with Dashboard data needs
    /// Provides proper dependency injection for dashboard components
    /// </summary>
    public class DashboardService
    {
        private readonly UnifiedServiceManager _serviceManager;

        public DashboardService()
        {
            _serviceManager = UnifiedServiceManager.Instance;
        }

        /// <summary>
        /// Creates a properly initialized DashboardViewModel with dependency injection
        /// </summary>
        public async Task<DashboardViewModel> CreateDashboardViewModelAsync()
        {
            try
            {
                Console.WriteLine("🔧 Creating DashboardViewModel with proper DI...");

                // Ensure services are initialized
                await _serviceManager.EnsureInitializedAsync();

                // Get services from DI container
                var routeAnalyticsService = await _serviceManager.GetServiceAsync<IRouteAnalyticsService>();
                var vehicleRepository = await _serviceManager.GetServiceAsync<IVehicleRepository>();
                var routeRepository = await _serviceManager.GetServiceAsync<IRouteRepository>();
                var driverRepository = await _serviceManager.GetServiceAsync<IDriverRepository>();
                var activityRepository = await _serviceManager.GetServiceAsync<IActivityRepository>();
                var errorHandler = await _serviceManager.GetServiceAsync<IErrorHandlerService>();

                // Create ViewModel with injected dependencies
                var viewModel = new DashboardViewModel(
                    routeAnalyticsService,
                    vehicleRepository,
                    routeRepository,
                    driverRepository,
                    activityRepository,
                    errorHandler
                );

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

        /// <summary>
        /// Gets a service from the unified service manager
        /// </summary>
        public async Task<T> GetServiceAsync<T>() where T : notnull
        {
            return await _serviceManager.GetServiceAsync<T>();
        }

        /// <summary>
        /// Gets a service synchronously (blocks if needed)
        /// </summary>
        public T GetService<T>() where T : notnull
        {
            return _serviceManager.GetService<T>();
        }

        /// <summary>
        /// Checks if services are initialized
        /// </summary>
        public async Task<bool> AreServicesReadyAsync()
        {
            try
            {
                await _serviceManager.EnsureInitializedAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
