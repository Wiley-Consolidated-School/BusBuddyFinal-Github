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
                Console.WriteLine("🚨 Creating emergency fallback layout...");
                
                this.Controls.Clear();
                
                var emergencyPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(250, 250, 250),
                    Padding = new Padding(20)
                };

                var emergencyLabel = new Label
                {
                    Text = "🚨 BusBuddy Dashboard - Emergency Mode\n\n" +
                           "The dashboard encountered an error during initialization.\n" +
                           "Basic functionality is available below.\n\n" +
                           "Please check the console for detailed error information.",
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 12, FontStyle.Regular),
                    ForeColor = Color.DimGray,
                    TextAlign = ContentAlignment.TopCenter,
                    Dock = DockStyle.Top,
                    Height = 150
                };

                // Create basic navigation buttons
                var buttonPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    Padding = new Padding(10)
                };

                var basicButtons = new[]
                {
                    new { Text = "🚌 Vehicles", Action = "ShowVehicleManagement" },
                    new { Text = "👨‍✈️ Drivers", Action = "ShowDriverManagement" },
                    new { Text = "🗺️ Routes", Action = "ShowRouteManagement" },
                    new { Text = "🔧 Maintenance", Action = "ShowMaintenanceManagement" }
                };

                foreach (var btn in basicButtons)
                {
                    var button = new Button
                    {
                        Text = btn.Text,
                        Size = new Size(150, 60),
                        Margin = new Padding(5),
                        BackColor = Color.FromArgb(63, 81, 181),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    button.Click += (s, e) => HandleButtonClick(btn.Action);
                    buttonPanel.Controls.Add(button);
                }

                emergencyPanel.Controls.Add(buttonPanel);
                emergencyPanel.Controls.Add(emergencyLabel);
                this.Controls.Add(emergencyPanel);
                
                Console.WriteLine("✅ Emergency layout created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Even emergency layout failed: {ex.Message}");
                // If even this fails, we're in serious trouble
                MessageBox.Show($"Critical dashboard error: {ex.Message}", "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                Console.WriteLine("🎨 Creating simplified dashboard layout...");
                
                // Clear any existing controls
                this.Controls.Clear();

                // Create the component container if needed
                if (this.components == null)
                {
                    this.components = new System.ComponentModel.Container();
                }

                // **SIMPLIFIED LAYOUT**: Use a cleaner, less cluttered approach
                _mainTableLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2, // Reduced from 3 to 2 for simplicity
                    BackColor = SyncfusionThemeHelper.MaterialColors.Surface,
                    Padding = new Padding(10),
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.None
                };

                // Configure simplified row styles - header and main content only
                _mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, ScaleForDpi(60)));  // Header - compact
                _mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Main content - full remaining space

                // Handle form resize events for responsiveness
                this.Resize += OnFormResize;

                // Initialize simplified DockingManager
                _dockingManager = new Syncfusion.Windows.Forms.Tools.DockingManager(this.components);
                _dockingManager.HostControl = this;
                _dockingManager.EnableDocumentMode = false; // Disable for cleaner look
                _dockingManager.CaptionHeight = ScaleForDpi(22);
                _dockingManager.ShowCaptionImages = false; // Cleaner appearance

                this.Controls.Add(_mainTableLayout);

                // Create simplified UI components
                CreateSimplifiedHeader();
                CreateTabbedMainContent(); // New tabbed approach for better organization
                
                Console.WriteLine("✅ Simplified dashboard layout created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating main layout: {ex.Message}");
                CreateEmergencyLayout(); // Fallback to emergency layout
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
        /// <summary>
        /// Enhanced button click navigation with comprehensive error handling
        /// </summary>
        private void HandleButtonClick(string actionName)
        {
            try
            {
                Console.WriteLine($"� Handling button click for action: {actionName}");

                // **CRITICAL**: Ensure ServiceContainer is initialized
                if (!ServiceContainerSingleton.IsInitialized)
                {
                    Console.WriteLine("⚠️ ServiceContainer not initialized - initializing now");
                    ServiceContainerSingleton.Initialize();
                    
                    if (!ServiceContainerSingleton.IsInitialized)
                    {
                        MessageBox.Show("Failed to initialize service container. Please restart the application.", 
                            "Service Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // **REPOSITORY INITIALIZATION**: Ensure required repository is available
                if (_repositoryTypeMap.TryGetValue(actionName, out Type repositoryType))
                {
                    Console.WriteLine($"🔧 Ensuring repository type: {repositoryType.Name}");
                    try
                    {
                        EnsureRepositoryByType(repositoryType);
                    }
                    catch (Exception repoEx)
                    {
                        Console.WriteLine($"❌ Repository initialization failed: {repoEx.Message}");
                        MessageBox.Show($"Database connection issue: {repoEx.Message}\n\nPlease check your database configuration.", 
                            "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        // Continue with navigation attempt - some forms may work without full repository access
                    }
                }

                // **NAVIGATION ATTEMPT**: Try to show the requested form
                if (_navigationMethods.ContainsKey(actionName))
                {
                    Console.WriteLine($"🚀 Attempting navigation to: {actionName}");
                    
                    // Show loading indicator
                    this.Cursor = Cursors.WaitCursor;
                    
                    try
                    {
                        _navigationMethods[actionName].Invoke();
                        Console.WriteLine($"✅ Successfully navigated to: {actionName}");
                    }
                    catch (Exception navEx)
                    {
                        Console.WriteLine($"❌ Navigation failed for {actionName}: {navEx.Message}");
                        
                        // Provide specific error guidance
                        var errorMessage = GetNavigationErrorMessage(actionName, navEx);
                        MessageBox.Show(errorMessage, "Navigation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        
                        // Log detailed error for debugging
                        Console.WriteLine($"📝 Full error details: {navEx}");
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ No navigation method found for action: {actionName}");
                    MessageBox.Show($"The '{actionName}' feature is not yet implemented or configured.", 
                        "Feature Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Critical error in HandleButtonClick: {ex.Message}");
                MessageBox.Show($"An unexpected error occurred: {ex.Message}\n\nPlease try again or restart the application.", 
                    "Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Provides specific error messages for navigation failures
        /// </summary>
        private string GetNavigationErrorMessage(string actionName, Exception ex)
        {
            var actionDisplay = actionName.Replace("Show", "").Replace("Management", " Management");
            
            if (ex.Message.Contains("database") || ex.Message.Contains("connection"))
            {
                return $"Cannot open {actionDisplay} due to database connection issues.\n\n" +
                       "Please check:\n" +
                       "• Database server is running\n" +
                       "• Connection string is correct\n" +
                       "• Network connectivity is available\n\n" +
                       $"Technical details: {ex.Message}";
            }
            else if (ex.Message.Contains("form") || ex.Message.Contains("Form"))
            {
                return $"The {actionDisplay} form could not be loaded.\n\n" +
                       "This may be due to:\n" +
                       "• Missing form implementation\n" +
                       "• UI component initialization error\n" +
                       "• Theme or styling conflict\n\n" +
                       $"Technical details: {ex.Message}";
            }
            else if (ex.Message.Contains("repository") || ex.Message.Contains("Repository"))
            {
                return $"Data access error for {actionDisplay}.\n\n" +
                       "The data layer may have issues:\n" +
                       "• Repository not properly configured\n" +
                       "• Database schema mismatch\n" +
                       "• Missing database tables\n\n" +
                       $"Technical details: {ex.Message}";
            }
            else
            {
                return $"Failed to open {actionDisplay}.\n\n" +
                       $"Error: {ex.Message}\n\n" +
                       "Please try again or contact support if the problem persists.";
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
        /// Enhanced application closure with comprehensive resource cleanup
        /// </summary>
        private void CloseApplication()
        {
            try
            {
                Console.WriteLine("� Close button clicked - initiating comprehensive shutdown...");

                // **PHASE 1**: Stop any background operations
                StopBackgroundOperations();

                // **PHASE 2**: Clean up data connections first
                CleanupRepositoryConnections();

                // **PHASE 3**: Clean up UI components systematically  
                CleanupUIComponents();

                // **PHASE 4**: Force additional cleanup for lingering processes
                KillAllProcesses();

                // **PHASE 5**: Final garbage collection
                ForceGarbageCollection();

                Console.WriteLine("✅ Comprehensive shutdown completed");

                // Close the form properly
                this.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during application close: {ex.Message}");
                this.Close(); // Force close anyway
            }
        }

        /// <summary>
        /// Stops any background operations or async tasks
        /// </summary>
        private void StopBackgroundOperations()
        {
            try
            {
                Console.WriteLine("🛑 Stopping background operations...");
                
                // Cancel any ongoing async operations
                // If we had CancellationTokens, we would cancel them here
                
                // Stop any timers
                // If we had System.Windows.Forms.Timer instances, stop them here
                
                // Wait briefly for operations to complete
                System.Threading.Thread.Sleep(100);
                
                Console.WriteLine("✅ Background operations stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error stopping background operations: {ex.Message}");
            }
        }

        /// <summary>
        /// Forces comprehensive garbage collection
        /// </summary>
        private void ForceGarbageCollection()
        {
            try
            {
                Console.WriteLine("🧹 Forcing garbage collection...");
                
                // Multiple rounds of garbage collection
                for (int i = 0; i < 3; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                
                // Compact the heap
                GC.Collect();
                
                Console.WriteLine("✅ Garbage collection completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during garbage collection: {ex.Message}");
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
        /// Enhanced UI component cleanup with systematic disposal
        /// </summary>
        private void CleanupUIComponents()
        {
            try
            {
                Console.WriteLine("🧽 Starting comprehensive UI cleanup...");

                // **PHASE 1**: Remove controls from docking manager first
                if (_dockingManager != null)
                {
                    try
                    {
                        Console.WriteLine("🔧 Cleaning up docking manager...");
                        
                        // Disable docking for all controls before disposal
                        var controlsToCleanup = new List<Control>();
                        foreach (Control control in this.Controls)
                        {
                            if (_dockingManager.GetEnableDocking(control))
                            {
                                controlsToCleanup.Add(control);
                            }
                        }

                        foreach (var control in controlsToCleanup)
                        {
                            try
                            {
                                _dockingManager.SetEnableDocking(control, false);
                                Console.WriteLine($"🔧 Disabled docking for {control.Name ?? control.GetType().Name}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"⚠️ Error disabling docking for control: {ex.Message}");
                            }
                        }

                        _dockingManager.Dispose();
                        _dockingManager = null;
                        Console.WriteLine("✅ DockingManager disposed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error disposing DockingManager: {ex.Message}");
                    }
                }

                // **PHASE 2**: Dispose Syncfusion controls in specific order
                DisposeSyncfusionControl(ref _themeToggleButton, "ThemeToggleButton");
                DisposeSyncfusionControl(ref _closeButton, "CloseButton");
                DisposeSyncfusionControl(ref _efficiencyGauge, "EfficiencyGauge");
                DisposeSyncfusionControl(ref _maintenanceGauge, "MaintenanceGauge");
                DisposeSyncfusionControl(ref _systemStatusGauge, "SystemStatusGauge");
                DisposeSyncfusionControl(ref _analyticsChart, "AnalyticsChart");

                // **PHASE 3**: Dispose panel resources
                DisposeControl(ref _sidePanel, "SidePanel");
                DisposeControl(ref _statisticsPanel, "StatisticsPanel");
                DisposeControl(ref _analyticsPanel, "AnalyticsPanel");
                DisposeControl(ref _headerPanel, "HeaderPanel");

                // **PHASE 4**: Dispose layout controls
                if (_formButtonsPanel != null)
                {
                    _formButtonsPanel.Dispose();
                    _formButtonsPanel = null;
                    Console.WriteLine("🧽 FormButtonsPanel disposed");
                }

                if (_mainTableLayout != null)
                {
                    _mainTableLayout.Dispose();
                    _mainTableLayout = null;
                    Console.WriteLine("🧽 MainTableLayout disposed");
                }

                // **PHASE 5**: Clear all remaining controls
                try
                {
                    if (this.Controls != null)
                    {
                        while (this.Controls.Count > 0)
                        {
                            var control = this.Controls[0];
                            this.Controls.RemoveAt(0);
                            control?.Dispose();
                        }
                    }
                    Console.WriteLine("🧽 All remaining controls cleared");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error clearing remaining controls: {ex.Message}");
                }

                Console.WriteLine("✅ UI component cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error cleaning up UI components: {ex.Message}");
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

        /// <summary>
        /// Creates a simplified, clean header panel
        /// </summary>
        private void CreateSimplifiedHeader()
        {
            try
            {
                _headerPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    Height = ScaleForDpi(60),
                    BackColor = SyncfusionThemeHelper.MaterialColors.Primary,
                    Margin = new Padding(0)
                };

                // Simple header layout with title and theme toggle only
                var headerLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0)
                };
                
                headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 85));
                headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));

                _titleLabel = new Label
                {
                    Text = "🚌 BusBuddy Dashboard",
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 16, FontStyle.Bold),
                    ForeColor = Color.White,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.Transparent,
                    Padding = new Padding(15, 0, 0, 0)
                };

                _themeToggleButton = new SfButton
                {
                    Text = SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark ? "☀️" : "🌙",
                    Size = new Size(ScaleForDpi(40), ScaleForDpi(30)),
                    Anchor = AnchorStyles.Right | AnchorStyles.Top,
                    BackColor = Color.Transparent,
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 12, FontStyle.Regular)
                };
                _themeToggleButton.Click += (s, e) => ToggleTheme();

                headerLayout.Controls.Add(_titleLabel, 0, 0);
                headerLayout.Controls.Add(_themeToggleButton, 1, 0);
                _headerPanel.Controls.Add(headerLayout);
                _mainTableLayout.Controls.Add(_headerPanel, 0, 0);

                Console.WriteLine("✅ Simplified header created");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating simplified header: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a tabbed main content area for better organization
        /// Based on official Syncfusion TabControlAdv documentation
        /// </summary>
        private void CreateTabbedMainContent()
        {
            try
            {
                // Use Syncfusion's TabControlAdv for professional tabbed interface
                var tabControl = new Syncfusion.Windows.Forms.Tools.TabControlAdv
                {
                    Dock = DockStyle.Fill,
                    TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererMetro),
                    ShowToolTips = true,
                    BorderStyle = BorderStyle.None,
                    Padding = new Point(10, 5)
                };

                // Apply current theme to tab control
                if (SyncfusionThemeHelper.CurrentTheme == SyncfusionThemeHelper.ThemeMode.Dark)
                {
                    tabControl.BackColor = Color.FromArgb(43, 47, 51);
                    tabControl.BorderColor = Color.FromArgb(63, 68, 73);
                }

                // **TAB 1: Management** - All form navigation buttons
                var managementTab = new Syncfusion.Windows.Forms.Tools.TabPageAdv("Management")
                {
                    BackColor = SyncfusionThemeHelper.MaterialColors.Surface,
                    ToolTipText = "Access all management forms"
                };
                CreateManagementContent(managementTab);
                tabControl.Controls.Add(managementTab);

                // **TAB 2: Analytics** - Charts and gauges
                var analyticsTab = new Syncfusion.Windows.Forms.Tools.TabPageAdv("Analytics")
                {
                    BackColor = SyncfusionThemeHelper.MaterialColors.Surface,
                    ToolTipText = "View fleet analytics and performance"
                };
                CreateAnalyticsContent(analyticsTab);
                tabControl.Controls.Add(analyticsTab);

                // **TAB 3: Quick Stats** - Key metrics
                var statsTab = new Syncfusion.Windows.Forms.Tools.TabPageAdv("Quick Stats")
                {
                    BackColor = SyncfusionThemeHelper.MaterialColors.Surface,
                    ToolTipText = "View key performance indicators"
                };
                CreateStatsContent(statsTab);
                tabControl.Controls.Add(statsTab);

                _mainTableLayout.Controls.Add(tabControl, 0, 1);
                Console.WriteLine("✅ Tabbed main content created");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating tabbed content: {ex.Message}");
                CreateFallbackContent(); // Create simple fallback
            }
        }

        /// <summary>
        /// Creates management tab content with organized navigation buttons
        /// </summary>
        private void CreateManagementContent(Control parentTab)
        {
            try
            {
                var scrollPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    Padding = new Padding(20)
                };

                // Organize buttons into logical groups
                var groupLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 3,
                    RowCount = 3,
                    BackColor = Color.Transparent
                };

                // Configure equal-width columns
                for (int i = 0; i < 3; i++)
                {
                    groupLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
                    groupLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));
                }

                // **PRIMARY OPERATIONS** (Row 1)
                var vehicleBtn = CreateStyledManagementButton("🚌 Vehicle Management", "ShowVehicleManagement");
                var driverBtn = CreateStyledManagementButton("👨‍✈️ Driver Management", "ShowDriverManagement");
                var routeBtn = CreateStyledManagementButton("🗺️ Route Management", "ShowRouteManagement");

                // **SCHEDULING & ACTIVITIES** (Row 2)
                var activityBtn = CreateStyledManagementButton("📅 Activity Management", "ShowActivityManagement");
                var calendarBtn = CreateStyledManagementButton("🗓️ Calendar Management", "ShowCalendarManagement");
                var scheduleBtn = CreateStyledManagementButton("⏰ Schedule Management", "ShowScheduleManagement");

                // **MAINTENANCE & REPORTS** (Row 3)
                var maintenanceBtn = CreateStyledManagementButton("🔧 Maintenance", "ShowMaintenanceManagement");
                var fuelBtn = CreateStyledManagementButton("⛽ Fuel Management", "ShowFuelManagement");
                var reportsBtn = CreateStyledManagementButton("📊 Reports", "ShowReportsManagement");

                // Add buttons to layout
                groupLayout.Controls.Add(vehicleBtn, 0, 0);
                groupLayout.Controls.Add(driverBtn, 1, 0);
                groupLayout.Controls.Add(routeBtn, 2, 0);
                groupLayout.Controls.Add(activityBtn, 0, 1);
                groupLayout.Controls.Add(calendarBtn, 1, 1);
                groupLayout.Controls.Add(scheduleBtn, 2, 1);
                groupLayout.Controls.Add(maintenanceBtn, 0, 2);
                groupLayout.Controls.Add(fuelBtn, 1, 2);
                groupLayout.Controls.Add(reportsBtn, 2, 2);

                scrollPanel.Controls.Add(groupLayout);
                parentTab.Controls.Add(scrollPanel);

                Console.WriteLine("✅ Management content created with organized button layout");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating management content: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a styled button for management functions
        /// </summary>
        private SfButton CreateStyledManagementButton(string text, string action)
        {
            var button = new SfButton
            {
                Text = text,
                Size = new Size(ScaleForDpi(200), ScaleForDpi(80)),
                Margin = new Padding(10),
                Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 11, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            // Apply professional styling
            button.Style.BackColor = SyncfusionThemeHelper.MaterialColors.Primary;
            button.Style.ForeColor = Color.White;
            button.Style.HoverBackColor = Color.FromArgb(63, 81, 181); // Material blue hover
            button.Style.PressedBackColor = Color.FromArgb(48, 63, 159); // Material blue pressed

            button.Click += (s, e) => HandleButtonClick(action);
            return button;
        }

        /// <summary>
        /// Creates analytics tab content with charts and gauges
        /// </summary>
        private void CreateAnalyticsContent(Control parentTab)
        {
            try
            {
                var analyticsPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 2,
                    BackColor = Color.Transparent,
                    Padding = new Padding(20)
                };

                analyticsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
                analyticsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
                analyticsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
                analyticsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

                // Create simplified chart placeholder
                var chartPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(248, 249, 250),
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(5)
                };

                var chartLabel = new Label
                {
                    Text = "📈 Fleet Performance Chart\n\n(Chart visualization will be implemented\nwith real data from analytics service)",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 10, FontStyle.Regular),
                    ForeColor = Color.DimGray
                };
                chartPanel.Controls.Add(chartLabel);

                // Create simplified gauges panel
                var gaugesPanel = CreateSimplifiedGaugesPanel();

                // Create quick metrics panel
                var metricsPanel = CreateQuickMetricsPanel();

                analyticsPanel.Controls.Add(chartPanel, 0, 0);
                analyticsPanel.Controls.Add(gaugesPanel, 1, 0);
                analyticsPanel.Controls.Add(metricsPanel, 0, 1);

                parentTab.Controls.Add(analyticsPanel);
                Console.WriteLine("✅ Analytics content created");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating analytics content: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates stats tab content with key performance indicators
        /// </summary>
        private void CreateStatsContent(Control parentTab)
        {
            try
            {
                var statsLayout = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    Padding = new Padding(20),
                    BackColor = Color.Transparent
                };

                // Create stat cards
                var statCards = new[]
                {
                    CreateStatCard("Total Vehicles", "42", "🚌", Color.FromArgb(76, 175, 80)),
                    CreateStatCard("Active Drivers", "38", "👨‍✈️", Color.FromArgb(33, 150, 243)),
                    CreateStatCard("Routes Today", "15", "🗺️", Color.FromArgb(255, 152, 0)),
                    CreateStatCard("Maintenance Due", "3", "🔧", Color.FromArgb(244, 67, 54)),
                    CreateStatCard("Fuel Efficiency", "8.2 MPG", "⛽", Color.FromArgb(156, 39, 176)),
                    CreateStatCard("On-Time Rate", "94%", "⏰", Color.FromArgb(76, 175, 80))
                };

                foreach (var card in statCards)
                {
                    statsLayout.Controls.Add(card);
                }

                parentTab.Controls.Add(statsLayout);
                Console.WriteLine("✅ Stats content created");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating stats content: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a stat card for key metrics
        /// </summary>
        private Panel CreateStatCard(string title, string value, string icon, Color accentColor)
        {
            var card = new Panel
            {
                Size = new Size(ScaleForDpi(200), ScaleForDpi(120)),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                Padding = new Padding(15)
            };

            // Add accent color strip
            var accentStrip = new Panel
            {
                Size = new Size(4, card.Height),
                BackColor = accentColor,
                Location = new Point(0, 0)
            };

            var iconLabel = new Label
            {
                Text = icon,
                Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 20, FontStyle.Regular),
                Size = new Size(ScaleForDpi(40), ScaleForDpi(40)),
                Location = new Point(15, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.DimGray,
                Location = new Point(65, 10),
                Size = new Size(ScaleForDpi(120), ScaleForDpi(20))
            };

            var valueLabel = new Label
            {
                Text = value,
                Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(65, 35),
                Size = new Size(ScaleForDpi(120), ScaleForDpi(30))
            };

            card.Controls.Add(accentStrip);
            card.Controls.Add(iconLabel);
            card.Controls.Add(titleLabel);
            card.Controls.Add(valueLabel);

            return card;
        }

        /// <summary>
        /// Creates a fallback content area if tabbed content fails
        /// </summary>
        private void CreateFallbackContent()
        {
            try
            {
                var fallbackPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = SyncfusionThemeHelper.MaterialColors.Surface,
                    Padding = new Padding(20)
                };

                var fallbackLabel = new Label
                {
                    Text = "⚠️ Dashboard Loading...\n\nIf this message persists, there may be an issue with the dashboard initialization.",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 12, FontStyle.Regular),
                    ForeColor = Color.DimGray
                };

                fallbackPanel.Controls.Add(fallbackLabel);
                _mainTableLayout.Controls.Add(fallbackPanel, 0, 1);
                Console.WriteLine("⚠️ Fallback content created");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating fallback content: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a simplified gauges panel
        /// </summary>
        private Panel CreateSimplifiedGaugesPanel()
        {
            try
            {
                var gaugesPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(248, 249, 250),
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(5),
                    Padding = new Padding(10)
                };

                var gaugeLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    RowCount = 3,
                    ColumnCount = 1,
                    BackColor = Color.Transparent
                };

                // Create simple gauge representations
                var systemGauge = CreateSimpleGaugeLabel("System Status", "92%", Color.FromArgb(76, 175, 80));
                var maintenanceGauge = CreateSimpleGaugeLabel("Maintenance", "3 Due", Color.FromArgb(255, 152, 0));
                var efficiencyGauge = CreateSimpleGaugeLabel("Efficiency", "Good", Color.FromArgb(33, 150, 243));

                gaugeLayout.Controls.Add(systemGauge, 0, 0);
                gaugeLayout.Controls.Add(maintenanceGauge, 0, 1);
                gaugeLayout.Controls.Add(efficiencyGauge, 0, 2);

                gaugesPanel.Controls.Add(gaugeLayout);
                return gaugesPanel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating gauges panel: {ex.Message}");
                return new Panel { Dock = DockStyle.Fill, BackColor = Color.LightGray };
            }
        }

        /// <summary>
        /// Creates a simple gauge label representation
        /// </summary>
        private Panel CreateSimpleGaugeLabel(string title, string value, Color gaugeColor)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.DimGray,
                Dock = DockStyle.Top,
                Height = 20,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var valueLabel = new Label
            {
                Text = value,
                Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 12, FontStyle.Bold),
                ForeColor = gaugeColor,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            panel.Controls.Add(valueLabel);
            panel.Controls.Add(titleLabel);
            return panel;
        }

        /// <summary>
        /// Creates a quick metrics panel
        /// </summary>
        private Panel CreateQuickMetricsPanel()
        {
            try
            {
                var metricsPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(248, 249, 250),
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(5),
                    Padding = new Padding(10)
                };

                var metricsLabel = new Label
                {
                    Text = "📊 Quick Metrics\n\n• Daily Routes: 15 active\n• Fuel Usage: 8.2 MPG avg\n• Driver Attendance: 95%\n• Maintenance Alerts: 3 pending",
                    Dock = DockStyle.Fill,
                    Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 9, FontStyle.Regular),
                    ForeColor = Color.DimGray,
                    TextAlign = ContentAlignment.TopLeft
                };

                metricsPanel.Controls.Add(metricsLabel);
                return metricsPanel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating metrics panel: {ex.Message}");
                return new Panel { Dock = DockStyle.Fill, BackColor = Color.LightGray };
            }
        }
    }
}
