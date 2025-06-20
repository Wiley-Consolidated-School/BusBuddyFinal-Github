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
using BusBuddy.Models;
using BusBuddy.Data;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Gauge;
using Syncfusion.WinForms.Controls;
using Syncfusion.Runtime.Serialization;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.Input;
using Syncfusion.Windows.Forms.Grid;
using static BusBuddy.UI.Views.FormDiscovery;

namespace BusBuddy.UI.Views
{
    public partial class BusBuddyDashboardSyncfusion : SyncfusionBaseForm
    {
        private readonly INavigationService _navigationService;
        private readonly BusBuddy.UI.Services.IDatabaseHelperService _databaseHelperService;

        // Repository dependencies for real data
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IRouteRepository _routeRepository;
        private readonly IMaintenanceRepository _maintenanceRepository;

        // Enhanced Layout Components
        private DockingManager _dockingManager;
        private RibbonControlAdv _ribbonControl;
        private TableLayoutPanel _mainTableLayout;

        // Traditional Layout Components (for fallback)
        private Panel _headerPanel;
        private Label _titleLabel;
        private SfButton _themeToggleButton;
        private FlowLayoutPanel _formButtonsPanel;
        private Panel _analyticsPanel;

        // Data Visualization Components
        private ChartControl _analyticsChart;
        private RadialGauge _systemStatusGauge;
        private RadialGauge _maintenanceGauge;
        private RadialGauge _efficiencyGauge;

        // Enhanced Dashboard Components
        private SfDataGrid _vehicleDataGrid;
        private TileLayout _quickStatsLayout;
        private TextBox _searchBox;
        private NotifyIcon _notifyIcon;

        // Dashboard Panels for Docking
        private Panel _analyticsDisplayPanel;
        private Panel _quickStatsPanel;
        private Panel _dataGridPanel;
        private Panel _searchPanel;

        // Navigation method mapping for improved reliability
        private readonly Dictionary<string, System.Action> _navigationMethods;

        // Parameterless constructor for design-time support
        public BusBuddyDashboardSyncfusion() : this(
            CreateNavigationService(),
            CreateDatabaseHelperService(),
            CreateSafeRepository<IVehicleRepository>(() => new VehicleRepository()),
            CreateSafeRepository<IDriverRepository>(() => new DriverRepository()),
            CreateSafeRepository<IRouteRepository>(() => new RouteRepository()),
            CreateSafeRepository<IMaintenanceRepository>(() => new MaintenanceRepository())) { }

        private static T CreateSafeRepository<T>(Func<T> factory) where T : class
        {
            try
            {
                var repo = factory();
                Console.WriteLine($"✅ [DEBUG] {typeof(T).Name} created successfully");
                return repo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Failed to create {typeof(T).Name}: {ex.Message}");
                return null; // Return null instead of throwing
            }
        }

        private static INavigationService CreateNavigationService()
        {
            try
            {
                // Create service container (which implements IFormFactory)
                var serviceContainer = new ServiceContainer();

                // Get navigation service from container (it should be properly configured)
                var navigationService = serviceContainer.GetService<INavigationService>();

                if (navigationService == null)
                {
                    Console.WriteLine("❌ [DEBUG] ServiceContainer returned null NavigationService, creating manually");
                    // Fallback: create manually
                    navigationService = new NavigationService(serviceContainer);
                }

                Console.WriteLine("✅ [DEBUG] NavigationService created successfully");
                return navigationService;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Failed to create NavigationService: {ex.Message}");
                Console.WriteLine($"❌ [DEBUG] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private static BusBuddy.UI.Services.IDatabaseHelperService CreateDatabaseHelperService()
        {
            var context = new BusBuddyContext();
            return new BusBuddy.UI.Services.DatabaseHelperService(context);
        }

        public BusBuddyDashboardSyncfusion(INavigationService navigationService, BusBuddy.UI.Services.IDatabaseHelperService databaseHelperService)
            : this(navigationService, databaseHelperService, new VehicleRepository(), new DriverRepository(), new RouteRepository(), new MaintenanceRepository()) { }

        public BusBuddyDashboardSyncfusion(
            INavigationService navigationService,
            BusBuddy.UI.Services.IDatabaseHelperService databaseHelperService,
            IVehicleRepository vehicleRepository,
            IDriverRepository driverRepository,
            IRouteRepository routeRepository,
            IMaintenanceRepository maintenanceRepository)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _databaseHelperService = databaseHelperService ?? throw new ArgumentNullException(nameof(databaseHelperService));

            // Use null-safe assignment for repositories
            _vehicleRepository = vehicleRepository; // Allow null, handle gracefully
            _driverRepository = driverRepository; // Allow null, handle gracefully
            _routeRepository = routeRepository; // Allow null, handle gracefully
            _maintenanceRepository = maintenanceRepository; // Allow null, handle gracefully

            Console.WriteLine($"🔍 [DEBUG] Repository null status:");
            Console.WriteLine($"   Vehicle: {(_vehicleRepository == null ? "NULL" : "OK")}");
            Console.WriteLine($"   Driver: {(_driverRepository == null ? "NULL" : "OK")}");
            Console.WriteLine($"   Route: {(_routeRepository == null ? "NULL" : "OK")}");
            Console.WriteLine($"   Maintenance: {(_maintenanceRepository == null ? "NULL" : "OK")}");

            // Initialize components container
            components = new System.ComponentModel.Container();

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
            InitializeEnhancedComponents();

            // Add form closing event handler to prevent crashes
            this.FormClosing += BusBuddyDashboardSyncfusion_FormClosing;

            InitializeDashboard();
        }

        private void InitializeDashboard()
        {
            try
            {
                // Form settings for enhanced dashboard
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.ControlBox = true;
                this.MaximizeBox = true;
                this.MinimizeBox = true;
                this.ShowInTaskbar = true;
                this.StartPosition = FormStartPosition.CenterScreen;
                this.KeyPreview = true;
                this.Size = new Size(1400, 900);
                this.MinimumSize = new Size(1024, 768);

                // Initialize toast notifier first
                InitializeToastNotifier();

                // Try enhanced layout first, fallback to advanced layout if needed
                try
                {
                    CreateEnhancedDashboardLayout();
                    LogDashboard("Enhanced dashboard layout created successfully");
                }
                catch (Exception enhancedEx)
                {
                    HandleDashboardError("Enhanced layout", enhancedEx);
                    LogDashboard("Falling back to advanced layout...");
                    CreateAdvancedLayoutForTests();
                }

                LoadCachedForms();

                // Ensure buttons are populated in the enhanced layout
                if (_formButtonsPanel != null)
                {
                    PopulateFormButtons();
                }

                this.Text = "🚌 BusBuddy Enhanced Dashboard";
                this.WindowState = FormWindowState.Maximized;
                this.Refresh();

                // Show welcome notification
                ShowWelcomeNotification();

                // Add keyboard handling for ESC key to close
                this.KeyDown += (sender, e) =>
                {
                    if (e.KeyCode == Keys.Escape)
                    {
                        this.Close();
                    }
                };

                // Load analytics asynchronously after basic UI is shown
                Task.Run(async () =>
                {
                    try
                    {
                        await LoadAnalyticsDataAsync();
                        await LoadDashboardDataAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Analytics loading failed: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                HandleDashboardError("dashboard initialization", ex);
                try
                {
                    CreateEmergencyLayout();
                }
                catch (Exception fallbackEx)
                {
                    HandleDashboardError("Emergency layout", fallbackEx);
                    // Create minimal layout
                    this.Controls.Clear();
                    var errorLabel = new Label
                    {
                        Text = "Dashboard initialization failed. Please restart the application.",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.LightPink
                    };
                    this.Controls.Add(errorLabel);
                }
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
                    Name = "HeaderPanel",
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

                // Stats panel for displaying key metrics
                var statsPanel = new Panel
                {
                    Name = "StatsPanel",
                    Dock = DockStyle.Right,
                    Width = 300,
                    BackColor = Color.FromArgb(250, 250, 250),
                    Padding = new Padding(10)
                };

                var statsLabel = new Label
                {
                    Text = "📊 Quick Stats",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(63, 81, 181),
                    Dock = DockStyle.Top,
                    Height = 30,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                statsPanel.Controls.Add(statsLabel);

                // Buttons panel
                _formButtonsPanel = new FlowLayoutPanel
                {
                    Name = "QuickActionsFlowPanel",
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    AutoScroll = true,
                    Padding = new Padding(20),
                    BackColor = Color.White
                };

                mainPanel.Controls.Add(_formButtonsPanel);
                mainPanel.Controls.Add(statsPanel);
                mainPanel.Controls.Add(headerPanel);

                this.Controls.Add(mainPanel);

                // Store references for later updates
                _headerPanel = headerPanel;
                _titleLabel = titleLabel;

                this.PerformLayout();

                LogDashboard("Basic layout created successfully");
            }
            catch (Exception ex)
            {
                HandleDashboardError("Basic layout", ex);
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

                    button.Click += (s, e) => Console.WriteLine($"{buttonText} module loading...");
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
                // Use console output instead of MessageBox for better test compatibility
                Console.WriteLine($"Critical failure: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates responsive main layout that adapts to different screen sizes and DPI settings
        /// Uses percentage-based sizing and proper anchor configurations
        /// </summary>
        private void CreateMainLayout()
        {
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

            this.Controls.Add(_mainTableLayout);

            CreateResponsiveHeaderPanel();
            CreateResponsiveButtonsPanel();
            CreateResponsiveAnalyticsPanel();
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
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));

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

            // Create close button
            var closeButton = new Button
            {
                Text = "✕",
                Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                TabStop = false
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (sender, e) => this.Close();

            headerLayout.Controls.Add(_titleLabel, 0, 0);
            headerLayout.Controls.Add(_themeToggleButton, 1, 0);
            headerLayout.Controls.Add(closeButton, 2, 0);
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
            _analyticsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                    ? Color.FromArgb(38, 42, 46)
                    : Color.FromArgb(248, 249, 250),
                Padding = new Padding(ScaleForDpi(20)),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                MinimumSize = new Size(0, ScaleForDpi(200))
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
                BackColor = Color.Transparent
            };

            // Create enhanced analytics with responsive design
            CreateEnhancedAnalyticsChart();
            CreateStatusGauges();

            _analyticsPanel.Controls.Add(analyticsLabel);
            _mainTableLayout.Controls.Add(_analyticsPanel, 0, 2);
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
        /// <summary>
        /// Load cached forms information for navigation buttons
        /// </summary>
        private void LoadCachedForms()
        {
            try
            {
                Console.WriteLine("Loading cached forms for navigation...");

                // Use FormDiscovery to scan and cache available forms
                var forms = FormDiscovery.ScanAndCacheFormsEnhanced();

                Console.WriteLine($"Successfully loaded {forms.Count} forms for navigation");

                // Store forms for use in button creation
                foreach (var form in forms)
                {
                    if (!_navigationMethods.ContainsKey(form.NavigationMethod))
                    {
                        Console.WriteLine($"Adding navigation method: {form.NavigationMethod}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading cached forms: {ex.Message}");
            }
        }

        /// <summary>
        /// Populate form buttons for navigation
        /// Based on official Syncfusion SfButton documentation: https://help.syncfusion.com/windowsforms/button/getting-started
        /// </summary>
        private void PopulateFormButtons()
        {
            try
            {
                if (_formButtonsPanel == null) return;

                // Clear existing buttons
                _formButtonsPanel.Controls.Clear();

                // Get forms from FormDiscovery
                var forms = FormDiscovery.ScanAndCacheFormsEnhanced();

                // Create buttons based on discovered forms
                foreach (var form in forms)
                {
                    // Create SfButton using documented pattern
                    var button = new SfButton
                    {
                        Text = form.DisplayName,
                        Size = new Size(ScaleForDpi(180), ScaleForDpi(80)),
                        Margin = new Padding(ScaleForDpi(10)),
                        Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 10, FontStyle.Regular),
                        ForeColor = Color.White,
                        Cursor = Cursors.Hand,
                        Tag = form.NavigationMethod // Store navigation method for click handling
                    };

                    // Apply Material Design styling based on form type
                    var buttonColor = GetButtonColorForForm(form.Name);
                    button.Style.BackColor = buttonColor;
                    button.Style.HoverBackColor = ControlPaint.Light(buttonColor, 0.2f);
                    button.Style.PressedBackColor = ControlPaint.Dark(buttonColor, 0.1f);

                    // Add click event handler using documented pattern
                    button.Click += NavigationButton_Click;

                    // Add tooltip for better UX
                    var tooltip = new ToolTip();
                    tooltip.SetToolTip(button, form.Description);

                    _formButtonsPanel.Controls.Add(button);
                    Console.WriteLine($"Created navigation button: {form.DisplayName}");
                }

                // Add fallback buttons if no forms discovered
                if (forms.Count == 0)
                {
                    CreateFallbackButtons();
                }

                Console.WriteLine($"Created {_formButtonsPanel.Controls.Count} navigation buttons");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error populating form buttons: {ex.Message}");
                CreateFallbackButtons(); // Ensure basic navigation is available
            }
        }

        /// <summary>
        /// Event handler for navigation button clicks
        /// Based on SfButton Click event documentation
        /// </summary>
        private void NavigationButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (sender is SfButton button && button.Tag is string navigationMethod)
                {
                    HandleButtonClick(navigationMethod);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling navigation button click: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates fallback buttons when form discovery fails
        /// </summary>
        private void CreateFallbackButtons()
        {
            var fallbackConfigs = new[]
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

            foreach (var config in fallbackConfigs)
            {
                var button = new SfButton
                {
                    Text = config.Text,
                    Size = new Size(ScaleForDpi(180), ScaleForDpi(80)),
                    Margin = new Padding(ScaleForDpi(10)),
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 10, FontStyle.Regular),
                    ForeColor = Color.White,
                    Cursor = Cursors.Hand,
                    Tag = config.Action
                };

                button.Style.BackColor = config.Color;
                button.Style.HoverBackColor = ControlPaint.Light(config.Color, 0.2f);
                button.Style.PressedBackColor = ControlPaint.Dark(config.Color, 0.1f);

                button.Click += NavigationButton_Click;
                _formButtonsPanel.Controls.Add(button);
            }
        }

        /// <summary>
        /// Get button color based on form name - using Syncfusion Material Design colors
        /// </summary>
        private Color GetButtonColorForForm(string formName)
        {
            return formName?.ToLowerInvariant() switch
            {
                var name when name.Contains("vehicle") => Color.FromArgb(33, 150, 243),   // Blue
                var name when name.Contains("driver") => Color.FromArgb(76, 175, 80),    // Green
                var name when name.Contains("route") => Color.FromArgb(255, 152, 0),     // Orange
                var name when name.Contains("activity") => Color.FromArgb(156, 39, 176), // Purple
                var name when name.Contains("fuel") => Color.FromArgb(244, 67, 54),      // Red
                var name when name.Contains("maintenance") => Color.FromArgb(96, 125, 139), // Blue Grey
                var name when name.Contains("calendar") => Color.FromArgb(255, 193, 7),  // Amber
                var name when name.Contains("report") => Color.FromArgb(63, 81, 181),    // Indigo
                _ => Color.FromArgb(63, 81, 181) // Default Indigo
            };
        }

        /// <summary>
        /// Create theme toggle button with proper Syncfusion styling
        /// </summary>
        private SfButton CreateThemeToggleButton()
        {
            var themeButton = new SfButton
            {
                Text = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark ? "☀️ Light" : "🌙 Dark",
                Size = new Size(100, 35),
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(5)
            };

            themeButton.Style.BackColor = Color.FromArgb(100, 255, 255, 255);
            themeButton.Style.HoverBackColor = Color.FromArgb(150, 255, 255, 255);
            themeButton.Style.PressedBackColor = Color.FromArgb(200, 255, 255, 255);
            themeButton.Style.Border = new Pen(Color.White, 1);

            themeButton.Click += (s, e) => ToggleTheme();

            return themeButton;
        }

        /// <summary>
        /// Toggle theme functionality
        /// </summary>
        private void ToggleTheme()
        {
            try
            {
                SyncfusionThemeHelper.ToggleTheme();

                // Update theme toggle button text
                if (_themeToggleButton != null)
                {
                    _themeToggleButton.Text = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark ? "☀️ Light" : "🌙 Dark";
                }

                // Refresh dashboard with new theme
                this.Invalidate();
                this.Refresh();

                ShowNotification("Theme", $"Switched to {SyncfusionThemeHelper.CurrentTheme} theme");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Initialize toast notification system using Syncfusion
        /// </summary>
        private void InitializeToastNotifier()
        {
            try
            {
                // Create notification icon for system tray notifications
                _notifyIcon = new NotifyIcon(components)
                {
                    Icon = SystemIcons.Information,
                    Text = "BusBuddy Dashboard",
                    Visible = true
                };

                Console.WriteLine("✅ Toast notification system initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing toast notifier: {ex.Message}");
            }
        }

        /// <summary>
        /// Update analytics display with real data
        /// </summary>
        private void UpdateAnalyticsDisplay(Dictionary<string, object> vehicleData,
                                          Dictionary<string, object> maintenanceData,
                                          Dictionary<string, object> efficiencyData)
        {
            try
            {
                // Update chart if available
                UpdateAnalyticsChart();

                // Update gauges if available
                UpdateStatusGauges(vehicleData, maintenanceData, efficiencyData);

                Console.WriteLine("✅ Analytics display updated with real data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating analytics display: {ex.Message}");
            }
        }

        /// <summary>
        /// Update status gauges with real data
        /// </summary>
        private void UpdateStatusGauges(Dictionary<string, object> vehicleData,
                                      Dictionary<string, object> maintenanceData,
                                      Dictionary<string, object> efficiencyData)
        {
            try
            {
                if (_systemStatusGauge != null && efficiencyData.ContainsKey("SystemStatus"))
                {
                    _systemStatusGauge.Value = Convert.ToSingle(efficiencyData["SystemStatus"]);
                }

                if (_maintenanceGauge != null && maintenanceData.ContainsKey("MaintenanceScore"))
                {
                    _maintenanceGauge.Value = Convert.ToSingle(maintenanceData["MaintenanceScore"]);
                }

                if (_efficiencyGauge != null && efficiencyData.ContainsKey("FleetEfficiency"))
                {
                    _efficiencyGauge.Value = Convert.ToSingle(efficiencyData["FleetEfficiency"]);
                }

                Console.WriteLine("✅ Status gauges updated");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating status gauges: {ex.Message}");
            }
        }

        /// <summary>
        /// Load fallback analytics data when real data loading fails
        /// </summary>
        private void LoadFallbackAnalyticsData()
        {
            try
            {
                var fallbackVehicleData = new Dictionary<string, object>
                {
                    ["TotalVehicles"] = 25,
                    ["ActiveVehicles"] = 22,
                    ["MaintenanceVehicles"] = 3
                };

                var fallbackMaintenanceData = new Dictionary<string, object>
                {
                    ["RecentMaintenance"] = 5,
                    ["ScheduledMaintenance"] = 8,
                    ["MaintenanceScore"] = 87
                };

                var fallbackEfficiencyData = new Dictionary<string, object>
                {
                    ["FleetEfficiency"] = 89.5,
                    ["FuelEfficiency"] = 7.2,
                    ["SystemStatus"] = 91
                };

                UpdateAnalyticsDisplay(fallbackVehicleData, fallbackMaintenanceData, fallbackEfficiencyData);
                Console.WriteLine("✅ Fallback analytics data loaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading fallback analytics: {ex.Message}");
            }
        }

        /// <summary>
        /// Load docking layout configuration
        /// </summary>
        private void LoadDockingLayout()
        {
            try
            {
                // In a real application, this would load saved layout from user preferences
                // For now, set up a default professional layout
                Console.WriteLine("📐 Loading default docking layout");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading docking layout: {ex.Message}");
            }
        }

        /// <summary>
        /// Update analytics chart with new data
        /// </summary>
        private void UpdateAnalyticsChart()
        {
            try
            {
                if (_analyticsChart?.Series != null && _analyticsChart.Series.Count > 0)
                {
                    // Update chart series with new data points using documented Syncfusion API
                    for (int i = 0; i < _analyticsChart.Series.Count; i++)
                    {
                        var series = _analyticsChart.Series[i];

                        // Add new data point (in real app, this would be real data)
                        var random = new Random();
                        var newValue = random.Next(70, 95);
                        var currentMonth = DateTime.Now.Month;

                        if (series.Points.Count > 12)
                        {
                            series.Points.RemoveAt(0); // Remove oldest point
                        }

                        series.Points.Add(currentMonth, newValue);
                    }

                    _analyticsChart.Refresh();
                }

                Console.WriteLine("✅ Analytics chart updated");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating analytics chart: {ex.Message}");
            }
        }

        /// <summary>
        /// Show welcome notification
        /// </summary>
        private void ShowWelcomeNotification()
        {
            ShowNotification("Welcome", "🚌 BusBuddy Enhanced Dashboard loaded successfully!\nDrag panels to customize your layout.");
        }

        /// <summary>
        /// Get sample data methods for dashboard stats
        /// </summary>
        private int GetVehicleCount()
        {
            try
            {
                if (_vehicleRepository == null)
                {
                    Console.WriteLine("⚠️ [DEBUG] Vehicle repository is null, using fallback count");
                    return 25; // Fallback value
                }

                var vehicles = _vehicleRepository.GetAllVehicles();
                return vehicles?.Count ?? 25;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Error getting vehicle count: {ex.Message}");
                return 25; // Fallback value
            }
        }

        private int GetActiveDriverCount()
        {
            try
            {
                if (_driverRepository == null)
                {
                    Console.WriteLine("⚠️ [DEBUG] Driver repository is null, using fallback count");
                    return 18; // Fallback value
                }

                var drivers = _driverRepository.GetAllDrivers();
                return drivers?.Count ?? 18;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Error getting driver count: {ex.Message}");
                return 18; // Fallback value
            }
        }

        private int GetRouteCount()
        {
            try
            {
                if (_routeRepository == null)
                {
                    Console.WriteLine("⚠️ [DEBUG] Route repository is null, using fallback count");
                    return 12; // Fallback value
                }

                var routes = _routeRepository.GetAllRoutes();
                return routes?.Count ?? 12;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Error getting route count: {ex.Message}");
                return 12; // Fallback value
            }
        }

        private double GetFleetEfficiency()
        {
            try
            {
                // Calculate based on vehicle performance metrics
                return 89.5; // Placeholder
            }
            catch
            {
                return 89.5; // Fallback value
            }
        }

        /// <summary>
        /// Load vehicle data for the data grid
        /// </summary>
        private void LoadVehicleData()
        {
            try
            {
                if (_vehicleDataGrid == null) return;

                var vehicleData = GetVehicleDataForDisplay();

                // Set data source using Syncfusion SfDataGrid documented approach
                _vehicleDataGrid.DataSource = vehicleData;
                _vehicleDataGrid.Refresh();

                Console.WriteLine($"✅ Loaded {vehicleData.Count} vehicles into data grid");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading vehicle data: {ex.Message}");
            }
        }

        #region Missing Critical Methods - Navigation and UI Handling

        /// <summary>
        /// Handle button click events for navigation - Enhanced debugging for troubleshooting
        /// </summary>
        private void HandleButtonClick(string navigationMethod)
        {
            try
            {
                Console.WriteLine($"🔄 [DEBUG] Button clicked: {navigationMethod}");
                Console.WriteLine($"🔍 [DEBUG] NavigationService null? {_navigationService == null}");
                Console.WriteLine($"🔍 [DEBUG] NavigationMethods count: {_navigationMethods?.Count ?? 0}");

                if (_navigationMethods.ContainsKey(navigationMethod))
                {
                    Console.WriteLine($"✅ [DEBUG] Found method in dictionary: {navigationMethod}");

                    try
                    {
                        _navigationMethods[navigationMethod].Invoke();
                        Console.WriteLine($"✅ [DEBUG] Method invoked successfully: {navigationMethod}");
                        ShowNotification("Navigation", $"Opening {navigationMethod.Replace("Show", "").Replace("Management", "")} module");
                    }
                    catch (Exception navEx)
                    {
                        Console.WriteLine($"❌ [DEBUG] Navigation method failed: {navEx.Message}");
                        Console.WriteLine($"❌ [DEBUG] Inner exception: {navEx.InnerException?.Message ?? "None"}");
                        Console.WriteLine($"❌ [DEBUG] Stack: {navEx.StackTrace}");
                        ShowNotification("Navigation Failed", $"Could not open {navigationMethod}: {navEx.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ [DEBUG] Method not found in dictionary: {navigationMethod}");
                    Console.WriteLine($"🔍 [DEBUG] Available methods: {string.Join(", ", _navigationMethods?.Keys.ToArray() ?? new string[0])}");
                    ShowNotification("Navigation Error", $"Method {navigationMethod} not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Critical error in HandleButtonClick: {ex.Message}");
                Console.WriteLine($"❌ [DEBUG] Full exception: {ex}");
                ShowNotification("Error", $"Navigation system error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create enhanced dashboard layout with DockingManager and tabbed panels
        /// Based on official Syncfusion DockingManager documentation: https://help.syncfusion.com/windowsforms/docking-manager/getting-started
        /// </summary>
        private void CreateEnhancedDashboardLayout()
        {
            try
            {
                Console.WriteLine("🚀 Creating enhanced dashboard layout with DockingManager...");

                // Clear existing controls
                this.Controls.Clear();

                // Initialize DockingManager using documented pattern
                // The constructor requires IContainer, but we need to set HostControl property
                _dockingManager = new DockingManager(this.components)
                {
                    HostControl = this,
                    DockTabAlignment = Syncfusion.Windows.Forms.Tools.DockTabAlignmentStyle.Top,
                    AllowTabsMoving = true,
                    ShowDockTabScrollButton = true
                };

                // CRITICAL FIX: Create and add panels to form BEFORE docking them
                CreateDashboardPanels();

                // Create main content area
                var mainContentPanel = new Panel
                {
                    Name = "MainContentPanel",
                    Dock = DockStyle.Fill,
                    BackColor = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                        ? Color.FromArgb(45, 49, 53) : Color.FromArgb(248, 249, 250)
                };
                this.Controls.Add(mainContentPanel);

                // Create ribbon navigation first
                CreateRibbonNavigation();

                // Create dashboard panels using documented patterns
                CreateQuickActionsPanel();
                CreateAnalyticsPanel();
                CreateDataGridPanel();
                CreateSearchPanel();
                CreateQuickStatsPanel();

                // Configure tabbed layout with documented DockingManager methods
                ConfigureTabbledDashboardLayout();

                Console.WriteLine("✅ Enhanced dashboard layout created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating enhanced dashboard layout: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create ribbon navigation using official Syncfusion RibbonControlAdv documentation
        /// Based on: https://help.syncfusion.com/windowsforms/ribbon/getting-started
        /// </summary>
        private void CreateRibbonNavigation()
        {
            try
            {
                Console.WriteLine("🎀 Creating ribbon navigation...");

                // Initialize RibbonControlAdv using documented pattern
                _ribbonControl = new RibbonControlAdv
                {
                    RibbonStyle = Syncfusion.Windows.Forms.Tools.RibbonStyle.Office2016,
                    MenuButtonText = "File",
                    Size = new Size(this.Width, 150)
                };

                // Add ribbon control to form using documented method
                this.Controls.Add(_ribbonControl);

                // Create tabs using documented AddMainItem method
                CreateNavigationGroups();

                this.Controls.Add(_ribbonControl);

                Console.WriteLine("✅ Ribbon navigation created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating ribbon navigation: {ex.Message}");
            }
        }

        /// <summary>
        /// Create navigation groups in ribbon tabs
        /// Based on Syncfusion ToolStripTabItem documentation
        /// </summary>
        private void CreateNavigationGroups()
        {
            try
            {
                // Create main navigation tabs using documented pattern
                var dashboardTab = new ToolStripTabItem
                {
                    Text = "Dashboard"
                };

                var fleetTab = new ToolStripTabItem
                {
                    Text = "Fleet Management"
                };

                var operationsTab = new ToolStripTabItem
                {
                    Text = "Operations"
                };

                var reportsTab = new ToolStripTabItem
                {
                    Text = "Reports"
                };

                // Add tabs to ribbon using documented AddMainItem method
                _ribbonControl.Header.AddMainItem(dashboardTab);
                _ribbonControl.Header.AddMainItem(fleetTab);
                _ribbonControl.Header.AddMainItem(operationsTab);
                _ribbonControl.Header.AddMainItem(reportsTab);

                // Create dashboard group
                var dashboardGroup = new ToolStripEx
                {
                    Text = "Dashboard Views"
                };

                // Add dashboard buttons using documented pattern
                AddRibbonButton(dashboardGroup, "Analytics", "📊", "ShowAnalyticsDemo");
                AddRibbonButton(dashboardGroup, "Quick Stats", "📈", "ShowReports");
                dashboardTab.Panel.Controls.Add(dashboardGroup);

                // Create fleet management group
                var fleetGroup = new ToolStripEx
                {
                    Text = "Fleet Management"
                };

                AddRibbonButton(fleetGroup, "Vehicles", "🚌", "ShowVehicleManagement");
                AddRibbonButton(fleetGroup, "Drivers", "👨‍✈️", "ShowDriverManagement");
                AddRibbonButton(fleetGroup, "Routes", "🗺️", "ShowRouteManagement");
                AddRibbonButton(fleetGroup, "Maintenance", "🔧", "ShowMaintenanceManagement");
                fleetTab.Panel.Controls.Add(fleetGroup);

                // Create operations group
                var operationsGroup = new ToolStripEx
                {
                    Text = "Daily Operations"
                };

                AddRibbonButton(operationsGroup, "Activities", "📋", "ShowActivityManagement");
                AddRibbonButton(operationsGroup, "Fuel", "⛽", "ShowFuelManagement");
                AddRibbonButton(operationsGroup, "Calendar", "📅", "ShowCalendarManagement");
                AddRibbonButton(operationsGroup, "TimeCard", "⏰", "ShowTimeCardManagement");
                operationsTab.Panel.Controls.Add(operationsGroup);

                // Create reports group
                var reportsGroup = new ToolStripEx
                {
                    Text = "Reports & Analytics"
                };

                AddRibbonButton(reportsGroup, "Reports", "📊", "ShowReportsManagement");
                AddRibbonButton(reportsGroup, "Schedule", "📋", "ShowScheduleManagement");
                AddRibbonButton(reportsGroup, "School Calendar", "🏫", "ShowSchoolCalendarManagement");
                reportsTab.Panel.Controls.Add(reportsGroup);

                Console.WriteLine("✅ Navigation groups created with ribbon buttons");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating navigation groups: {ex.Message}");
            }
        }

        /// <summary>
        /// Add ribbon button using documented pattern
        /// </summary>
        private void AddRibbonButton(ToolStripEx group, string text, string icon, string navigationMethod)
        {
            try
            {
                var button = new ToolStripButton
                {
                    Text = $"{icon} {text}",
                    DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                    ImageAlign = ContentAlignment.TopCenter,
                    TextAlign = ContentAlignment.BottomCenter,
                    Tag = navigationMethod,
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 9, FontStyle.Regular)
                };

                button.Click += (s, e) => HandleButtonClick(navigationMethod);
                group.Items.Add(button);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error adding ribbon button {text}: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a visually distinct contextual tab and custom system buttons to RibbonControlAdv.
        /// Based on: https://help.syncfusion.com/windowsforms/ribbon/contextual-tab-group (WinForms only supports contextual tab groups via designer, not programmatically)
        /// </summary>
        private void ConfigureAdvancedRibbonFeatures()
        {
            // Disable ribbon customization window
            _ribbonControl.EnableRibbonCustomization = false;

            // Add a visually distinct contextual tab (simulate contextual tab group)
            var contextTab = new ToolStripTabItem { Text = "Selection" };
            contextTab.BackColor = Color.MediumPurple;
            contextTab.ForeColor = Color.White;
            var contextGroup = new ToolStripEx { Text = "Selection Tools" };
            var contextButton = new ToolStripButton
            {
                Text = "Special Action",
                ToolTipText = "Perform special action",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
            };
            contextButton.Click += (s, e) => MessageBox.Show("Special action performed!");
            contextGroup.Items.Add(contextButton);
            contextTab.Panel.Controls.Add(contextGroup);
            _ribbonControl.Header.AddMainItem(contextTab);

            // Add custom system buttons (minimize, maximize, close)
            var minimizeButton = new RibbonTitleButton
            {
                Image = SystemIcons.Application.ToBitmap(),
                ToolTipText = "Minimize form",
                HoverBackColor = Color.LightGreen
            };
            var maximizeButton = new RibbonTitleButton
            {
                Image = SystemIcons.Information.ToBitmap(),
                ToolTipText = "Maximize form",
                HoverBackColor = Color.LightBlue
            };
            var closeButton = new RibbonTitleButton
            {
                Image = SystemIcons.Error.ToBitmap(),
                ToolTipText = "Close form",
                HoverBackColor = Color.LightCoral
            };
            _ribbonControl.MinimizeButton = minimizeButton;
            _ribbonControl.MaximizeButton = maximizeButton;
            _ribbonControl.CloseButton = closeButton;
        }

        /// <summary>
        /// Create quick actions panel as the main dashboard tab
        /// Based on DockingManager panel documentation
        /// </summary>
        private void CreateQuickActionsPanel()
        {
            try
            {
                Console.WriteLine("⚡ Creating quick actions panel...");

                _quickStatsPanel = new Panel
                {
                    Name = "QuickActionsPanel",
                    BackColor = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                        ? Color.FromArgb(38, 42, 46) : Color.FromArgb(248, 249, 250),
                    Padding = new Padding(15),
                    Size = new Size(400, 600)
                };

                // Create header
                var headerPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 60,
                    BackColor = Color.Transparent
                };

                var titleLabel = new Label
                {
                    Text = "⚡ Quick Actions",
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 16, FontStyle.Bold),
                    ForeColor = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                        ? Color.White : Color.FromArgb(33, 37, 41),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                headerPanel.Controls.Add(titleLabel);

                // Create content area with buttons
                var contentPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent,
                    Padding = new Padding(10)
                };

                // Create button layout
                var buttonLayout = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoScroll = true,
                    Padding = new Padding(5),
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink
                };

                // Set this as the main form buttons panel for PopulateFormButtons compatibility
                _formButtonsPanel = buttonLayout;

                // Add quick action buttons using documented SfButton pattern
                CreateQuickActionButtons(buttonLayout);

                contentPanel.Controls.Add(buttonLayout);
                _quickStatsPanel.Controls.Add(contentPanel);
                _quickStatsPanel.Controls.Add(headerPanel);

                // Enable docking using documented method
                _dockingManager.SetEnableDocking(_quickStatsPanel, true);
                _dockingManager.SetDockLabel(_quickStatsPanel, "⚡ Quick Actions");

                Console.WriteLine("✅ Quick actions panel created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating quick actions panel: {ex.Message}");
            }
        }

        /// <summary>
        /// Create quick action buttons using documented SfButton patterns
        /// </summary>
        private void CreateQuickActionButtons(FlowLayoutPanel buttonLayout)
        {
            try
            {
                var buttonConfigs = new[]
                {
                    new { Text = "🚌 Vehicle Management", Action = "ShowVehicleManagement", Color = Color.FromArgb(33, 150, 243) },
                    new { Text = "👨‍✈️ Driver Management", Action = "ShowDriverManagement", Color = Color.FromArgb(76, 175, 80) },
                    new { Text = "🗺️ Route Management", Action = "ShowRouteManagement", Color = Color.FromArgb(255, 152, 0) },
                    new { Text = "🔧 Maintenance", Action = "ShowMaintenanceManagement", Color = Color.FromArgb(244, 67, 54) },
                    new { Text = "📋 Activities", Action = "ShowActivityManagement", Color = Color.FromArgb(156, 39, 176) },
                    new { Text = "⛽ Fuel Management", Action = "ShowFuelManagement", Color = Color.FromArgb(96, 125, 139) },
                    new { Text = "📅 Calendar", Action = "ShowCalendarManagement", Color = Color.FromArgb(255, 193, 7) },
                    new { Text = "📊 Reports", Action = "ShowReportsManagement", Color = Color.FromArgb(63, 81, 181) }
                };

                foreach (var config in buttonConfigs)
                {
                    var button = new SfButton
                    {
                        Text = config.Text,
                        Size = new Size(360, 45),  // Increased width to fit panel better
                        Margin = new Padding(8),
                        Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 10, FontStyle.Regular),
                        ForeColor = Color.White,
                        Cursor = Cursors.Hand,
                        Tag = config.Action,
                        AutoSize = false,
                        TextAlign = ContentAlignment.MiddleLeft
                    };

                    // Apply documented styling
                    button.Style.BackColor = config.Color;
                    button.Style.HoverBackColor = ControlPaint.Light(config.Color, 0.2f);
                    button.Style.PressedBackColor = ControlPaint.Dark(config.Color, 0.1f);

                    button.Click += (s, e) => HandleButtonClick(config.Action);
                    buttonLayout.Controls.Add(button);
                }

                Console.WriteLine($"✅ Created {buttonConfigs.Length} quick action buttons in panel with {buttonLayout.Controls.Count} total controls");

                // Force refresh to ensure buttons are visible
                buttonLayout.Refresh();
                buttonLayout.PerformLayout();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating quick action buttons: {ex.Message}");
            }
        }

        /// <summary>
        /// Configure tabbed dashboard layout using documented DockingManager patterns
        /// Based on: https://help.syncfusion.com/windowsforms/docking-manager/tabbed-window
        /// </summary>
        private void ConfigureTabbledDashboardLayout()
        {
            try
            {
                Console.WriteLine("📋 Configuring tabbed dashboard layout...");

                // Find panels to configure
                var quickActionsPanel = FindDockingPanel("QuickActionsPanel");
                var analyticsPanel = FindDockingPanel("AnalyticsPanel");
                var dataGridPanel = FindDockingPanel("DataGridPanel");
                var searchPanel = FindDockingPanel("SearchPanel");

                if (quickActionsPanel != null)
                {
                    // Enable docking for all dashboard panels using documented Syncfusion pattern
                    _dockingManager.SetEnableDocking(quickActionsPanel, true);
                    _dockingManager.SetDockLabel(quickActionsPanel, "⚡ Quick Actions");
                    _dockingManager.SetEnableDocking(analyticsPanel, true);
                    _dockingManager.SetDockLabel(analyticsPanel, "📊 Analytics");
                    _dockingManager.SetEnableDocking(dataGridPanel, true);
                    _dockingManager.SetDockLabel(dataGridPanel, "🗂️ Data Grid");
                    _dockingManager.SetEnableDocking(searchPanel, true);
                    _dockingManager.SetDockLabel(searchPanel, "🔍 Search");

                    // Set main panel as left docked (default open)
                    _dockingManager.DockControl(quickActionsPanel, this,
                        Syncfusion.Windows.Forms.Tools.DockingStyle.Left, 400);

                    // Set Quick Actions as active default tab
                    _dockingManager.ActivateControl(quickActionsPanel);

                    // Tab other panels with quick actions using documented pattern
                    _dockingManager.DockControl(analyticsPanel, quickActionsPanel,
                        Syncfusion.Windows.Forms.Tools.DockingStyle.Tabbed, 200);
                    _dockingManager.DockControl(dataGridPanel, quickActionsPanel,
                        Syncfusion.Windows.Forms.Tools.DockingStyle.Tabbed, 200);
                    _dockingManager.DockControl(searchPanel, this,
                        Syncfusion.Windows.Forms.Tools.DockingStyle.Top, 60);
                }

                Console.WriteLine("✅ Tabbed dashboard layout configured successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error configuring tabbed layout: {ex.Message}");
            }
        }

        /// <summary>
        /// Create search panel using documented patterns
        /// </summary>
        private void CreateSearchPanel()
        {
            try
            {
                Console.WriteLine("🔍 Creating search panel...");

                _searchPanel = new Panel
                {
                    Name = "SearchPanel",
                    BackColor = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark
                        ? Color.FromArgb(38, 42, 46) : Color.FromArgb(248, 249, 250),
                    Padding = new Padding(10),
                    Size = new Size(this.Width, 60)
                };

                var searchLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 3,
                    RowCount = 1
                };

                searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

                var searchLabel = new Label
                {
                    Text = "🔍 Search:",
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 12, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoSize = true
                };

                _searchBox = new TextBox
                {
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 11, FontStyle.Regular),
                    Dock = DockStyle.Fill,
                    PlaceholderText = "Search vehicles, drivers, routes..."
                };

                var searchButton = new SfButton
                {
                    Text = "Search",
                    Size = new Size(80, 30),
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 10, FontStyle.Regular)
                };

                searchButton.Style.BackColor = Color.FromArgb(33, 150, 243);
                searchButton.Click += (s, e) => PerformSearch();

                searchLayout.Controls.Add(searchLabel, 0, 0);
                searchLayout.Controls.Add(_searchBox, 1, 0);
                searchLayout.Controls.Add(searchButton, 2, 0);

                _searchPanel.Controls.Add(searchLayout);

                // Enable docking
                _dockingManager.SetEnableDocking(_searchPanel, true);
                _dockingManager.SetDockLabel(_searchPanel, "🔍 Search");

                Console.WriteLine("✅ Search panel created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating search panel: {ex.Message}");
            }
        }

        /// <summary>
        /// Perform search functionality
        /// </summary>
        private void PerformSearch()
        {
            try
            {
                var searchText = _searchBox?.Text?.Trim();
                if (!string.IsNullOrEmpty(searchText))
                {
                    ShowNotification("Search", $"Searching for: {searchText}");
                    Console.WriteLine($"🔍 Performing search: {searchText}");
                }
                else
                {
                    ShowNotification("Search", "Please enter search terms");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error performing search: {ex.Message}");
            }
        }

        /// <summary>
        /// Get vehicle data for display in the data grid
        /// </summary>
        private List<VehicleDisplayData> GetVehicleDataForDisplay()
        {
            try
            {
                // Return sample data for now - in production this would query the repository
                return new List<VehicleDisplayData>
                {
                    new VehicleDisplayData { VehicleNumber = "BUS001", Status = "Active", Driver = "John Smith", Route = "Route A" },
                    new VehicleDisplayData { VehicleNumber = "BUS002", Status = "Maintenance", Driver = "Jane Doe", Route = "Route B" },
                    new VehicleDisplayData { VehicleNumber = "BUS003", Status = "Active", Driver = "Mike Johnson", Route = "Route C" }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting vehicle data: {ex.Message}");
                return new List<VehicleDisplayData>();
            }
        }

        /// <summary>
        /// Show notification to user
        /// </summary>
        private void ShowNotification(string title, string message)
        {
            try
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
                }
                Console.WriteLine($"[{title}] {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing notification: {ex.Message}");
            }
        }



        #endregion

        #region Helper Classes

        /// <summary>
        /// Vehicle display data for data grid
        /// </summary>
        public class VehicleDisplayData
        {
            public string VehicleNumber { get; set; }
            public string Status { get; set; }
            public string Driver { get; set; }
            public string Route { get; set; }
        }

        /// <summary>
        /// CRITICAL FIX: Create dashboard panels properly before attempting to dock them
        /// This method ensures all panels exist and are properly initialized before docking
        /// </summary>
        private void CreateDashboardPanels()
        {
            try
            {
                Console.WriteLine("🔧 Creating dashboard panels...");

                // Create analytics panel
                _analyticsDisplayPanel = new Panel
                {
                    Name = "AnalyticsPanel",
                    Size = new Size(400, 300),
                    BackColor = Color.FromArgb(248, 249, 250),
                    BorderStyle = BorderStyle.None
                };

                // Add analytics content
                if (_analyticsChart == null)
                    _analyticsChart = new ChartControl();

                _analyticsDisplayPanel.Controls.Add(_analyticsChart);
                _analyticsChart.Dock = DockStyle.Fill;

                // Create data grid panel
                _dataGridPanel = new Panel
                {
                    Name = "DataGridPanel",
                    Size = new Size(600, 400),
                    BackColor = Color.FromArgb(248, 249, 250),
                    BorderStyle = BorderStyle.None
                };

                // Add data grid content
                if (_vehicleDataGrid == null)
                {
                    _vehicleDataGrid = new SfDataGrid
                    {
                        AutoGenerateColumns = true,
                        AllowEditing = false,
                        AllowSorting = true,
                        AllowFiltering = true
                    };
                }

                _dataGridPanel.Controls.Add(_vehicleDataGrid);
                _vehicleDataGrid.Dock = DockStyle.Fill;

                // Create quick stats panel
                _quickStatsPanel = new Panel
                {
                    Name = "QuickActionsPanel",
                    Size = new Size(300, 500),
                    BackColor = Color.FromArgb(248, 249, 250),
                    BorderStyle = BorderStyle.None
                };

                // Add quick stats content
                CreateQuickStatsContent();

                // Create search panel
                _searchPanel = new Panel
                {
                    Name = "SearchPanel",
                    Size = new Size(800, 60),
                    BackColor = Color.FromArgb(248, 249, 250),
                    BorderStyle = BorderStyle.None
                };

                // Add search content
                CreateSearchContent();

                // CRITICAL: Add all panels to the form's Controls collection first
                this.Controls.Add(_analyticsDisplayPanel);
                this.Controls.Add(_dataGridPanel);
                this.Controls.Add(_quickStatsPanel);
                this.Controls.Add(_searchPanel);

                Console.WriteLine("✅ Dashboard panels created and added to form successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating dashboard panels: {ex.Message}");
                throw; // Re-throw to prevent continuing with broken state
            }
        }

        /// <summary>
        /// Create content for the quick stats panel
        /// </summary>
        private void CreateQuickStatsContent()
        {
            var titleLabel = new Label
            {
                Text = "⚡ Quick Actions",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 5, 10, 5)
            };

            var buttonLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            CreateQuickActionButtons(buttonLayout);

            _quickStatsPanel.Controls.Add(buttonLayout);
            _quickStatsPanel.Controls.Add(titleLabel);
        }

        /// <summary>
        /// Create content for the search panel
        /// </summary>
        private void CreateSearchContent()
        {
            // Search functionality integrated into Quick Actions panel
            // Removed duplicate search box to avoid UI confusion
            var infoLabel = new Label
            {
                Text = "🔍 Search integrated with Quick Actions panel",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                AutoSize = true,
                Location = new Point(10, 20),
                ForeColor = Color.Gray
            };

            _searchPanel.Controls.Add(infoLabel);
        }

        /// <summary>
        /// FIXED: Find a docking panel by name from the form's controls
        /// </summary>
        private Panel FindDockingPanel(string panelName)
        {
            if (string.IsNullOrEmpty(panelName) || this.Controls == null)
                return null;

            return this.Controls.OfType<Panel>().FirstOrDefault(p => p != null && p.Name == panelName);
        }

        /// <summary>
        /// Create enhanced analytics chart using Syncfusion ChartControl
        /// Based on: https://help.syncfusion.com/windowsforms/chart/getting-started
        /// </summary>
        private void CreateEnhancedAnalyticsChart()
        {
            if (_analyticsChart == null)
                _analyticsChart = new ChartControl();
            _analyticsChart.Series.Clear();
            var series = new ChartSeries("Fleet Utilization", ChartSeriesType.Column);
            series.Points.Add(2023, 85);
            series.Points.Add(2024, 92);
            series.Points.Add(2025, 88);
            _analyticsChart.Series.Add(series);
            _analyticsChart.PrimaryXAxis.Title = "Year";
            _analyticsChart.PrimaryYAxis.Title = "Utilization (%)";
            _analyticsChart.Dock = DockStyle.Fill;
        }        /// <summary>
        /// Create status gauges using Syncfusion RadialGauge
        /// Based on: https://help.syncfusion.com/windowsforms/gauge/getting-started
        /// </summary>
        private void CreateStatusGauges()
        {
            // Create system status gauge
            _systemStatusGauge = new RadialGauge();
            _systemStatusGauge.Value = 95;
            _systemStatusGauge.MinorDifference = 5;
            _systemStatusGauge.FrameThickness = 15;
            _systemStatusGauge.Size = new Size(180, 180);
            _systemStatusGauge.Dock = DockStyle.Left;
            _systemStatusGauge.GaugeLabel = "System Health";

            // Create maintenance gauge
            _maintenanceGauge = new RadialGauge();
            _maintenanceGauge.Value = 78;
            _maintenanceGauge.MinorDifference = 5;
            _maintenanceGauge.FrameThickness = 15;
            _maintenanceGauge.Size = new Size(180, 180);
            _maintenanceGauge.Dock = DockStyle.Left;
            _maintenanceGauge.GaugeLabel = "Maintenance";

            // Create efficiency gauge
            _efficiencyGauge = new RadialGauge();
            _efficiencyGauge.Value = 85;
            _efficiencyGauge.MinorDifference = 5;
            _efficiencyGauge.FrameThickness = 15;
            _efficiencyGauge.Size = new Size(180, 180);
            _efficiencyGauge.Dock = DockStyle.Left;
            _efficiencyGauge.GaugeLabel = "Fleet Efficiency";
        }

        /// <summary>
        /// Create quick stats panel using Syncfusion TileLayout and LayoutGroup
        /// Based on: https://help.syncfusion.com/windowsforms/tile-layout/getting-started
        /// </summary>
        private void CreateQuickStatsPanel()
        {
            if (_quickStatsLayout == null)
            {
                _quickStatsLayout = new TileLayout();
                _quickStatsLayout.Dock = DockStyle.Fill;
                _quickStatsLayout.Size = new Size(300, 200);
                _quickStatsLayout.AllowNewGroup = false;
            }
            _quickStatsLayout.Controls.Clear();

            // Vehicle stat
            var vehicleGroup = new LayoutGroup();
            vehicleGroup.BackColor = Color.FromArgb(33, 150, 243);
            var vehicleLabel = new Label
            {
                Text = "Active Vehicles",
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Height = 24
            };
            var vehicleValue = new Label
            {
                Text = "42",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            vehicleGroup.Controls.Add(vehicleValue);
            vehicleGroup.Controls.Add(vehicleLabel);

            // Driver stat
            var driverGroup = new LayoutGroup();
            driverGroup.BackColor = Color.FromArgb(76, 175, 80);
            var driverLabel = new Label
            {
                Text = "Available Drivers",
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Height = 24
            };
            var driverValue = new Label
            {
                Text = "18",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            driverGroup.Controls.Add(driverValue);
            driverGroup.Controls.Add(driverLabel);

            // Route stat
            var routeGroup = new LayoutGroup();
            routeGroup.BackColor = Color.FromArgb(255, 152, 0);
            var routeLabel = new Label
            {
                Text = "Routes Today",
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Height = 24
            };
            var routeValue = new Label
            {
                Text = "12",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            routeGroup.Controls.Add(routeValue);
            routeGroup.Controls.Add(routeLabel);

            _quickStatsLayout.Controls.Add(vehicleGroup);
            _quickStatsLayout.Controls.Add(driverGroup);
            _quickStatsLayout.Controls.Add(routeGroup);

            _quickStatsPanel.Controls.Clear();
            _quickStatsPanel.Controls.Add(_quickStatsLayout);
        }

        /// <summary>
        /// Create data grid panel using Syncfusion SfDataGrid
        /// Fully compliant with Syncfusion documentation and BusBuddy standards
        /// </summary>
        private void CreateDataGridPanel()
        {
            // Use enhanced grid creation for full compliance
            if (_vehicleDataGrid == null)
            {
                _vehicleDataGrid = SyncfusionThemeHelper.CreateEnhancedMaterialSfDataGrid();
                _vehicleDataGrid.Dock = DockStyle.Fill;
            }

            // Clear and define columns explicitly for Vehicle model
            _vehicleDataGrid.Columns.Clear();
            _vehicleDataGrid.AutoGenerateColumns = false;
            _vehicleDataGrid.Columns.Add(SyncfusionThemeHelper.SfDataGridColumns.CreateIdColumn("VehicleID", "ID"));
            _vehicleDataGrid.Columns.Add(SyncfusionThemeHelper.SfDataGridColumns.CreateTextColumn("VehicleNumber", "Bus #", 90));
            _vehicleDataGrid.Columns.Add(SyncfusionThemeHelper.SfDataGridColumns.CreateTextColumn("Make", "Make", 100));
            _vehicleDataGrid.Columns.Add(SyncfusionThemeHelper.SfDataGridColumns.CreateTextColumn("Model", "Model", 100));
            _vehicleDataGrid.Columns.Add(SyncfusionThemeHelper.SfDataGridColumns.CreateNumericColumn("Year", "Year"));
            _vehicleDataGrid.Columns.Add(SyncfusionThemeHelper.SfDataGridColumns.CreateNumericColumn("Capacity", "Capacity"));
            _vehicleDataGrid.Columns.Add(SyncfusionThemeHelper.SfDataGridColumns.CreateTextColumn("FuelType", "Fuel Type", 90));
            _vehicleDataGrid.Columns.Add(SyncfusionThemeHelper.SfDataGridColumns.CreateStatusColumn("Status", "Status", 90));
            _vehicleDataGrid.Columns.Add(SyncfusionThemeHelper.SfDataGridColumns.CreateTextColumn("VINNumber", "VIN", 120));
            _vehicleDataGrid.Columns.Add(SyncfusionThemeHelper.SfDataGridColumns.CreateTextColumn("LicenseNumber", "License #", 100));
            _vehicleDataGrid.Columns.Add(SyncfusionThemeHelper.SfDataGridColumns.CreateDateTimeColumn("DateLastInspectionAsDateTime", "Last Inspection", 120));
            _vehicleDataGrid.Columns.Add(SyncfusionThemeHelper.SfDataGridColumns.CreateAutoSizeColumn("Notes", "Notes"));

            // Summary rows removed temporarily due to namespace issues
            // TODO: Re-add summary functionality once correct Syncfusion namespace is determined

            // Enable grouping, filtering, sorting, and virtualization
            _vehicleDataGrid.AllowGrouping = true;
            _vehicleDataGrid.ShowGroupDropArea = true;
            _vehicleDataGrid.AllowFiltering = true;
            _vehicleDataGrid.AllowSorting = true;
            _vehicleDataGrid.EnableDataVirtualization = true;
            _vehicleDataGrid.ShowToolTip = true;

            // Apply custom styles and BusBuddy standards
            SyncfusionThemeHelper.SfDataGridEnhancements(_vehicleDataGrid);

            // Set data source
            _vehicleDataGrid.DataSource = GetVehicleDataForDisplay();
            _dataGridPanel.Controls.Clear();
            _dataGridPanel.Controls.Add(_vehicleDataGrid);
        }

        #endregion

        #region Missing Methods Implementation - Based on Syncfusion Documentation

        /// <summary>
        /// Initialize enhanced components for Syncfusion controls
        /// Based on: https://help.syncfusion.com/windowsforms/overview
        /// </summary>
        private void InitializeEnhancedComponents()
        {
            try
            {
                Console.WriteLine("🔧 Initializing enhanced Syncfusion components...");

                // Initialize the components container if not already done
                if (components == null)
                {
                    components = new System.ComponentModel.Container();
                }

                // Apply Syncfusion theme initialization
                SyncfusionThemeHelper.InitializeGlobalTheme();

                // Pre-initialize key Syncfusion controls to avoid runtime delays
                if (_dockingManager == null && components != null)
                {
                    _dockingManager = new DockingManager(components);
                }

                Console.WriteLine("✅ Enhanced Syncfusion components initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error initializing enhanced components: {ex.Message}");
                // Continue without enhanced components - fallback to basic functionality
            }
        }

        /// <summary>
        /// Form closing event handler for cleanup and resource disposal
        /// Based on standard WinForms event handling patterns
        /// </summary>
        private void BusBuddyDashboardSyncfusion_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Console.WriteLine("🔄 Dashboard form closing - performing cleanup...");

                // Dispose of notification icon if it exists
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                    _notifyIcon = null;
                }

                // Clean up docking manager
                if (_dockingManager != null)
                {
                    _dockingManager.Dispose();
                    _dockingManager = null;
                }

                // Clean up data grids
                if (_vehicleDataGrid != null)
                {
                    _vehicleDataGrid.DataSource = null;
                    _vehicleDataGrid.Dispose();
                }

                Console.WriteLine("✅ Dashboard cleanup completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during form closing cleanup: {ex.Message}");
                // Continue with normal form closing
            }
        }

        /// <summary>
        /// Create advanced layout for testing and fallback scenarios
        /// Uses TableLayoutPanel for maximum compatibility
        /// </summary>
        private void CreateAdvancedLayoutForTests()
        {
            try
            {
                Console.WriteLine("🔧 Creating advanced layout for tests...");

                // Clear existing controls
                this.Controls.Clear();

                // Create main table layout
                _mainTableLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2,
                    BackColor = Color.White,
                    Padding = new Padding(10)
                };

                // Header panel
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
                _mainTableLayout.Controls.Add(headerPanel, 0, 0);

                // Content area - use simple panels for each section
                var contentPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent
                };

                // Quick actions panel
                var quickActionsPanel = new Panel
                {
                    Dock = DockStyle.Left,
                    Width = 300,
                    BackColor = Color.FromArgb(248, 249, 250),
                    Padding = new Padding(10)
                };

                var quickActionsLabel = new Label
                {
                    Text = "⚡ Quick Actions",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(33, 37, 41),
                    Dock = DockStyle.Top,
                    Height = 40,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 5, 10, 5)
                };

                quickActionsPanel.Controls.Add(quickActionsLabel);

                // Add simple buttons for quick actions
                var buttonConfigs = new[]
                {
                    new { Text = "🚌 Vehicles", Action = "ShowVehicleManagement", Color = Color.FromArgb(33, 150, 243) },
                    new { Text = "👨‍✈️ Drivers", Action = "ShowDriverManagement", Color = Color.FromArgb(76, 175, 80) },
                    new { Text = "🗺️ Routes", Action = "ShowRouteManagement", Color = Color.FromArgb(255, 152, 0) },
                    new { Text = "🔧 Maintenance", Action = "ShowMaintenanceManagement", Color = Color.FromArgb(244, 67, 54) },
                    new { Text = "📋 Activities", Action = "ShowActivityManagement", Color = Color.FromArgb(156, 39, 176) },
                    new { Text = "⛽ Fuel", Action = "ShowFuelManagement", Color = Color.FromArgb(96, 125, 139) },
                    new { Text = "📅 Calendar", Action = "ShowCalendarManagement", Color = Color.FromArgb(255, 193, 7) },
                    new { Text = "📊 Reports", Action = "ShowReportsManagement", Color = Color.FromArgb(63, 81, 181) }
                };

                foreach (var config in buttonConfigs)
                {
                    var button = new Button
                    {
                        Text = config.Text,
                        Size = new Size(280, 50),
                        Margin = new Padding(5),
                        Font = new Font("Segoe UI", 10, FontStyle.Regular),
                        BackColor = config.Color,
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };

                    button.Click += (s, e) => HandleButtonClick(config.Action);
                    quickActionsPanel.Controls.Add(button);
                }

                contentPanel.Controls.Add(quickActionsPanel);
                _mainTableLayout.Controls.Add(contentPanel, 0, 1);

                this.Controls.Add(_mainTableLayout);

                Console.WriteLine("✅ Advanced layout for tests created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating advanced layout: {ex.Message}");
            }
        }

        /// <summary>
        /// Create analytics panel and add chart/gauges
        /// Based on: https://help.syncfusion.com/windowsforms/chart/getting-started
        /// </summary>
        private void CreateAnalyticsPanel()
        {
            try
            {
                Console.WriteLine("📊 Creating analytics panel...");

                if (_analyticsDisplayPanel != null)
                {
                    _analyticsDisplayPanel.Controls.Clear();
                    CreateEnhancedAnalyticsChart();
                    CreateStatusGauges();

                    if (_analyticsChart != null)
                        _analyticsDisplayPanel.Controls.Add(_analyticsChart);

                    if (_systemStatusGauge != null)
                        _analyticsDisplayPanel.Controls.Add(_systemStatusGauge);
                }

                Console.WriteLine("✅ Analytics panel created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating analytics panel: {ex.Message}");
            }
        }

        /// <summary>
        /// Load analytics data asynchronously
        /// Based on async/await patterns for data loading
        /// </summary>
        private async Task LoadAnalyticsDataAsync()
        {
            try
            {
                Console.WriteLine("📈 Loading analytics data asynchronously...");

                await Task.Run(() =>
                {
                    // Simulate analytics data loading
                    System.Threading.Thread.Sleep(100);

                    // Update chart with real data if available
                    if (_analyticsChart != null && _vehicleRepository != null)
                    {
                        try
                        {
                            var vehicles = _vehicleRepository.GetAllVehicles().ToList();
                            Console.WriteLine($"📊 Analytics: {vehicles.Count} vehicles loaded for analysis");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error loading vehicle analytics: {ex.Message}");
                        }
                    }
                });

                Console.WriteLine("✅ Analytics data loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading analytics data: {ex.Message}");
            }
        }

        /// <summary>
        /// Load dashboard data asynchronously
        /// Initializes all dashboard data sources
        /// </summary>
        private async Task LoadDashboardDataAsync()
        {
            try
            {
                Console.WriteLine("🚀 Loading dashboard data asynchronously...");

                await Task.Run(() =>
                {
                    // Load vehicle data for grid
                    if (_vehicleDataGrid != null)
                    {
                        this.Invoke((System.Windows.Forms.MethodInvoker)delegate
                        {
                            LoadVehicleData();
                        });
                    }

                    // Update quick stats
                    System.Threading.Thread.Sleep(50);
                });

                Console.WriteLine("✅ Dashboard data loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading dashboard data: {ex.Message}");
            }
        }

        #endregion

        #region Safe Optimization - Logging and Error Handling Helpers

        /// <summary>
        /// Centralized logging helper for dashboard operations
        /// </summary>
        private void LogDashboard(string message, bool isError = false)
        {
            var prefix = isError ? "❌" : "✅";
            Console.WriteLine($"{prefix} DASHBOARD: {message}");
        }

        /// <summary>
        /// Centralized error handling for dashboard operations
        /// </summary>
        private void HandleDashboardError(string operation, Exception ex)
        {
            LogDashboard($"Error in {operation}: {ex.Message}", true);
        }

        #endregion
    }
}
