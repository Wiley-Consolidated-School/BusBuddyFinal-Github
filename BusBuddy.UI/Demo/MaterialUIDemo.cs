using System;
using System.Windows.Forms;
using BusBuddy.UI.Views;
using BusBuddy.UI.Components;

namespace BusBuddy.UI.Demo
{
    /// <summary>
    /// Demo application to showcase the new standardized Material Design UI components
    /// </summary>
    public static class MaterialUIDemo
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Show demo of the new Material Design interface
            ShowUIDemo();
        }

        private static void ShowUIDemo()
        {
            try
            {
                // Demo: Material Message Box
                using var parentForm = new Form(); // Create a parent form for proper dialog centering
                var result = MaterialMessageBox.ShowConfirmation(parentForm,
                    "Welcome to the new BusBuddy Material Design interface!\n\nWould you like to see the Route Management demo?",
                    "Material Design Demo");

                if (result == DialogResult.Yes)
                {
                    // Demo: Route Management Form with new standardized UI
                    using var routeForm = new RouteManagementForm();
                    routeForm.Text = "🚌 Material Design Demo - Route Management";

                    Application.Run(routeForm);
                }
                else
                {
                    // Demo: Maintenance Management Form
                    using var maintenanceForm = new MaintenanceManagementForm();
                    maintenanceForm.Text = "🔧 Material Design Demo - Maintenance Management";

                    Application.Run(maintenanceForm);
                }
            }
            catch (Exception ex)
            {
                using var parentForm = new Form(); // Create a parent form for error dialog
                MaterialMessageBox.ShowError(parentForm,
                    $"Demo Error: {ex.Message}\n\nThis is normal in a demo environment where database connections may not be available.",
                    "Demo Notice");
            }
        }
    }
}
