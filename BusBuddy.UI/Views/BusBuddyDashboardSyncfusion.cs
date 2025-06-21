using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
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
        private readonly INavigationService _navigationService;
        private readonly BusBuddy.UI.Services.IDatabaseHelperService _databaseHelperService;
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

        public BusBuddyDashboardSyncfusion(INavigationService navigationService, BusBuddy.UI.Services.IDatabaseHelperService databaseHelperService)
        {
            Console.WriteLine($"[DEBUG] Dashboard constructor called at {DateTime.Now:O}");
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _databaseHelperService = databaseHelperService ?? throw new ArgumentNullException(nameof(databaseHelperService));

            // Initialize repository type mapping
            _repositoryTypeMap = new Dictionary<string, Type>
            {
                { "ShowVehicleManagement", typeof(IVehicleRepository) },
                { "ShowDriverManagement", typeof(IDriverRepository) },
                { "ShowRouteManagement", typeof(IRouteRepository) },
                { "ShowFuelManagement", typeof(IFuelRepository) },
                { "ShowMaintenanceManagement", typeof(IMaintenanceRepository) },
                { "ShowTimeCardManagement", typeof(ITimeCardRepository) },
                { "ShowActivityManagement", typeof(IActivityRepository) }
                // Add other repository types as they become available
            };

            // Initialize navigation methods dictionary
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
            Console.WriteLine($"[DEBUG] Dashboard initialized at {DateTime.Now:O}");
        }

        // Constructor for testing - uses ServiceContainerSingleton to get services
        public BusBuddyDashboardSyncfusion()
        {
            try
            {
                Console.WriteLine($"[DEBUG] Dashboard test constructor called at {DateTime.Now:O}");

                // Initialize the ServiceContainerSingleton
                if (!ServiceContainerSingleton.IsInitialized)
                {
                    Console.WriteLine("⚠️ ServiceContainerSingleton not initialized, initializing now");
                    ServiceContainerSingleton.Initialize();
                }

                // Get services from the singleton
                _navigationService = ServiceContainerSingleton.Instance.GetService<INavigationService>();
                _databaseHelperService = ServiceContainerSingleton.Instance.GetService<BusBuddy.UI.Services.IDatabaseHelperService>();

                // Create a fallback navigation service if needed (for testing)
                if (_navigationService == null)
                {
                    Console.WriteLine("⚠️ Creating fallback navigation service for testing");
                    var container = ServiceContainerSingleton.Instance;
                    _navigationService = new NavigationService(container);
                }

                // Create a fallback database helper service if needed (for testing)
                if (_databaseHelperService == null)
                {
                    Console.WriteLine("⚠️ Skipping database helper service for testing - not needed for basic UI tests");
                    // In test environments, we can proceed without the database helper service
                    // as it's primarily used for diagnostics and not core navigation functionality
                }

                // Initialize repository type mapping - GitHub Lens recommended pattern
                _repositoryTypeMap = new Dictionary<string, Type>
                {
                    { "ShowVehicleManagement", typeof(IVehicleRepository) },
                    { "ShowDriverManagement", typeof(IDriverRepository) },
                    { "ShowRouteManagement", typeof(IRouteRepository) },
                    { "ShowFuelManagement", typeof(IFuelRepository) },
                    { "ShowMaintenanceManagement", typeof(IMaintenanceRepository) },
                    { "ShowTimeCardManagement", typeof(ITimeCardRepository) },
                    { "ShowActivityManagement", typeof(IActivityRepository) }
                    // Add other repository types as they become available
                };

                // Initialize navigation methods dictionary
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
                Console.WriteLine($"[DEBUG] Dashboard test instance initialized at {DateTime.Now:O}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in BusBuddyDashboardSyncfusion constructor: {ex.Message}");
                MessageBox.Show($"Error initializing dashboard: {ex.Message}", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Console.WriteLine($"[DEBUG] Dashboard OnLoad at {DateTime.Now:O}");
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Console.WriteLine($"[DEBUG] Dashboard OnShown at {DateTime.Now:O}");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                Console.WriteLine($"🧽 BusBuddyDashboardSyncfusion form closing at {DateTime.Now:O}");
                LogCurrentResources();
                CleanupRepositoryConnections();
                CleanupUIComponents();
                KillAllProcesses();
                Console.WriteLine($"✅ BusBuddyDashboardSyncfusion cleanup completed at {DateTime.Now:O}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during BusBuddyDashboardSyncfusion closing: {ex.Message}");
            }
            finally
            {
                base.OnFormClosing(e);
            }
        }

        private async Task RunBackgroundTask(Func<Task> taskFunc, string taskName)
        {
            Console.WriteLine($"[DEBUG] Starting background task: {taskName} at {DateTime.Now:O}");
            try
            {
                await taskFunc();
                Console.WriteLine($"[DEBUG] Background task '{taskName}' completed at {DateTime.Now:O}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception in background task '{taskName}': {ex.Message}");
            }
        }

        private void InitializeDashboard()
        {
            try
            {
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
                        await LoadAnalyticsDataAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Analytics loading failed: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize dashboard: {ex.Message}\n\nStack: {ex.StackTrace}",
                    "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                // Clear any existing controls
                this.Controls.Clear();

                // Create simple working layout
                var mainPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Padding = new Padding(10)
                };

                // Header
                var headerPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 60,
                    BackColor = Color.FromArgb(63, 81, 181),
                    Padding = new Padding(20, 15, 20, 15)
                };

                var titleLabel = new Label
                {
                    Text = "🚌 BusBuddy Management Dashboard",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
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
                    Padding = new Padding(20),
                    BackColor = Color.White
                };

                mainPanel.Controls.Add(_formButtonsPanel);
                mainPanel.Controls.Add(headerPanel);

                this.Controls.Add(mainPanel);

                // Store references for later updates
                _headerPanel = headerPanel;
                _titleLabel = titleLabel;

                this.PerformLayout();

                Console.WriteLine("Basic layout created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Basic layout failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Emergency layout for when everything else fails
        /// </summary>
        private void CreateEmergencyLayout()
        {
            try
            {
                this.Controls.Clear();

                var emergencyPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.LightGray,
                    Padding = new Padding(50)
                };

                var emergencyLabel = new Label
                {
                    Text = "BusBuddy Dashboard\n\nBasic Mode - Some features unavailable",
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    ForeColor = Color.DarkBlue,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };

                var buttonPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom,
                    Height = 200,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    Padding = new Padding(20)
                };

                // Add basic navigation buttons
                var basicButtons = new string[]
                {
                    "Vehicles", "Drivers", "Routes", "Maintenance", "Reports", "Settings"
                };

                foreach (var buttonText in basicButtons)
                {
                    var button = new Button
                    {
                        Text = buttonText,
                        Size = new Size(120, 60),
                        Margin = new Padding(10),
                        Font = new Font("Segoe UI", 10),
                        BackColor = Color.FromArgb(63, 81, 181),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };

                    button.Click += (s, e) => MessageBox.Show($"{buttonText} module loading...", "Info");
                    buttonPanel.Controls.Add(button);
                }

                emergencyPanel.Controls.Add(emergencyLabel);
                emergencyPanel.Controls.Add(buttonPanel);
                this.Controls.Add(emergencyPanel);

                _formButtonsPanel = buttonPanel;

                this.PerformLayout();
                this.Refresh();

                Console.WriteLine("Emergency layout created");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical failure: {ex.Message}", "Fatal Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Creates responsive main layout that adapts to different screen sizes and DPI settings
        /// Uses percentage-based sizing and proper anchor configurations
        /// </summary>
        private void CreateMainLayout()
        {
            try
            {
                // Clear any existing controls
                this.Controls.Clear();

                // Create the component container if needed
                if (this.components == null)
                {
                    this.components = new System.ComponentModel.Container();
                }

                // Create main table layout with responsive design
                _mainTableLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 3,
                    BackColor = SyncfusionThemeHelper.MaterialColors.Surface,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    AutoSize = true
                };

                // Configure responsive row styles
                _mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, ScaleForDpi(80)));  // Header - fixed height
                _mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));   // Buttons - takes majority
                _mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));   // Analytics - flexible

                // Handle form resize events for responsiveness
                this.Resize += OnFormResize;
                this.DpiChanged += OnDpiChanged;

                // Initialize the DockingManager with the component container
                _dockingManager = new Syncfusion.Windows.Forms.Tools.DockingManager(this.components);
                _dockingManager.HostControl = this;
                _dockingManager.EnableDocumentMode = true;
                _dockingManager.CaptionHeight = ScaleForDpi(25);
                _dockingManager.ShowCaptionImages = true;

                this.Controls.Add(_mainTableLayout);

                CreateResponsiveHeaderPanel();
                CreateResponsiveButtonsPanel();
                CreateResponsiveAnalyticsPanel();
                CreateSidePanel();
                CreateStatisticsPanel();
                CreateCloseButton();

                // Add panels to the docking manager
                SetupDockingLayout();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating main layout: {ex.Message}");
                CreateBasicLayout(); // Fallback to basic layout
            }
        }

        /// <summary>
        /// Scales size for current DPI settings
        /// </summary>
        private int ScaleForDpi(int baseSize)
        {
            try
            {
                using (var g = this.CreateGraphics())
                {
                    var scaleFactor = g.DpiX / 96.0f;
                    return (int)(baseSize * scaleFactor);
                }
            }
            catch
            {
                return baseSize;
            }
        }

        /// <summary>
        /// Handles form resize events for responsive layout
        /// </summary>
        private void OnFormResize(object sender, EventArgs e)
        {
            try
            {
                if (this.WindowState == FormWindowState.Minimized) return;

                // Adjust button layout based on form width
                AdjustButtonLayout();

                // Adjust analytics panel layout
                AdjustAnalyticsLayout();

                // Refresh the layout
                _mainTableLayout.PerformLayout();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during form resize: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles DPI change events
        /// </summary>
        private void OnDpiChanged(object sender, DpiChangedEventArgs e)
        {
            try
            {
                // Update row height for new DPI
                _mainTableLayout.RowStyles[0].Height = ScaleForDpi(80);

                // Update font sizes
                UpdateFontsForDpi();

                // Refresh layout
                this.PerformLayout();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during DPI change: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates fonts for DPI changes
        /// </summary>
        private void UpdateFontsForDpi()
        {
            if (_titleLabel != null)
                _titleLabel.Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 18, FontStyle.Bold);

            if (_formButtonsPanel?.Controls != null)
            {
                foreach (Control control in _formButtonsPanel.Controls)
                {
                    if (control is SfButton button)
                        button.Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 10, FontStyle.Regular);
                }
            }
        }

        /// <summary>
        /// Creates responsive header panel that adapts to screen size
        /// </summary>
        private void CreateResponsiveHeaderPanel()
        {
            _headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SyncfusionThemeHelper.MaterialColors.Primary,
                Padding = new Padding(ScaleForDpi(20), ScaleForDpi(10), ScaleForDpi(20), ScaleForDpi(10)),
                MinimumSize = new Size(0, ScaleForDpi(60))
            };

            // Add gradient background effect that responds to theme changes
            _headerPanel.Paint += (s, e) =>
            {
                var headerColor = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                    ? Color.FromArgb(33, 37, 41)
                    : Color.FromArgb(63, 81, 181);

                var gradientEnd = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                    ? Color.FromArgb(23, 27, 31)
                    : Color.FromArgb(48, 63, 159);

                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    _headerPanel.ClientRectangle,
                    headerColor,
                    gradientEnd,
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, _headerPanel.ClientRectangle);
                }
            };

            // Create responsive header layout
            var headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            _titleLabel = new Label
            {
                Text = "🚌 BusBuddy Management Dashboard",
                Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                AutoEllipsis = true  // Truncate text on small screens
            };

            // Add responsive theme toggle button
            _themeToggleButton = CreateResponsiveThemeToggleButton();

            headerLayout.Controls.Add(_titleLabel, 0, 0);
            headerLayout.Controls.Add(_themeToggleButton, 1, 0);
            _headerPanel.Controls.Add(headerLayout);
            _mainTableLayout.Controls.Add(_headerPanel, 0, 0);
        }

        /// <summary>
        /// Creates theme toggle button that adapts to available space
        /// </summary>
        private SfButton CreateResponsiveThemeToggleButton()
        {
            var buttonSize = new Size(ScaleForDpi(100), ScaleForDpi(35));

            var themeButton = new SfButton
            {
                Text = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark ? "☀️ Light" : "🌙 Dark",
                Size = buttonSize,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(ScaleForDpi(5)),
                AutoSize = false
            };

            themeButton.Style.BackColor = Color.FromArgb(100, 255, 255, 255);
            themeButton.Style.HoverBackColor = Color.FromArgb(150, 255, 255, 255);
            themeButton.Style.PressedBackColor = Color.FromArgb(200, 255, 255, 255);
            themeButton.Style.Border = new Pen(Color.White, 1);

            themeButton.Click += (s, e) => ToggleTheme();

            var tooltip = new ToolTip();
            tooltip.SetToolTip(themeButton, "Toggle between light and dark theme");

            return themeButton;
        }

        /// <summary>
        /// Creates responsive buttons panel with flexible layout
        /// </summary>
        private void CreateResponsiveButtonsPanel()
        {
            var buttonContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                    ? Color.FromArgb(43, 47, 51)
                    : Color.White,
                AutoScroll = true
            };

            _formButtonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(ScaleForDpi(10)),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            buttonContainer.Controls.Add(_formButtonsPanel);
            _mainTableLayout.Controls.Add(buttonContainer, 0, 1);
        }

        /// <summary>
        /// Creates responsive analytics panel with flexible sizing
        /// </summary>
        private void CreateResponsiveAnalyticsPanel()
        {
            try
            {
                _analyticsPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                        ? Color.FromArgb(38, 42, 46)
                        : Color.FromArgb(248, 249, 250),
                    Padding = new Padding(ScaleForDpi(20)),
                    BorderStyle = BorderStyle.FixedSingle,
                    AutoScroll = true,
                    MinimumSize = new Size(0, ScaleForDpi(200)),
                    Tag = "AnalyticsPanel"
                };

                var analyticsLabel = new Label
                {
                    Text = "📊 Analytics Dashboard",
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                        ? Color.White
                        : Color.FromArgb(33, 37, 41),
                    Location = new Point(ScaleForDpi(10), ScaleForDpi(10)),
                    Size = new Size(ScaleForDpi(250), ScaleForDpi(30)),
                    BackColor = Color.Transparent,
                    Dock = DockStyle.Top
                };

                // Create enhanced analytics with responsive design
                CreateEnhancedAnalyticsChart();
                CreateStatusGauges();

                _analyticsPanel.Controls.Add(analyticsLabel);
                _mainTableLayout.Controls.Add(_analyticsPanel, 0, 2);
                this.Controls.Add(_analyticsPanel);

                Console.WriteLine("Analytics panel created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating analytics panel: {ex.Message}");
            }
        }

        /// <summary>
        /// Adjusts button layout based on available space
        /// </summary>
        private void AdjustButtonLayout()
        {
            if (_formButtonsPanel == null) return;

            try
            {
                var availableWidth = _formButtonsPanel.ClientSize.Width - _formButtonsPanel.Padding.Horizontal;
                var buttonWidth = ScaleForDpi(180);
                var buttonsPerRow = Math.Max(1, availableWidth / (buttonWidth + ScaleForDpi(20)));

                // Adjust button sizes for very narrow screens
                if (buttonsPerRow == 1 && availableWidth < ScaleForDpi(200))
                {
                    buttonWidth = Math.Max(ScaleForDpi(120), availableWidth - ScaleForDpi(40));

                    foreach (Control control in _formButtonsPanel.Controls)
                    {
                        if (control is SfButton button)
                        {
                            button.Size = new Size(buttonWidth, ScaleForDpi(60));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adjusting button layout: {ex.Message}");
            }
        }

        /// <summary>
        /// Adjusts analytics panel layout based on available space
        /// </summary>
        private void AdjustAnalyticsLayout()
        {
            if (_analyticsPanel == null) return;

            try
            {
                var availableWidth = _analyticsPanel.ClientSize.Width - _analyticsPanel.Padding.Horizontal;
                var availableHeight = _analyticsPanel.ClientSize.Height - _analyticsPanel.Padding.Vertical;

                // Adjust chart size
                if (_analyticsChart != null)
                {
                    var chartWidth = Math.Min(ScaleForDpi(500), (int)(availableWidth * 0.6));
                    var chartHeight = Math.Min(ScaleForDpi(250), (int)(availableHeight * 0.7));

                    _analyticsChart.Size = new Size(chartWidth, chartHeight);
                }

                // Adjust gauge positions for narrow screens
                AdjustGaugeLayout(availableWidth);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adjusting analytics layout: {ex.Message}");
            }
        }

        /// <summary>
        /// Adjusts gauge layout for different screen sizes
        /// </summary>
        private void AdjustGaugeLayout(int availableWidth)
        {
            if (_systemStatusGauge == null || _maintenanceGauge == null || _efficiencyGauge == null) return;

            try
            {
                var gaugeSize = ScaleForDpi(100);
                var spacing = ScaleForDpi(20);
                var totalGaugeWidth = 3 * gaugeSize + 2 * spacing;

                if (totalGaugeWidth > availableWidth)
                {
                    // Stack gauges vertically on narrow screens
                    _systemStatusGauge.Location = new Point(ScaleForDpi(20), ScaleForDpi(50));
                    _maintenanceGauge.Location = new Point(ScaleForDpi(20), ScaleForDpi(170));
                    _efficiencyGauge.Location = new Point(ScaleForDpi(20), ScaleForDpi(290));
                }
                else
                {
                    // Arrange horizontally on wider screens
                    var startX = (availableWidth - totalGaugeWidth) / 2;
                    _systemStatusGauge.Location = new Point(startX, ScaleForDpi(80));
                    _maintenanceGauge.Location = new Point(startX + gaugeSize + spacing, ScaleForDpi(80));
                    _efficiencyGauge.Location = new Point(startX + 2 * (gaugeSize + spacing), ScaleForDpi(80));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adjusting gauge layout: {ex.Message}");
            }
        }

        private void CreateHeaderPanel()
        {
            _headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SyncfusionThemeHelper.MaterialColors.Primary,
                Padding = new Padding(20, 10, 20, 10)
            };

            // Add gradient background effect
            _headerPanel.Paint += (s, e) =>
            {
                var headerColor = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                    ? Color.FromArgb(33, 37, 41)
                    : Color.FromArgb(63, 81, 181);

                var gradientEnd = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                    ? Color.FromArgb(23, 27, 31)
                    : Color.FromArgb(48, 63, 159);

                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    _headerPanel.ClientRectangle,
                    headerColor,
                    gradientEnd,
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, _headerPanel.ClientRectangle);
                }
            };

            // Create header layout
            var headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            _titleLabel = new Label
            {
                Text = "🚌 BusBuddy Management Dashboard",
                Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            // Add theme toggle button
            _themeToggleButton = CreateThemeToggleButton();

            headerLayout.Controls.Add(_titleLabel, 0, 0);
            headerLayout.Controls.Add(_themeToggleButton, 1, 0);
            _headerPanel.Controls.Add(headerLayout);
            _mainTableLayout.Controls.Add(_headerPanel, 0, 0);
        }

        /// <summary>
        /// Load cached forms information for navigation buttons
        /// </summary>
        private void LoadCachedForms()
        {
            try
            {
                Console.WriteLine("Loading cached forms for navigation...");
                // This method loads form metadata for creating navigation buttons
                // Implementation will populate form buttons based on discovered forms
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading cached forms: {ex.Message}");
            }
        }

        /// <summary>
        /// Populate form buttons for navigation
        /// </summary>
        private void PopulateFormButtons()
        {
            try
            {
                if (_formButtonsPanel == null) return;

                var buttonConfigs = new[]
                {
                    new { Text = "🚌 Vehicles", Action = "ShowVehicleManagement", Color = Color.FromArgb(33, 150, 243) },
                    new { Text = "👨‍✈️ Drivers", Action = "ShowDriverManagement", Color = Color.FromArgb(76, 175, 80) },
                    new { Text = "🗺️ Routes", Action = "ShowRouteManagement", Color = Color.FromArgb(255, 152, 0) },
                    new { Text = "📋 Activities", Action = "ShowActivityManagement", Color = Color.FromArgb(156, 39, 176) },
                    new { Text = "⛽ Fuel", Action = "ShowFuelManagement", Color = Color.FromArgb(244, 67, 54) },
                    new { Text = "🔧 Maintenance", Action = "ShowMaintenanceManagement", Color = Color.FromArgb(96, 125, 139) },
                    new { Text = "📅 Calendar", Action = "ShowCalendarManagement", Color = Color.FromArgb(255, 193, 7) },
                    new { Text = "📊 Reports", Action = "ShowReportsManagement", Color = Color.FromArgb(63, 81, 181) }
                };

                foreach (var config in buttonConfigs)
                {
                    var button = new SfButton
                    {
                        Text = config.Text,
                        Size = new Size(180, 80),
                        Margin = new Padding(10),
                        Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 10, FontStyle.Regular),
                        ForeColor = Color.White,
                        Cursor = Cursors.Hand
                    };

                    button.Style.BackColor = config.Color;
                    button.Style.HoverBackColor = ControlPaint.Light(config.Color, 0.3f);
                    button.Style.PressedBackColor = ControlPaint.Dark(config.Color, 0.1f);

                    string actionName = config.Action;
                    button.Click += (s, e) => HandleButtonClick(actionName);

                    _formButtonsPanel.Controls.Add(button);
                }

                Console.WriteLine($"Created {buttonConfigs.Length} navigation buttons");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error populating form buttons: {ex.Message}");
            }
        }        /// <summary>
        /// Handle button click navigation
        /// </summary>
        private void HandleButtonClick(string actionName)
        {
            try
            {
                Console.WriteLine($"📋 Navigation button clicked: {actionName}");

                // Make sure the ServiceContainerSingleton is initialized
                if (!ServiceContainerSingleton.IsInitialized)
                {
                    Console.WriteLine("⚠️ ServiceContainerSingleton not initialized, initializing now");
                    ServiceContainerSingleton.Initialize();
                }

                // GitHub Lens Pattern: Initialize repository using type mapping
                // This provides a centralized, systematic approach to repository initialization
                if (_repositoryTypeMap.TryGetValue(actionName, out Type repositoryType))
                {
                    Console.WriteLine($"� Initializing {repositoryType.Name} before navigation");
                    EnsureRepositoryByType(repositoryType);
                }

                // After ensuring repository initialization, navigate to the form
                if (_navigationMethods.ContainsKey(actionName))
                {
                    _navigationMethods[actionName]?.Invoke();
                }
                else
                {
                    MessageBox.Show($"Navigation for {actionName} not implemented yet.", "Info");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error navigating to {actionName}: {ex.Message}");
                MessageBox.Show($"Error navigating to {actionName}: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Load analytics data asynchronously
        /// </summary>
        private async Task LoadAnalyticsDataAsync()
        {
            try
            {
                await Task.Delay(100); // Simulate loading
                Console.WriteLine("Analytics data loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading analytics: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggle between light and dark theme
        /// </summary>
        private void ToggleTheme()
        {
            try
            {
                SyncfusionThemeHelper.CurrentTheme = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                    ? SyncfusionThemeHelper.ThemeMode.Light
                    : SyncfusionThemeHelper.ThemeMode.Dark;

                SyncfusionThemeHelper.MaterialTheme.IsDarkMode = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark;

                if (_themeToggleButton != null)
                {
                    _themeToggleButton.Text = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark ? "☀️ Light" : "🌙 Dark";
                }

                // Refresh the form with new theme
                this.Invalidate(true);
                Console.WriteLine($"Theme toggled to {SyncfusionThemeHelper.CurrentTheme}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Create enhanced analytics chart
        /// </summary>
        private void CreateEnhancedAnalyticsChart()
        {
            try
            {
                // Initialize the chart field to avoid warnings
                _analyticsChart = new ChartControl();

                // Create a simple placeholder for analytics chart
                var chartPanel = new Panel
                {
                    Size = new Size(400, 200),
                    Location = new Point(20, 50),
                    BackColor = Color.FromArgb(248, 249, 250),
                    BorderStyle = BorderStyle.FixedSingle
                };

                var chartLabel = new Label
                {
                    Text = "📈 Fleet Analytics Chart\n(Chart implementation pending)",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 12, FontStyle.Regular)
                };

                chartPanel.Controls.Add(chartLabel);
                _analyticsPanel?.Controls.Add(chartPanel);

                Console.WriteLine("Analytics chart placeholder created");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating analytics chart: {ex.Message}");
            }
        }

        /// <summary>
        /// Create status gauges
        /// </summary>
        private void CreateStatusGauges()
        {
            try
            {
                // Initialize the gauge fields to avoid warnings
                _systemStatusGauge = new RadialGauge();
                _maintenanceGauge = new RadialGauge();
                _efficiencyGauge = new RadialGauge();

                // Create simple gauge placeholders
                var gaugeNames = new[] { "System Status", "Maintenance", "Efficiency" };
                var startX = 450;
                var gaugeSize = 100;

                for (int i = 0; i < gaugeNames.Length; i++)
                {
                    var gaugePanel = new Panel
                    {
                        Size = new Size(gaugeSize, gaugeSize),
                        Location = new Point(startX + (i * (gaugeSize + 20)), 80),
                        BackColor = Color.FromArgb(240, 240, 240),
                        BorderStyle = BorderStyle.FixedSingle
                    };

                    var gaugeLabel = new Label
                    {
                        Text = $"{gaugeNames[i]}\n85%",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 9, FontStyle.Regular)
                    };

                    gaugePanel.Controls.Add(gaugeLabel);
                    _analyticsPanel?.Controls.Add(gaugePanel);
                }

                Console.WriteLine("Status gauges created");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating status gauges: {ex.Message}");
            }
        }

        /// <summary>
        /// Create theme toggle button
        /// </summary>
        private SfButton CreateThemeToggleButton()
        {
            try
            {
                return CreateResponsiveThemeToggleButton();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating theme toggle button: {ex.Message}");
                return new SfButton { Text = "Theme", Size = new Size(80, 30) };
            }
        }

        /// <summary>
        /// Creates the side panel for the dashboard
        /// </summary>
        private void CreateSidePanel()
        {
            try
            {
                _sidePanel = new Panel
                {
                    Size = new Size(250, 400),
                    BackColor = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                        ? Color.FromArgb(43, 47, 51)
                        : Color.FromArgb(240, 240, 240),
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(10),
                    Tag = "SidePanel"
                };

                // Add a header label
                var sidePanelHeader = new Label
                {
                    Text = "Navigation",
                    Dock = DockStyle.Top,
                    Height = 30,
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                        ? Color.White
                        : Color.FromArgb(63, 81, 181),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Create a treeview for navigation
                var navTreeView = new TreeView
                {
                    Dock = DockStyle.Fill,
                    BorderStyle = BorderStyle.None,
                    ShowLines = true,
                    ShowPlusMinus = true,
                    BackColor = _sidePanel.BackColor,
                    ForeColor = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                        ? Color.White
                        : Color.Black
                };

                // Add some nodes
                var vehiclesNode = navTreeView.Nodes.Add("Vehicles");
                vehiclesNode.Nodes.Add("View All");
                vehiclesNode.Nodes.Add("Add New");
                vehiclesNode.Nodes.Add("Maintenance");

                var driversNode = navTreeView.Nodes.Add("Drivers");
                driversNode.Nodes.Add("View All");
                driversNode.Nodes.Add("Add New");
                driversNode.Nodes.Add("Time Cards");

                var routesNode = navTreeView.Nodes.Add("Routes");
                routesNode.Nodes.Add("View All");
                routesNode.Nodes.Add("Add New");
                routesNode.Nodes.Add("Map View");

                var activitiesNode = navTreeView.Nodes.Add("Activities");
                activitiesNode.Nodes.Add("Calendar");
                activitiesNode.Nodes.Add("Scheduling");

                navTreeView.ExpandAll();

                // Add controls to the side panel
                _sidePanel.Controls.Add(navTreeView);
                _sidePanel.Controls.Add(sidePanelHeader);

                this.Controls.Add(_sidePanel);
                Console.WriteLine("Side panel created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating side panel: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates the statistics panel
        /// </summary>
        private void CreateStatisticsPanel()
        {
            try
            {
                _statisticsPanel = new Panel
                {
                    Size = new Size(300, 250),
                    BackColor = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                        ? Color.FromArgb(43, 47, 51)
                        : Color.FromArgb(245, 245, 245),
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(10),
                    Tag = "StatisticsPanel"
                };

                // Add a header label
                var statsHeader = new Label
                {
                    Text = "Fleet Statistics",
                    Dock = DockStyle.Top,
                    Height = 30,
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                        ? Color.White
                        : Color.FromArgb(63, 81, 181),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Create a table layout for statistics
                var statsLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 4,
                    BackColor = Color.Transparent
                };

                // Add some stat labels
                AddStatLabel(statsLayout, "Total Vehicles:", "42", 0);
                AddStatLabel(statsLayout, "Active Drivers:", "38", 1);
                AddStatLabel(statsLayout, "Routes Today:", "15", 2);
                AddStatLabel(statsLayout, "Maintenance Due:", "3", 3);

                // Add controls to the stats panel
                _statisticsPanel.Controls.Add(statsLayout);
                _statisticsPanel.Controls.Add(statsHeader);

                this.Controls.Add(_statisticsPanel);
                Console.WriteLine("Statistics panel created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating statistics panel: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper method to add a stat label pair to the layout
        /// </summary>
        private void AddStatLabel(TableLayoutPanel layout, string name, string value, int row)
        {
            var nameLabel = new Label
            {
                Text = name,
                Dock = DockStyle.Fill,
                Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 9, FontStyle.Regular),
                ForeColor = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                    ? Color.LightGray
                    : Color.DimGray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var valueLabel = new Label
            {
                Text = value,
                Dock = DockStyle.Fill,
                Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 9, FontStyle.Bold),
                ForeColor = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                    ? Color.White
                    : Color.Black,
                TextAlign = ContentAlignment.MiddleRight
            };

            layout.Controls.Add(nameLabel, 0, row);
            layout.Controls.Add(valueLabel, 1, row);
        }

        /// <summary>
        /// Creates the close button
        /// </summary>
        private void CreateCloseButton()
        {
            try
            {
                _closeButton = new SfButton
                {
                    Text = "Close",
                    Size = new Size(100, 35),
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                    Location = new Point(this.ClientSize.Width - 120, this.ClientSize.Height - 50),
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 9, FontStyle.Regular),
                    ForeColor = Color.White
                };

                _closeButton.Style.BackColor = Color.FromArgb(211, 47, 47); // Red
                _closeButton.Style.HoverBackColor = Color.FromArgb(229, 57, 53);
                _closeButton.Style.PressedBackColor = Color.FromArgb(183, 28, 28);
                _closeButton.Style.FocusedBackColor = Color.FromArgb(229, 57, 53);

                _closeButton.Click += (s, e) => CloseApplication();

                this.Controls.Add(_closeButton);
                Console.WriteLine("Close button created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating close button: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets up the docking layout with all panels
        /// </summary>
        private void SetupDockingLayout()
        {
            try
            {
                if (_dockingManager == null)
                {
                    Console.WriteLine("⚠️ DockingManager is null, cannot setup layout");
                    return;
                }

                _dockingManager.BeginInit();

                // Enable docking for the side panel
                if (_sidePanel != null)
                {
                    _dockingManager.SetEnableDocking(_sidePanel, true);
                    _dockingManager.DockControl(_sidePanel, this, Syncfusion.Windows.Forms.Tools.DockingStyle.Left, 250);
                    _dockingManager.SetDockLabel(_sidePanel, "Navigation");
                }

                // Enable docking for the statistics panel
                if (_statisticsPanel != null)
                {
                    _dockingManager.SetEnableDocking(_statisticsPanel, true);
                    _dockingManager.DockControl(_statisticsPanel, this, Syncfusion.Windows.Forms.Tools.DockingStyle.Bottom, 250);
                    _dockingManager.SetDockLabel(_statisticsPanel, "Fleet Statistics");
                }

                // Enable docking for the analytics panel
                if (_analyticsPanel != null)
                {
                    _dockingManager.SetEnableDocking(_analyticsPanel, true);
                    _dockingManager.DockControl(_analyticsPanel, this, Syncfusion.Windows.Forms.Tools.DockingStyle.Right, 300);
                    _dockingManager.SetDockLabel(_analyticsPanel, "Analytics");
                }

                _dockingManager.EndInit();
                Console.WriteLine("Docking layout setup successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up docking layout: {ex.Message}");
            }
        }

        /// <summary>
        /// Safely closes the application and ensures all resources are cleaned up
        /// </summary>
        private void CloseApplication()
        {
            try
            {
                Console.WriteLine("📋 Close button clicked - closing application");

                // Begin cleanup process
                // Force additional cleanup to stop lingering .NET instances
                KillAllProcesses();

                // Close the form properly
                this.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during application close: {ex.Message}");
                this.Close(); // Force close anyway
            }
        }

        /// <summary>
        /// Enhanced method to kill all potential process handles that might be lingering
        /// </summary>
        private void KillAllProcesses()
        {
            try
            {
                // Dispose any additional components first
                if (_dockingManager != null)
                {
                    try
                    {
                        // Remove all controls from docking manager first
                        foreach (Control control in this.Controls)
                        {
                            if (_dockingManager.GetEnableDocking(control))
                            {
                                _dockingManager.SetEnableDocking(control, false);
                            }
                        }

                        _dockingManager.Dispose();
                        _dockingManager = null;
                        Console.WriteLine("🧽 DockingManager disposed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error disposing DockingManager: {ex.Message}");
                    }
                }

                // Dispose all Syncfusion controls
                if (_analyticsChart != null)
                {
                    _analyticsChart.Dispose();
                    _analyticsChart = null;
                    Console.WriteLine("🧽 AnalyticsChart disposed");
                }

                if (_systemStatusGauge != null)
                {
                    _systemStatusGauge.Dispose();
                    _systemStatusGauge = null;
                    Console.WriteLine("🧽 SystemStatusGauge disposed");
                }

                if (_maintenanceGauge != null)
                {
                    _maintenanceGauge.Dispose();
                    _maintenanceGauge = null;
                    Console.WriteLine("🧽 MaintenanceGauge disposed");
                }

                if (_efficiencyGauge != null)
                {
                    _efficiencyGauge.Dispose();
                    _efficiencyGauge = null;
                    Console.WriteLine("🧽 EfficiencyGauge disposed");
                }

                // Clean up panel resources
                if (_sidePanel != null)
                {
                    _sidePanel.Dispose();
                    _sidePanel = null;
                    Console.WriteLine("🧽 SidePanel disposed");
                }

                if (_statisticsPanel != null)
                {
                    _statisticsPanel.Dispose();
                    _statisticsPanel = null;
                    Console.WriteLine("🧽 StatisticsPanel disposed");
                }

                if (_analyticsPanel != null)
                {
                    _analyticsPanel.Dispose();
                    _analyticsPanel = null;
                    Console.WriteLine("🧽 AnalyticsPanel disposed");
                }

                // Force garbage collection to clean up lingering resources
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                Console.WriteLine("All processes and resources cleaned up");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error killing processes: {ex.Message}");
            }
        }

        /// <summary>
        /// GitHub Lens Pattern: Log all resources before cleanup for diagnostics
        /// </summary>
        private void LogCurrentResources()
        {
            try
            {
                Console.WriteLine("📊 Resource Diagnostics:");
                Console.WriteLine($"   - DockingManager: {(_dockingManager != null ? "Active" : "Null")}");
                Console.WriteLine($"   - Panels: Side={_sidePanel != null}, Analytics={_analyticsPanel != null}, Statistics={_statisticsPanel != null}");
                Console.WriteLine($"   - Charts: {(_analyticsChart != null ? "Active" : "Null")}");
                Console.WriteLine($"   - Gauges: {_systemStatusGauge != null && _maintenanceGauge != null && _efficiencyGauge != null}");
                Console.WriteLine($"   - ServiceContainer: {(ServiceContainerSingleton.IsInitialized ? "Initialized" : "Not Initialized")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error logging resources: {ex.Message}");
            }
        }

        /// <summary>
        /// GitHub Lens Pattern: Systematic cleanup of data connections
        /// </summary>
        private void CleanupRepositoryConnections()
        {
            try
            {
                // Always disconnect from data sources before UI cleanup
                if (ServiceContainerSingleton.IsInitialized)
                {
                    foreach (var entry in _repositoryTypeMap)
                    {
                        try
                        {
                            Console.WriteLine($"🔄 Closing repository connection for {entry.Value.Name}");
                            // This would ideally call a method to cleanly close the repository connection
                            // For now, we just ensure proper logging for diagnostic purposes
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error closing repository {entry.Value.Name}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error cleaning up repository connections: {ex.Message}");
            }
        }

        /// <summary>
        /// GitHub Lens Pattern: Systematic UI component cleanup
        /// </summary>
        private void CleanupUIComponents()
        {
            try
            {
                // Remove all controls from docking manager first
                if (_dockingManager != null)
                {
                    try
                    {
                        foreach (Control control in this.Controls)
                        {
                            if (_dockingManager.GetEnableDocking(control))
                            {
                                _dockingManager.SetEnableDocking(control, false);
                            }
                        }

                        _dockingManager.Dispose();
                        _dockingManager = null;
                        Console.WriteLine("🧽 DockingManager disposed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error disposing DockingManager: {ex.Message}");
                    }
                }

                // Dispose all Syncfusion charts and controls in reverse creation order
                DisposeSyncfusionControl(ref _efficiencyGauge, "EfficiencyGauge");
                DisposeSyncfusionControl(ref _maintenanceGauge, "MaintenanceGauge");
                DisposeSyncfusionControl(ref _systemStatusGauge, "SystemStatusGauge");
                DisposeSyncfusionControl(ref _analyticsChart, "AnalyticsChart");

                // Dispose panel resources
                DisposeControl(ref _sidePanel, "SidePanel");
                DisposeControl(ref _statisticsPanel, "StatisticsPanel");
                DisposeControl(ref _analyticsPanel, "AnalyticsPanel");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error cleaning up UI components: {ex.Message}");
            }
        }

        /// <summary>
        /// GitHub Lens Pattern: Generic Syncfusion control disposal with proper null-setting
        /// </summary>
        private void DisposeSyncfusionControl<T>(ref T control, string name) where T : class, IDisposable
        {
            try
            {
                if (control != null)
                {
                    control.Dispose();
                    control = null;
                    Console.WriteLine($"🧽 {name} disposed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error disposing {name}: {ex.Message}");
            }
        }

        /// <summary>
        /// GitHub Lens Pattern: Generic control disposal with proper null-setting
        /// </summary>
        private void DisposeControl(ref Panel control, string name)
        {
            try
            {
                if (control != null)
                {
                    control.Dispose();
                    control = null;
                    Console.WriteLine($"🧽 {name} disposed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error disposing {name}: {ex.Message}");
            }
        }

        /// <summary>
        /// GitHub Lens Pattern: Generic repository initialization by type
        /// This method provides a flexible way to initialize any repository type
        /// </summary>
        /// <param name="repositoryType">The type of repository to initialize</param>
        private void EnsureRepositoryByType(Type repositoryType)
        {
            try
            {
                // Use reflection to create the correct generic method call
                // This is more maintainable than numerous if/else statements
                var method = typeof(ServiceContainerSingleton).GetMethod("EnsureRepository")
                    ?.MakeGenericMethod(repositoryType);

                if (method != null)
                {
                    method.Invoke(null, null);
                }
                else
                {
                    Console.WriteLine($"⚠️ Could not find EnsureRepository method for {repositoryType.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error initializing repository {repositoryType.Name}: {ex.Message}");
            }
        }
    }
}
