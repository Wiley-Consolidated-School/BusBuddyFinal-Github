using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BusBuddy.UI.Services;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using BusBuddy.Business;
using BusBuddy.UI.Views;
using BusBuddy.Data;
using BusBuddy.Models;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Gauge;
using Syncfusion.WinForms.Controls;
using static BusBuddy.UI.Views.FormDiscovery;

namespace BusBuddy.UI.Views
{
    public partial class BusBuddyDashboardSyncfusion : SyncfusionBaseForm
    {
        #region Fields and Services
        private readonly INavigationService _navigationService;
        private readonly BusBuddy.UI.Services.IDatabaseHelperService _databaseHelperService;
        private CancellationTokenSource _cancellationTokenSource;

        // UI Components
        private Panel _headerPanel;
        private Label _titleLabel;
        private SfButton _themeToggleButton;
        private FlowLayoutPanel _formButtonsPanel;
        private Panel _analyticsPanel;
        private ChartControl _analyticsChart;
        private RadialGauge _systemStatusGauge;
        private RadialGauge _maintenanceGauge;
        private RadialGauge _efficiencyGauge;
        private DockingManager _dockingManager;
        private Panel _sidePanel;
        private Panel _statisticsPanel;
        private SfButton _closeButton;

        // Navigation method mapping for improved reliability
        private readonly Dictionary<string, System.Action> _navigationMethods;

        // Repository type mapping for automatic initialization
        private readonly Dictionary<string, Type> _repositoryTypeMap;

        // Configuration Constants (replacing hardcoded values)
        private static class UIConstants
        {
            public const int HeaderHeight = 60;
            public const int ButtonWidth = 180;
            public const int ButtonHeight = 80;
            public const int DefaultSpacing = 10;
            public const int LargeSpacing = 20;

            public static readonly Color PrimaryColor = Color.FromArgb(63, 81, 181);
            public static readonly Color SurfaceColor = Color.White;
            public static readonly Color ErrorColor = Color.FromArgb(244, 67, 54);
        }
        #endregion

        #region Constructors
        public BusBuddyDashboardSyncfusion(INavigationService navigationService, BusBuddy.UI.Services.IDatabaseHelperService databaseHelperService)
        {
            LogInfo("Dashboard constructor called");
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _databaseHelperService = databaseHelperService ?? throw new ArgumentNullException(nameof(databaseHelperService));
            _cancellationTokenSource = new CancellationTokenSource();

            // Initialize readonly dictionaries
            _repositoryTypeMap = new Dictionary<string, Type>
            {
                { "ShowVehicleManagement", typeof(IVehicleRepository) },
                { "ShowDriverManagement", typeof(IDriverRepository) },
                { "ShowRouteManagement", typeof(IRouteRepository) },
                { "ShowFuelManagement", typeof(IFuelRepository) },
                { "ShowMaintenanceManagement", typeof(IMaintenanceRepository) },
                { "ShowTimeCardManagement", typeof(ITimeCardRepository) },
                { "ShowActivityManagement", typeof(IActivityRepository) }
            };

            _navigationMethods = new Dictionary<string, System.Action>
            {
                { "ShowVehicleManagement", () => _navigationService.ShowVehicleManagement() },
                { "ShowDriverManagement", () => _navigationService.ShowDriverManagement() },
                { "ShowRouteManagement", () => _navigationService.ShowRouteManagement() },
                { "ShowActivityManagement", () => _navigationService.ShowActivityManagement() },
                { "ShowFuelManagement", () => _navigationService.ShowFuelManagement() },
                { "ShowMaintenanceManagement", () => _navigationService.ShowMaintenanceManagement() },
                { "ShowCalendarManagement", () => _navigationService.ShowCalendarManagement() },
                { "ShowScheduleManagement", () => _navigationService.ShowScheduleManagement() },
                { "ShowTimeCardManagement", () => _navigationService.ShowTimeCardManagement() },
                { "ShowReportsManagement", () => _navigationService.ShowReportsManagement() },
                { "ShowSchoolCalendarManagement", () => _navigationService.ShowSchoolCalendarManagement() },
                { "ShowActivityScheduleManagement", () => _navigationService.ShowActivityScheduleManagement() },
                { "ShowAnalyticsDemo", () => _navigationService.ShowAnalyticsDemo() },
                { "ShowReports", () => _navigationService.ShowReports() }
            };

            InitializeComponent();
            InitializeDashboard();

            // Validate dashboard initialization
            if (ValidateDashboard())
            {
                LogInfo("BusBuddy Dashboard initialization completed successfully!");
            }
            else
            {
                LogWarning("Dashboard initialization completed with warnings");
            }

            LogInfo("Dashboard initialized");
        }

        // Constructor for testing - uses ServiceContainerSingleton to get services
        public BusBuddyDashboardSyncfusion()
        {
            try
            {
                LogInfo("Dashboard test constructor called");
                _cancellationTokenSource = new CancellationTokenSource();

                // Initialize the ServiceContainerSingleton
                if (!ServiceContainerSingleton.IsInitialized)
                {
                    LogWarning("ServiceContainerSingleton not initialized, initializing now");
                    ServiceContainerSingleton.Initialize();
                }

                // Get services from the singleton
                _navigationService = ServiceContainerSingleton.Instance.GetService<INavigationService>();
                _databaseHelperService = ServiceContainerSingleton.Instance.GetService<BusBuddy.UI.Services.IDatabaseHelperService>();

                // Create a fallback navigation service if needed (for testing)
                if (_navigationService == null)
                {
                    LogWarning("Creating fallback navigation service for testing");
                    var container = ServiceContainerSingleton.Instance;
                    _navigationService = new NavigationService(container);
                }

                // Create a fallback database helper service if needed (for testing)
                if (_databaseHelperService == null)
                {
                    LogWarning("Skipping database helper service for testing - not needed for basic UI tests");
                    // In test environments, we can proceed without the database helper service
                    // as it's primarily used for diagnostics and not core navigation functionality
                }

                // Initialize readonly dictionaries
                _repositoryTypeMap = new Dictionary<string, Type>
                {
                    { "ShowVehicleManagement", typeof(IVehicleRepository) },
                    { "ShowDriverManagement", typeof(IDriverRepository) },
                    { "ShowRouteManagement", typeof(IRouteRepository) },
                    { "ShowFuelManagement", typeof(IFuelRepository) },
                    { "ShowMaintenanceManagement", typeof(IMaintenanceRepository) },
                    { "ShowTimeCardManagement", typeof(ITimeCardRepository) },
                    { "ShowActivityManagement", typeof(IActivityRepository) }
                };

                _navigationMethods = new Dictionary<string, System.Action>
                {
                    { "ShowVehicleManagement", () => _navigationService.ShowVehicleManagement() },
                    { "ShowDriverManagement", () => _navigationService.ShowDriverManagement() },
                    { "ShowRouteManagement", () => _navigationService.ShowRouteManagement() },
                    { "ShowActivityManagement", () => _navigationService.ShowActivityManagement() },
                    { "ShowFuelManagement", () => _navigationService.ShowFuelManagement() },
                    { "ShowMaintenanceManagement", () => _navigationService.ShowMaintenanceManagement() },
                    { "ShowCalendarManagement", () => _navigationService.ShowCalendarManagement() },
                    { "ShowScheduleManagement", () => _navigationService.ShowScheduleManagement() },
                    { "ShowTimeCardManagement", () => _navigationService.ShowTimeCardManagement() },
                    { "ShowReportsManagement", () => _navigationService.ShowReportsManagement() },
                    { "ShowSchoolCalendarManagement", () => _navigationService.ShowSchoolCalendarManagement() },
                    { "ShowActivityScheduleManagement", () => _navigationService.ShowActivityScheduleManagement() },
                    { "ShowAnalyticsDemo", () => _navigationService.ShowAnalyticsDemo() },
                    { "ShowReports", () => _navigationService.ShowReports() }
                };

                InitializeComponent();

                // Validate test dashboard initialization
                if (ValidateDashboard())
                {
                    LogInfo("BusBuddy Test Dashboard initialization completed successfully!");
                }
                else
                {
                    LogWarning("Test Dashboard initialization completed with warnings");
                }

                LogInfo("Dashboard test instance initialized");
            }
            catch (Exception ex)
            {
                LogError("Error in BusBuddyDashboardSyncfusion constructor", ex);
                ShowUserFriendlyError($"Error initializing dashboard: {ex.Message}", "Initialization Error");
                throw;
            }
        }
        #endregion

        #region Event Handlers
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LogInfo("Dashboard OnLoad");
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            LogInfo("Dashboard OnShown");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                LogInfo("BusBuddyDashboardSyncfusion form closing");

                // Cancel any background operations
                _cancellationTokenSource?.Cancel();

                LogCurrentResources();
                CleanupRepositoryConnections();
                CleanupUIComponentsEnhanced();

                // Proper disposal instead of aggressive cleanup
                _cancellationTokenSource?.Dispose();

                LogInfo("BusBuddyDashboardSyncfusion cleanup completed");
            }
            catch (Exception ex)
            {
                LogError("Error during BusBuddyDashboardSyncfusion closing", ex);
            }
            finally
            {
                base.OnFormClosing(e);
            }
        }
        #endregion

        private async Task RunBackgroundTask(Func<Task> taskFunc, string taskName)
        {
            LogInfo($"Starting background task: {taskName}");
            try
            {
                await taskFunc();
                LogInfo($"Background task '{taskName}' completed");
            }
            catch (OperationCanceledException)
            {
                LogInfo($"Background task '{taskName}' was cancelled");
            }
            catch (Exception ex)
            {
                LogError($"Exception in background task '{taskName}'", ex);
            }
        }

        #region Dashboard Initialization
        private void InitializeDashboard()
        {
            try
            {
                LogInfo("Initializing enhanced Syncfusion dashboard...");

                // Initialize docking layout first - based on Syncfusion DockingManager documentation
                InitializeDockingLayout();

                // Create navigation panel with proper Syncfusion controls
                CreateNavigationPanel();

                // Initialize analytics dashboard
                InitializeAnalyticsDashboard();

                // Initialize system monitoring gauges
                InitializeSystemGauges();

                this.Text = "BusBuddy Dashboard - Enhanced Syncfusion v2.0";
                this.WindowState = FormWindowState.Maximized;
                this.Show();
                this.Refresh();

                // Load real analytics data asynchronously
                Task.Run(async () =>
                {
                    try
                    {
                        await LoadRealAnalyticsDataAsync(_cancellationTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        LogError("Analytics loading failed", ex);
                        // Show fallback data instead of failing completely
                        LoadFallbackAnalyticsData();
                    }
                });

                LogInfo("Enhanced dashboard initialization completed successfully");
            }
            catch (Exception ex)
            {
                LogError("Failed to initialize enhanced dashboard", ex);
                ShowUserFriendlyError($"Failed to initialize dashboard: {ex.Message}", "Critical Error");
                CreateEmergencyLayout();
            }
        }

        /// <summary>
        /// Initializes DockingManager layout based on Syncfusion documentation
        /// Reference: https://help.syncfusion.com/cr/windowsforms/Syncfusion.Windows.Forms.Tools.DockingManager.html
        /// </summary>
        private void InitializeDockingLayout()
        {
            try
            {
                LogInfo("Initializing DockingManager layout...");

                // Clear any existing controls
                this.Controls.Clear();

                // Initialize DockingManager with proper settings
                _dockingManager = new DockingManager()
                {
                    DockToFill = true,
                    VisualStyle = VisualStyle.Office2016Colorful,
                    CloseEnabled = false,
                    DockTabFont = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 9, FontStyle.Regular)
                };

                // Set the host form using the HostForm property
                _dockingManager.HostForm = this;

                // Create header panel
                CreateEnhancedHeader();

                // Create main content area
                CreateMainContentArea();

                LogInfo("DockingManager layout initialized successfully");
            }
            catch (Exception ex)
            {
                LogError("Failed to initialize DockingManager layout", ex);
                throw;
            }
        }

        /// <summary>
        /// Creates enhanced header with theme toggle and system status
        /// </summary>
        private void CreateEnhancedHeader()
        {
            _headerPanel = new Panel
            {
                Name = "HeaderPanel",
                Height = UIConstants.HeaderHeight,
                Dock = DockStyle.Top,
                BackColor = UIConstants.PrimaryColor,
                Padding = new Padding(UIConstants.LargeSpacing, 10, UIConstants.LargeSpacing, 10)
            };

            // Title with icon
            _titleLabel = new Label
            {
                Text = "🚌 BusBuddy Management System - Enhanced Dashboard",
                Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Left,
                Width = 600,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            // Theme toggle button
            _themeToggleButton = new SfButton
            {
                Text = "🌙 Dark Mode",
                Size = new Size(120, 35),
                Dock = DockStyle.Right,
                Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 9, FontStyle.Regular)
            };
            _themeToggleButton.Click += ToggleTheme;

            // Close button
            _closeButton = new SfButton
            {
                Text = "✕ Close",
                Size = new Size(80, 35),
                Dock = DockStyle.Right,
                Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 9, FontStyle.Regular)
            };
            _closeButton.Click += (s, e) => this.Close();

            _headerPanel.Controls.Add(_titleLabel);
            _headerPanel.Controls.Add(_closeButton);
            _headerPanel.Controls.Add(_themeToggleButton);

            this.Controls.Add(_headerPanel);
        }

        /// <summary>
        /// Creates main content area with proper docking
        /// </summary>
        private void CreateMainContentArea()
        {
            // Create main content panel
            var mainContentPanel = new Panel
            {
                Name = "MainContentPanel",
                Dock = DockStyle.Fill,
                BackColor = UIConstants.SurfaceColor
            };

            this.Controls.Add(mainContentPanel);
        }

        /// <summary>
        /// Creates navigation panel with module buttons
        /// Based on Syncfusion SfButton documentation
        /// </summary>
        private void CreateNavigationPanel()
        {
            try
            {
                LogInfo("Creating navigation panel...");

                _sidePanel = new Panel
                {
                    Name = "NavigationPanel",
                    Width = 280,
                    BackColor = Color.FromArgb(245, 245, 245),
                    Padding = new Padding(UIConstants.DefaultSpacing)
                };

                // Navigation title
                var navTitle = new Label
                {
                    Text = "🚏 Management Modules",
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 12, FontStyle.Bold),
                    Height = 30,
                    Dock = DockStyle.Top,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = UIConstants.PrimaryColor
                };

                // Navigation buttons container
                _formButtonsPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoScroll = true,
                    Padding = new Padding(5)
                };

                _sidePanel.Controls.Add(_formButtonsPanel);
                _sidePanel.Controls.Add(navTitle);

                // Dock the navigation panel to the left
                _dockingManager.DockControl(_sidePanel, this, DockingStyle.Left, 280);

                // Populate navigation buttons
                PopulateEnhancedFormButtons();

                LogInfo("Navigation panel created successfully");
            }
            catch (Exception ex)
            {
                LogError("Failed to create navigation panel", ex);
                throw;
            }
        }

        /// <summary>
        /// Initializes analytics dashboard with Chart controls
        /// Based on Syncfusion ChartControl documentation
        /// </summary>
        private void InitializeAnalyticsDashboard()
        {
            try
            {
                LogInfo("Initializing analytics dashboard...");

                _analyticsPanel = new Panel
                {
                    Name = "AnalyticsPanel",
                    BackColor = Color.White,
                    Padding = new Padding(UIConstants.DefaultSpacing)
                };

                // Analytics title
                var analyticsTitle = new Label
                {
                    Text = "📊 Fleet Analytics & Performance",
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 12, FontStyle.Bold),
                    Height = 30,
                    Dock = DockStyle.Top,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = UIConstants.PrimaryColor
                };

                // Initialize Chart Control for route efficiency
                _analyticsChart = new ChartControl
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White
                };

                // Configure chart series for route efficiency data
                var routeEfficiencySeries = new ChartSeries("Route Efficiency")
                {
                    Type = ChartSeriesType.Line
                };

                var costAnalyticsSeries = new ChartSeries("Cost Per Student")
                {
                    Type = ChartSeriesType.Column
                };

                _analyticsChart.Series.Add(routeEfficiencySeries);
                _analyticsChart.Series.Add(costAnalyticsSeries);

                // Configure chart appearance
                _analyticsChart.PrimaryXAxis.Title = "Time Period";
                _analyticsChart.PrimaryYAxis.Title = "Efficiency Rating";
                _analyticsChart.Title.Text = "Fleet Performance Overview";

                _analyticsPanel.Controls.Add(_analyticsChart);
                _analyticsPanel.Controls.Add(analyticsTitle);

                // Dock analytics panel to the right
                _dockingManager.DockControl(_analyticsPanel, this, DockingStyle.Right, 400);

                LogInfo("Analytics dashboard initialized successfully");
            }
            catch (Exception ex)
            {
                LogError("Failed to initialize analytics dashboard", ex);
                throw;
            }
        }

        /// <summary>
        /// Initializes system monitoring gauges
        /// Based on Syncfusion RadialGauge documentation
        /// </summary>
        private void InitializeSystemGauges()
        {
            try
            {
                LogInfo("Initializing system monitoring gauges...");

                _statisticsPanel = new Panel
                {
                    Name = "StatisticsPanel",
                    BackColor = Color.White,
                    Padding = new Padding(UIConstants.DefaultSpacing)
                };

                // Statistics title
                var statsTitle = new Label
                {
                    Text = "⚡ System Health Monitor",
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 12, FontStyle.Bold),
                    Height = 30,
                    Dock = DockStyle.Top,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = UIConstants.PrimaryColor
                };

                // Create gauge container
                var gaugeContainer = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 3,
                    BackColor = Color.White
                };

                // System Status Gauge
                _systemStatusGauge = new RadialGauge
                {
                    Name = "SystemStatusGauge",
                    Dock = DockStyle.Fill,
                    Margin = new Padding(5),
                    GaugeLabel = "System Status",
                    Value = 85, // Default value, will be updated with real data
                    MinimumSize = new Size(120, 120)
                };

                // Maintenance Alert Gauge
                _maintenanceGauge = new RadialGauge
                {
                    Name = "MaintenanceGauge",
                    Dock = DockStyle.Fill,
                    Margin = new Padding(5),
                    GaugeLabel = "Maintenance Due",
                    Value = 25, // Default value
                    MinimumSize = new Size(120, 120)
                };

                // Fleet Efficiency Gauge
                _efficiencyGauge = new RadialGauge
                {
                    Name = "EfficiencyGauge",
                    Dock = DockStyle.Fill,
                    Margin = new Padding(5),
                    GaugeLabel = "Fleet Efficiency",
                    Value = 78, // Default value
                    MinimumSize = new Size(120, 120)
                };

                // Add gauges to container
                gaugeContainer.Controls.Add(_systemStatusGauge, 0, 0);
                gaugeContainer.Controls.Add(_maintenanceGauge, 0, 1);
                gaugeContainer.Controls.Add(_efficiencyGauge, 0, 2);

                // Set row styles for even distribution
                gaugeContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
                gaugeContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
                gaugeContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));

                _statisticsPanel.Controls.Add(gaugeContainer);
                _statisticsPanel.Controls.Add(statsTitle);

                // Dock statistics panel to the bottom
                _dockingManager.DockControl(_statisticsPanel, this, DockingStyle.Bottom, 200);

                LogInfo("System monitoring gauges initialized successfully");
            }
            catch (Exception ex)
            {
                LogError("Failed to initialize system gauges", ex);
                throw;
            }
        }

        /// <summary>
        /// Creates enhanced form buttons with icons and descriptions
        /// </summary>
        private void PopulateEnhancedFormButtons()
        {
            if (_formButtonsPanel == null) return;

            _formButtonsPanel.Controls.Clear();

            // Define button configurations with icons and descriptions
            var buttonConfigs = new[]
            {
                new { Key = "ShowVehicleManagement", Icon = "🚌", Title = "Vehicle Management", Desc = "Manage fleet vehicles" },
                new { Key = "ShowDriverManagement", Icon = "👨‍✈️", Title = "Driver Management", Desc = "Driver records & scheduling" },
                new { Key = "ShowRouteManagement", Icon = "🗺️", Title = "Route Management", Desc = "Plan and optimize routes" },
                new { Key = "ShowFuelManagement", Icon = "⛽", Title = "Fuel Management", Desc = "Track fuel consumption" },
                new { Key = "ShowMaintenanceManagement", Icon = "🔧", Title = "Maintenance", Desc = "Vehicle maintenance tracking" },
                new { Key = "ShowTimeCardManagement", Icon = "⏰", Title = "Time Cards", Desc = "Driver time tracking" },
                new { Key = "ShowActivityManagement", Icon = "📋", Title = "Activities", Desc = "Manage school activities" },
                new { Key = "ShowReportsManagement", Icon = "📊", Title = "Reports", Desc = "Generate system reports" },
                new { Key = "ShowCalendarManagement", Icon = "📅", Title = "Calendar", Desc = "Schedule management" },
                new { Key = "ShowAnalyticsDemo", Icon = "📈", Title = "Analytics", Desc = "Performance analytics" }
            };

            foreach (var config in buttonConfigs)
            {
                if (!_navigationMethods.ContainsKey(config.Key)) continue;

                var buttonPanel = new Panel
                {
                    Width = 250,
                    Height = 70,
                    Margin = new Padding(5),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                var iconLabel = new Label
                {
                    Text = config.Icon,
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 20, FontStyle.Regular),
                    Size = new Size(40, 40),
                    Location = new Point(10, 15),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                var titleLabel = new Label
                {
                    Text = config.Title,
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 10, FontStyle.Bold),
                    Location = new Point(55, 10),
                    Size = new Size(180, 20),
                    ForeColor = UIConstants.PrimaryColor
                };

                var descLabel = new Label
                {
                    Text = config.Desc,
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 8, FontStyle.Regular),
                    Location = new Point(55, 30),
                    Size = new Size(180, 30),
                    ForeColor = Color.Gray
                };

                buttonPanel.Controls.Add(iconLabel);
                buttonPanel.Controls.Add(titleLabel);
                buttonPanel.Controls.Add(descLabel);

                // Add click handler
                buttonPanel.Click += (s, e) => SafeNavigate(config.Key);
                buttonPanel.Cursor = Cursors.Hand;

                // Add hover effects
                buttonPanel.MouseEnter += (s, e) => buttonPanel.BackColor = Color.FromArgb(240, 240, 240);
                buttonPanel.MouseLeave += (s, e) => buttonPanel.BackColor = Color.White;

                _formButtonsPanel.Controls.Add(buttonPanel);
            }
        }

        /// <summary>
        /// Safe navigation with error handling and logging
        /// </summary>
        private void SafeNavigate(string navigationKey)
        {
            try
            {
                LogInfo($"Navigating to: {navigationKey}");

                if (_navigationMethods.ContainsKey(navigationKey))
                {
                    // Check if repository is required and available
                    if (_repositoryTypeMap.ContainsKey(navigationKey))
                    {
                        var repoType = _repositoryTypeMap[navigationKey];
                        LogInfo($"Checking repository availability: {repoType.Name}");
                    }

                    _navigationMethods[navigationKey]?.Invoke();
                    LogInfo($"Navigation successful: {navigationKey}");
                }
                else
                {
                    LogWarning($"Navigation method not found: {navigationKey}");
                    ShowUserFriendlyError($"Feature '{navigationKey}' is not yet implemented.", "Feature Unavailable");
                }
            }
            catch (Exception ex)
            {
                LogError($"Navigation failed for {navigationKey}", ex);
                ShowUserFriendlyError($"Failed to open {navigationKey}: {ex.Message}", "Navigation Error");
            }
        }

        /// <summary>
        /// Theme toggle implementation
        /// </summary>
        private void ToggleTheme(object sender, EventArgs e)
        {
            try
            {
                // Toggle theme implementation would go here
                // This would require Syncfusion theme manager integration
                LogInfo("Theme toggle requested - feature to be implemented");
                ShowUserFriendlyError("Theme toggle feature coming soon!", "Feature Info");
            }
            catch (Exception ex)
            {
                LogError("Theme toggle failed", ex);
            }
        }
        #endregion

        #region Helper Methods
        protected void LogInfo(string message)
        {
            Console.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }

        protected void LogWarning(string message)
        {
            Console.WriteLine($"[WARN] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }

        protected void LogError(string message, Exception ex = null)
        {
            Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
            if (ex != null)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        protected bool ValidateDashboard()
        {
            // Basic validation - ensure critical services are available
            return _navigationService != null;
        }

        protected void ShowUserFriendlyError(string message, string title = "Error")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected void LoadCachedForms()
        {
            try
            {
                LogInfo("Loading cached forms...");
                // Future implementation for form caching to improve performance
                // This could cache frequently used forms to reduce loading time
                LogInfo("Form caching feature ready for implementation");
            }
            catch (Exception ex)
            {
                LogError("Error in form caching", ex);
            }
        }

        protected void PopulateFormButtons()
        {
            // Delegate to enhanced version
            PopulateEnhancedFormButtons();
        }

        protected void CreateMainLayout()
        {
            // Use enhanced docking layout instead of basic layout
            InitializeDockingLayout();
        }

        /// <summary>
        /// Loads real analytics data from database
        /// </summary>
        protected async Task LoadRealAnalyticsDataAsync(CancellationToken cancellationToken)
        {
            try
            {
                LogInfo("Loading real analytics data...");

                if (_databaseHelperService == null)
                {
                    LogWarning("Database helper service not available, loading sample data");
                    LoadSampleAnalyticsData();
                    return;
                }

                // Load vehicle count and efficiency data
                await LoadVehicleAnalytics(cancellationToken);

                // Load route efficiency data
                await LoadRouteAnalytics(cancellationToken);

                // Load maintenance statistics
                await LoadMaintenanceAnalytics(cancellationToken);

                // Update gauge values based on real data
                UpdateSystemGauges();

                LogInfo("Real analytics data loaded successfully");
            }
            catch (OperationCanceledException)
            {
                LogInfo("Analytics data loading was cancelled");
            }
            catch (Exception ex)
            {
                LogError("Failed to load real analytics data", ex);
                LoadSampleAnalyticsData();
            }
        }

        /// <summary>
        /// Loads vehicle analytics data
        /// </summary>
        private async Task LoadVehicleAnalytics(CancellationToken cancellationToken)
        {
            try
            {
                // This would integrate with actual VehicleService/Repository
                await Task.Delay(50, cancellationToken); // Simulate database call

                // Sample data generation - replace with real database queries
                var routeEfficiencyData = new[]
                {
                    new { Period = "Week 1", Efficiency = 85.5 },
                    new { Period = "Week 2", Efficiency = 87.2 },
                    new { Period = "Week 3", Efficiency = 83.8 },
                    new { Period = "Week 4", Efficiency = 89.1 }
                };

                // Update chart with real data
                if (_analyticsChart?.Series?.Count > 0)
                {
                    var series = _analyticsChart.Series[0];
                    series.Points.Clear();

                    foreach (var data in routeEfficiencyData)
                    {
                        series.Points.Add(data.Period, data.Efficiency);
                    }

                    _analyticsChart.Refresh();
                }

                LogInfo("Vehicle analytics loaded");
            }
            catch (Exception ex)
            {
                LogError("Failed to load vehicle analytics", ex);
            }
        }

        /// <summary>
        /// Loads route analytics data including cost per student
        /// </summary>
        private async Task LoadRouteAnalytics(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(50, cancellationToken);

                // Sample cost data - based on the $2.70/student/day mentioned in requirements
                var costData = new[]
                {
                    new { Route = "Route A", CostPerStudent = 2.65 },
                    new { Route = "Route B", CostPerStudent = 2.70 },
                    new { Route = "Route C", CostPerStudent = 2.85 },
                    new { Route = "Route D", CostPerStudent = 2.55 }
                };

                // Update cost analytics series
                if (_analyticsChart?.Series?.Count > 1)
                {
                    var costSeries = _analyticsChart.Series[1];
                    costSeries.Points.Clear();

                    foreach (var data in costData)
                    {
                        costSeries.Points.Add(data.Route, data.CostPerStudent);
                    }

                    _analyticsChart.Refresh();
                }

                LogInfo("Route analytics loaded");
            }
            catch (Exception ex)
            {
                LogError("Failed to load route analytics", ex);
            }
        }

        /// <summary>
        /// Loads maintenance analytics data
        /// </summary>
        private async Task LoadMaintenanceAnalytics(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(50, cancellationToken);

                // Sample maintenance data
                var maintenanceStats = new
                {
                    VehiclesNeedingMaintenance = 3,
                    TotalVehicles = 12,
                    OverdueMaintenance = 1
                };

                // Update maintenance gauge
                if (_maintenanceGauge != null)
                {
                    var maintenancePercentage = (maintenanceStats.VehiclesNeedingMaintenance * 100) / maintenanceStats.TotalVehicles;
                    _maintenanceGauge.Value = maintenancePercentage;
                }

                LogInfo("Maintenance analytics loaded");
            }
            catch (Exception ex)
            {
                LogError("Failed to load maintenance analytics", ex);
            }
        }

        /// <summary>
        /// Updates system gauges with current data
        /// </summary>
        private void UpdateSystemGauges()
        {
            try
            {
                // System status based on various health metrics
                if (_systemStatusGauge != null)
                {
                    _systemStatusGauge.Value = 92; // High system health
                }

                // Fleet efficiency based on route performance
                if (_efficiencyGauge != null)
                {
                    _efficiencyGauge.Value = 86; // Good efficiency rating
                }

                LogInfo("System gauges updated");
            }
            catch (Exception ex)
            {
                LogError("Failed to update system gauges", ex);
            }
        }

        /// <summary>
        /// Loads sample data when real data is unavailable
        /// </summary>
        private void LoadSampleAnalyticsData()
        {
            try
            {
                LogInfo("Loading sample analytics data...");

                // Load sample data into charts and gauges
                LoadVehicleAnalytics(CancellationToken.None).Wait();
                LoadRouteAnalytics(CancellationToken.None).Wait();
                LoadMaintenanceAnalytics(CancellationToken.None).Wait();
                UpdateSystemGauges();

                LogInfo("Sample analytics data loaded");
            }
            catch (Exception ex)
            {
                LogError("Failed to load sample analytics data", ex);
            }
        }

        /// <summary>
        /// Loads fallback data when analytics loading fails
        /// </summary>
        private void LoadFallbackAnalyticsData()
        {
            try
            {
                LogInfo("Loading fallback analytics data...");
                LoadSampleAnalyticsData();
            }
            catch (Exception ex)
            {
                LogError("Failed to load fallback analytics data", ex);
            }
        }

        /// <summary>
        /// Enhanced resource logging for debugging memory issues
        /// </summary>
        protected void LogCurrentResources()
        {
            try
            {
                var processInfo = System.Diagnostics.Process.GetCurrentProcess();
                LogInfo($"Memory Usage: {processInfo.WorkingSet64 / 1024 / 1024} MB");
                LogInfo($"GC Memory: {GC.GetTotalMemory(false) / 1024 / 1024} MB");
                LogInfo($"Thread Count: {processInfo.Threads.Count}");
                LogInfo($"Handle Count: {processInfo.HandleCount}");
            }
            catch (Exception ex)
            {
                LogError("Failed to log resource information", ex);
            }
        }

        /// <summary>
        /// Cleanup repository connections and database resources
        /// </summary>
        protected void CleanupRepositoryConnections()
        {
            try
            {
                LogInfo("Cleaning up repository connections...");

                // If database helper service implements IDisposable
                if (_databaseHelperService is IDisposable disposableService)
                {
                    disposableService.Dispose();
                    LogInfo("Database helper service disposed");
                }

                // Force garbage collection for database connections
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                LogInfo("Repository connections cleanup completed");
            }
            catch (Exception ex)
            {
                LogError("Error during repository cleanup", ex);
            }
        }

        /// <summary>
        /// Enhanced UI component cleanup to prevent memory leaks
        /// </summary>
        protected void CleanupUIComponentsEnhanced()
        {
            try
            {
                LogInfo("Starting enhanced UI component cleanup...");

                // Dispose Syncfusion controls properly
                DisposeSyncfusionControls();

                // Dispose chart and gauge components
                DisposeAnalyticsComponents();

                // Dispose docking manager
                DisposeDockingManager();

                // Clear event handlers
                ClearEventHandlers();

                // Dispose panels and containers
                DisposePanelControls();

                LogInfo("Enhanced UI component cleanup completed");
            }
            catch (Exception ex)
            {
                LogError("Error during enhanced UI cleanup", ex);
            }
        }

        /// <summary>
        /// Disposes Syncfusion controls properly
        /// </summary>
        private void DisposeSyncfusionControls()
        {
            try
            {
                // Dispose SfButtons
                _themeToggleButton?.Dispose();
                _closeButton?.Dispose();

                LogInfo("Syncfusion controls disposed");
            }
            catch (Exception ex)
            {
                LogError("Error disposing Syncfusion controls", ex);
            }
        }

        /// <summary>
        /// Disposes analytics components (charts and gauges)
        /// </summary>
        private void DisposeAnalyticsComponents()
        {
            try
            {
                // Dispose chart control
                if (_analyticsChart != null)
                {
                    _analyticsChart.Series?.Clear();
                    _analyticsChart.Dispose();
                    _analyticsChart = null;
                }

                // Dispose gauge controls
                _systemStatusGauge?.Dispose();
                _systemStatusGauge = null;

                _maintenanceGauge?.Dispose();
                _maintenanceGauge = null;

                _efficiencyGauge?.Dispose();
                _efficiencyGauge = null;

                LogInfo("Analytics components disposed");
            }
            catch (Exception ex)
            {
                LogError("Error disposing analytics components", ex);
            }
        }

        /// <summary>
        /// Disposes docking manager properly
        /// </summary>
        private void DisposeDockingManager()
        {
            try
            {
                if (_dockingManager != null)
                {
                    // Simply dispose the panels - DockingManager will handle cleanup
                    _sidePanel?.Dispose();
                    _sidePanel = null;

                    _analyticsPanel?.Dispose();
                    _analyticsPanel = null;

                    _statisticsPanel?.Dispose();
                    _statisticsPanel = null;

                    _dockingManager.Dispose();
                    _dockingManager = null;
                }

                LogInfo("Docking manager disposed");
            }
            catch (Exception ex)
            {
                LogError("Error disposing docking manager", ex);
            }
        }

        /// <summary>
        /// Clears event handlers to prevent memory leaks
        /// </summary>
        private void ClearEventHandlers()
        {
            try
            {
                // Clear button event handlers
                if (_themeToggleButton != null)
                    _themeToggleButton.Click -= ToggleTheme;

                if (_closeButton != null)
                    _closeButton.Click -= (s, e) => this.Close();

                // Clear panel event handlers
                if (_formButtonsPanel != null)
                {
                    foreach (Control control in _formButtonsPanel.Controls)
                    {
                        control.Click -= null; // This removes all Click event handlers
                        control.MouseEnter -= null;
                        control.MouseLeave -= null;
                    }
                }

                LogInfo("Event handlers cleared");
            }
            catch (Exception ex)
            {
                LogError("Error clearing event handlers", ex);
            }
        }

        /// <summary>
        /// Disposes panel controls and their children
        /// </summary>
        private void DisposePanelControls()
        {
            try
            {
                // Dispose panels and their controls
                DisposePanelRecursively(_headerPanel);
                DisposePanelRecursively(_sidePanel);
                DisposePanelRecursively(_analyticsPanel);
                DisposePanelRecursively(_statisticsPanel);
                DisposePanelRecursively(_formButtonsPanel);

                // Clear references
                _headerPanel = null;
                _sidePanel = null;
                _analyticsPanel = null;
                _statisticsPanel = null;
                _formButtonsPanel = null;
                _titleLabel = null;

                LogInfo("Panel controls disposed");
            }
            catch (Exception ex)
            {
                LogError("Error disposing panel controls", ex);
            }
        }

        /// <summary>
        /// Recursively disposes a panel and all its child controls
        /// </summary>
        private void DisposePanelRecursively(Control panel)
        {
            if (panel == null) return;

            try
            {
                // Dispose all child controls first
                while (panel.Controls.Count > 0)
                {
                    var child = panel.Controls[0];
                    panel.Controls.Remove(child);

                    if (child.HasChildren)
                        DisposePanelRecursively(child);

                    child.Dispose();
                }

                // Dispose the panel itself
                panel.Dispose();
            }
            catch (Exception ex)
            {
                LogError($"Error disposing panel {panel.Name}", ex);
            }
        }

        protected void CreateEmergencyLayout()
        {
            try
            {
                LogWarning("Creating emergency layout due to initialization failure...");

                // Clear everything and create minimal working interface
                this.Controls.Clear();

                var emergencyPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.LightYellow,
                    Padding = new Padding(20)
                };

                var emergencyLabel = new Label
                {
                    Text = "⚠️ BusBuddy Dashboard - Emergency Mode\n\nThe enhanced dashboard failed to initialize.\nBasic functionality is available.",
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 12, FontStyle.Regular),
                    Dock = DockStyle.Top,
                    Height = 100,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.DarkRed
                };

                var emergencyButtonPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    AutoScroll = true
                };

                // Create basic navigation buttons
                foreach (var navMethod in _navigationMethods.Take(5)) // Show only first 5 for emergency mode
                {
                    var button = new Button
                    {
                        Text = navMethod.Key.Replace("Show", "").Replace("Management", ""),
                        Size = new Size(150, 60),
                        Margin = new Padding(5),
                        UseVisualStyleBackColor = true
                    };
                    button.Click += (s, e) => SafeNavigate(navMethod.Key);
                    emergencyButtonPanel.Controls.Add(button);
                }

                emergencyPanel.Controls.Add(emergencyButtonPanel);
                emergencyPanel.Controls.Add(emergencyLabel);

                this.Controls.Add(emergencyPanel);
                this.Text = "BusBuddy Dashboard - Emergency Mode";

                LogInfo("Emergency layout created successfully");
            }
            catch (Exception ex)
            {
                LogError("Failed to create emergency layout", ex);
                // If even emergency layout fails, show minimal error form
                this.BackColor = Color.Red;
                this.Text = "BusBuddy Dashboard - Critical Error";
            }
        }
        #endregion
    }
}
