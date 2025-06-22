using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using BusBuddy.UI.Base;
using BusBuddy.UI.Services;
using BusBuddy.Business;
using BusBuddy.Models;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Gauge;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Enums;
using Syncfusion.WinForms.DataGrid.Styles;
using Syncfusion.Drawing;
using Syncfusion.WinForms.DataGrid.Events;
using Syncfusion.WinForms.Input.Enums;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// CDE-40 Dashboard Prototype showcasing Syncfusion controls for data visualization and reporting
    /// Based on requirements from CDE40_Requirements.md
    /// </summary>
    public class DashboardPrototype : SyncfusionBaseForm
    {
        #region Fields and Services
        private readonly INavigationService _navigationService;
        private readonly BusBuddy.Business.IDatabaseHelperService _databaseHelperService;
        private readonly IRouteAnalyticsService _routeAnalyticsService;
        private readonly IReportService _reportService;
        private readonly BusBuddy.UI.Services.IAnalyticsService _analyticsService;

        // UI Components
        private NavigationDrawer _navigationDrawer;
        private Panel _contentPanel;
        private Panel _headerPanel;
        private Label _titleLabel;
        private SfDataGrid _routesDataGrid;
        private ChartControl _mileageChart;
        private ChartControl _pupilCountChart;
        private RadialGauge _costPerStudentGauge;
        private RadialGauge _totalMilesGauge;
        private RadialGauge _totalPupilsGauge;
        private Panel _metricsPanel;
        private Panel _chartsPanel;
        private SfButton _applyThemeButton;
        private SfButton _generateReportButton;
        private SfButton _refreshDataButton;
        private SfButton _generateAnalyticsButton;
        private SfButton _driverPayReportButton;

        // Data containers
        private List<Route> _routes;
        #endregion

        #region Constructor
        public DashboardPrototype(
            INavigationService navigationService,
            BusBuddy.Business.IDatabaseHelperService databaseHelperService,
            IRouteAnalyticsService routeAnalyticsService,
            IReportService reportService,
            BusBuddy.UI.Services.IAnalyticsService analyticsService)
        {
            _navigationService = navigationService;
            _databaseHelperService = databaseHelperService;
            _routeAnalyticsService = routeAnalyticsService;
            _reportService = reportService;
            _analyticsService = analyticsService;

            InitializeComponent();

            // Set initial data
            LoadDashboardData();

            // Task 4: Initialize navigation demonstration
            InitializeNavigationDemo();
        }
        #endregion

        #region Initialization
        private void InitializeComponent()
        {
            // Form configuration
            this.Text = "BusBuddy CDE-40 Dashboard Prototype";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create header panel
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ColorTranslator.FromHtml("#212121")
            };

            _titleLabel = new Label
            {
                Text = "CDE-40 Transportation Dashboard",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(20, 15)
            };

            _headerPanel.Controls.Add(_titleLabel);

            // Create content panel
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            // Setup navigation drawer using Syncfusion NavigationDrawer
            // Based on official Syncfusion NavigationDrawer documentation
            _navigationDrawer = new NavigationDrawer
            {
                Position = SlidePosition.Left,
                DrawerWidth = 250,
                Visible = false
            };

            // Create and add drawer header to Items collection
            DrawerHeader header = new DrawerHeader
            {
                Text = "Navigation Menu",
                Height = 60
            };
            _navigationDrawer.Items.Add(header);

            // Add navigation menu items to Items collection
            DrawerMenuItem menuDashboard = new DrawerMenuItem { Text = "Dashboard" };
            DrawerMenuItem menuRoutes = new DrawerMenuItem { Text = "Routes" };
            DrawerMenuItem menuVehicles = new DrawerMenuItem { Text = "Vehicles" };
            DrawerMenuItem menuDrivers = new DrawerMenuItem { Text = "Drivers" };
            DrawerMenuItem menuMaintenance = new DrawerMenuItem { Text = "Maintenance" };
            DrawerMenuItem menuCDE40Report = new DrawerMenuItem { Text = "CDE-40 Report" };
            DrawerMenuItem menuSettings = new DrawerMenuItem { Text = "Settings" };

            _navigationDrawer.Items.Add(menuDashboard);
            _navigationDrawer.Items.Add(menuRoutes);
            _navigationDrawer.Items.Add(menuVehicles);
            _navigationDrawer.Items.Add(menuDrivers);
            _navigationDrawer.Items.Add(menuMaintenance);
            _navigationDrawer.Items.Add(menuCDE40Report);
            _navigationDrawer.Items.Add(menuSettings);

            // Create panels for layout
            _metricsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 200
            };

            _chartsPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            // Configure metrics gauges
            // Based on RadialGauge documentation: https://help.syncfusion.com/windowsforms/radial-gauge/getting-started
            _costPerStudentGauge = new RadialGauge
            {
                Width = 200,
                Height = 180,
                Location = new Point(20, 10),
                GaugeLabel = "Cost/Student",
                MaximumValue = 5,
                MinimumValue = 0,
                Value = 2.70f
            };

            _totalMilesGauge = new RadialGauge
            {
                Width = 200,
                Height = 180,
                Location = new Point(240, 10),
                GaugeLabel = "Total Miles (K)",
                MaximumValue = 100,
                MinimumValue = 0,
                Value = 45.2f
            };

            _totalPupilsGauge = new RadialGauge
            {
                Width = 200,
                Height = 180,
                Location = new Point(460, 10),
                GaugeLabel = "Pupils Transported",
                MaximumValue = 3000,
                MinimumValue = 0,
                Value = 1850f
            };

            // Add gauges to metrics panel
            _metricsPanel.Controls.Add(_costPerStudentGauge);
            _metricsPanel.Controls.Add(_totalMilesGauge);
            _metricsPanel.Controls.Add(_totalPupilsGauge);

            // Add action buttons
            _generateReportButton = new SfButton
            {
                Width = 160,
                Height = 40,
                Location = new Point(680, 80),
                Text = "Generate CDE-40 Report"
            };

            _refreshDataButton = new SfButton
            {
                Width = 160,
                Height = 40,
                Location = new Point(860, 80),
                Text = "Refresh Data"
            };

            _applyThemeButton = new SfButton
            {
                Width = 160,
                Height = 40,
                Location = new Point(1020, 80),
                Text = "Toggle Theme"
            };

            // Task 6: Add Analytics Service buttons
            _generateAnalyticsButton = new SfButton
            {
                Width = 160,
                Height = 40,
                Location = new Point(680, 120),
                Text = "Generate Analytics"
            };

            _driverPayReportButton = new SfButton
            {
                Width = 160,
                Height = 40,
                Location = new Point(860, 120),
                Text = "Driver Pay Report"
            };

            _metricsPanel.Controls.Add(_generateReportButton);
            _metricsPanel.Controls.Add(_refreshDataButton);
            _metricsPanel.Controls.Add(_applyThemeButton);
            _metricsPanel.Controls.Add(_generateAnalyticsButton);
            _metricsPanel.Controls.Add(_driverPayReportButton);

            // Configure charts
            // Based on ChartControl documentation: https://help.syncfusion.com/windowsforms/chart/getting-started
            _mileageChart = new ChartControl
            {
                Dock = DockStyle.Left,
                Width = _chartsPanel.Width / 2,
                Text = "Mileage Trends",
                ShowLegend = true
            };

            _pupilCountChart = new ChartControl
            {
                Dock = DockStyle.Right,
                Width = _chartsPanel.Width / 2,
                Text = "Pupil Count Trends",
                ShowLegend = true
            };

            // Configure data grid for routes
            // Based on SfDataGrid documentation: https://help.syncfusion.com/windowsforms/datagrid/getting-started
            _routesDataGrid = new SfDataGrid
            {
                Dock = DockStyle.Bottom,
                Height = 250,
                AutoSizeColumnsMode = AutoSizeColumnsMode.Fill,
                AllowFiltering = true,
                AllowSorting = true,
                AllowEditing = false,
                AllowGrouping = true,
                ShowGroupDropArea = true
            };

            // Add charts to charts panel
            _chartsPanel.Controls.Add(_mileageChart);
            _chartsPanel.Controls.Add(_pupilCountChart);

            // Add panels to content area
            _contentPanel.Controls.Add(_routesDataGrid);
            _contentPanel.Controls.Add(_chartsPanel);
            _contentPanel.Controls.Add(_metricsPanel);

            // Add all components to form
            this.Controls.Add(_contentPanel);
            this.Controls.Add(_navigationDrawer);
            this.Controls.Add(_headerPanel);

            // Wire up events
            _applyThemeButton.Click += ApplyThemeButton_Click;
            _refreshDataButton.Click += RefreshDataButton_Click;
            _generateReportButton.Click += GenerateReportButton_Click;
            _generateAnalyticsButton.Click += GenerateAnalyticsButton_Click;
            _driverPayReportButton.Click += DriverPayReportButton_Click;

            // Task 4: Enhanced Navigation Service - wire up navigation drawer items
            // Note: NavigationDrawer in Syncfusion uses different event pattern
            // Individual menu items would need to be wired up separately in production

            // Task 4: Enhanced Navigation Service - add test navigation button
            var _testNavigationButton = new SfButton
            {
                Width = 160,
                Height = 40,
                Location = new Point(680, 120),
                Text = "Test Navigation"
            };
            _testNavigationButton.Click += (s, e) => {
                // Demonstrate navigation to vehicles module
                NavigateToModule("Vehicles");
            };
            _metricsPanel.Controls.Add(_testNavigationButton);

            // Task 4: Enhanced Navigation Service - call demonstration method on load
            InitializeNavigationDemo();
        }

        private void InitializeNavigationDemo()
        {
            // Demonstrate navigation service capabilities for Task 4
            DemonstrateNavigation();

            // Log available modules for CDE-40 dashboard
            Console.WriteLine("🚌 TASK 4: BusBuddy Dashboard Navigation Service Initialized");
            Console.WriteLine("📊 CDE-40 Reporting: Enabled");
            Console.WriteLine("💰 Financial Analytics: Enabled");
            Console.WriteLine("🚌 Transportation Value: Enabled");
        }
        #endregion

        #region Event Handlers
        private void ApplyThemeButton_Click(object sender, EventArgs e)
        {
            // Toggle between dark and light themes
            if (_headerPanel.BackColor == ColorTranslator.FromHtml("#212121"))
            {
                _headerPanel.BackColor = ColorTranslator.FromHtml("#F1F1F1");
                _titleLabel.ForeColor = Color.Black;
            }
            else
            {
                _headerPanel.BackColor = ColorTranslator.FromHtml("#212121");
                _titleLabel.ForeColor = Color.White;
            }
        }

        private void RefreshDataButton_Click(object sender, EventArgs e)
        {
            LoadDashboardData();
        }

        private async void GenerateReportButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Task 5: Use ReportService to generate CDE-40 report
                _generateReportButton.Text = "Generating...";
                _generateReportButton.Enabled = false;

                var reportData = await _reportService.GenerateCDE40ReportAsync("2024-25");

                if (reportData != null)
                {
                    MessageBox.Show("CDE-40 Report generated successfully!\n\nReport includes:\n• Transportation mileage analysis\n• Pupil count statistics\n• Cost per student calculations\n• Financial contributions breakdown\n• AI-powered insights and recommendations",
                        "CDE-40 Report Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to generate CDE-40 report. Please check the data and try again.",
                        "Report Generation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating CDE-40 report: {ex.Message}",
                    "Report Generation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _generateReportButton.Text = "Generate CDE-40 Report";
                _generateReportButton.Enabled = true;
            }
        }

        private void ToggleNavigationDrawer()
        {
            // Toggle navigation drawer visibility
            _navigationDrawer.Visible = !_navigationDrawer.Visible;
        }

        // Task 4: Enhanced Navigation Service - helper method for navigation
        private void NavigateToModule(string moduleText)
        {
            // Task 4: Enhanced mapping for CDE-40 and financial analytics modules
            string moduleName = moduleText.ToLower() switch
            {
                "dashboard" => "dashboard",
                "routes" => "routes",
                "vehicles" => "vehicles",
                "drivers" => "drivers",
                "maintenance" => "maintenance",
                "cde-40 report" => "cde40",
                "settings" => "settings",
                "financial analytics" => "financial",
                "cost analysis" => "costanalysis",
                "transportation value" => "value",
                "analytics" => "analytics",
                _ => moduleText.ToLower()
            };

            try
            {
                Console.WriteLine($"🔍 TASK 4: Attempting navigation to '{moduleText}' -> '{moduleName}'");

                if (_navigationService.IsModuleAvailable(moduleName))
                {
                    bool success = _navigationService.Navigate(moduleName);
                    if (!success)
                    {
                        MessageBox.Show($"Failed to navigate to {moduleText}. Please try again.",
                            "Navigation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        Console.WriteLine($"✅ TASK 4: Successfully navigated to '{moduleText}'");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ TASK 4: Module '{moduleName}' not available");
                    MessageBox.Show($"The {moduleText} module is not available in the current configuration.",
                        "Module Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ TASK 4: Navigation error for '{moduleText}': {ex.Message}");
                MessageBox.Show($"An error occurred while navigating to {moduleText}: {ex.Message}",
                    "Navigation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void GenerateAnalyticsButton_Click(object sender, EventArgs e)
        {
            try
            {
                _generateAnalyticsButton.Text = "Generating...";
                _generateAnalyticsButton.Enabled = false;

                // Generate analytics using the AnalyticsService
                var startDate = DateTime.Now.AddMonths(-1);
                var endDate = DateTime.Now;

                var mileageStats = await _analyticsService.GetMileageStatsAsync(startDate, endDate);
                var pupilCounts = await _analyticsService.GetPupilCountsAsync(startDate, endDate);
                var costPerStudent = await _analyticsService.GetCostPerStudentAsync(startDate, endDate);

                MessageBox.Show($"Analytics Generated Successfully!\n\n" +
                    $"Total Mileage: {mileageStats:F1} miles\n" +
                    $"Total Pupils: {pupilCounts}\n" +
                    $"Cost per Student: ${costPerStudent:F2}",
                    "Analytics Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating analytics: {ex.Message}",
                    "Analytics Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _generateAnalyticsButton.Text = "Generate Analytics";
                _generateAnalyticsButton.Enabled = true;
            }
        }

        private async void DriverPayReportButton_Click(object sender, EventArgs e)
        {
            try
            {
                _driverPayReportButton.Text = "Generating...";
                _driverPayReportButton.Enabled = false;

                // Generate driver pay report
                var startDate = DateTime.Now.AddMonths(-1);
                var endDate = DateTime.Now;

                var payReports = await _analyticsService.GenerateDriverPayReportAsync(startDate, endDate);

                var totalPay = payReports.Sum(r => r.TotalPay);
                var driverCount = payReports.Count;

                MessageBox.Show($"Driver Pay Report Generated!\n\n" +
                    $"Total Drivers: {driverCount}\n" +
                    $"Total Pay: ${totalPay:F2}\n" +
                    $"Average Pay: ${(driverCount > 0 ? totalPay / driverCount : 0):F2}",
                    "Driver Pay Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating driver pay report: {ex.Message}",
                    "Pay Report Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _driverPayReportButton.Text = "Driver Pay Report";
                _driverPayReportButton.Enabled = true;
            }
        }
        #endregion

        #region Data Methods
        private void LoadDashboardData()
        {
            try
            {
                // In the prototype, load sample data
                LoadSampleRoutes();
                LoadSampleChartData();
                UpdateGauges();

                // In the full implementation, this would load real data from the database
                // For example:
                // var routes = await _databaseHelperService.GetRoutesAsync();
                // _routesDataGrid.DataSource = routes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard data: {ex.Message}",
                    "Data Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSampleRoutes()
        {
            // Create sample route data for the prototype
            _routes = new List<Route>
            {
                new Route { RouteID = 1, RouteName = "East Route", Date = DateTime.Today.ToString("yyyy-MM-dd"),
                    AMBeginMiles = 1000, AMEndMiles = 1045, AMRiders = 42,
                    PMBeginMiles = 1050, PMEndMiles = 1095, PMRiders = 40 },

                new Route { RouteID = 2, RouteName = "West Route", Date = DateTime.Today.ToString("yyyy-MM-dd"),
                    AMBeginMiles = 2000, AMEndMiles = 2035, AMRiders = 38,
                    PMBeginMiles = 2040, PMEndMiles = 2075, PMRiders = 36 },

                new Route { RouteID = 3, RouteName = "North Route", Date = DateTime.Today.ToString("yyyy-MM-dd"),
                    AMBeginMiles = 3000, AMEndMiles = 3050, AMRiders = 45,
                    PMBeginMiles = 3055, PMEndMiles = 3105, PMRiders = 43 },

                new Route { RouteID = 4, RouteName = "South Route", Date = DateTime.Today.ToString("yyyy-MM-dd"),
                    AMBeginMiles = 4000, AMEndMiles = 4030, AMRiders = 30,
                    PMBeginMiles = 4035, PMEndMiles = 4065, PMRiders = 28 },

                new Route { RouteID = 5, RouteName = "SPED Route", Date = DateTime.Today.ToString("yyyy-MM-dd"),
                    AMBeginMiles = 5000, AMEndMiles = 5025, AMRiders = 12,
                    PMBeginMiles = 5030, PMEndMiles = 5055, PMRiders = 12 }
            };

            _routesDataGrid.DataSource = _routes;
        }

        private void LoadSampleChartData()
        {
            // Configure mileage chart with sample data
            _mileageChart.Series.Clear();
            ChartSeries mileageSeries = new ChartSeries("Daily Mileage", ChartSeriesType.Line);
            mileageSeries.Points.Add(1, 120);
            mileageSeries.Points.Add(2, 135);
            mileageSeries.Points.Add(3, 128);
            mileageSeries.Points.Add(4, 142);
            mileageSeries.Points.Add(5, 115);
            _mileageChart.Series.Add(mileageSeries);

            // Configure pupil count chart with sample data
            _pupilCountChart.Series.Clear();
            ChartSeries pupilSeries = new ChartSeries("Daily Pupil Count", ChartSeriesType.Column);
            pupilSeries.Points.Add(1, 175);
            pupilSeries.Points.Add(2, 182);
            pupilSeries.Points.Add(3, 168);
            pupilSeries.Points.Add(4, 179);
            pupilSeries.Points.Add(5, 160);
            _pupilCountChart.Series.Add(pupilSeries);
        }

        private void UpdateGauges()
        {
            // Calculate metrics based on sample data
            decimal totalMiles = 0;
            int totalRiders = 0;

            foreach (var route in _routes)
            {
                decimal routeMiles = ((route.AMEndMiles ?? 0) - (route.AMBeginMiles ?? 0)) +
                                     ((route.PMEndMiles ?? 0) - (route.PMBeginMiles ?? 0));
                totalMiles += routeMiles;

                totalRiders += (route.AMRiders ?? 0) + (route.PMRiders ?? 0);
            }

            // Update gauges with calculated values
            _totalMilesGauge.Value = (float)(totalMiles / 1000); // Convert to thousands
            _totalPupilsGauge.Value = totalRiders;

            // Cost per student is a sample value for the prototype
            _costPerStudentGauge.Value = 2.70f;
        }
        #endregion

        #region IDisposable Implementation
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                _navigationDrawer?.Dispose();
                _routesDataGrid?.Dispose();
                _mileageChart?.Dispose();
                _pupilCountChart?.Dispose();
                _costPerStudentGauge?.Dispose();
                _totalMilesGauge?.Dispose();
                _totalPupilsGauge?.Dispose();
                _applyThemeButton?.Dispose();
                _generateReportButton?.Dispose();
                _refreshDataButton?.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Task 4: Enhanced Navigation Service - demonstration method for testing navigation
        private void DemonstrateNavigation()
        {
            // Task 4: Test navigation service functionality for CDE-40 reporting and financial analytics
            var availableModules = new[] {
                "vehicles", "drivers", "routes", "maintenance",
                "cde40", "analytics", "financial", "costanalysis"
            };

            Console.WriteLine("🔍 TASK 4: Testing Navigation Service Module Availability:");
            foreach (var module in availableModules)
            {
                bool isAvailable = _navigationService.IsModuleAvailable(module);
                Console.WriteLine($"  📋 Module '{module}' is {(isAvailable ? "✅ available" : "❌ not available")}");
            }

            // Test CDE-40 specific modules
            Console.WriteLine("📊 TASK 4: CDE-40 Dashboard Modules:");
            Console.WriteLine($"  🚌 Transportation Value: {(_navigationService.IsModuleAvailable("value") ? "✅" : "❌")}");
            Console.WriteLine($"  💰 Financial Analytics: {(_navigationService.IsModuleAvailable("financial") ? "✅" : "❌")}");
            Console.WriteLine($"  📈 Cost Analysis: {(_navigationService.IsModuleAvailable("costanalysis") ? "✅" : "❌")}");
        }
        #endregion
    }
}
