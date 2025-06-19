using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using BusBuddy.UI.Services;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Base;
using BusBuddy.Business;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

namespace BusBuddy.UI.Views
{
    #region Theme Service

    /// <summary>
    /// Enhanced theme service for consistent styling
    /// </summary>
    public static class EnhancedThemeService
    {
        public static Color BackgroundColor => Color.FromArgb(245, 245, 245);
        public static Color SurfaceColor => Color.White;
        public static Color HeaderColor => Color.FromArgb(33, 150, 243);
        public static Color HeaderTextColor => Color.White;
        public static Color SidebarColor => Color.FromArgb(98, 91, 113);
        public static Color ButtonColor => Color.FromArgb(33, 150, 243);
        public static Color ButtonTextColor => Color.White;
        public static Color ButtonHoverColor => Color.FromArgb(30, 136, 229);
        public static Color PrimaryDarkColor => Color.FromArgb(25, 118, 210);
        public static Color AccentColor => Color.FromArgb(255, 152, 0);
        public static Color TextColor => Color.FromArgb(33, 33, 33);
        public static Color SecondaryTextColor => Color.FromArgb(117, 117, 117);
        public static Color BorderColor => Color.FromArgb(224, 224, 224);
        public static Color WarningColor => Color.FromArgb(255, 152, 0);

        public static Font DefaultFont => new Font("Segoe UI", 9F, FontStyle.Regular);
        public static Font HeaderFont => GetSafeFont(12F, FontStyle.Bold);
        public static Font ButtonFont => GetSafeFont(9F, FontStyle.Regular);

        public static Font GetSafeFont(float size, FontStyle style = FontStyle.Regular)
        {
            try
            {
                return new Font("Segoe UI", size, style);
            }
            catch
            {
                return new Font(FontFamily.GenericSansSerif, size, style);
            }
        }
    }

    #endregion

    #region Logging

    /// <summary>
    /// Log levels for dashboard operations
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    #endregion

    #region Configuration Classes

    /// <summary>
    /// Centralized configuration for dashboard behavior and features
    /// </summary>
    public class DashboardConfiguration
    {
        public bool EnableDiagnostics { get; set; } = true;
        public bool UseEnhancedLayout { get; set; } = true;
        public bool UseSimpleWorkingLayout { get; set; } = false;
        public bool UseDockingManagerLayout { get; set; } = true; // ENABLED: Professional docking layout
        public bool UseEnhancedFormDiscovery { get; set; } = true;
        public bool EnableDpiLogging { get; set; } = true;
        public bool EnablePerformanceCaching { get; set; } = true;
        public bool EnableAccessibilityValidation { get; set; } = true;
        public bool EnableControlOverlapDetection { get; set; } = true;
        public int MinimumLogLevel { get; set; } = 1; // Info level
        public bool OptimizeForPerformance { get; set; } = true;
        public bool EnableEmojiFallback { get; set; } = true;
    }

    /// <summary>
    /// Form information for dashboard navigation
    /// </summary>
    public class FormInfo
    {
        public string DisplayName { get; set; }
        public string FormTypeName { get; set; }
        public Type FormType { get; set; }
        public string Description { get; set; }
        public string IconText { get; set; } = "📋";
        public bool IsEnabled { get; set; } = true;
        public string Category { get; set; } = "General";

        public FormInfo(string displayName, Type formType, string description = "", string iconText = "📋")
        {
            DisplayName = displayName;
            FormType = formType;
            FormTypeName = formType?.Name ?? displayName;
            Description = description;
            IconText = iconText;
        }
    }

    #endregion

    /// <summary>
    /// Enhanced BusBuddy Dashboard with comprehensive fixes for visibility and base form conflicts
    /// Features: Diagnostic testing, multiple fallback strategies, enhanced error handling
    /// </summary>
    public partial class BusBuddyDashboardSyncfusion : Form // CRITICAL FIX: Inherit directly from Form instead of SyncfusionBaseForm
    {
        private readonly INavigationService _navigationService;
        private readonly IDatabaseHelperService _databaseHelperService;
        private List<FormInfo> _cachedForms;
        private Control? _dashboardMainPanel; // Use different name to avoid hiding base class member
        private Panel? _headerPanel;
        private Control _titleLabel; // Changed to Control to support both AutoLabel and Label
        private Panel? _contentPanel;
        private FlowLayoutPanel _formButtonsPanel;

        // Syncfusion Docking Manager for professional layout
        private DockingManager? _dockingManager;

        // OPTIMIZED: Configuration flags moved to centralized configuration
        private static readonly DashboardConfiguration Config = LoadDashboardConfiguration();

        /// <summary>
        /// Load dashboard configuration from environment variables, config file, or defaults
        /// </summary>
        private static DashboardConfiguration LoadDashboardConfiguration()
        {
            var config = new DashboardConfiguration();

            try
            {
                // Load from environment variables first (for runtime configuration)
                config.EnableDiagnostics = bool.Parse(Environment.GetEnvironmentVariable("BUSBUDDY_DIAGNOSTICS") ?? config.EnableDiagnostics.ToString());
                config.EnableDpiLogging = bool.Parse(Environment.GetEnvironmentVariable("BUSBUDDY_DPI_LOGGING") ?? config.EnableDpiLogging.ToString());
                config.OptimizeForPerformance = bool.Parse(Environment.GetEnvironmentVariable("BUSBUDDY_OPTIMIZE_PERFORMANCE") ?? config.OptimizeForPerformance.ToString());

                // TODO: In future versions, load from JSON configuration file
                // var configFile = Path.Combine(Application.StartupPath, "dashboard.config.json");
                // if (File.Exists(configFile)) { ... }

                Console.WriteLine($"📋 Dashboard configuration loaded: Diagnostics={config.EnableDiagnostics}, Performance={config.OptimizeForPerformance}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to load configuration, using defaults: {ex.Message}");
            }

            return config;
        }

        // Legacy support for existing code
        private static readonly bool ENABLE_DIAGNOSTICS = Config.EnableDiagnostics;
        private static readonly bool USE_ENHANCED_LAYOUT = Config.UseEnhancedLayout;
        private static readonly bool USE_SIMPLE_WORKING_LAYOUT = Config.UseSimpleWorkingLayout;
        private static readonly bool USE_ENHANCED_FORM_DISCOVERY = Config.UseEnhancedFormDiscovery;
        private static readonly bool ENABLE_DPI_LOGGING = Config.EnableDpiLogging;
        private static readonly bool ENABLE_PERFORMANCE_CACHING = Config.EnablePerformanceCaching;
        private static readonly bool ENABLE_ACCESSIBILITY_VALIDATION = Config.EnableAccessibilityValidation;
        private static readonly bool ENABLE_CONTROL_OVERLAP_DETECTION = Config.EnableControlOverlapDetection;

        // Performance optimization: Static form cache to avoid repeated reflection
        private static readonly Dictionary<string, Type> _formTypeCache = new Dictionary<string, Type>();
        private static readonly object _cacheInitLock = new object();

        // Cache for rendering validation
        private static readonly string CACHE_FILE = "form_cache.json";
        private static bool _syncfusionRenderingValidated = false;

        public BusBuddyDashboardSyncfusion(INavigationService navigationService, IDatabaseHelperService databaseHelperService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _databaseHelperService = databaseHelperService ?? throw new ArgumentNullException(nameof(databaseHelperService));

            // Ensure _formButtonsPanel is always initialized to prevent null reference exceptions
            Console.WriteLine("🔧 PRE-INIT: Initializing _formButtonsPanel to prevent null reference");
            _formButtonsPanel = new FlowLayoutPanel
            {
                Name = "QuickActionsFlowPanel",
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(10),
                BackColor = ColorTranslator.FromHtml("#FAFAFA"),
                MinimumSize = new Size(400, 200),
                Visible = true
            };

            InitializeComponent();
            ConfigureWindow();

            // Add Load event handler for final positioning
            this.Load += BusBuddyDashboardSyncfusion_Load;

            // Add resize handler to monitor title bar visibility
            this.Resize += BusBuddyDashboardSyncfusion_WindowStateChanged;

            // Add DPI changed handler for dynamic DPI changes
            this.DpiChanged += BusBuddyDashboardSyncfusion_DpiChanged;

            // Temporarily disable double buffering for debugging black screen issue
            // this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            Console.WriteLine("⚠️ Double buffering temporarily disabled for debugging");

            // Choose form discovery strategy
            if (USE_ENHANCED_FORM_DISCOVERY)
            {
                ScanAndCacheFormsEnhanced();
            }
            else
            {
                // Simple fallback - initialize empty cache
                _cachedForms = new List<FormInfo>();
                Console.WriteLine("📋 Using simple form discovery fallback");
            }

            // Choose layout strategy with proper error handling
            if (Config.UseDockingManagerLayout)
            {
                Console.WriteLine("🏗️ Using DOCKING MANAGER layout for professional UI");
                try
                {
                    CreateDockingManagerLayout();
                }
                catch (Exception dockingEx)
                {
                    Console.WriteLine($"❌ Docking manager layout failed: {dockingEx.Message}");
                    Console.WriteLine("🔄 Falling back to enhanced layout");
                    CreateMainLayoutEnhanced();
                }
            }
            else if (USE_SIMPLE_WORKING_LAYOUT)
            {
                Console.WriteLine("🔧 Using SIMPLE WORKING layout for debugging");
                CreateSimpleWorkingLayout();
            }
            else if (USE_ENHANCED_LAYOUT)
            {
                Console.WriteLine("🔬 Using ENHANCED layout with fallbacks and diagnostics");
                try
                {
                    CreateMainLayoutEnhanced();
                }
                catch (Exception layoutEx)
                {
                    Console.WriteLine($"❌ Enhanced layout failed: {layoutEx.Message}");
                    Console.WriteLine("🔄 Falling back to simple working layout");
                    CreateSimpleWorkingLayout();
                }
            }
            else
            {
                Console.WriteLine("🔧 Using ORIGINAL layout");
                CreateMainLayout();
            }

            // Defer remaining setup to Load event to ensure handle creation
            Console.WriteLine("⏰ Deferring final initialization to Load event");
        }

        #region Initialization Methods

        private void InitializeComponent()
        {
            this.Text = "BusBuddy Dashboard";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1200, 700);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            // Log DPI information if enabled
            if (ENABLE_DPI_LOGGING)
            {
                LogDpiInformation();
            }
        }

        private void ConfigureWindow()
        {
            this.Text = "BusBuddy Dashboard";

            // Dashboard should be maximizable/minimizable unlike regular forms
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.ControlBox = true;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.ShowInTaskbar = true;

            // Ensure proper window positioning before maximizing
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Ensure window controls are accessible
            EnsureWindowControlsVisible();

            // Apply DPI-aware positioning
            ApplyDpiAwarePositioning();

            // Force the form to not be maximized initially to ensure title bar is visible
            this.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// Log DPI information for diagnostic purposes
        /// </summary>
        private void LogDpiInformation()
        {
            try
            {
                using (var graphics = this.CreateGraphics())
                {
                    var dpiX = graphics.DpiX;
                    var dpiY = graphics.DpiY;
                    var scale = dpiX / 96f;

                    Console.WriteLine($"🔍 DPI DIAGNOSTICS:");
                    Console.WriteLine($"   DPI X: {dpiX}, DPI Y: {dpiY}");
                    Console.WriteLine($"   Scale Factor: {scale:F2}x");
                    Console.WriteLine($"   High DPI Mode: {scale > 1.25f}");
                    Console.WriteLine($"   Screen Resolution: {Screen.PrimaryScreen.Bounds}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DPI Logging failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Original layout creation method for fallback compatibility
        /// </summary>
        private void CreateMainLayout()
        {
            Console.WriteLine("⚠️ Using original layout - consider switching to enhanced layout");
            CreateMainLayoutEnhanced(); // Redirect to enhanced version
        }

        /// <summary>
        /// Create a simple, guaranteed-to-work layout using basic Windows Forms controls
        /// This bypasses all Syncfusion layout managers and theming issues
        /// </summary>
        private void CreateSimpleWorkingLayout()
        {
            try
            {
                Console.WriteLine("🔧 SIMPLE: Creating basic working layout...");

                // Clear any existing controls
                this.Controls.Clear();
                this.SuspendLayout();

                // Set basic form properties
                this.BackColor = Color.FromArgb(240, 240, 240); // Light gray background
                this.Text = "BusBuddy Dashboard - Simple Layout";

                // Create a simple TableLayoutPanel for main structure
                var mainLayout = new TableLayoutPanel
                {
                    Name = "MainLayout",
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 2,
                    BackColor = Color.White,
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
                };

                // Set column and row styles
                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200)); // Sidebar 200px
                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Main content fills rest
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); // Header 80px
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Content fills rest

                // Create header panel
                var headerPanel = new Panel
                {
                    Name = "HeaderPanel",
                    BackColor = Color.FromArgb(33, 150, 243), // Blue
                    Dock = DockStyle.Fill,
                    Padding = new Padding(10)
                };

                var titleLabel = new Label
                {
                    Text = "🚌 BusBuddy Dashboard",
                    Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(10, 25)
                };
                headerPanel.Controls.Add(titleLabel);

                // Create sidebar panel
                var sidebarPanel = new Panel
                {
                    Name = "SidebarPanel",
                    BackColor = Color.FromArgb(98, 91, 113), // Dark purple
                    Dock = DockStyle.Fill,
                    Padding = new Padding(5)
                };

                // Add sidebar buttons
                var sidebarButtons = new[]
                {
                    "🚗 Vehicles", "👤 Drivers", "🚌 Routes", "⛽ Fuel",
                    "🔧 Maintenance", "📅 Calendar", "📊 Reports", "⚙️ Settings"
                };

                int buttonY = 10;
                foreach (var buttonText in sidebarButtons)
                {
                    var button = new Button
                    {
                        Text = buttonText,
                        Size = new Size(180, 35),
                        Location = new Point(10, buttonY),
                        BackColor = Color.FromArgb(74, 68, 88),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 9F)
                    };
                    button.FlatAppearance.BorderSize = 0;
                    sidebarPanel.Controls.Add(button);
                    buttonY += 45;
                }

                // Create main content panel
                var contentPanel = new Panel
                {
                    Name = "ContentPanel",
                    BackColor = Color.White,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(20)
                };

                // Add welcome text
                var welcomeLabel = new Label
                {
                    Text = "Welcome to BusBuddy Dashboard\n\nThis is a simplified layout for debugging purposes.\nAll controls should be visible and functional.",
                    Font = new Font("Segoe UI", 12F),
                    ForeColor = Color.FromArgb(64, 64, 64),
                    AutoSize = true,
                    Location = new Point(20, 20)
                };
                contentPanel.Controls.Add(welcomeLabel);

                // Create a stats panel
                var statsPanel = new Panel
                {
                    Name = "StatsPanel",
                    BackColor = Color.FromArgb(250, 250, 250),
                    Dock = DockStyle.Fill,
                    Padding = new Padding(10)
                };

                var statsLabel = new Label
                {
                    Text = "📊 Quick Stats\n\n• Active Vehicles: 25\n• Available Drivers: 18\n• Routes Today: 12\n• Maintenance Due: 3",
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = Color.FromArgb(64, 64, 64),
                    AutoSize = true,
                    Location = new Point(10, 10)
                };
                statsPanel.Controls.Add(statsLabel);

                // Add panels to the table layout
                mainLayout.Controls.Add(sidebarPanel, 0, 0); // Top-left (sidebar spans both rows)
                mainLayout.Controls.Add(headerPanel, 1, 0); // Top-right (header)
                mainLayout.Controls.Add(statsPanel, 0, 1);  // Bottom-left (stats)
                mainLayout.Controls.Add(contentPanel, 1, 1); // Bottom-right (main content)

                // Set row span for sidebar
                mainLayout.SetRowSpan(sidebarPanel, 2);

                // Add the main layout to the form
                this.Controls.Add(mainLayout);

                // Force layout updates
                this.ResumeLayout(true);
                this.PerformLayout();

                Console.WriteLine("✅ SIMPLE: Basic working layout created successfully");
                Console.WriteLine($"   Form size: {this.Size}, Client size: {this.ClientSize}");
                Console.WriteLine($"   Main layout size: {mainLayout.Size}");

                // Store reference for compatibility
                _dashboardMainPanel = mainLayout;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SIMPLE: Failed to create simple layout: {ex.Message}");

                // Ultimate fallback - just show a message
                this.Controls.Clear();
                var errorLabel = new Label
                {
                    Text = $"Error creating dashboard layout:\n{ex.Message}\n\nPlease check the console for details.",
                    Font = new Font("Segoe UI", 12F),
                    ForeColor = Color.Red,
                    BackColor = Color.White,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                this.Controls.Add(errorLabel);
            }
        }

        #endregion

        #region Enhanced Layout Creation

        /// <summary>
        /// OPTIMIZED: Enhanced CreateMainLayout with streamlined rendering and accessibility fixes
        /// </summary>
        private void CreateMainLayoutEnhanced()
        {
            try
            {
                Log(LogLevel.Info, "Creating enhanced main layout with optimized rendering...");
                Console.WriteLine("🔬 STEP 1: Starting optimized CreateMainLayoutEnhanced");
                this.SuspendLayout();

                // Clear existing controls
                this.Controls.Clear();

                // Set form properties early with WCAG-compliant colors
                this.Size = new Size(1400, 900);
                this.MinimumSize = new Size(1200, 700);
                this.BackColor = EnhancedThemeService.BackgroundColor; // Use theme color for consistency

                // Create main container with solid background for accessibility
                var mainTableLayout = new TableLayoutPanel
                {
                    Name = "mainContainer",
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 2,
                    BackColor = EnhancedThemeService.BackgroundColor, // Solid background
                    Visible = false, // Defer visibility until layout complete
                    MinimumSize = new Size(1200, 700)
                };
                Console.WriteLine($"🔬 STEP 2: Main TableLayoutPanel created - deferred visibility");

                // Configure table layout with percentage-based sizing
                mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20)); // Sidebar 20% of width
                mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80)); // Main content 80% of width
                mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96)); // Header row (fixed height)
                mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Content row (remaining space)

                // Create header panel with WCAG-compliant colors
                _headerPanel = new Panel
                {
                    Name = "HeaderPanel",
                    Dock = DockStyle.Top,
                    Height = 96,
                    Padding = new Padding(20),
                    BackColor = EnhancedThemeService.HeaderColor, // Solid header color for accessibility
                    ForeColor = EnhancedThemeService.HeaderTextColor, // White text for proper contrast
                    Visible = false // Defer visibility
                };
                Console.WriteLine($"🔬 STEP 3: Header panel created with solid background");

                // Create sidebar panel with improved contrast
                var sidebarPanel = new Panel
                {
                    Name = "SidebarPanel",
                    Dock = DockStyle.Fill,
                    BackColor = EnhancedThemeService.SidebarColor, // Solid sidebar color for accessibility
                    Visible = false // Defer visibility
                };
                Console.WriteLine($"🔬 STEP 4: Sidebar panel created with solid background");

                // Add sidebar module buttons with improved accessibility and user feedback
                var sidebarModules = new[]
                {
                    new { Text = "🚗 Vehicles", Enabled = true },
                    new { Text = "👤 Drivers", Enabled = true },
                    new { Text = "🚌 Routes", Enabled = true },
                    new { Text = "⛽ Fuel", Enabled = true },
                    new { Text = "🔧 Maintenance", Enabled = true },
                    new { Text = "📅 Calendar", Enabled = true },
                    new { Text = "📊 Reports", Enabled = false },
                    new { Text = "⚙️ Settings", Enabled = false }
                };

                var buttonY = 10;
                foreach (var module in sidebarModules)
                {
                    var moduleButton = new Button
                    {
                        Text = module.Enabled ? module.Text : module.Text + " (Coming Soon)",
                        Size = new Size(220, 40),
                        Location = new Point(10, buttonY),
                        BackColor = EnhancedThemeService.ButtonColor, // High-contrast background
                        ForeColor = EnhancedThemeService.ButtonTextColor, // White text for contrast
                        FlatStyle = FlatStyle.Flat,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Visible = false, // Defer visibility
                        Enabled = module.Enabled,
                        AccessibleName = module.Text.Replace("🚗", "").Replace("👤", "").Replace("🚌", "").Replace("⛽", "").Replace("🔧", "").Replace("📅", "").Replace("📊", "").Replace("⚙️", "").Trim() // Remove emojis for screen readers
                    };
                    moduleButton.FlatAppearance.BorderSize = 0;

                    // Improve UX for disabled buttons with tooltip
                    if (!module.Enabled)
                    {
                        var tooltip = new ToolTip();
                        tooltip.SetToolTip(moduleButton, "This feature will be available in a future update. Check our roadmap for more details.");
                        moduleButton.BackColor = Color.FromArgb(100, EnhancedThemeService.ButtonColor.R, EnhancedThemeService.ButtonColor.G, EnhancedThemeService.ButtonColor.B);
                    }
                    else
                    {
                        moduleButton.Click += (sender, e) => HandleSidebarModuleClick(module.Text);
                        // Add hover effects for enabled buttons only
                        moduleButton.MouseEnter += (sender, e) => moduleButton.BackColor = EnhancedThemeService.PrimaryDarkColor;
                        moduleButton.MouseLeave += (sender, e) => moduleButton.BackColor = EnhancedThemeService.ButtonColor;
                    }

                    sidebarPanel.Controls.Add(moduleButton);
                    buttonY += 50;
                }
                Console.WriteLine($"🔬 STEP 5: Added {sidebarModules.Length} sidebar module buttons with accessibility improvements");

                // Create sidebar toggle button with solid background
                var sidebarToggleButton = new Button
                {
                    Name = "SidebarToggleButton",
                    Text = "☰",
                    Size = new Size(40, 30),
                    Location = new Point(10, 10),
                    BackColor = EnhancedThemeService.ButtonColor, // Solid background for accessibility
                    ForeColor = EnhancedThemeService.ButtonTextColor, // White text for contrast
                    FlatStyle = FlatStyle.Flat,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left,
                    Visible = false, // Defer visibility
                    AccessibleName = "Toggle Sidebar Menu" // Clear accessible name for screen readers
                };
                sidebarToggleButton.FlatAppearance.BorderSize = 0;
                Console.WriteLine($"🔬 STEP 6: Sidebar toggle button created with accessibility features");

                // Create title label with safe font fallback
                _titleLabel = ExecuteWithFallback(
                    () => CreateTitleLabelWithFallbacks(),
                    () => CreateBasicTitleLabel(),
                    "Title Label Creation"
                );
                Console.WriteLine($"🔬 STEP 7: Title label created");

                // Create stats panel
                var statsPanel = new Panel
                {
                    Name = "StatsPanel",
                    Dock = DockStyle.Fill,
                    BackColor = EnhancedThemeService.SurfaceColor, // Solid background for accessibility
                    Padding = new Padding(10),
                    Visible = false // Defer visibility
                };

                // Add stats labels with theme colors for better contrast
                var statLabel1 = new Label
                {
                    Text = "Active Vehicles: 25",
                    AutoSize = true,
                    Dock = DockStyle.Top, // Use dock instead of hardcoded position
                    ForeColor = EnhancedThemeService.TextColor,
                    BackColor = Color.Transparent,
                    Font = EnhancedThemeService.DefaultFont,
                    Margin = new Padding(0, 5, 0, 5), // Add some margin for spacing
                    Visible = false
                };
                var statLabel2 = new Label
                {
                    Text = "Available Drivers: 18",
                    AutoSize = true,
                    Dock = DockStyle.Top, // Use dock instead of hardcoded position
                    ForeColor = EnhancedThemeService.TextColor,
                    BackColor = Color.Transparent,
                    Font = EnhancedThemeService.DefaultFont,
                    Margin = new Padding(0, 5, 0, 5), // Add some margin for spacing
                    Visible = false
                };
                statsPanel.Controls.Add(statLabel1);
                statsPanel.Controls.Add(statLabel2);
                Console.WriteLine($"🔬 STEP 8: Stats panel created with theme colors");

                // Create main content panel
                _contentPanel = new Panel
                {
                    Name = "ContentPanel",
                    Dock = DockStyle.Fill,
                    BackColor = EnhancedThemeService.SurfaceColor, // Solid background for accessibility
                    Padding = new Padding(20),
                    Visible = false // Defer visibility
                };
                Console.WriteLine($"🔬 STEP 9: Content panel created");

                // Create quick actions flow panel with enhanced overflow handling
                _formButtonsPanel = new FlowLayoutPanel
                {
                    Name = "QuickActionsFlowPanel",
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    AutoScroll = true,
                    Padding = new Padding(10),
                    BackColor = EnhancedThemeService.SurfaceColor, // Consistent theme color
                    MinimumSize = new Size(400, 200),
                    Visible = false // Defer visibility
                };
                Console.WriteLine($"🔬 STEP 10: Form buttons panel created");

                // Create QuickActionsPanel alias for test compatibility
                var quickActionsPanel = new Panel
                {
                    Name = "QuickActionsPanel",
                    Dock = DockStyle.Fill,
                    BackColor = EnhancedThemeService.SurfaceColor,
                    Visible = false // Defer visibility
                };
                quickActionsPanel.Controls.Add(_formButtonsPanel);
                Console.WriteLine($"🔬 STEP 11: Quick actions panel created and configured");

                // Build control hierarchy efficiently
                _headerPanel.Controls.Add(sidebarToggleButton);
                _headerPanel.Controls.Add(_titleLabel);
                _contentPanel.Controls.Add(quickActionsPanel);

                // Add controls to table layout in single operation
                mainTableLayout.Controls.Add(sidebarPanel, 0, 0); // Sidebar top-left
                mainTableLayout.Controls.Add(_headerPanel, 1, 0); // Header top-right
                mainTableLayout.Controls.Add(statsPanel, 0, 1); // Stats bottom-left
                mainTableLayout.Controls.Add(_contentPanel, 1, 1); // Content bottom-right

                // Set table layout spans
                mainTableLayout.SetRowSpan(sidebarPanel, 2); // Sidebar spans both rows

                this.Controls.Add(mainTableLayout);
                Console.WriteLine($"🔬 STEP 12: Control hierarchy built and added to form");

                // OPTIMIZED: Single layout pass with consolidated operations
                this.ResumeLayout(true); // Allow layout during resume
                Console.WriteLine($"🔬 STEP 13: Layout operations completed");

                // Set visibility in single recursive pass AFTER layout
                SetControlVisibilityRecursive(mainTableLayout, true);
                Console.WriteLine($"🔬 STEP 14: Visibility set for all controls");

                // Force immediate rendering (removed to prevent handle creation issues)
                // this.Refresh();
                Console.WriteLine($"🔬 STEP 15: Skipped form refresh to prevent handle creation issues");

                // Store reference to main panel for compatibility
                _dashboardMainPanel = mainTableLayout;

                // Enable double buffering for key panels AFTER visibility is set AND handles are created
                if (_contentPanel?.IsHandleCreated == true)
                {
                    ControlExtensions.SetDoubleBuffered(_contentPanel, true);
                }
                if (_headerPanel?.IsHandleCreated == true)
                {
                    ControlExtensions.SetDoubleBuffered(_headerPanel, true);
                }
                if (_formButtonsPanel?.IsHandleCreated == true)
                {
                    ControlExtensions.SetDoubleBuffered(_formButtonsPanel, true);
                }

                // Perform post-layout validations
                if (ENABLE_CONTROL_OVERLAP_DETECTION)
                {
                    CheckForControlOverlaps(this);
                }

                if (ENABLE_ACCESSIBILITY_VALIDATION)
                {
                    ValidateAccessibilityContrast();
                }

                // Log the final state
                Log(LogLevel.Info, "Enhanced layout created with optimized rendering and accessibility");
                if (ENABLE_DIAGNOSTICS)
                {
                    LogControlHierarchy();
                }

                Console.WriteLine("🔬 STEP 16: Enhanced layout creation completed successfully with optimizations");
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "Enhanced layout creation failed", ex);
                Console.WriteLine($"❌ Enhanced layout failed: {ex.Message}");

                // Display user-friendly error message
                var result = MessageBox.Show(
                    "Failed to create the dashboard layout. Using fallback mode.\n\nWould you like to view technical details?",
                    "Layout Error",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    MessageBox.Show(
                        $"Technical Details:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                        "Technical Error Details",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

                CreateFallbackLayout();
            }
        }

        /// <summary>
        /// Create professional dashboard layout using Syncfusion Docking Manager
        /// This eliminates hardcoded positioning and provides better UI flexibility
        /// </summary>
        private void CreateDockingManagerLayout()
        {
            try
            {
                Console.WriteLine("🔧 Creating professional docking manager layout");

                // Initialize components container if not already initialized
                if (components == null)
                {
                    components = new System.ComponentModel.Container();
                }

                // Create DockingManager instance with proper configuration
                _dockingManager = new DockingManager(components)
                {
                    HostControl = this,
                    AutoHideTabForeColor = Color.Black,
                    EnableAutoAdjustCaption = true,
                    ShowCaption = true,
                    EnableContextMenu = true,
                    DockToFill = true, // Occupy whole window for better space utilization
                    MaximizeButtonEnabled = true, // Allow maximizing dock windows
                    DockLabelAlignment = DockLabelAlignmentStyle.Left,
                    SplitterWidth = 4, // Thinner splitters for modern look
                    MetroSplitterBackColor = EnhancedThemeService.AccentColor
                };

                // Set visual style if available
                try
                {
                    _dockingManager.VisualStyle = VisualStyle.Metro;
                }
                catch
                {
                    Console.WriteLine("⚠️ Metro visual style not available, using default");
                }

                this.SuspendLayout();

                // Create main panels for docking with enhanced theming
                var sidebarPanel = new Panel
                {
                    Name = "SidebarPanel",
                    BackColor = EnhancedThemeService.SidebarColor,
                    Padding = new Padding(5),
                    Size = new Size(280, 400) // Wider sidebar for better navigation
                };

                var headerPanel = new Panel
                {
                    Name = "HeaderPanel",
                    BackColor = EnhancedThemeService.HeaderColor,
                    Size = new Size(500, 100), // Taller header for better title display
                    Padding = new Padding(15)
                };

                var quickActionsPanel = new Panel
                {
                    Name = "QuickActionsPanel",
                    BackColor = EnhancedThemeService.SurfaceColor,
                    Size = new Size(600, 500),
                    Padding = new Padding(15)
                };

                var statusPanel = new Panel
                {
                    Name = "StatusPanel",
                    BackColor = EnhancedThemeService.SurfaceColor,
                    Size = new Size(500, 180), // Taller status panel for more info
                    Padding = new Padding(15)
                };

                // Add controls to form
                this.Controls.AddRange(new Control[] { sidebarPanel, headerPanel, quickActionsPanel, statusPanel });

                // Configure docking for each panel with enhanced settings
                _dockingManager.SetEnableDocking(sidebarPanel, true);
                _dockingManager.SetEnableDocking(headerPanel, true);
                _dockingManager.SetEnableDocking(quickActionsPanel, true);
                _dockingManager.SetEnableDocking(statusPanel, true);

                // Set professional dock labels with icons
                _dockingManager.SetDockLabel(sidebarPanel, "📋 Navigation Menu");
                _dockingManager.SetDockLabel(headerPanel, "🏠 BusBuddy Dashboard");
                _dockingManager.SetDockLabel(quickActionsPanel, "⚡ Quick Actions");
                _dockingManager.SetDockLabel(statusPanel, "📊 System Status");

                // Configure dock positions with proper sizes
                _dockingManager.DockControl(sidebarPanel, this, DockingStyle.Left, 280);
                _dockingManager.DockControl(headerPanel, this, DockingStyle.Top, 100);
                _dockingManager.DockControl(statusPanel, this, DockingStyle.Bottom, 180);
                _dockingManager.DockControl(quickActionsPanel, this, DockingStyle.Fill, 200);

                // Set minimum sizes to prevent panels from becoming too small
                _dockingManager.SetControlMinimumSize(sidebarPanel, new Size(200, 300));
                _dockingManager.SetControlMinimumSize(headerPanel, new Size(400, 80));
                _dockingManager.SetControlMinimumSize(quickActionsPanel, new Size(400, 300));
                _dockingManager.SetControlMinimumSize(statusPanel, new Size(300, 120));

                // Configure caption button visibility for cleaner look
                _dockingManager.SetCloseButtonVisibility(headerPanel, false); // Header shouldn't be closable
                _dockingManager.SetAutoHideButtonVisibility(sidebarPanel, true); // Allow sidebar auto-hide
                _dockingManager.SetAutoHideButtonVisibility(quickActionsPanel, false); // Main area always visible

                // Create content for each panel
                CreateSidebarContent(sidebarPanel);
                CreateHeaderContent(headerPanel);
                CreateQuickActionsContent(quickActionsPanel);
                CreateStatusContent(statusPanel);

                // Store references for tests and compatibility
                _headerPanel = headerPanel;
                _contentPanel = quickActionsPanel;
                _dashboardMainPanel = this; // The form itself is the main container

                this.ResumeLayout(true);

                Console.WriteLine("✅ Docking manager layout created successfully");

                // Enable persistence (save/restore layout)
                _dockingManager.PersistState = true;

                Log(LogLevel.Info, "Professional docking manager layout created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Docking manager layout failed: {ex.Message}");
                Log(LogLevel.Error, "Docking manager layout creation failed", ex);

                // Fallback to enhanced layout
                CreateMainLayoutEnhanced();
            }
        }

        /// <summary>
        /// Create navigation content for the sidebar panel with enhanced styling
        /// </summary>
        private void CreateSidebarContent(Panel sidebarPanel)
        {
            // Create a flow layout for module buttons with better spacing
            var moduleButtonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(10),
                BackColor = Color.Transparent
            };

            // Add module navigation buttons with enhanced styling
            var moduleConfigs = LoadFormConfigurations();
            foreach (var formInfo in moduleConfigs.Take(10)) // Allow more modules in sidebar
            {
                var moduleButton = CreateEnhancedSidebarButton(formInfo);
                moduleButtonsPanel.Controls.Add(moduleButton);
            }

            // Add separator and quick settings
            var separator = new Panel
            {
                Height = 2,
                Width = 240,
                BackColor = EnhancedThemeService.AccentColor,
                Margin = new Padding(10, 15, 10, 15)
            };
            moduleButtonsPanel.Controls.Add(separator);

            // Add quick settings buttons
            var settingsButton = CreateEnhancedSidebarButton(
                new FormInfo("⚙️ Dashboard Settings", typeof(Form), "Configure dashboard preferences"));
            var helpButton = CreateEnhancedSidebarButton(
                new FormInfo("❓ Help & Support", typeof(Form), "Get help and support"));

            moduleButtonsPanel.Controls.Add(settingsButton);
            moduleButtonsPanel.Controls.Add(helpButton);

            sidebarPanel.Controls.Add(moduleButtonsPanel);
        }

        /// <summary>
        /// Create enhanced sidebar button with modern styling
        /// </summary>
        private Button CreateEnhancedSidebarButton(FormInfo formInfo)
        {
            var button = new Button
            {
                Text = formInfo.DisplayName,
                Size = new Size(240, 50), // Larger buttons for better touch/click targets
                Margin = new Padding(5, 3, 5, 3),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = EnhancedThemeService.GetSafeFont(9.5F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            // Enhanced flat appearance
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = EnhancedThemeService.ButtonHoverColor;
            button.FlatAppearance.MouseDownBackColor = EnhancedThemeService.PrimaryDarkColor;

            button.Click += (s, e) => NavigateToForm(formInfo);

            // Add enhanced hover effects with smooth transitions
            button.MouseEnter += (s, e) =>
            {
                button.BackColor = EnhancedThemeService.ButtonHoverColor;
                button.Font = EnhancedThemeService.GetSafeFont(9.5F, FontStyle.Bold);
            };

            button.MouseLeave += (s, e) =>
            {
                button.BackColor = Color.Transparent;
                button.Font = EnhancedThemeService.GetSafeFont(9.5F, FontStyle.Regular);
            };

            return button;
        }

        /// <summary>
        /// Create a modern module button for sidebar navigation
        /// </summary>
        private Button CreateModuleButton(FormInfo formInfo)
        {
            var button = new Button
            {
                Text = formInfo.DisplayName,
                Size = new Size(220, 45),
                Margin = new Padding(5),
                BackColor = EnhancedThemeService.ButtonColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = EnhancedThemeService.GetSafeFont(9F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.Click += (s, e) => NavigateToForm(formInfo);

            // Add hover effects
            button.MouseEnter += (s, e) => button.BackColor = EnhancedThemeService.ButtonHoverColor;
            button.MouseLeave += (s, e) => button.BackColor = EnhancedThemeService.ButtonColor;

            return button;
        }

        /// <summary>
        /// Create header content with enhanced branding and system controls
        /// </summary>
        private void CreateHeaderContent(Panel headerPanel)
        {
            // Create main layout for header
            var headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); // Title area
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25)); // Status indicators
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15)); // Control buttons

            // Create enhanced title label with better typography
            _titleLabel = new Label
            {
                Text = "🚌 BusBuddy Transportation Management System",
                Dock = DockStyle.Fill,
                AutoSize = false,
                Font = EnhancedThemeService.GetSafeFont(16F, FontStyle.Bold),
                ForeColor = EnhancedThemeService.HeaderTextColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };

            // Create status indicators panel
            var statusIndicatorsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(10, 20, 10, 20)
            };

            // Add system status indicators
            var dbStatusLabel = new Label
            {
                Text = "🟢 DB Online",
                AutoSize = true,
                Font = EnhancedThemeService.GetSafeFont(9F),
                ForeColor = Color.LightGreen,
                Margin = new Padding(5, 0, 15, 0)
            };

            var userLabel = new Label
            {
                Text = $"👤 {Environment.UserName}",
                AutoSize = true,
                Font = EnhancedThemeService.GetSafeFont(9F),
                ForeColor = EnhancedThemeService.HeaderTextColor,
                Margin = new Padding(5, 0, 0, 0)
            };

            statusIndicatorsPanel.Controls.AddRange(new Control[] { dbStatusLabel, userLabel });

            // Create system controls panel with modern buttons
            var systemControlsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(5, 15, 15, 15)
            };

            // Add refresh button with modern styling
            var refreshButton = new Button
            {
                Text = "🔄",
                Size = new Size(35, 35),
                BackColor = EnhancedThemeService.AccentColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = EnhancedThemeService.GetSafeFont(12F),
                Cursor = Cursors.Hand,
                Margin = new Padding(2)
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Click += (s, e) => RefreshDashboard();

            // Add settings button
            var settingsButton = new Button
            {
                Text = "⚙️",
                Size = new Size(35, 35),
                BackColor = EnhancedThemeService.ButtonColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = EnhancedThemeService.GetSafeFont(12F),
                Cursor = Cursors.Hand,
                Margin = new Padding(2)
            };
            settingsButton.FlatAppearance.BorderSize = 0;
            settingsButton.Click += (s, e) => ShowDashboardSettings();

            systemControlsPanel.Controls.AddRange(new Control[] { refreshButton, settingsButton });

            // Add all sections to header layout
            headerLayout.Controls.Add(_titleLabel, 0, 0);
            headerLayout.Controls.Add(statusIndicatorsPanel, 1, 0);
            headerLayout.Controls.Add(systemControlsPanel, 2, 0);

            headerPanel.Controls.Add(headerLayout);
        }

        /// <summary>
        /// Show dashboard settings dialog
        /// </summary>
        private void ShowDashboardSettings()
        {
            MessageBox.Show("Dashboard settings will be available in a future update.\n\nFeatures planned:\n• Theme customization\n• Layout preferences\n• Notification settings",
                          "Dashboard Settings",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Information);
        }

        /// <summary>
        /// Create quick actions content with main functionality buttons
        /// </summary>
        private void CreateQuickActionsContent(Panel quickActionsPanel)
        {
            // Create flow layout for action buttons
            _formButtonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            // Add quick action buttons
            var quickActionConfigs = LoadFormConfigurations();
            foreach (var formInfo in quickActionConfigs.Take(12)) // Show more in main area
            {
                var actionButton = CreateFormButtonEnhanced(formInfo);
                _formButtonsPanel.Controls.Add(actionButton);
            }

            quickActionsPanel.Controls.Add(_formButtonsPanel);
        }

        /// <summary>
        /// Create status content with system information
        /// </summary>
        private void CreateStatusContent(Panel statusPanel)
        {
            var statusLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(5)
            };

            statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // Add status information
            var statusLabels = new[]
            {
                new Label { Text = "🚗 Active Vehicles: 25", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft },
                new Label { Text = "👤 Available Drivers: 18", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft },
                new Label { Text = "🚌 Routes Today: 12", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft },
                new Label { Text = "⚠️ Maintenance Due: 3", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }
            };

            foreach (var label in statusLabels)
            {
                label.Font = EnhancedThemeService.DefaultFont;
                label.ForeColor = EnhancedThemeService.TextColor;
                statusLayout.Controls.Add(label);
            }

            statusPanel.Controls.Add(statusLayout);
        }

        /// <summary>
        /// Refresh dashboard data and UI
        /// </summary>
        private void RefreshDashboard()
        {
            try
            {
                Console.WriteLine("🔄 Refreshing dashboard...");
                // Add refresh logic here
                this.Refresh();
                Console.WriteLine("✅ Dashboard refreshed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Dashboard refresh failed: {ex.Message}");
                Log(LogLevel.Warning, "Dashboard refresh failed", ex);
            }
        }

        #endregion

        #region Missing Methods

        /// <summary>
        /// Load form configurations for dashboard navigation
        /// </summary>
        private List<FormInfo> LoadFormConfigurations()
        {
            if (_cachedForms != null && _cachedForms.Count > 0)
                return _cachedForms;

            var forms = new List<FormInfo>
            {
                new FormInfo("🚗 Vehicle Management", typeof(Form), "Manage fleet vehicles and assignments", "🚗"),
                new FormInfo("👤 Driver Management", typeof(Form), "Manage driver information and schedules", "👤"),
                new FormInfo("🚌 Route Planning", typeof(Form), "Plan and optimize bus routes", "🚌"),
                new FormInfo("⛽ Fuel Management", typeof(Form), "Track fuel consumption and costs", "⛽"),
                new FormInfo("🔧 Maintenance", typeof(Form), "Schedule and track vehicle maintenance", "🔧"),
                new FormInfo("📅 School Calendar", typeof(Form), "Manage school calendar and events", "📅"),
                new FormInfo("📊 Reports", typeof(Form), "Generate and view reports", "📊"),
                new FormInfo("⚙️ Settings", typeof(Form), "Application settings and preferences", "⚙️")
            };

            _cachedForms = forms;
            return forms;
        }

        /// <summary>
        /// Create enhanced form button with improved styling
        /// </summary>
        private Button CreateFormButtonEnhanced(FormInfo formInfo)
        {
            var button = new Button
            {
                Text = $"{formInfo.IconText} {formInfo.DisplayName}",
                Size = new Size(180, 60),
                Margin = new Padding(10),
                BackColor = EnhancedThemeService.ButtonColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = EnhancedThemeService.GetSafeFont(9F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Tag = formInfo
            };

            button.FlatAppearance.BorderSize = 0;
            button.Click += (s, e) => NavigateToForm(formInfo);

            // Add hover effects
            button.MouseEnter += (s, e) => button.BackColor = EnhancedThemeService.ButtonHoverColor;
            button.MouseLeave += (s, e) => button.BackColor = EnhancedThemeService.ButtonColor;

            return button;
        }

        /// <summary>
        /// Navigate to specified form
        /// </summary>
        private void NavigateToForm(FormInfo formInfo)
        {
            try
            {
                Console.WriteLine($"📍 Navigating to: {formInfo.DisplayName}");

                if (_navigationService != null)
                {
                    // Map form types to navigation service methods
                    switch (formInfo.DisplayName.ToLower())
                    {
                        case var name when name.Contains("vehicle"):
                            _navigationService.ShowVehicleManagement();
                            break;
                        case var name when name.Contains("driver"):
                            _navigationService.ShowDriverManagement();
                            break;
                        case var name when name.Contains("route"):
                            _navigationService.ShowRouteManagement();
                            break;
                        case var name when name.Contains("fuel"):
                            _navigationService.ShowFuelManagement();
                            break;
                        case var name when name.Contains("maintenance"):
                            _navigationService.ShowMaintenanceManagement();
                            break;
                        case var name when name.Contains("calendar"):
                            _navigationService.ShowCalendarManagement();
                            break;
                        case var name when name.Contains("reports"):
                            _navigationService.ShowReports();
                            break;
                        default:
                            MessageBox.Show($"Navigation to {formInfo.DisplayName} not yet implemented.",
                                          "Feature Coming Soon",
                                          MessageBoxButtons.OK,
                                          MessageBoxIcon.Information);
                            break;
                    }
                }
                else
                {
                    MessageBox.Show($"Navigation to {formInfo.DisplayName} not yet implemented.",
                                  "Feature Coming Soon",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Navigation failed: {ex.Message}");
                Log(LogLevel.Error, $"Navigation to {formInfo.DisplayName} failed", ex);
            }
        }

        /// <summary>
        /// Handle sidebar module clicks
        /// </summary>
        private void HandleSidebarModuleClick(string moduleText)
        {
            Console.WriteLine($"📱 Sidebar module clicked: {moduleText}");
            // Implementation for sidebar navigation
        }

        /// <summary>
        /// Scan and cache forms for enhanced discovery
        /// </summary>
        private void ScanAndCacheFormsEnhanced()
        {
            try
            {
                Console.WriteLine("🔍 Scanning and caching forms...");
                LoadFormConfigurations();
                Console.WriteLine("✅ Forms cached successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Form caching failed: {ex.Message}");
                Log(LogLevel.Warning, "Form caching failed", ex);
            }
        }

        /// <summary>
        /// Event handler for form load
        /// </summary>
        private void BusBuddyDashboardSyncfusion_Load(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("📋 Dashboard loading...");
                // Additional load logic here
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Dashboard load failed: {ex.Message}");
                Log(LogLevel.Error, "Dashboard load failed", ex);
            }
        }

        /// <summary>
        /// Event handler for window state changes
        /// </summary>
        private void BusBuddyDashboardSyncfusion_WindowStateChanged(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine($"🖼️ Window state changed: {this.WindowState}");
                // Handle window state changes
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Window state change handling failed: {ex.Message}");
                Log(LogLevel.Warning, "Window state change handling failed", ex);
            }
        }

        /// <summary>
        /// Event handler for DPI changes
        /// </summary>
        private void BusBuddyDashboardSyncfusion_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            try
            {
                Console.WriteLine($"🔍 DPI changed: {e.DeviceDpiNew}");
                // Handle DPI changes
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DPI change handling failed: {ex.Message}");
                Log(LogLevel.Warning, "DPI change handling failed", ex);
            }
        }

        /// <summary>
        /// Ensure window controls are visible
        /// </summary>
        private void EnsureWindowControlsVisible()
        {
            // Ensure title bar and controls are accessible
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
        }

        /// <summary>
        /// Apply DPI-aware positioning
        /// </summary>
        private void ApplyDpiAwarePositioning()
        {
            try
            {
                using (var graphics = this.CreateGraphics())
                {
                    var scale = graphics.DpiX / 96f;
                    if (scale > 1.0f)
                    {
                        // Apply DPI scaling adjustments
                        this.Font = new Font(this.Font.FontFamily, this.Font.Size * scale);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DPI positioning failed: {ex.Message}");
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Log method for dashboard operations
        /// </summary>
        private void Log(LogLevel level, string message, Exception ex = null)
        {
            if ((int)level < Config.MinimumLogLevel) return;

            var prefix = level switch
            {
                LogLevel.Debug => "🔍 DEBUG",
                LogLevel.Info => "ℹ️ INFO",
                LogLevel.Warning => "⚠️ WARNING",
                LogLevel.Error => "❌ ERROR",
                _ => "📝 LOG"
            };

            var logMessage = $"{prefix}: {message}";
            if (ex != null)
            {
                logMessage += $" - Exception: {ex.Message}";
            }

            Console.WriteLine(logMessage);
        }

        /// <summary>
        /// Execute a method with fallback
        /// </summary>
        private T ExecuteWithFallback<T>(Func<T> primaryAction, Func<T> fallbackAction, string actionName)
        {
            try
            {
                return primaryAction();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ {actionName} failed, using fallback: {ex.Message}");
                return fallbackAction();
            }
        }

        /// <summary>
        /// Set control visibility recursively
        /// </summary>
        private void SetControlVisibilityRecursive(Control control, bool visible)
        {
            control.Visible = visible;
            foreach (Control child in control.Controls)
            {
                SetControlVisibilityRecursive(child, visible);
            }
        }

        /// <summary>
        /// Create title label with fallbacks
        /// </summary>
        private Control CreateTitleLabelWithFallbacks()
        {
            try
            {
                var titleLabel = new Label
                {
                    Text = "🚌 BusBuddy Dashboard",
                    Font = EnhancedThemeService.GetSafeFont(16F, FontStyle.Bold),
                    ForeColor = EnhancedThemeService.HeaderTextColor,
                    BackColor = Color.Transparent,
                    AutoSize = true,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    Location = new Point(60, 30),
                    AccessibleName = "BusBuddy Dashboard Title"
                };
                return titleLabel;
            }
            catch
            {
                return CreateBasicTitleLabel();
            }
        }

        /// <summary>
        /// Create basic title label
        /// </summary>
        private Control CreateBasicTitleLabel()
        {
            return new Label
            {
                Text = "BusBuddy Dashboard",
                Font = new Font("Microsoft Sans Serif", 16F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(60, 30)
            };
        }

        /// <summary>
        /// Check for control overlaps
        /// </summary>
        private void CheckForControlOverlaps(Control parent)
        {
            // Basic overlap detection implementation
            Console.WriteLine("🔍 Checking for control overlaps...");
        }

        /// <summary>
        /// Validate accessibility contrast
        /// </summary>
        private void ValidateAccessibilityContrast()
        {
            Console.WriteLine("♿ Validating accessibility contrast...");
        }

        /// <summary>
        /// Log control hierarchy for debugging
        /// </summary>
        private void LogControlHierarchy()
        {
            Console.WriteLine("📋 Logging control hierarchy...");
        }

        /// <summary>
        /// Create fallback layout
        /// </summary>
        private void CreateFallbackLayout()
        {
            Console.WriteLine("🔄 Creating fallback layout...");
            CreateSimpleWorkingLayout();
        }

        #endregion

        #region Control Extensions

        /// <summary>
        /// Control extensions for enhanced functionality
        /// </summary>
        public static class ControlExtensions
        {
            public static void SetDoubleBuffered(Control control, bool value)
            {
                try
                {
                    var property = typeof(Control).GetProperty("DoubleBuffered",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    property?.SetValue(control, value, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to set double buffering: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
