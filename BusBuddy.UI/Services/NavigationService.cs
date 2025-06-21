using System;
using System.Windows.Forms;
using BusBuddy.UI.Views;
using BusBuddy.Data;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Service for managing form navigation and lifecycle
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

        DialogResult ShowDialog<T>() where T : Form;
        DialogResult ShowDialog<T>(params object[] parameters) where T : Form;
    }

    /// <summary>
    /// Implementation of navigation service using form factory
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IFormFactory _formFactory;

        public NavigationService(IFormFactory formFactory)
        {
            _formFactory = formFactory ?? throw new ArgumentNullException(nameof(formFactory));
        }        public void ShowVehicleManagement()
        {
            Console.WriteLine("🔍 BREADCRUMB: NavigationService.ShowVehicleManagement() called");
            EnsureRepositoryInitialized(() => new VehicleRepository().GetAllVehicles());
            ShowFormDialog<VehicleManagementFormSyncfusion>();
        }

        public void ShowDriverManagement()
        {
            EnsureRepositoryInitialized(() => new DriverRepository().GetAllDrivers());
            ShowFormDialog<DriverManagementFormSyncfusion>();
        }

        public void ShowRouteManagement()
        {
            EnsureRepositoryInitialized(() => new RouteRepository().GetAllRoutes());
            ShowFormDialog<RouteManagementFormSyncfusion>();
        }

        public void ShowActivityManagement()
        {
            EnsureRepositoryInitialized(() => new ActivityRepository().GetAllActivities());
            ShowFormDialog<ActivityManagementFormSyncfusion>();
        }

        public void ShowFuelManagement()
        {
            EnsureRepositoryInitialized(() => new FuelRepository().GetAllFuelRecords());
            ShowFormDialog<FuelManagementFormSyncfusion>();
        }

        public void ShowMaintenanceManagement()
        {
            EnsureRepositoryInitialized(() => new MaintenanceRepository().GetAllMaintenanceRecords());
            ShowFormDialog<MaintenanceManagementFormSyncfusion>();
        }        public void ShowCalendarManagement()
        {
            EnsureRepositoryInitialized(() => new SchoolCalendarRepository().GetAllCalendarEntries());
            ShowFormDialog<SchoolCalendarManagementFormSyncfusion>();
        }

        public void ShowScheduleManagement()
        {
            EnsureRepositoryInitialized(() => new ActivityScheduleRepository().GetAllScheduledActivities());
            ShowFormDialog<ActivityScheduleManagementFormSyncfusion>();
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
        }        public void ShowActivityScheduleManagement()
        {
            EnsureRepositoryInitialized(() => new ActivityScheduleRepository().GetAllScheduledActivities());
            ShowFormDialog<ActivityScheduleManagementFormSyncfusion>();
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
            return form.ShowDialog();
        }

        private DialogResult ShowFormDialog<T>() where T : Form
        {
            try
            {
                Console.WriteLine($"🔍 BREADCRUMB: ShowFormDialog<{typeof(T).Name}>() called");
                var form = _formFactory.CreateForm<T>();
                Console.WriteLine($"🔍 BREADCRUMB: Form created successfully: {form.GetType().Name}");
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
    }
}
