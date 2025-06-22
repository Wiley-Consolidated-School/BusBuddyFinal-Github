using System;
using System.Collections.Generic;
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
        private TableLayoutPanel _mainTableLayout;
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
                LogInfo("Initializing dashboard...");

                // Start with enhanced layout first
                CreateMainLayout();
                LoadCachedForms();
                PopulateFormButtons();

                this.Text = "BusBuddy Dashboard - Enhanced Syncfusion";
                this.WindowState = FormWindowState.Maximized;
                this.Show();
                this.Refresh();

                // Load analytics asynchronously after basic UI is shown
                Task.Run(async () =>
                {
                    try
                    {
                        await LoadAnalyticsDataAsync(_cancellationTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        LogError("Analytics loading failed", ex);
                    }
                });

                LogInfo("Dashboard initialization completed");
            }
            catch (Exception ex)
            {
                LogError("Failed to initialize dashboard", ex);
                ShowUserFriendlyError($"Failed to initialize dashboard: {ex.Message}", "Critical Error");
                CreateEmergencyLayout();
            }
        }

        /// <summary>
        /// Creates a basic working layout that always works as primary approach
        /// </summary>
        private void CreateBasicLayout()
        {
            try
            {
                LogInfo("Creating basic layout...");

                // Clear any existing controls
                this.Controls.Clear();

                // Create simple working layout
                var mainPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = UIConstants.SurfaceColor,
                    Padding = new Padding(UIConstants.DefaultSpacing)
                };

                // Header
                var headerPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = UIConstants.HeaderHeight,
                    BackColor = UIConstants.PrimaryColor,
                    Padding = new Padding(UIConstants.LargeSpacing, 15, UIConstants.LargeSpacing, 15)
                };

                var titleLabel = new Label
                {
                    Text = "🚌 BusBuddy Management Dashboard",
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = Color.White,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent
                };

                headerPanel.Controls.Add(titleLabel);

                // Buttons panel
                _formButtonsPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    AutoScroll = true,
                    Padding = new Padding(UIConstants.LargeSpacing),
                    BackColor = UIConstants.SurfaceColor
                };

                mainPanel.Controls.Add(_formButtonsPanel);
                mainPanel.Controls.Add(headerPanel);

                this.Controls.Add(mainPanel);

                // Store references for later updates
                _headerPanel = headerPanel;
                _titleLabel = titleLabel;

                this.PerformLayout();

                LogInfo("Basic layout created successfully");
            }
            catch (Exception ex)
            {
                LogError("Basic layout failed", ex);
                throw;
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
            // Placeholder for form caching logic
        }

        protected void PopulateFormButtons()
        {
            // Create buttons for each navigation method
            if (_formButtonsPanel == null) return;

            _formButtonsPanel.Controls.Clear();

            foreach (var navigationMethod in _navigationMethods)
            {
                var button = new SfButton
                {
                    Text = navigationMethod.Key.Replace("Show", "").Replace("Management", ""),
                    Size = new Size(UIConstants.ButtonWidth, UIConstants.ButtonHeight),
                    Margin = new Padding(UIConstants.DefaultSpacing),
                    Tag = navigationMethod.Key
                };
                button.Click += (s, e) => navigationMethod.Value?.Invoke();
                _formButtonsPanel.Controls.Add(button);
            }
        }

        protected void CreateMainLayout()
        {
            CreateBasicLayout();
        }

        protected async Task LoadAnalyticsDataAsync(CancellationToken cancellationToken)
        {
            // Placeholder for analytics loading
            await Task.Delay(100, cancellationToken);
        }

        protected void LogCurrentResources()
        {
            // Resource logging placeholder
        }

        protected void CleanupRepositoryConnections()
        {
            // Repository cleanup placeholder
        }

        protected void CleanupUIComponentsEnhanced()
        {
            // UI cleanup placeholder
        }

        protected void CreateEmergencyLayout()
        {
            CreateBasicLayout();
        }
        #endregion
    }
}
