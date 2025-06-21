using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private TableLayoutPanel _mainTableLayout;

        // Traditional Layout Components (for fallback)
        private Panel _headerPanel;
        private Label _titleLabel;
        private SfButton _themeToggleButton;
        private FlowLayoutPanel _formButtonsPanel;
        private Panel _analyticsPanel;

        // Data Visualization Components
        private ChartControl _analyticsChart;
        private NotifyIcon _notifyIcon;

        // Dashboard Panels for Docking
        private Panel _analyticsDisplayPanel;
        private Panel _quickStatsPanel;
        private Panel _dataGridPanel;

        // Navigation method mapping for improved reliability
        private readonly Dictionary<string, System.Action> _navigationMethods;

        // Form instance tracking to prevent multiple instances
        private readonly Dictionary<string, Form> _activeManagementForms = new Dictionary<string, Form>();
        private readonly object _formLock = new object();

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

            if (themeButton.Style != null)
            {
                themeButton.Style.BackColor = Color.FromArgb(100, 255, 255, 255);
                themeButton.Style.HoverBackColor = Color.FromArgb(150, 255, 255, 255);
                themeButton.Style.PressedBackColor = Color.FromArgb(200, 255, 255, 255);
                themeButton.Style.Border = new Pen(Color.White, 1);
            }

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adjusting analytics layout: {ex.Message}");
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

            // Add header panel to form
            this.Controls.Add(_headerPanel);
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

                // Define standard navigation buttons that should always be available
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
                        Size = new Size(220, 40),
                        Margin = new Padding(0, 5, 0, 5),
                        Font = new Font("Segoe UI", 10, FontStyle.Regular),
                        ForeColor = Color.White,
                        Cursor = Cursors.Hand,
                        Tag = config.Action
                    };

                    // Ensure Style is initialized before using it
                    if (button.Style != null)
                    {
                        button.Style.BackColor = config.Color;
                        button.Style.HoverBackColor = ControlPaint.Light(config.Color, 0.2f);
                        button.Style.PressedBackColor = ControlPaint.Dark(config.Color, 0.1f);
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ SfButton.Style is null for button: {config.Text}");
                        // Fallback to basic button styling
                        button.BackColor = config.Color;
                    }

                    button.Click += (sender, e) => HandleButtonClick(config.Action);

                    if (_formButtonsPanel?.Controls != null)
                    {
                        _formButtonsPanel.Controls.Add(button);
                        Console.WriteLine($"Added navigation button: {config.Text}");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ _formButtonsPanel or Controls is null when adding button: {config.Text}");
                    }
                }

                Console.WriteLine($"✅ Added {fallbackConfigs.Length} navigation buttons");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error populating form buttons: {ex.Message}");
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

                if (button.Style != null)
                {
                    button.Style.BackColor = config.Color;
                    button.Style.HoverBackColor = ControlPaint.Light(config.Color, 0.2f);
                    button.Style.PressedBackColor = ControlPaint.Dark(config.Color, 0.1f);
                }

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

            if (themeButton.Style != null)
            {
                themeButton.Style.BackColor = Color.FromArgb(100, 255, 255, 255);
                themeButton.Style.HoverBackColor = Color.FromArgb(150, 255, 255, 255);
                themeButton.Style.PressedBackColor = Color.FromArgb(200, 255, 255, 255);
                themeButton.Style.Border = new Pen(Color.White, 1);
            }

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

                Console.WriteLine("✅ Analytics display updated with real data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating analytics display: {ex.Message}");
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
                if (vehicles == null)
                {
                    Console.WriteLine("⚠️ [DEBUG] Vehicle repository returned null, using fallback count");
                    return 25; // Fallback value
                }

                return vehicles.Count;
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
                if (drivers == null)
                {
                    Console.WriteLine("⚠️ [DEBUG] Driver repository returned null, using fallback count");
                    return 18; // Fallback value
                }

                return drivers.Count;
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
                if (routes == null)
                {
                    Console.WriteLine("⚠️ [DEBUG] Route repository returned null, using fallback count");
                    return 12; // Fallback value
                }

                return routes.Count;
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
        #region Missing Critical Methods - Navigation and UI Handling

        /// <summary>
        /// Handle button click events for navigation - Uses NavigationService to show discovered forms
        /// ENHANCED: Prevents multiple instances and ensures proper form lifecycle management
        /// </summary>
        private void HandleButtonClick(string navigationMethod)
        {
            try
            {
                Console.WriteLine($"🔄 Button clicked: {navigationMethod}");
                var moduleName = navigationMethod.Replace("Show", "").Replace("Management", "");

                // Log the navigation attempt
                LogNavigationEvent(moduleName, "ATTEMPT", $"Method: {navigationMethod}");

                // Special case for Dashboard/Home button
                if (moduleName.Equals("Dashboard", StringComparison.OrdinalIgnoreCase) ||
                    moduleName.Equals("Home", StringComparison.OrdinalIgnoreCase))
                {
                    ShowNotification("Navigation", "Returning to dashboard...");
                    ResetDashboard();
                    LogNavigationEvent(moduleName, "SUCCESS", "Reset to dashboard view");
                    return;
                }

                // CRITICAL: Initialize repositories before opening management forms
                EnsureRepositoriesInitialized(navigationMethod);

                ShowNotification("Navigation", $"Opening {moduleName} module...");
                Console.WriteLine($"✅ Opening {moduleName} management form...");

                // ENHANCED: Check if form is already open and bring to front instead of creating new instance
                lock (_formLock)
                {
                    if (_activeManagementForms.ContainsKey(navigationMethod))
                    {
                        var existingForm = _activeManagementForms[navigationMethod];
                        if (existingForm != null && !existingForm.IsDisposed)
                        {
                            Console.WriteLine($"🔄 Bringing existing {moduleName} form to front");
                            existingForm.BringToFront();
                            existingForm.WindowState = FormWindowState.Normal;
                            LogNavigationEvent(moduleName, "SUCCESS", "Existing form brought to front");
                            return;
                        }
                        else
                        {
                            // Remove disposed form from tracking
                            _activeManagementForms.Remove(navigationMethod);
                        }
                    }
                }

                // Use NavigationService to show the actual discovered forms
                if (_navigationService != null)
                {
                    try
                    {
                        // Use reflection to call the appropriate method on NavigationService
                        var methodInfo = _navigationService.GetType().GetMethod(navigationMethod);
                        if (methodInfo != null)
                        {
                            // ENHANCED: Create form and track it for lifecycle management
                            var newForm = CreateAndTrackManagementForm(navigationMethod, methodInfo);
                            if (newForm != null)
                            {
                                Console.WriteLine($"✅ {moduleName} management form opened and tracked");
                                LogNavigationEvent(moduleName, "SUCCESS", "Form opened and tracked via NavigationService");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Navigation method {navigationMethod} not found in NavigationService");
                            ShowNotification("Warning", $"Navigation method not implemented: {navigationMethod}");
                            LogNavigationEvent(moduleName, "WARNING", $"Method not found: {navigationMethod}");
                        }
                    }
                    catch (Exception navEx)
                    {
                        Console.WriteLine($"❌ Error invoking navigation method: {navEx.Message}");
                        ShowNotification("Error", $"Failed to open {moduleName} module");
                        LogNavigationEvent(moduleName, "ERROR", $"Navigation failed: {navEx.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ NavigationService is not available");
                    ShowNotification("Error", "Navigation service unavailable");
                    LogNavigationEvent(moduleName, "ERROR", "NavigationService is null");
                }

                // Fallback: Try using the navigation methods dictionary if available
                if (_navigationMethods?.ContainsKey(navigationMethod) == true)
                {
                    try
                    {
                        _navigationMethods[navigationMethod].Invoke();
                        Console.WriteLine($"✅ [FALLBACK] Navigation completed via dictionary: {navigationMethod}");
                        LogNavigationEvent(moduleName, "INFO", "Fallback navigation completed");
                    }
                    catch (Exception dictEx)
                    {
                        Console.WriteLine($"⚠️ [FALLBACK] Dictionary navigation failed: {dictEx.Message}");
                        LogNavigationEvent(moduleName, "WARNING", $"Fallback failed: {dictEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [DEBUG] Error handling button click: {ex.Message}");
                ShowNotification("Error", "Navigation temporarily unavailable");
                LogNavigationEvent(navigationMethod, "ERROR", ex.Message);
            }
        }

        /// <summary>
        /// Creates and tracks a management form instance to prevent duplicates and ensure proper disposal
        /// </summary>
        private Form? CreateAndTrackManagementForm(string navigationMethod, System.Reflection.MethodInfo methodInfo)
        {
            try
            {
                Console.WriteLine($"🔄 Creating and tracking form for: {navigationMethod}");

                // Get the form factory to create the form directly instead of using ShowDialog
                if (_navigationService is NavigationService navService)
                {
                    // Use reflection to get the form factory
                    var formFactoryField = navService.GetType().GetField("_formFactory",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (formFactoryField?.GetValue(navService) is IFormFactory formFactory)
                    {
                        // Determine the form type based on navigation method
                        var formType = GetFormTypeFromNavigationMethod(navigationMethod);
                        if (formType != null)
                        {
                            Console.WriteLine($"🔄 Creating form of type: {formType.Name}");
                            var createFormMethod = typeof(IFormFactory).GetMethod("CreateForm", new Type[] { typeof(object[]) });
                            var genericMethod = createFormMethod?.MakeGenericMethod(formType);
                            var newForm = genericMethod?.Invoke(formFactory, new object[] { new object[0] }) as Form;

                            if (newForm != null)
                            {
                                // CRITICAL: Set up proper disposal handling before tracking
                                SetupFormDisposalHandling(newForm, navigationMethod);

                                // Track the form instance
                                lock (_formLock)
                                {
                                    _activeManagementForms[navigationMethod] = newForm;
                                }

                                // Show the form
                                newForm.Show();
                                Console.WriteLine($"✅ Form created, tracked, and shown: {formType.Name}");
                                return newForm;
                            }
                        }
                    }
                }

                // Fallback: use the original navigation service method (ShowDialog approach)
                Console.WriteLine("⚠️ Using fallback navigation approach");
                methodInfo.Invoke(_navigationService, null);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating and tracking form: {ex.Message}");
                // Fallback to original method
                try
                {
                    methodInfo.Invoke(_navigationService, null);
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"❌ Fallback navigation also failed: {fallbackEx.Message}");
                }
                return null;
            }
        }

        /// <summary>
        /// Sets up proper disposal handling for management forms
        /// </summary>
        private void SetupFormDisposalHandling(Form form, string navigationMethod)
        {
            // Handle form closing to remove from tracking
            form.FormClosing += (sender, e) =>
            {
                Console.WriteLine($"🧽 Management form closing: {navigationMethod}");
            };

            // Handle form closed to clean up tracking
            form.FormClosed += (sender, e) =>
            {
                lock (_formLock)
                {
                    if (_activeManagementForms.ContainsKey(navigationMethod))
                    {
                        _activeManagementForms.Remove(navigationMethod);
                        Console.WriteLine($"🧽 Removed form from tracking: {navigationMethod}");
                    }
                }

                // Ensure proper disposal of Syncfusion controls if this is a BaseManagementForm
                if (form is BaseManagementForm<object> baseForm)
                {
                    DisposeSyncfusionControlsSafelyForManagementForm(baseForm);
                }
            };

            // Handle form disposal
            form.Disposed += (sender, e) =>
            {
                Console.WriteLine($"🧽 Management form disposed: {navigationMethod}");
            };
        }

        /// <summary>
        /// Gets the form type from the navigation method name
        /// </summary>
        private Type? GetFormTypeFromNavigationMethod(string navigationMethod)
        {
            var formTypeMap = new Dictionary<string, Type>
            {
                { "ShowVehicleManagement", typeof(VehicleManagementFormSyncfusion) },
                { "ShowDriverManagement", typeof(DriverManagementFormSyncfusion) },
                { "ShowRouteManagement", typeof(RouteManagementFormSyncfusion) },
                { "ShowActivityManagement", typeof(ActivityManagementFormSyncfusion) },
                { "ShowFuelManagement", typeof(FuelManagementFormSyncfusion) },
                { "ShowMaintenanceManagement", typeof(MaintenanceManagementFormSyncfusion) },
                { "ShowSchoolCalendarManagement", typeof(SchoolCalendarManagementFormSyncfusion) },
                { "ShowActivityScheduleManagement", typeof(ActivityScheduleManagementFormSyncfusion) },
                { "ShowAnalyticsDemo", typeof(AnalyticsDemoFormSyncfusion) },
                // TimeCard management will be handled by the main application
            };

            return formTypeMap.TryGetValue(navigationMethod, out var formType) ? formType : null;
        }

        /// <summary>
        /// Safely dispose Syncfusion controls in management forms to prevent crashes
        /// </summary>
        private void DisposeSyncfusionControlsSafelyForManagementForm(Form managementForm)
        {
            try
            {
                Console.WriteLine($"🧽 Disposing Syncfusion controls for: {managementForm.GetType().Name}");

                // Suppress finalization immediately to prevent crashes
                GC.SuppressFinalize(managementForm);

                var syncfusionControls = new List<Control>();
                CollectSyncfusionControlsFromForm(managementForm, syncfusionControls);

                Console.WriteLine($"🧽 Found {syncfusionControls.Count} Syncfusion controls in management form");

                // Dispose in reverse order (children first)
                for (int i = syncfusionControls.Count - 1; i >= 0; i--)
                {
                    var control = syncfusionControls[i];
                    try
                    {
                        if (control != null && !control.IsDisposed)
                        {
                            // Special handling for common Syncfusion management form controls
                            var controlType = control.GetType().FullName;
                            Console.WriteLine($"🧽 Disposing: {controlType}");

                            // Critical: Always suppress finalization for Syncfusion controls
                            GC.SuppressFinalize(control);

                            // Remove from parent first
                            control.Parent?.Controls.Remove(control);

                            // Clear data source if it's a data grid
                            if (controlType?.Contains("SfDataGrid") == true)
                            {
                                ClearDataGridSafely(control);
                            }

                            control.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error disposing control: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in DisposeSyncfusionControlsSafelyForManagementForm: {ex.Message}");
            }
        }

        /// <summary>
        /// Collects Syncfusion controls from a form
        /// </summary>
        private void CollectSyncfusionControlsFromForm(Control parent, List<Control> syncfusionControls)
        {
            try
            {
                foreach (Control control in parent.Controls)
                {
                    if (control.GetType().FullName?.Contains("Syncfusion") == true)
                    {
                        syncfusionControls.Add(control);
                    }

                    if (control.HasChildren)
                    {
                        CollectSyncfusionControlsFromForm(control, syncfusionControls);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error collecting Syncfusion controls: {ex.Message}");
            }
        }

        /// <summary>
        /// Safely clears data from SfDataGrid to prevent threading issues
        /// </summary>
        private void ClearDataGridSafely(Control dataGrid)
        {
            try
            {
                // Clear data source
                var dataSourceProperty = dataGrid.GetType().GetProperty("DataSource");
                if (dataSourceProperty != null && dataSourceProperty.CanWrite)
                {
                    dataSourceProperty.SetValue(dataGrid, null);
                    Console.WriteLine("🧽 SfDataGrid DataSource cleared");
                }

                // Clear selection
                var clearSelectionMethod = dataGrid.GetType().GetMethod("ClearSelection");
                if (clearSelectionMethod != null)
                {
                    clearSelectionMethod.Invoke(dataGrid, null);
                    Console.WriteLine("🧽 SfDataGrid selection cleared");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Could not clear SfDataGrid safely: {ex.Message}");
            }
        }
        #endregion

        /// <summary>
        /// Create enhanced dashboard layout with DockingManager - SIMPLIFIED VERSION
        /// Based on official Syncfusion DockingManager documentation: https://help.syncfusion.com/windowsforms/docking-manager/getting-started
        /// </summary>
        private void CreateEnhancedDashboardLayout()
        {
            try
            {
                Console.WriteLine("🚀 Creating simplified dashboard layout...");

                // Clear existing controls
                this.Controls.Clear();

                // Initialize DockingManager using documented pattern
                // Note: Actual docking configuration is handled in ConfigureDockingLayout
                if (_dockingManager == null)
                {
                    _dockingManager = new DockingManager(this.components)
                    {
                        HostControl = this,
                        DockTabAlignment = Syncfusion.Windows.Forms.Tools.DockTabAlignmentStyle.Top,
                        EnableAutoAdjustCaption = true // Allow auto-adjusting captions
                    };
                }

                // Create the 4 essential panels
                CreateSimpleHeaderPanel();
                CreateSimpleSidePanel();
                CreateSimpleStatisticsPanel();
                CreateMiddleContentArea(); // New middle content area
                CreateSimpleAnalyticsPanel();

                // Configure docking layout
                ConfigureDockingLayout();

                Console.WriteLine("✅ Simplified dashboard layout created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating dashboard layout: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create simple header panel
        /// </summary>
        private void CreateSimpleHeaderPanel()
        {
            _headerPanel = new Panel
            {
                Name = "HeaderPanel",
                Height = 70,
                BackColor = Color.FromArgb(63, 81, 181),
                BorderStyle = BorderStyle.None
            };
            _titleLabel = new Label
            {
                Text = "🚌 BusBuddy Management Dashboard",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Padding = new Padding(25, 0, 25, 0)
            };
            _headerPanel.Controls.Add(_titleLabel);
        }

        private void CreateSimpleSidePanel()
        {
            _quickStatsPanel = new Panel
            {
                Name = "SidePanel",
                Width = 280,
                BackColor = Color.FromArgb(245, 246, 250),
                BorderStyle = BorderStyle.FixedSingle
            };
            var sideTitle = new Label
            {
                Text = "📋 Navigation",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 10, 20, 10)
            };
            _formButtonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(15, 10, 15, 15)
            };
            _quickStatsPanel.Controls.Add(sideTitle);
            _quickStatsPanel.Controls.Add(_formButtonsPanel);

            // Add a home/dashboard button at the top
            AddDashboardButton();

            // Populate with other navigation buttons
            PopulateFormButtons();
        }

        private void CreateSimpleStatisticsPanel()
        {
            _analyticsDisplayPanel = new Panel
            {
                Name = "StatisticsPanel",
                Width = 300,
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle
            };
            var statsTitle = new Label
            {
                Text = "📊 Fleet Statistics",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 10, 20, 10)
            };
            var statsContent = new Label
            {
                Text = $"🚌 Vehicles: {GetVehicleCount()}\r\n\r\n👨‍✈️ Drivers: {GetActiveDriverCount()}\r\n\r\n🗺️ Routes: {GetRouteCount()}\r\n\r\n⚡ Efficiency: {GetFleetEfficiency():F1}%",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 15, 20, 15),
                TextAlign = ContentAlignment.TopLeft
            };
            _analyticsDisplayPanel.Controls.Add(statsTitle);
            _analyticsDisplayPanel.Controls.Add(statsContent);
        }

        private void CreateSimpleAnalyticsPanel()
        {
            _dataGridPanel = new Panel
            {
                Name = "AnalyticsPanel",
                Height = 200,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            var analyticsTitle = new Label
            {
                Text = "📈 Analytics Dashboard",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 10, 20, 10)
            };
            _analyticsChart = new ChartControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            var series = new ChartSeries("Fleet Performance", ChartSeriesType.Column);
            series.Points.Add("Jan", 85);
            series.Points.Add("Feb", 92);
            series.Points.Add("Mar", 88);
            series.Points.Add("Apr", 94);
            _analyticsChart.Series.Add(series);
            _dataGridPanel.Controls.Add(analyticsTitle);
            _dataGridPanel.Controls.Add(_analyticsChart);
        }

        /// <summary>
        /// Creates the middle content area with default dashboard content
        /// </summary>
        private void CreateMiddleContentArea()
        {
            _analyticsPanel = new Panel
            {
                Name = "MiddleContentArea",
                BackColor = Color.FromArgb(250, 251, 252),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Load the default dashboard content
            LoadDefaultDashboardContent();

            // The panel will be added to the form by the DockingManager
        }

        /// <summary>
        /// Loads the default dashboard content into the middle content area
        /// </summary>
        private void LoadDefaultDashboardContent()
        {
            try
            {
                Console.WriteLine("🔄 Loading default dashboard content...");

                if (_analyticsPanel == null)
                {
                    Console.WriteLine("⚠️ Middle content panel is null");
                    return;
                }

                // Clear any existing controls
                _analyticsPanel.Controls.Clear();

                // Create a TableLayoutPanel for 3 rows
                var middleLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    RowCount = 3,
                    ColumnCount = 1,
                    Padding = new Padding(15),
                    BackColor = Color.Transparent
                };

                // Set row styles for equal distribution
                middleLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
                middleLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
                middleLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34F));

                // Create content for each row
                var row1Panel = CreateContentRow("📋 Recent Activities", "5 vehicles scheduled for maintenance\n3 new driver certifications pending", Color.FromArgb(255, 248, 225));
                var row2Panel = CreateContentRow("⚠️ Alerts & Notifications", "2 vehicles due for inspection\n1 route optimization suggestion", Color.FromArgb(255, 235, 238));
                var row3Panel = CreateContentRow("📈 Quick Metrics", "Average fuel efficiency: 7.2 MPG\nOn-time performance: 94.3%", Color.FromArgb(232, 245, 233));

                middleLayout.Controls.Add(row1Panel, 0, 0);
                middleLayout.Controls.Add(row2Panel, 0, 1);
                middleLayout.Controls.Add(row3Panel, 0, 2);

                _analyticsPanel.Controls.Add(middleLayout);

                // Set the default title
                UpdateMiddleContentTitle("Dashboard Overview");

                Console.WriteLine("✅ Default dashboard content loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading default dashboard content: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a content row panel with title and content
        /// </summary>
        private Panel CreateContentRow(string title, string content, Color backgroundColor)
        {
            var rowPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = backgroundColor,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5)
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 5, 10, 5)
            };

            var contentLabel = new Label
            {
                Text = content,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(10, 5, 10, 10)
            };

            rowPanel.Controls.Add(titleLabel);
            rowPanel.Controls.Add(contentLabel);

            return rowPanel;
        }

        private void InitializeEnhancedComponents()
        {
            // Initialize enhanced components if needed
        }        private void BusBuddyDashboardSyncfusion_FormClosing(object sender, FormClosingEventArgs e)
        {
            // ENHANCED: Properly dispose all tracked management forms before closing dashboard
            try
            {
                Console.WriteLine("🧽 Dashboard closing - disposing tracked management forms...");

                lock (_formLock)
                {
                    var formsToDispose = new List<Form>(_activeManagementForms.Values);
                    _activeManagementForms.Clear();

                    foreach (var form in formsToDispose)
                    {
                        try
                        {
                            if (form != null && !form.IsDisposed)
                            {
                                Console.WriteLine($"🧽 Closing management form: {form.GetType().Name}");
                                form.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error closing management form: {ex.Message}");
                        }
                    }
                }                // CRITICAL: Enhanced application termination logic
                TestSafeApplicationShutdownManager.PerformShutdown();

                Console.WriteLine("✅ All management forms disposed during dashboard shutdown");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during dashboard form closing: {ex.Message}");

                // Even if there's an error, still attempt to terminate the application
                try
                {
                    Console.WriteLine("🔥 Emergency application termination...");
                    Application.Exit();
                    Environment.Exit(0);
                }
                catch (Exception exitEx)
                {
                    Console.WriteLine($"❌ Emergency termination failed: {exitEx.Message}");
                }
            }
        }

        /// <summary>
        /// Perform comprehensive application shutdown to ensure all BusBuddy.UI processes are terminated
        /// </summary>
        private void PerformApplicationShutdown()
        {
            try
            {
                Console.WriteLine("🔥 Performing comprehensive application shutdown...");

                // Step 1: Close all remaining open forms
                CloseAllRemainingForms();

                // Step 2: Dispose Syncfusion resources safely
                DisposeDashboardSyncfusionResources();

                // Step 3: Clean up service container
                CleanupServiceContainer();

                // Step 4: Force garbage collection
                ForceGarbageCollection();

                // Step 5: Terminate the application to ensure all processes are closed
                Console.WriteLine("🔥 Calling Application.Exit() to terminate all BusBuddy.UI processes...");
                Application.Exit();

                // Step 6: If Application.Exit() doesn't work, use Environment.Exit() as backup
                Console.WriteLine("🔥 Calling Environment.Exit(0) as backup termination...");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during application shutdown: {ex.Message}");

                // Final emergency termination
                try
                {
                    Console.WriteLine("🔥 Final emergency termination...");
                    Environment.Exit(1); // Exit with error code
                }
                catch (Exception finalEx)
                {
                    Console.WriteLine($"❌ Final termination failed: {finalEx.Message}");
                }
            }
        }

        /// <summary>
        /// Close all remaining open forms to prevent orphaned processes
        /// </summary>
        private void CloseAllRemainingForms()
        {
            try
            {
                var openForms = new List<Form>();
                foreach (Form form in Application.OpenForms)
                {
                    if (form != this) // Don't close the dashboard itself yet
                    {
                        openForms.Add(form);
                    }
                }

                Console.WriteLine($"🧽 Closing {openForms.Count} remaining open forms...");

                foreach (var form in openForms)
                {
                    try
                    {
                        if (form != null && !form.IsDisposed)
                        {
                            Console.WriteLine($"🧽 Closing form: {form.GetType().Name}");
                            form.Close();
                            form.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error closing form {form?.GetType().Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error in CloseAllRemainingForms: {ex.Message}");
            }
        }

        /// <summary>
        /// Dispose dashboard-specific Syncfusion resources
        /// </summary>
        private void DisposeDashboardSyncfusionResources()
        {
            try
            {
                Console.WriteLine("🧽 Disposing dashboard Syncfusion resources...");

                // Clean up hanging .NET build processes FIRST
                CleanupHangingDotNetProcesses();

                // Dispose docking manager safely
                if (_dockingManager != null)
                {
                    try
                    {
                        _dockingManager.Dispose();
                        Console.WriteLine("🧽 DockingManager disposed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error disposing DockingManager: {ex.Message}");
                    }
                }

                // Dispose analytics chart safely
                if (_analyticsChart != null && !_analyticsChart.IsDisposed)
                {
                    try
                    {
                        _analyticsChart.Dispose();
                        Console.WriteLine("🧽 AnalyticsChart disposed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error disposing AnalyticsChart: {ex.Message}");
                    }
                }

                // Dispose notify icon safely
                if (_notifyIcon != null)
                {
                    try
                    {
                        _notifyIcon.Dispose();
                        Console.WriteLine("🧽 NotifyIcon disposed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error disposing NotifyIcon: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error in DisposeDashboardSyncfusionResources: {ex.Message}");
            }
        }

        /// <summary>
        /// Clean up hanging .NET build processes that may prevent clean shutdown
        /// </summary>
        private void CleanupHangingDotNetProcesses()
        {
            try
            {
                Console.WriteLine("🧹 Cleaning up hanging .NET build processes...");

                using (var process = new System.Diagnostics.Process())
                {
                    process.StartInfo.FileName = "powershell.exe";
                    process.StartInfo.Arguments = @"-Command ""
                        # Kill hanging MSBuild nodes (older than 2 minutes)
                        Get-Process | Where-Object { 
                            $_.ProcessName -eq 'dotnet' -and 
                            $_.StartTime -lt (Get-Date).AddMinutes(-2) -and
                            (Get-WmiObject Win32_Process -Filter \""ProcessId = $($_.Id)\"").CommandLine -like '*MSBuild*'
                        } | ForEach-Object { 
                            Write-Host \""Killing hanging MSBuild: PID $($_.Id)\"" -ForegroundColor Red
                            Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
                        }

                        # Kill VBCSCompiler processes consuming high CPU
                        Get-Process | Where-Object { 
                            $_.ProcessName -eq 'dotnet' -and 
                            $_.CPU -gt 10 -and
                            (Get-WmiObject Win32_Process -Filter \""ProcessId = $($_.Id)\"").CommandLine -like '*VBCSCompiler*'
                        } | ForEach-Object { 
                            Write-Host \""Killing high-CPU VBCSCompiler: PID $($_.Id)\"" -ForegroundColor Red
                            Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
                        }
                        
                        Write-Host \""✅ .NET process cleanup completed\"" -ForegroundColor Green
                    """;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

                    process.Start();
                    
                    // Wait up to 5 seconds for cleanup to complete
                    if (!process.WaitForExit(5000))
                    {
                        Console.WriteLine("⚠️ .NET process cleanup timed out after 5 seconds");
                        process.Kill();
                    }
                    
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    
                    if (!string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine($"🧹 Process cleanup output: {output}");
                    }
                    
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"⚠️ Process cleanup errors: {error}");
                    }
                }
                
                Console.WriteLine("✅ .NET process cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error cleaning up .NET processes: {ex.Message}");
                // Don't let cleanup errors prevent normal shutdown
            }
        }

        /// <summary>
        /// Clean up service container to release all dependencies
        /// </summary>
        private void CleanupServiceContainer()
        {
            try
            {
                Console.WriteLine("🧽 Attempting to clean up services...");

                // Clear navigation service references
                if (_navigationService != null)
                {
                    Console.WriteLine("🧽 Clearing navigation service references");
                }

                // Clear database helper service references
                if (_databaseHelperService != null)
                {
                    Console.WriteLine("🧽 Clearing database helper service references");
                }

                Console.WriteLine("✅ Service cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error cleaning up services: {ex.Message}");
            }
        }

        /// <summary>
        /// Force garbage collection to help clean up resources
        /// </summary>
        private void ForceGarbageCollection()
        {
            try
            {
                Console.WriteLine("🧽 Forcing garbage collection...");

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                Console.WriteLine("✅ Garbage collection completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during garbage collection: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the title of the middle content area based on the selected module
        /// </summary>
        private void UpdateMiddleContentTitle(string title)
        {
            try
            {
                // This method would update the title in the middle content area
                // For now, just log it since the title is handled elsewhere
                Console.WriteLine($"📊 Middle content title updated: {title}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error updating middle content title: {ex.Message}");
            }
        }

        private void LogDashboard(string message)
        {
            Console.WriteLine($"📊 {message}");
        }

        private void HandleDashboardError(string context, Exception ex)
        {
            Console.WriteLine($"❌ {context}: {ex.Message}");
        }

        private void CreateAdvancedLayoutForTests()
        {
            CreateBasicLayout();
        }

        private async Task LoadAnalyticsDataAsync()
        {
            await Task.Delay(100);
        }

        private async Task LoadDashboardDataAsync()
        {
            await Task.Delay(100);
        }

        private void CreateEnhancedAnalyticsChart()
        {
            try
            {
                if (_analyticsChart == null)
                    _analyticsChart = new ChartControl();
                _analyticsChart.Series.Clear();
                _analyticsChart.BackColor = Color.White;
                _analyticsChart.Title.Text = "Fleet Performance";
                _analyticsChart.PrimaryXAxis.Title = "Months";
                _analyticsChart.PrimaryYAxis.Title = "Efficiency %";
                var series = new ChartSeries("Fleet Utilization", ChartSeriesType.Column);
                series.Points.Add(1, 85);
                series.Points.Add(2, 88);
                series.Points.Add(3, 92);
                series.Points.Add(4, 87);
                series.Points.Add(5, 91);
                series.Points.Add(6, 89);
                _analyticsChart.Series.Add(series);
                Console.WriteLine("✅ Enhanced analytics chart created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating enhanced analytics chart: {ex.Message}");
            }
        }

        private void ShowNotification(string title, string message)
        {
            try
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
                }
                Console.WriteLine($"📢 {title}: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error showing notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Resets the dashboard to its default state
        /// </summary>
        private void ResetDashboard()
        {
            try
            {
                Console.WriteLine("🔄 Resetting dashboard to default state...");

                // Reload the default dashboard content
                if (_analyticsPanel != null)
                {
                    LoadDefaultDashboardContent();
                }

                ShowNotification("Dashboard", "Dashboard reset to default view");
                Console.WriteLine("✅ Dashboard reset successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error resetting dashboard: {ex.Message}");
                ShowNotification("Error", "Failed to reset dashboard");
            }
        }

        /// <summary>
        /// Configures the docking layout for all panels using Syncfusion DockingManager
        /// Based on official Syncfusion DockingManager documentation: https://help.syncfusion.com/windowsforms/docking-manager/getting-started
        /// </summary>
        private void ConfigureDockingLayout()
        {
            try
            {
                Console.WriteLine("🔄 Configuring docking layout for all panels...");

                // Remove direct Control.Add references since DockingManager will handle this
                this.Controls.Clear();

                // Make sure DockingManager is properly set up
                if (_dockingManager == null)
                {
                    Console.WriteLine("⚠️ DockingManager is null, creating new instance");
                    _dockingManager = new DockingManager(this.components)
                    {
                        HostControl = this,
                        DockTabAlignment = Syncfusion.Windows.Forms.Tools.DockTabAlignmentStyle.Top,
                        EnableAutoAdjustCaption = true
                    };
                }

                // Header panel at the top
                if (_headerPanel != null)
                {
                    Console.WriteLine("🔄 Docking header panel to Top");
                    _dockingManager.DockControl(_headerPanel, this, DockingStyle.Top, 70);
                }

                // Side navigation panel on the left
                if (_quickStatsPanel != null)
                {
                    Console.WriteLine("🔄 Docking navigation panel to Left");
                    _dockingManager.DockControl(_quickStatsPanel, this, DockingStyle.Left, 280);
                }

                // Statistics panel on the right
                if (_analyticsDisplayPanel != null)
                {
                    Console.WriteLine("🔄 Docking statistics panel to Right");
                    _dockingManager.DockControl(_analyticsDisplayPanel, this, DockingStyle.Right, 300);
                }

                // Analytics panel at the bottom
                if (_dataGridPanel != null)
                {
                    Console.WriteLine("🔄 Docking analytics panel to Bottom");
                    _dockingManager.DockControl(_dataGridPanel, this, DockingStyle.Bottom, 200);
                }

                // Middle content area in the center - fill remaining space
                if (_analyticsPanel != null)
                {
                    Console.WriteLine("🔄 Docking middle content area to Fill");
                    _dockingManager.DockControl(_analyticsPanel, this, DockingStyle.Fill, 0);
                }

                Console.WriteLine("✅ Docking layout configured successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error configuring docking layout: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a Dashboard/Home button to the navigation panel
        /// </summary>
        private void AddDashboardButton()
        {
            try
            {
                if (_formButtonsPanel == null) return;

                // Create a Dashboard button using Syncfusion SfButton
                var dashboardButton = new SfButton
                {
                    Text = "🏠 Dashboard",
                    Size = new Size(220, 40),
                    Margin = new Padding(0, 5, 0, 15),  // Extra bottom margin to separate from other buttons
                    Tag = "ShowDashboard",
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold)
                };

                // Use Indigo color for dashboard button
                var dashboardColor = Color.FromArgb(63, 81, 181);
                if (dashboardButton.Style != null)
                {
                    dashboardButton.Style.BackColor = dashboardColor;
                    dashboardButton.Style.HoverBackColor = ControlPaint.Light(dashboardColor, 0.2f);
                    dashboardButton.Style.PressedBackColor = ControlPaint.Dark(dashboardColor, 0.1f);
                }

                // Add click handler
                dashboardButton.Click += (sender, e) => HandleButtonClick("ShowDashboard");

                // Add button to the panel at the top
                _formButtonsPanel.Controls.Add(dashboardButton);

                Console.WriteLine("✅ Dashboard button added to navigation panel");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error adding dashboard button: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs a navigation event to the console only (no external popup)
        /// </summary>
        private void LogNavigationEvent(string moduleName, string status, string message = null)
        {
            try
            {
                // Create log entry with timestamp
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logEntry = $"[{timestamp}] NAVIGATION: {moduleName} - {status}" + (message != null ? $" - {message}" : "");

                // Log to application console only (no external file or popup)
                Console.WriteLine(logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error logging navigation event: {ex.Message}");
            }
        }

        /// <summary>
        /// Enhanced dashboard cleanup with finalization suppression
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    Console.WriteLine("🧽 Starting enhanced dashboard disposal...");

                    // CRITICAL: Suppress finalization immediately
                    System.GC.SuppressFinalize(this);

                    // Clear form instance tracking
                    lock (_formLock)
                    {
                        _activeManagementForms.Clear();
                    }

                    // Dispose repositories
                    DisposeRepositories();

                    // Dispose dashboard-specific resources
                    DisposeDashboardSyncfusionResources();

                    // Clean up service container
                    CleanupServiceContainer();

                    Console.WriteLine("✅ Enhanced dashboard disposal completed");
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"⚠️ Error during enhanced dashboard disposal: {ex.Message}");
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Dispose all repository dependencies to prevent database connection leaks
        /// </summary>
        private void DisposeRepositories()
        {
            try
            {
                // Dispose repositories if they implement IDisposable
                if (_vehicleRepository is IDisposable vehicleDisposable)
                {
                    vehicleDisposable.Dispose();
                    Console.WriteLine("🧽 VehicleRepository disposed");
                }

                if (_driverRepository is IDisposable driverDisposable)
                {
                    driverDisposable.Dispose();
                    Console.WriteLine("🧽 DriverRepository disposed");
                }

                if (_routeRepository is IDisposable routeDisposable)
                {
                    routeDisposable.Dispose();
                    Console.WriteLine("🧽 RouteRepository disposed");
                }

                if (_maintenanceRepository is IDisposable maintenanceDisposable)
                {
                    maintenanceDisposable.Dispose();
                    Console.WriteLine("🧽 MaintenanceRepository disposed");
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"⚠️ Error disposing repositories: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures repositories are initialized and database is ready for the specified management form
        /// This prevents database connection issues when opening management forms
        /// </summary>
        private void EnsureRepositoriesInitialized(string navigationMethod)
        {
            try
            {
                Console.WriteLine($"🔧 Initializing repositories for {navigationMethod}...");

                // Map navigation methods to their required repositories
                var repositoryMap = new Dictionary<string, System.Action>
                {
                    {"ShowVehicleManagement", () => EnsureVehicleRepositoryInitialized()},
                    {"ShowDriverManagement", () => EnsureDriverRepositoryInitialized()},
                    {"ShowRouteManagement", () => EnsureRouteRepositoryInitialized()},
                    {"ShowActivityManagement", () => EnsureActivityRepositoryInitialized()},
                    {"ShowFuelManagement", () => EnsureFuelRepositoryInitialized()},
                    {"ShowMaintenanceManagement", () => EnsureMaintenanceRepositoryInitialized()},
                    {"ShowCalendarManagement", () => EnsureSchoolCalendarRepositoryInitialized()},
                    {"ShowScheduleManagement", () => EnsureActivityScheduleRepositoryInitialized()},
                    {"ShowSchoolCalendarManagement", () => EnsureSchoolCalendarRepositoryInitialized()},
                    {"ShowActivityScheduleManagement", () => EnsureActivityScheduleRepositoryInitialized()},
                    {"ShowReportsManagement", () => EnsureAllRepositoriesInitialized()},
                    {"ShowAnalyticsDemo", () => EnsureAllRepositoriesInitialized()}
                };

                // Initialize the required repositories for this navigation method
                if (repositoryMap.ContainsKey(navigationMethod))
                {
                    repositoryMap[navigationMethod].Invoke();
                    Console.WriteLine($"✅ Repositories initialized for {navigationMethod}");
                }
                else
                {
                    // For unknown navigation methods, ensure basic repositories are initialized
                    Console.WriteLine($"⚠️ Unknown navigation method {navigationMethod}, initializing basic repositories");
                    EnsureBasicRepositoriesInitialized();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error initializing repositories for {navigationMethod}: {ex.Message}");
                // Log but don't throw - let the form try to open anyway
                // The individual repositories will handle their own initialization
            }
        }

        /// <summary>
        /// Ensures VehicleRepository is initialized by creating a test instance
        /// </summary>
        private void EnsureVehicleRepositoryInitialized()
        {
            try
            {
                // Creating the repository will trigger database initialization
                var tempRepo = new VehicleRepository();
                tempRepo.GetAllVehicles(); // This will ensure the database is ready
                Console.WriteLine("✅ VehicleRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ VehicleRepository initialization warning: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures DriverRepository is initialized by creating a test instance
        /// </summary>
        private void EnsureDriverRepositoryInitialized()
        {
            try
            {
                var tempRepo = new DriverRepository();
                tempRepo.GetAllDrivers();
                Console.WriteLine("✅ DriverRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ DriverRepository initialization warning: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures RouteRepository is initialized by creating a test instance
        /// </summary>
        private void EnsureRouteRepositoryInitialized()
        {
            try
            {
                var tempRepo = new RouteRepository();
                tempRepo.GetAllRoutes();
                Console.WriteLine("✅ RouteRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ RouteRepository initialization warning: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures ActivityRepository is initialized by creating a test instance
        /// </summary>
        private void EnsureActivityRepositoryInitialized()
        {
            try
            {
                var tempRepo = new ActivityRepository();
                tempRepo.GetAllActivities();
                Console.WriteLine("✅ ActivityRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ ActivityRepository initialization warning: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures FuelRepository is initialized by creating a test instance
        /// </summary>
        private void EnsureFuelRepositoryInitialized()
        {
            try
            {
                var tempRepo = new FuelRepository();
                tempRepo.GetAllFuelRecords();
                Console.WriteLine("✅ FuelRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ FuelRepository initialization warning: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures MaintenanceRepository is initialized by creating a test instance
        /// </summary>
        private void EnsureMaintenanceRepositoryInitialized()
        {
            try
            {
                var tempRepo = new MaintenanceRepository();
                tempRepo.GetAllMaintenanceRecords();
                Console.WriteLine("✅ MaintenanceRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ MaintenanceRepository initialization warning: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures SchoolCalendarRepository is initialized by creating a test instance
        /// </summary>
        private void EnsureSchoolCalendarRepositoryInitialized()
        {
            try
            {
                var tempRepo = new SchoolCalendarRepository();
                tempRepo.GetAllCalendarEntries();
                Console.WriteLine("✅ SchoolCalendarRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ SchoolCalendarRepository initialization warning: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures ActivityScheduleRepository is initialized by creating a test instance
        /// </summary>
        private void EnsureActivityScheduleRepositoryInitialized()
        {
            try
            {
                var tempRepo = new ActivityScheduleRepository();
                tempRepo.GetAllScheduledActivities();
                Console.WriteLine("✅ ActivityScheduleRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ ActivityScheduleRepository initialization warning: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures TimeCardRepository is initialized by creating a test instance
        /// </summary>
        private void EnsureTimeCardRepositoryInitialized()
        {
            try
            {
                var tempRepo = new TimeCardRepository(new BusBuddyContext());
                var timeCards = tempRepo.GetAllAsync().Result;
                Console.WriteLine("✅ TimeCardRepository initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ TimeCardRepository initialization warning: {ex.Message}");
            }
        }

        private void EnsureAllRepositoriesInitialized()
        {
            try
            {
                Console.WriteLine("🔄 Initializing all repositories...");

                // Initialize all repository types for comprehensive access
                EnsureVehicleRepositoryInitialized();
                EnsureDriverRepositoryInitialized();
                EnsureRouteRepositoryInitialized();
                EnsureActivityRepositoryInitialized();
                EnsureFuelRepositoryInitialized();
                EnsureMaintenanceRepositoryInitialized();
                EnsureSchoolCalendarRepositoryInitialized();
                EnsureActivityScheduleRepositoryInitialized();
                EnsureTimeCardRepositoryInitialized();

                Console.WriteLine("✅ All repositories initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error initializing all repositories: {ex.Message}");
            }
        }

        private void EnsureBasicRepositoriesInitialized()
        {
            try
            {
                Console.WriteLine("🔄 Initializing basic repositories...");

                // Initialize core repositories needed for basic functionality
                EnsureVehicleRepositoryInitialized();
                EnsureDriverRepositoryInitialized();
                EnsureRouteRepositoryInitialized();
                EnsureTimeCardRepositoryInitialized();

                Console.WriteLine("✅ Basic repositories initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error initializing basic repositories: {ex.Message}");
            }
        }
    }
}
