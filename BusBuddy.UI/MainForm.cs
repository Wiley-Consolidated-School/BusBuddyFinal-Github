using System;
using System.Windows.Forms;
using BusBuddy.Business;
using BusBuddy.UI;

namespace BusBuddy
{
    public partial class MainForm : Form
    {
        private readonly DatabaseHelperService _databaseService;
        
        public MainForm()
        {
            InitializeComponent();
            _databaseService = new DatabaseHelperService();
        }

        private void InitializeComponent()
        {
            this.Text = "BusBuddy - Bus Tracking Companion";
            this.Size = new System.Drawing.Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Create main menu
            MenuStrip mainMenu = new MenuStrip();
            this.MainMenuStrip = mainMenu;
            this.Controls.Add(mainMenu);
            
            // File menu
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            mainMenu.Items.Add(fileMenu);
            
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit", null, (s, e) => Application.Exit());
            fileMenu.DropDownItems.Add(exitMenuItem);
            
            // Data menu
            ToolStripMenuItem dataMenu = new ToolStripMenuItem("Data Management");
            mainMenu.Items.Add(dataMenu);
            
            ToolStripMenuItem vehiclesMenuItem = new ToolStripMenuItem("Vehicles", null, (s, e) => OpenVehicleManagement());
            dataMenu.DropDownItems.Add(vehiclesMenuItem);
            
            ToolStripMenuItem driversMenuItem = new ToolStripMenuItem("Drivers", null, (s, e) => OpenDriverManagement());
            dataMenu.DropDownItems.Add(driversMenuItem);
            
            ToolStripMenuItem routesMenuItem = new ToolStripMenuItem("Routes", null, (s, e) => OpenRouteManagement());
            dataMenu.DropDownItems.Add(routesMenuItem);
            
            ToolStripMenuItem activitiesMenuItem = new ToolStripMenuItem("Activities", null, (s, e) => OpenActivityManagement());
            dataMenu.DropDownItems.Add(activitiesMenuItem);
            
            ToolStripMenuItem fuelMenuItem = new ToolStripMenuItem("Fuel Records", null, (s, e) => OpenFuelManagement());
            dataMenu.DropDownItems.Add(fuelMenuItem);
            
            ToolStripMenuItem maintenanceMenuItem = new ToolStripMenuItem("Maintenance", null, (s, e) => OpenMaintenanceManagement());
            dataMenu.DropDownItems.Add(maintenanceMenuItem);
            
            ToolStripMenuItem calendarMenuItem = new ToolStripMenuItem("School Calendar", null, (s, e) => OpenCalendarManagement());
            dataMenu.DropDownItems.Add(calendarMenuItem);
            
            ToolStripMenuItem scheduleMenuItem = new ToolStripMenuItem("Activity Schedule", null, (s, e) => OpenScheduleManagement());
            dataMenu.DropDownItems.Add(scheduleMenuItem);
            
            ToolStripMenuItem timeCardMenuItem = new ToolStripMenuItem("Time Cards", null, (s, e) => OpenTimeCardManagement());
            dataMenu.DropDownItems.Add(timeCardMenuItem);
            
            // Reports menu
            ToolStripMenuItem reportsMenu = new ToolStripMenuItem("Reports");
            mainMenu.Items.Add(reportsMenu);
            
            ToolStripMenuItem routeReportMenuItem = new ToolStripMenuItem("Route Reports", null, (s, e) => OpenRouteReports());
            reportsMenu.DropDownItems.Add(routeReportMenuItem);
            
            ToolStripMenuItem driverReportMenuItem = new ToolStripMenuItem("Driver Reports", null, (s, e) => OpenDriverReports());
            reportsMenu.DropDownItems.Add(driverReportMenuItem);
            
            ToolStripMenuItem vehicleReportMenuItem = new ToolStripMenuItem("Vehicle Reports", null, (s, e) => OpenVehicleReports());
            reportsMenu.DropDownItems.Add(vehicleReportMenuItem);
            
            // Add a welcome label
            Label welcomeLabel = new Label();
            welcomeLabel.Text = "Welcome to BusBuddy!";
            welcomeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold);
            welcomeLabel.AutoSize = true;
            welcomeLabel.Location = new System.Drawing.Point(50, 100);
            this.Controls.Add(welcomeLabel);
            
            // Add a description label
            Label descriptionLabel = new Label();
            descriptionLabel.Text = "The comprehensive school bus tracking and management system";
            descriptionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            descriptionLabel.AutoSize = true;
            descriptionLabel.Location = new System.Drawing.Point(50, 150);
            this.Controls.Add(descriptionLabel);
            
            // Create dashboard area
            Panel dashboardPanel = new Panel();
            dashboardPanel.BorderStyle = BorderStyle.FixedSingle;
            dashboardPanel.Location = new System.Drawing.Point(50, 200);
            dashboardPanel.Size = new System.Drawing.Size(900, 450);
            this.Controls.Add(dashboardPanel);
            
            // Add dashboard title
            Label dashboardTitle = new Label();
            dashboardTitle.Text = "Dashboard";
            dashboardTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold);
            dashboardTitle.AutoSize = true;
            dashboardTitle.Location = new System.Drawing.Point(10, 10);
            dashboardPanel.Controls.Add(dashboardTitle);
            
            // Add buttons for quick access
            CreateDashboardButton(dashboardPanel, "Manage Vehicles", 10, 60, () => OpenVehicleManagement());
            CreateDashboardButton(dashboardPanel, "Manage Drivers", 180, 60, () => OpenDriverManagement());
            CreateDashboardButton(dashboardPanel, "Manage Routes", 350, 60, () => OpenRouteManagement());
            CreateDashboardButton(dashboardPanel, "Manage Activities", 520, 60, () => OpenActivityManagement());
            CreateDashboardButton(dashboardPanel, "View Reports", 690, 60, () => OpenRouteReports());
        }
        
        private void CreateDashboardButton(Panel parent, string text, int x, int y, Action clickAction)
        {
            Button button = new Button();
            button.Text = text;
            button.Location = new System.Drawing.Point(x, y);
            button.Size = new System.Drawing.Size(150, 40);
            button.Click += (s, e) => clickAction();
            parent.Controls.Add(button);
        }
          // Navigation methods
        private void OpenVehicleManagement()
        {
            using (var vehicleForm = new VehicleManagementForm())
            {
                vehicleForm.ShowDialog();
            }
        }
        
        private void OpenDriverManagement()
        {
            using (var driverForm = new DriverManagementForm())
            {
                driverForm.ShowDialog();
            }
        }
          private void OpenRouteManagement()
        {
            using (var routeForm = new RouteManagementForm())
            {
                routeForm.ShowDialog();
            }
        }
        
        private void OpenActivityManagement()
        {
            MessageBox.Show("Activity Management form will be implemented", "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void OpenFuelManagement()
        {
            MessageBox.Show("Fuel Management form will be implemented", "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void OpenMaintenanceManagement()
        {
            MessageBox.Show("Maintenance Management form will be implemented", "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void OpenCalendarManagement()
        {
            MessageBox.Show("School Calendar Management form will be implemented", "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void OpenScheduleManagement()
        {
            MessageBox.Show("Activity Schedule Management form will be implemented", "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void OpenTimeCardManagement()
        {
            MessageBox.Show("Time Card Management form will be implemented", "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void OpenRouteReports()
        {
            MessageBox.Show("Route Reports will be implemented", "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void OpenDriverReports()
        {
            MessageBox.Show("Driver Reports will be implemented", "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void OpenVehicleReports()
        {
            MessageBox.Show("Vehicle Reports will be implemented", "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}