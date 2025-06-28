using System;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BusBuddy.Business;
using BusBuddy.Data;
using BusBuddy.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer; // This provides EnableRetryOnFailure
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// 🚀 UNIFIED SERVICE MANAGER - SINGLE SOURCE OF TRUTH FOR DEPENDENCY INJECTION
    ///
    /// CRITICAL: This is the ONLY service container in BusBuddy application.
    /// Eliminates triple service container anti-pattern and provides unified DI approach.
    ///
    /// REPLACES:
    /// - ServiceContainerInstance (Business layer) - REMOVED
    /// - ServiceContainerSingleton (UI wrapper) - REMOVED
    ///
    /// ALL service resolution must go through this container ONLY.
    /// </summary>
    public class UnifiedServiceManager
    {
        private static readonly Lazy<UnifiedServiceManager> _instance = new(() => new UnifiedServiceManager());
        private ServiceProvider _serviceProvider;
        private readonly SemaphoreSlim _initializationLock = new(1, 1);
        private bool _isInitialized = false;
        private Task _initializationTask;

        /// <summary>
        /// Gets the singleton unified service manager instance - ONLY SERVICE CONTAINER
        ///
        /// CRITICAL: This is the single source of truth for all dependency injection.
        /// Do NOT create or use any other service containers in the application.
        /// </summary>
        public static UnifiedServiceManager Instance => _instance.Value;

        private UnifiedServiceManager()
        {
            // Constructor does MINIMAL work - actual initialization deferred
        }

        /// <summary>
        /// 🔥 TRUE ASYNC SERVICE INITIALIZATION - Solves performance bottleneck
        /// Services are initialized asynchronously on background thread
        /// </summary>
        public async Task<T> GetServiceAsync<T>() where T : notnull
        {
            await EnsureInitializedAsync();
            return Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(_serviceProvider);
        }

        /// <summary>
        /// Synchronous service access for cases where async is not possible
        ///
        /// CENTRAL SERVICE RESOLUTION - Use this throughout the application:
        /// UnifiedServiceManager.Instance.GetService<IVehicleRepository>()
        ///
        /// Replaces all other GetService calls from removed service containers:
        /// - ServiceContainerInstance.Instance.GetService<T>() - REMOVED
        /// - ServiceContainerSingleton.Instance.GetService<T>() - REMOVED
        /// </summary>
        public T GetService<T>() where T : notnull
        {
            if (!_isInitialized)
            {
                // Block on initialization if not already done
                EnsureInitializedAsync().GetAwaiter().GetResult();
            }
            return Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<T>(_serviceProvider);
        }

        /// <summary>
        /// 🚀 SERVICE PRE-WARMING - Starts service initialization immediately
        /// Call this early in application startup to pre-warm services
        /// </summary>
        public Task PreWarmServicesAsync()
        {
            return EnsureInitializedAsync();
        }

        /// <summary>
        /// Ensures services are initialized exactly once using async lock
        /// Now public for direct access from UI components that need to await initialization
        /// </summary>
        public async Task EnsureInitializedAsync()
        {
            if (_isInitialized) return;

            await _initializationLock.WaitAsync();
            try
            {
                if (_isInitialized) return;

                if (_initializationTask == null)
                {
                    _initializationTask = InitializeServicesAsync();
                }

                await _initializationTask;
                _isInitialized = true;
            }
            finally
            {
                _initializationLock.Release();
            }
        }

        /// <summary>
        /// 🔧 STREAMLINED SERVICE REGISTRATION - No duplicate containers
        /// All services registered in single unified container
        /// </summary>
        private async Task InitializeServicesAsync()
        {
            await Task.Run(() =>
            {
                var services = new ServiceCollection();

                // UNIFIED DATABASE CONNECTION - single DbContext configuration
                ConfigureDatabaseServices(services);

                // Register all layers in single container
                RegisterUIServices(services);
                RegisterBusinessServices(services);
                RegisterDataServices(services);

                _serviceProvider = services.BuildServiceProvider();

                Console.WriteLine("✅ UNIFIED SERVICE CONTAINER: All services initialized asynchronously");
            });
        }

        /// <summary>
        /// Configure database services with connection pooling for performance
        /// </summary>
        private void ConfigureDatabaseServices(ServiceCollection services)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString
                ?? "Server=.\\SQLEXPRESS01;Database=BusBuddy;Trusted_Connection=True;TrustServerCertificate=True;";

            services.AddDbContext<BusBuddyContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlServerOptions =>
                {
                    // 🚀 CONNECTION POOLING for better performance
                    sqlServerOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
                    sqlServerOptions.CommandTimeout(60); // Extended timeout to prevent OperationCanceledException
                });

                // 🔍 Enhanced logging for debugging exceptions
                options.EnableSensitiveDataLogging(false);
                options.EnableDetailedErrors(true); // Better error messages for ApplicationException debugging
                options.EnableServiceProviderCaching(true);

                // 📊 Add logging to help diagnose data loading issues
                options.LogTo(message =>
                {
                    if (message.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                        message.Contains("exception", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"🔍 EF Core: {message}");
                    }
                }, Microsoft.Extensions.Logging.LogLevel.Warning);
            }, ServiceLifetime.Scoped);
        }

        private void RegisterUIServices(ServiceCollection services)
        {
            services.AddSingleton<IFormFactory, ServiceContainer>();
            services.AddSingleton<INavigationService>(provider =>
                new NavigationService(Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<IFormFactory>(provider)));

            // 🔧 FIXED: Register renamed UI data service (was IDatabaseHelperService)
            services.AddScoped<IUIDataService, UIDataService>();

            services.AddSingleton<HttpClient>();
            services.AddScoped<IReportService>(provider =>
                new ReportService(
                    Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<IUIDataService>(provider),
                    Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<HttpClient>(provider)));

            // 🔧 FIXED: AnalyticsService now correctly uses Business layer IDatabaseHelperService
            services.AddScoped<IAnalyticsService>(provider =>
                new AnalyticsService(
                    Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<BusBuddy.Business.IDatabaseHelperService>(provider),
                    Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<BusBuddy.Business.IRouteAnalyticsService>(provider)));

            services.AddScoped<IErrorHandlerService, ErrorHandlerService>();

            // 🔧 MISSING SERVICE FIX: ValidationService not yet fully implemented
            // services.AddScoped<BusBuddy.Business.IValidationService, BusBuddy.Business.ValidationService>();

            // 🔧 MISSING SERVICE FIX: Register PayRateManager
            services.AddScoped<PayRateManager>(provider =>
                new PayRateManager(Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<IErrorHandlerService>(provider)));

            // CRITICAL FIX: Register DashboardService for proper Dashboard DI
            services.AddScoped<DashboardService>();
        }

        private void RegisterBusinessServices(ServiceCollection services)
        {
            services.AddScoped<BusBuddy.Business.IDatabaseHelperService, BusBuddy.Business.DatabaseHelperService>();
            services.AddScoped<BusBuddy.Business.IRouteAnalyticsService, BusBuddy.Business.RouteAnalyticsService>();
            services.AddScoped<BusBuddy.Business.IPredictiveMaintenanceService, BusBuddy.Business.PredictiveMaintenanceService>();
            services.AddScoped<BusBuddy.Business.IVehicleService, BusBuddy.Business.VehicleService>();
            services.AddScoped<BusBuddy.Business.IValidationService, BusBuddy.Business.ValidationService>();

            // Register BusService for accessing vehicle data as buses
            services.AddScoped<BusBuddy.Business.IBusService, BusBuddy.Business.BusService>();
        }

        private void RegisterDataServices(ServiceCollection services)
        {
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
        /// 🗄️ DATABASE CONNECTION PRE-WARMING
        /// Warms up database connection pool for instant access
        /// </summary>
        public async Task PreWarmDatabaseAsync()
        {
            try
            {
                await EnsureInitializedAsync();

                using var scope = _serviceProvider.CreateScope();
                var context = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<BusBuddyContext>(scope.ServiceProvider);

                // Simple query to warm up connection
                await context.Database.CanConnectAsync();

                Console.WriteLine("✅ DATABASE PRE-WARMING: Connection pool ready");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ DATABASE PRE-WARMING WARNING: {ex.Message}");
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                _serviceProvider?.Dispose();
                _initializationLock?.Dispose();
                _isInitialized = false;
                Console.WriteLine("✅ UnifiedServiceManager disposed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error disposing UnifiedServiceManager: {ex.Message}");
            }
        }
    }
}
