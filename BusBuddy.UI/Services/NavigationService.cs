using System;
using System.Windows.Forms;
using BusBuddy.UI.Views;

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
            ShowFormDialog<VehicleManagementFormSyncfusion>();
        }

        public void ShowDriverManagement()
        {
            ShowFormDialog<DriverManagementFormSyncfusion>();
        }

        public void ShowRouteManagement()
        {
            ShowFormDialog<RouteManagementFormSyncfusion>();
        }

        public void ShowActivityManagement()
        {
            ShowFormDialog<ActivityManagementFormSyncfusion>();
        }

        public void ShowFuelManagement()
        {
            ShowFormDialog<FuelManagementFormSyncfusion>();
        }

        public void ShowMaintenanceManagement()
        {
            ShowFormDialog<MaintenanceManagementFormSyncfusion>();
        }        public void ShowCalendarManagement()
        {
            ShowFormDialog<SchoolCalendarManagementFormSyncfusion>();
        }

        public void ShowScheduleManagement()
        {
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
            Console.WriteLine("INFO: Reports Management functionality will be implemented soon.");
        }        public void ShowSchoolCalendarManagement()
        {
            ShowFormDialog<SchoolCalendarManagementFormSyncfusion>();
        }        public void ShowActivityScheduleManagement()
        {
            ShowFormDialog<ActivityScheduleManagementFormSyncfusion>();
        }

        public void ShowAnalyticsDemo()
        {
            // TODO: Implement AnalyticsDemoForm
            Console.WriteLine("INFO: Analytics demo will be implemented soon.");
        }

        public void ShowReports()
        {
            Console.WriteLine("INFO: Reports functionality will be implemented soon.");
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
    }
}
