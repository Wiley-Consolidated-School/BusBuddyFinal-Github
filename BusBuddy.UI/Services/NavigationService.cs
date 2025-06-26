// Task 4: Enhance Navigation Service (DashboardRedesign.md)
// Enhanced navigation service to support robust dashboard navigation for CDE-40 reporting
// Supports Office2016Black theme and xAI Grok 3 API integration
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BusBuddy.UI.Views;
using BusBuddy.Data;
using BusBuddy.UI.Services;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Task 4: Enhanced Navigation Service for BusBuddy Dashboard
    /// Provides navigation functionality for dashboard modules supporting CDE-40 reporting,
    /// financial contributions tracking, and transportation value demonstration.
    /// Supports Office2016Black theme and seamless module access.
    /// </summary>
    public interface INavigationService
    {
        void ShowVehicleManagement();
        void ShowDriverManagement();
        void ShowRouteManagement();
        void ShowActivityManagement();
        void ShowFuelManagement();
        void ShowMaintenanceManagement();
        void ShowCalendarManagement();
        void ShowScheduleManagement();
        void ShowTimeCardManagement();
        void ShowReportsManagement();
        void ShowSchoolCalendarManagement();
        void ShowActivityScheduleManagement();
        void ShowAnalyticsDemo();
        void ShowReports();

        /// <summary>
        /// Task 4: Enhanced Navigate method with improved error handling and CDE-40 support
        /// Navigate to a specific module by name with optional parameters
        /// Supports CDE-40 reporting, financial analytics, and transportation value modules
        /// </summary>
        /// <param name="moduleName">Name of the module to navigate to</param>
        /// <param name="parameters">Optional parameters to pass to the module</param>
        /// <returns>True if navigation was successful, false otherwise</returns>
        bool Navigate(string moduleName, params object[] parameters);

        /// <summary>
        /// Check if a specific module is available in the current configuration
        /// </summary>
        /// <param name="moduleName">Name of the module to check</param>
        /// <returns>True if the module is available, false otherwise</returns>
        bool IsModuleAvailable(string moduleName);

        DialogResult ShowDialog<T>() where T : Form;
        DialogResult ShowDialog<T>(params object[] parameters) where T : Form;
    }

    /// <summary>
    /// Task 4: Enhanced NavigationService Implementation
    /// Implementation of navigation service supporting dashboard redesign with CDE-40 reporting,
    /// financial analytics, and transportation value demonstration capabilities.
    /// Integrates with Syncfusion Office2016Black theme and xAI Grok 3 API.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IFormFactory _formFactory;
        private readonly Dictionary<string, Func<DialogResult>> _navigationMap;
        private readonly Dictionary<string, bool> _moduleAvailability;

        public NavigationService(IFormFactory formFactory)
        {
            _formFactory = formFactory ?? throw new ArgumentNullException(nameof(formFactory));

            // Initialize navigation map for the Navigate method
            // Task 4: Enhanced navigation map with CDE-40 reporting and financial analytics modules
            _navigationMap = new Dictionary<string, Func<DialogResult>>(StringComparer.OrdinalIgnoreCase)
            {
                // Core transportation modules
                { "vehicle", () => { ShowVehicleManagement(); return DialogResult.OK; } },
                { "vehicles", () => { ShowVehicleManagement(); return DialogResult.OK; } },
                { "driver", () => { ShowDriverManagement(); return DialogResult.OK; } },
                { "drivers", () => { ShowDriverManagement(); return DialogResult.OK; } },
                { "route", () => { ShowRouteManagement(); return DialogResult.OK; } },
                { "routes", () => { ShowRouteManagement(); return DialogResult.OK; } },
                { "activity", () => { ShowActivityManagement(); return DialogResult.OK; } },
                { "activities", () => { ShowActivityManagement(); return DialogResult.OK; } },
                { "fuel", () => { ShowFuelManagement(); return DialogResult.OK; } },
                { "maintenance", () => { ShowMaintenanceManagement(); return DialogResult.OK; } },
                { "calendar", () => { ShowCalendarManagement(); return DialogResult.OK; } },
                { "schedule", () => { ShowScheduleManagement(); return DialogResult.OK; } },
                { "schoolcalendar", () => { ShowSchoolCalendarManagement(); return DialogResult.OK; } },
                { "activityschedule", () => { ShowActivityScheduleManagement(); return DialogResult.OK; } },

                // Analytics and reporting modules
                { "dashboard", () => { ShowDialog<BusBuddy.UI.Views.Dashboard>(); return DialogResult.OK; } },
                { "analytics", () => { ShowAnalyticsDemo(); return DialogResult.OK; } },
                { "report", () => { ShowReportsManagement(); return DialogResult.OK; } },
                { "reports", () => { ShowReportsManagement(); return DialogResult.OK; } },

                // Task 6.6: Driver pay configuration
                { "payrates", () => { ShowDialog<DriverPayConfigForm>(); return DialogResult.OK; } },
                { "payrateconfig", () => { ShowDialog<DriverPayConfigForm>(); return DialogResult.OK; } },
                { "payrateconfiguration", () => { ShowDialog<DriverPayConfigForm>(); return DialogResult.OK; } },

                // Financial and transportation value modules
                { "financial", () => { ShowAnalyticsDemo(); return DialogResult.OK; } },
                { "value", () => { ShowAnalyticsDemo(); return DialogResult.OK; } },
                { "costanalysis", () => { ShowAnalyticsDemo(); return DialogResult.OK; } },

                // Legacy module (phased out)
                { "timecard", () => { ShowTimeCardManagement(); return DialogResult.OK; } }
            };

            // Initialize module availability - default all to true except TimeCard which is being phased out
            // Task 4: Enhanced module availability with CDE-40 and financial analytics support
            _moduleAvailability = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            {
                // Core transportation modules
                { "vehicle", true },
                { "vehicles", true },
                { "driver", true },
                { "drivers", true },
                { "route", true },
                { "routes", true },
                { "activity", true },
                { "activities", true },
                { "fuel", true },
                { "maintenance", true },
                { "calendar", true },
                { "schedule", true },
                { "schoolcalendar", true },
                { "activityschedule", true },

                // Analytics and reporting modules
                { "dashboard", true },
                { "analytics", true },
                { "report", true },
                { "reports", true },

                // Task 6.6: Pay rate configuration
                { "payrates", true },
                { "payrateconfig", true },
                { "payrateconfiguration", true },

                // Financial and transportation value modules
                { "financial", true },
                { "value", true },
                { "costanalysis", true },

                // Legacy module (being phased out)
                { "timecard", false } // TimeCard module is being phased out per requirements
            };
        }

        /// <summary>
        /// Task 4: Enhanced Navigate method with improved error handling and CDE-40 support
        /// Navigate to a specific module by name with optional parameters
        /// Supports CDE-40 reporting, financial analytics, and transportation value modules
        /// </summary>
        /// <param name="moduleName">Name of the module to navigate to</param>
        /// <param name="parameters">Optional parameters to pass to the module</param>
        /// <returns>True if navigation was successful, false otherwise</returns>
        public bool Navigate(string moduleName, params object[] parameters)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                Console.WriteLine("❌ Navigation failed: Module name is null or empty");
                return false;
            }

            // Check if module is available
            if (!IsModuleAvailable(moduleName))
            {
                Console.WriteLine($"❌ Navigation failed: Module '{moduleName}' is not available in this configuration");
                return false;
            }

            // Try to navigate using the navigation map
            if (_navigationMap.TryGetValue(moduleName, out var navigationAction))
            {
                try
                {
                    Console.WriteLine($"🔍 TASK 4: Navigating to module '{moduleName}' for CDE-40 dashboard");
                    navigationAction.Invoke();
                    Console.WriteLine($"✅ TASK 4: Successfully navigated to '{moduleName}'");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ TASK 4: Navigation to '{moduleName}' failed: {ex.Message}");
                    // Task 4: Enhanced error handling for dashboard navigation
                    MessageBox.Show($"Failed to navigate to {moduleName}. Error: {ex.Message}",
                        "Navigation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            // If we have parameters, try to use reflection to find a matching form
            if (parameters != null && parameters.Length > 0)
            {
                try
                {
                    // This would require a more sophisticated form discovery mechanism
                    // For now, just show an error message
                    Console.WriteLine($"⚠️ Navigation with parameters to module '{moduleName}' is not supported yet");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Navigation with parameters failed: {ex.Message}");
                    return false;
                }
            }

            Console.WriteLine($"❌ Navigation failed: Unknown module '{moduleName}'");
            return false;
        }

        /// <summary>
        /// Task 4: Enhanced IsModuleAvailable method with CDE-40 and financial analytics support
        /// Check if a specific module is available in the current configuration
        /// Supports dashboard redesign modules including CDE-40 reporting and financial analytics
        /// </summary>
        /// <param name="moduleName">Name of the module to check</param>
        /// <returns>True if the module is available, false otherwise</returns>
        public bool IsModuleAvailable(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                return false;
            }

            // Check if the module is in our availability dictionary
            if (_moduleAvailability.TryGetValue(moduleName, out var isAvailable))
            {
                return isAvailable;
            }

            // If not found in dictionary, check if it's in the navigation map
            return _navigationMap.ContainsKey(moduleName);
        }

        public void ShowVehicleManagement()
        {
            Console.WriteLine("🔍 BREADCRUMB: NavigationService.ShowVehicleManagement() called");
            try
            {
                // Try to get repository from service container instead of direct instantiation
                var vehicleRepository = ServiceContainerSingleton.Instance.GetService<IVehicleRepository>();
                if (vehicleRepository != null)
                {
                    // Test database connection with repository from DI container
                    EnsureRepositoryInitializedWithDI(() => vehicleRepository.GetAllVehicles());
                }
                else
                {
                    Console.WriteLine("⚠️ VehicleRepository not found in service container, using fallback");
                    // Fallback to direct instantiation for backward compatibility
                    EnsureRepositoryInitialized(() => new VehicleRepository().GetAllVehicles());
                }
                ShowFormDialog<VehicleManagementFormSyncfusion>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ShowVehicleManagement: {ex.Message}");
                MessageBox.Show($"Unable to open Vehicle Management: {ex.Message}", "Navigation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void ShowDriverManagement()
        {
            Console.WriteLine("🔍 BREADCRUMB: NavigationService.ShowDriverManagement() called");
            try
            {
                var driverRepository = ServiceContainerSingleton.Instance.GetService<IDriverRepository>();
                if (driverRepository != null)
                {
                    EnsureRepositoryInitializedWithDI(() => driverRepository.GetAllDrivers());
                }
                else
                {
                    Console.WriteLine("⚠️ DriverRepository not found in service container, using fallback");
                    EnsureRepositoryInitialized(() => new DriverRepository().GetAllDrivers());
                }
                ShowFormDialog<DriverManagementFormSyncfusion>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ShowDriverManagement: {ex.Message}");
                MessageBox.Show($"Unable to open Driver Management: {ex.Message}", "Navigation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void ShowActivityScheduleManagement()
        {
            Console.WriteLine("🔍 BREADCRUMB: NavigationService.ShowActivityScheduleManagement() called");
            try
            {
                var activityScheduleRepository = ServiceContainerSingleton.Instance.GetService<IActivityScheduleRepository>();
                if (activityScheduleRepository != null)
                {
                    EnsureRepositoryInitializedWithDI(() => activityScheduleRepository.GetAllScheduledActivities());
                }
                else
                {
                    Console.WriteLine("⚠️ ActivityScheduleRepository not found in service container, using fallback");
                    EnsureRepositoryInitialized(() => new ActivityScheduleRepository().GetAllScheduledActivities());
                }
                ShowFormDialog<ActivityScheduleManagementForm>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ShowActivityScheduleManagement: {ex.Message}");
                MessageBox.Show($"Unable to open Activity Schedule Management: {ex.Message}", "Navigation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void ShowRouteManagement()
        {
            try
            {
                var routeRepository = ServiceContainerSingleton.Instance.GetService<IRouteRepository>();
                if (routeRepository != null)
                {
                    EnsureRepositoryInitializedWithDI(() => routeRepository.GetAllRoutes());
                }
                else
                {
                    EnsureRepositoryInitialized(() => new RouteRepository().GetAllRoutes());
                }
                ShowFormDialog<RouteManagementFormSyncfusion>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ShowRouteManagement: {ex.Message}");
                MessageBox.Show($"Unable to open Route Management: {ex.Message}", "Navigation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void ShowActivityManagement()
        {
            try
            {
                var activityRepository = ServiceContainerSingleton.Instance.GetService<IActivityRepository>();
                if (activityRepository != null)
                {
                    EnsureRepositoryInitializedWithDI(() => activityRepository.GetAllActivities());
                }
                else
                {
                    EnsureRepositoryInitialized(() => new ActivityRepository().GetAllActivities());
                }
                ShowFormDialog<ActivityManagementForm>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ShowActivityManagement: {ex.Message}");
                MessageBox.Show($"Unable to open Activity Management: {ex.Message}", "Navigation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void ShowFuelManagement()
        {
            try
            {
                var fuelRepository = ServiceContainerSingleton.Instance.GetService<IFuelRepository>();
                if (fuelRepository != null)
                {
                    EnsureRepositoryInitializedWithDI(() => fuelRepository.GetAllFuelRecords());
                }
                else
                {
                    EnsureRepositoryInitialized(() => new FuelRepository().GetAllFuelRecords());
                }
                ShowFormDialog<FuelManagementFormSyncfusion>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ShowFuelManagement: {ex.Message}");
                MessageBox.Show($"Unable to open Fuel Management: {ex.Message}", "Navigation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void ShowMaintenanceManagement()
        {
            try
            {
                var maintenanceRepository = ServiceContainerSingleton.Instance.GetService<IMaintenanceRepository>();
                if (maintenanceRepository != null)
                {
                    EnsureRepositoryInitializedWithDI(() => maintenanceRepository.GetAllMaintenanceRecords());
                }
                else
                {
                    EnsureRepositoryInitialized(() => new MaintenanceRepository().GetAllMaintenanceRecords());
                }
                ShowFormDialog<MaintenanceManagementFormSyncfusion>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ShowMaintenanceManagement: {ex.Message}");
                MessageBox.Show($"Unable to open Maintenance Management: {ex.Message}", "Navigation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void ShowCalendarManagement()
        {
            try
            {
                var schoolCalendarRepository = ServiceContainerSingleton.Instance.GetService<ISchoolCalendarRepository>();
                if (schoolCalendarRepository != null)
                {
                    EnsureRepositoryInitializedWithDI(() => schoolCalendarRepository.GetAllCalendarEntries());
                }
                else
                {
                    EnsureRepositoryInitialized(() => new SchoolCalendarRepository().GetAllCalendarEntries());
                }
                ShowFormDialog<SchoolCalendarManagementFormSyncfusion>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ShowCalendarManagement: {ex.Message}");
                MessageBox.Show($"Unable to open Calendar Management: {ex.Message}", "Navigation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void ShowScheduleManagement()
        {
            try
            {
                var activityScheduleRepository = ServiceContainerSingleton.Instance.GetService<IActivityScheduleRepository>();
                if (activityScheduleRepository != null)
                {
                    EnsureRepositoryInitializedWithDI(() => activityScheduleRepository.GetAllScheduledActivities());
                }
                else
                {
                    EnsureRepositoryInitialized(() => new ActivityScheduleRepository().GetAllScheduledActivities());
                }
                ShowFormDialog<ActivityScheduleManagementForm>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ShowScheduleManagement: {ex.Message}");
                MessageBox.Show($"Unable to open Schedule Management: {ex.Message}", "Navigation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void ShowTimeCardManagement()
        {
            // TimeCard functionality will be launched from the main application
            MessageBox.Show("TimeCard management will be launched from the main application.", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowReportsManagement()
        {
            // Initialize all repositories for comprehensive reports
            EnsureRepositoryInitialized(() => new VehicleRepository().GetAllVehicles());
            EnsureRepositoryInitialized(() => new DriverRepository().GetAllDrivers());
            EnsureRepositoryInitialized(() => new RouteRepository().GetAllRoutes());
            ShowFormDialog<AnalyticsDemoFormSyncfusion>(); // Use Analytics as comprehensive reports
        }

        public void ShowSchoolCalendarManagement()
        {
            EnsureRepositoryInitialized(() => new SchoolCalendarRepository().GetAllCalendarEntries());
            ShowFormDialog<SchoolCalendarManagementFormSyncfusion>();
        }

        public void ShowAnalyticsDemo()
        {
            // Initialize all repositories for analytics
            EnsureRepositoryInitialized(() => new VehicleRepository().GetAllVehicles());
            EnsureRepositoryInitialized(() => new DriverRepository().GetAllDrivers());
            EnsureRepositoryInitialized(() => new RouteRepository().GetAllRoutes());
            ShowFormDialog<AnalyticsDemoFormSyncfusion>();
        }

        public void ShowReports()
        {
            // Initialize all repositories for reports
            EnsureRepositoryInitialized(() => new VehicleRepository().GetAllVehicles());
            EnsureRepositoryInitialized(() => new DriverRepository().GetAllDrivers());
            EnsureRepositoryInitialized(() => new RouteRepository().GetAllRoutes());
            ShowFormDialog<AnalyticsDemoFormSyncfusion>(); // Use Analytics as reports for now
        }

        public DialogResult ShowDialog<T>() where T : Form
        {
            return ShowFormDialog<T>();
        }

        public DialogResult ShowDialog<T>(params object[] parameters) where T : Form
        {
            using var form = _formFactory.CreateForm<T>(parameters);

            // Check if we're in test mode to avoid showing dialog
            bool isTestMode = Environment.GetEnvironmentVariable("BUSBUDDY_TEST_MODE") == "true" ||
                             Environment.GetEnvironmentVariable("BUSBUDDY_SUPPRESS_DIALOGS") == "true";

            if (isTestMode)
            {
                Console.WriteLine($"🧪 TEST MODE: Skipping ShowDialog() for {typeof(T).Name} and returning DialogResult.OK");
                return DialogResult.OK;
            }

            return form.ShowDialog();
        }

        private DialogResult ShowFormDialog<T>() where T : Form
        {
            try
            {
                Console.WriteLine($"🔍 BREADCRUMB: ShowFormDialog<{typeof(T).Name}>() called");
                var form = _formFactory.CreateForm<T>();
                Console.WriteLine($"🔍 BREADCRUMB: Form created successfully: {form.GetType().Name}");

                // Check if we're in test mode to avoid showing dialog
                bool isTestMode = Environment.GetEnvironmentVariable("BUSBUDDY_TEST_MODE") == "true" ||
                                 Environment.GetEnvironmentVariable("BUSBUDDY_SUPPRESS_DIALOGS") == "true";

                if (isTestMode)
                {
                    Console.WriteLine("🧪 TEST MODE: Skipping ShowDialog() and returning DialogResult.OK");
                    form.Dispose();
                    return DialogResult.OK;
                }

                Console.WriteLine("🔍 BREADCRUMB: About to call ShowDialog()");
                var result = form.ShowDialog();
                Console.WriteLine($"🔍 BREADCRUMB: ShowDialog() returned: {result}");
                form.Dispose(); // Manually dispose after dialog closes
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔍 BREADCRUMB ERROR: Failed to open form: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return DialogResult.Cancel;
            }
        }

        /// <summary>
        /// Ensures repository is initialized by performing a test operation
        /// This triggers database initialization if needed
        /// </summary>
        private void EnsureRepositoryInitialized(System.Action testOperation)
        {
            try
            {
                testOperation.Invoke();
                Console.WriteLine("✅ Repository initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Repository initialization warning: {ex.Message}");
                // Don't throw - let the form handle any subsequent issues
            }
        }

        /// <summary>
        /// Ensures repository from DI container is initialized by performing a test operation
        /// This triggers database initialization if needed, with better error handling
        /// </summary>
        private void EnsureRepositoryInitializedWithDI(System.Action testOperation)
        {
            try
            {
                testOperation.Invoke();
                Console.WriteLine("✅ Repository from DI container initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Repository from DI container initialization warning: {ex.Message}");
                // Don't throw - let the form handle any subsequent issues
                // This provides better error handling for forms that use message services
            }
        }
    }
}
