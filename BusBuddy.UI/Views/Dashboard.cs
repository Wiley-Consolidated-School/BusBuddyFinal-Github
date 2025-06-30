using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusBuddy.UI.Views; // For *ManagementFormSyncfusion
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;

namespace BusBuddy.UI
{
    public partial class Dashboard : Form
    {
        public Dashboard()
        {
            InitializeComponent();
            InitializeSyncfusionControls();
            // Syncfusion.Windows.Forms.Diagnostics.SfLogger.EnableLogging = true; // Not available in v31
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
        }

        private void InitializeSyncfusionControls()
        {
            // Remove SfNavigationDrawer and use a panel with buttons
            navigationPanel.Controls.Clear();
            var vehicleButton = new SfButton { Text = "Vehicles", Size = new Size(180, 30), Location = new Point(10, 10) };
            var driverButton = new SfButton { Text = "Drivers", Size = new Size(180, 30), Location = new Point(10, 50) };
            var routeButton = new SfButton { Text = "Routes", Size = new Size(180, 30), Location = new Point(10, 90) };
            navigationPanel.Controls.AddRange(new Control[] { vehicleButton, driverButton, routeButton });

            vehicleButton.Click += (s, e) =>
            {
                dockingManager1.DockControl(new DriverManagementFormSyncfusion(null, null, null), this, DockingStyle.Right, 300);
                // Add data loading logic if needed
            };
            driverButton.Click += (s, e) =>
            {
                dockingManager1.DockControl(new DriverManagementFormSyncfusion(null, null, null), this, DockingStyle.Right, 300);
                // Add data loading logic if needed
            };
            routeButton.Click += (s, e) =>
            {
                dockingManager1.DockControl(new RouteManagementFormSyncfusion(null, null, null, null, null), this, DockingStyle.Right, 300);
                // Add data loading logic if needed
            };
        }

        // Data loading methods can be added here if/when services are implemented
    }
}
