using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using BusBuddy.UI.Services;
using BusBuddy.UI.Base;
using BusBuddy.Business;
using BusBuddy.UI.Views;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Gauge;

namespace BusBuddy.UI.Views
{
    public partial class BusBuddyDashboardSyncfusion : SyncfusionBaseForm
    {
        private readonly INavigationService _navigationService;
        private readonly IDatabaseHelperService _databaseHelperService;
        private List<FormInfo> _cachedForms;
        private Control? _dashboardMainPanel;
        private Panel? _headerPanel;
        private Control _titleLabel;
        private Panel? _contentPanel;
        private FlowLayoutPanel _formButtonsPanel;
        private Panel? _statsPanel;

        private static readonly bool ENABLE_DIAGNOSTICS = bool.Parse(Environment.GetEnvironmentVariable("BUSBUDDY_DIAGNOSTICS") ?? "true");
        private static readonly bool USE_ENHANCED_LAYOUT = true;
        private static readonly bool USE_SIMPLE_WORKING_LAYOUT = false;
        private static readonly bool USE_ENHANCED_FORM_DISCOVERY = true;
        private static readonly bool ENABLE_DPI_LOGGING = bool.Parse(Environment.GetEnvironmentVariable("BUSBUDDY_DPI_LOGGING") ?? "true");
        private static readonly bool ENABLE_PERFORMANCE_CACHING = true;
        private static readonly bool ENABLE_ACCESSIBILITY_VALIDATION = true;
        private static readonly bool ENABLE_CONTROL_OVERLAP_DETECTION = true;

        private static readonly Dictionary<string, Type> _formTypeCache = new Dictionary<string, Type>();
        private static readonly object _cacheInitLock = new object();
        private static readonly string CACHE_FILE = "form_cache.json";
        private static bool _syncfusionRenderingValidated = false;

        public BusBuddyDashboardSyncfusion(INavigationService navigationService, IDatabaseHelperService databaseHelperService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _databaseHelperService = databaseHelperService ?? throw new ArgumentNullException(nameof(databaseHelperService));

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

            this.Load += BusBuddyDashboardSyncfusion_Load;
            this.Resize += BusBuddyDashboardSyncfusion_WindowStateChanged;
            this.DpiChanged += BusBuddyDashboardSyncfusion_DpiChanged;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            if (USE_ENHANCED_FORM_DISCOVERY)
                ScanAndCacheFormsEnhanced();
            else
                _cachedForms = new List<FormInfo>();

            if (USE_SIMPLE_WORKING_LAYOUT)
                CreateSimpleWorkingLayout();
            else if (USE_ENHANCED_LAYOUT)
                try { CreateMainLayoutEnhanced(); }
                catch (Exception ex) { Log(LogLevel.Error, "Enhanced layout failed", ex); CreateSimpleWorkingLayout(); }
            else
                CreateMainLayout();
        }

        #region Initialization Methods

        private void InitializeComponent()
        {
            this.Text = "BusBuddy Dashboard";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1200, 700);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            if (ENABLE_DPI_LOGGING)
                LogDpiInformation();
        }

        private void ConfigureWindow()
        {
            this.Text = "BusBuddy Dashboard";
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.ControlBox = true;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;

            EnsureWindowControlsVisible();
            ApplyDpiAwarePositioning();
        }

        private void LogDpiInformation()
        {
            try
            {
                using (var graphics = this.CreateGraphics())
                {
                    var dpiX = graphics.DpiX;
                    var dpiY = graphics.DpiY;
                    var scale = dpiX / 96f;
                    Console.WriteLine($"🔍 DPI DIAGNOSTICS: DPI X: {dpiX}, DPI Y: {dpiY}, Scale: {scale:F2}x, High DPI: {scale > 1.25f}, Screen: {Screen.PrimaryScreen.Bounds}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DPI Logging failed: {ex.Message}");
            }
        }

        private void CreateMainLayout()
        {
            Console.WriteLine("⚠️ Using original layout - redirecting to enhanced");
            CreateMainLayoutEnhanced();
        }

        private void CreateSimpleWorkingLayout()
        {
            try
            {
                Console.WriteLine("🔧 SIMPLE: Creating basic working layout...");
                this.Controls.Clear();
                this.SuspendLayout();

                this.BackColor = Color.FromArgb(240, 240, 240);
                this.Text = "BusBuddy Dashboard - Simple Layout";

                var mainLayout = new TableLayoutPanel
                {
                    Name = "MainLayout",
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 2,
                    BackColor = Color.White,
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
                };

                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

                var headerPanel = new Panel
                {
                    Name = "HeaderPanel",
                    BackColor = EnhancedThemeService.HeaderColor,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(10)
                };

                var titleLabel = new Label
                {
                    Text = "🚌 BusBuddy Dashboard",
                    Font = EnhancedThemeService.HeaderFont,
                    ForeColor = EnhancedThemeService.HeaderTextColor,
                    AutoSize = true,
                    Location = new Point(10, 25)
                };
                headerPanel.Controls.Add(titleLabel);

                var sidebarPanel = new Panel
                {
                    Name = "SidebarPanel",
                    BackColor = EnhancedThemeService.SidebarColor,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(5)
                };

                var sidebarButtons = new[] { "🚗 Vehicles", "👤 Drivers", "🚌 Routes", "⛽ Fuel", "🔧 Maintenance", "📅 Calendar", "📊 Reports", "⚙️ Settings" };
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
                        Font = EnhancedThemeService.ButtonFont
                    };
                    button.FlatAppearance.BorderSize = 0;
                    sidebarPanel.Controls.Add(button);
                    buttonY += 45;
                }

                var contentPanel = new Panel
                {
                    Name = "ContentPanel",
                    BackColor = Color.White,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(20)
                };

                var welcomeLabel = new Label
                {
                    Text = "Welcome to BusBuddy Dashboard\n\nThis is a simplified layout for debugging.",
                    Font = EnhancedThemeService.DefaultFont,
                    ForeColor = EnhancedThemeService.TextColor,
                    AutoSize = true,
                    Location = new Point(20, 20)
                };
                contentPanel.Controls.Add(welcomeLabel);

                _statsPanel = new Panel
                {
                    Name = "StatsPanel",
                    BackColor = EnhancedThemeService.BackgroundColor,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(10)
                };

                var statsLabel = new Label
                {
                    Text = "📊 Quick Stats\n\n• Active Vehicles: 25\n• Available Drivers: 18\n• Routes Today: 12\n• Maintenance Due: 3",
                    Font = EnhancedThemeService.DefaultFont,
                    ForeColor = EnhancedThemeService.TextColor,
                    AutoSize = true,
                    Location = new Point(10, 10)
                };
                _statsPanel.Controls.Add(statsLabel);

                mainLayout.Controls.Add(sidebarPanel, 0, 0);
                mainLayout.Controls.Add(headerPanel, 1, 0);
                mainLayout.Controls.Add(_statsPanel, 0, 1);
                mainLayout.Controls.Add(contentPanel, 1, 1);
                mainLayout.SetRowSpan(sidebarPanel, 2);

                this.Controls.Add(mainLayout);
                this.ResumeLayout(true);
                this.PerformLayout();

                Console.WriteLine($"✅ SIMPLE: Layout created. Form size: {this.Size}, Client size: {this.ClientSize}");
                _dashboardMainPanel = mainLayout;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SIMPLE: Layout failed: {ex.Message}");
                this.Controls.Clear();
                var errorLabel = new Label
                {
                    Text = $"Error creating layout:\n{ex.Message}\n\nCheck console.",
                    Font = EnhancedThemeService.DefaultFont,
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

        private void CreateMainLayoutEnhanced()
        {
            try
            {
                Log(LogLevel.Info, "Creating enhanced main layout...");
                this.SuspendLayout();
                this.Controls.Clear();

                this.Size = new Size(1400, 900);
                this.MinimumSize = new Size(1200, 700);

                var mainTableLayout = new TableLayoutPanel
                {
                    Name = "mainContainer",
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 2,
                    BackColor = EnhancedThemeService.BackgroundColor,
                    Visible = true,
                    MinimumSize = new Size(1200, 700)
                };

                ControlExtensions.SetDoubleBuffered(mainTableLayout, true);
                mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
                mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));
                mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
                mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

                _headerPanel = new Panel
                {
                    Name = "HeaderPanel",
                    Dock = DockStyle.Top,
                    Height = 96,
                    Padding = new Padding(20),
                    BackColor = EnhancedThemeService.HeaderColor,
                    ForeColor = EnhancedThemeService.HeaderTextColor,
                    Visible = true
                };

                var sidebarPanel = new Panel
                {
                    Name = "SidebarPanel",
                    Dock = DockStyle.Fill,
                    BackColor = EnhancedThemeService.SidebarColor,
                    Visible = true
                };

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

                int buttonY = 10;
                foreach (var module in sidebarModules)
                {
                    var moduleButton = new Button
                    {
                        Text = module.Enabled ? module.Text : module.Text + " (Coming Soon)",
                        Size = new Size(220, 40),
                        Location = new Point(10, buttonY),
                        BackColor = EnhancedThemeService.ButtonColor,
                        ForeColor = EnhancedThemeService.ButtonTextColor,
                        FlatStyle = FlatStyle.Flat,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Visible = true,
                        Enabled = module.Enabled
                    };
                    moduleButton.FlatAppearance.BorderSize = 0;
                    if (module.Enabled)
                        moduleButton.Click += (sender, e) => HandleSidebarModuleClick(module.Text);
                    moduleButton.MouseEnter += (sender, e) => moduleButton.BackColor = EnhancedThemeService.PrimaryDarkColor;
                    moduleButton.MouseLeave += (sender, e) => moduleButton.BackColor = EnhancedThemeService.ButtonColor;
                    sidebarPanel.Controls.Add(moduleButton);
                    buttonY += 50;
                }

                var sidebarToggleButton = new Button
                {
                    Name = "SidebarToggleButton",
                    Text = "☰",
                    Size = new Size(40, 30),
                    Location = new Point(10, 10),
                    BackColor = EnhancedThemeService.ButtonColor,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = EnhancedThemeService.ButtonTextColor,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left,
                    Visible = true
                };
                sidebarToggleButton.FlatAppearance.BorderSize = 0;

                _titleLabel = ExecuteWithFallback(
                    () => CreateTitleLabelWithFallbacks(),
                    () => CreateBasicTitleLabel(),
                    "Title Label Creation"
                );

                _statsPanel = new Panel
                {
                    Name = "StatsPanel",
                    Dock = DockStyle.Fill,
                    BackColor = EnhancedThemeService.BackgroundColor,
                    Padding = new Padding(10),
                    Visible = true
                };

                // Add ChartControl for vehicle usage (Windows Forms)
                var vehicleChart = new ChartControl
                {
                    Name = "VehicleUsageChart",
                    Size = new Size(300, 200),
                    Location = new Point(10, 10),
                    Visible = true,
                    Text = "Vehicle Usage",
                    BackColor = EnhancedThemeService.BackgroundColor
                };

                // Create series with sample data
                var series = new ChartSeries("Vehicle Usage", ChartSeriesType.Column);
                series.Points.Add(1, 20); // Monday
                series.Points.Add(2, 22); // Tuesday
                series.Points.Add(3, 18); // Wednesday
                series.Points.Add(4, 25); // Thursday
                series.Points.Add(5, 23); // Friday
                series.Text = "Vehicles in Use";
                vehicleChart.Series.Add(series);
                _statsPanel.Controls.Add(vehicleChart);

                // Add RadialGauge for maintenance status (Windows Forms)
                var maintenanceGauge = new RadialGauge
                {
                    Name = "MaintenanceGauge",
                    Size = new Size(200, 200),
                    Location = new Point(320, 10),
                    Visible = true,
                    Value = 75,
                    MinimumValue = 0,
                    MaximumValue = 100,
                    MajorDifference = 20,
                    MinorDifference = 5,
                    BackColor = EnhancedThemeService.BackgroundColor
                };
                _statsPanel.Controls.Add(maintenanceGauge);

                // Add cost analytics panel
                AddCostAnalyticsToStatsPanel();

                _contentPanel = new Panel
                {
                    Name = "ContentPanel",
                    Dock = DockStyle.Fill,
                    BackColor = EnhancedThemeService.BackgroundColor,
                    Padding = new Padding(20),
                    Visible = true
                };

                _formButtonsPanel = new FlowLayoutPanel
                {
                    Name = "QuickActionsFlowPanel",
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    AutoScroll = true,
                    Padding = new Padding(10),
                    BackColor = EnhancedThemeService.BackgroundColor,
                    MinimumSize = new Size(400, 200),
                    Visible = true
                };

                var quickActionsPanel = new Panel
                {
                    Name = "QuickActionsPanel",
                    Dock = DockStyle.Fill,
                    BackColor = EnhancedThemeService.BackgroundColor,
                    Visible = true
                };
                quickActionsPanel.Controls.Add(_formButtonsPanel);

                _headerPanel.Controls.Add(sidebarToggleButton);
                _headerPanel.Controls.Add(_titleLabel);
                _contentPanel.Controls.Add(quickActionsPanel);

                mainTableLayout.Controls.Add(sidebarPanel, 0, 0);
                mainTableLayout.Controls.Add(_headerPanel, 1, 0);
                mainTableLayout.Controls.Add(_statsPanel, 0, 1);
                mainTableLayout.Controls.Add(_contentPanel, 1, 1);
                mainTableLayout.SetRowSpan(sidebarPanel, 2);

                this.Controls.Add(mainTableLayout);
                this.ResumeLayout(false);
                mainTableLayout.PerformLayout();
                this.PerformLayout();

                SetControlVisibility(mainTableLayout, true);
                mainTableLayout.Refresh();
                this.Refresh();

                Application.DoEvents();

                _dashboardMainPanel = mainTableLayout;
                ControlExtensions.SetDoubleBuffered(_contentPanel, true);
                ControlExtensions.SetDoubleBuffered(_headerPanel, true);
                ControlExtensions.SetDoubleBuffered(_formButtonsPanel, true);
                ControlExtensions.SetDoubleBuffered(_statsPanel, true);

                if (ENABLE_CONTROL_OVERLAP_DETECTION)
                    CheckForControlOverlaps(this);

                if (ENABLE_ACCESSIBILITY_VALIDATION)
                    ValidateAccessibilityContrast();

                Log(LogLevel.Info, "Enhanced layout created successfully");
                if (ENABLE_DIAGNOSTICS)
                    LogControlHierarchy();
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "Enhanced layout creation failed", ex);
                var result = MessageBox.Show("Failed to create dashboard layout. View details?", "Layout Error", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                    MessageBox.Show($"Error: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CreateFallbackLayout();
            }
        }

        private Control CreateBasicTitleLabel()
        {
            Log(LogLevel.Warning, "Creating basic title label");
            return new Label
            {
                Text = "🚌 BusBuddy Dashboard",
                Location = new Point(60, 25),
                ForeColor = EnhancedThemeService.HeaderTextColor,
                AutoSize = true,
                Font = EnhancedThemeService.HeaderFont,
                BackColor = Color.Transparent,
                Visible = true
            };
        }

        private Control CreateTitleLabelWithFallbacks()
        {
            Log(LogLevel.Info, "Creating title label");
            return ControlFactory.CreateLabel(
                "🚌 BusBuddy Dashboard",
                EnhancedThemeService.HeaderFont,
                EnhancedThemeService.HeaderTextColor,
                true
            );
        }

        private Font GetSafeFontWithFallback(string preferredFontName, float size, FontStyle style)
        {
            try
            {
                var font = new Font(preferredFontName, size, style);
                Console.WriteLine($"✅ Font: {font.FontFamily.Name}");
                return font;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Font '{preferredFontName}' failed: {ex.Message}");
                string[] fallbackFonts = { "Arial", "Microsoft Sans Serif", "Tahoma" };
                foreach (var fallbackFont in fallbackFonts)
                {
                    try
                    {
                        var font = new Font(fallbackFont, size, style);
                        Console.WriteLine($"✅ Fallback font: {font.FontFamily.Name}");
                        return font;
                    }
                    catch { }
                }
                Console.WriteLine("⚠️ Using default font");
                return SystemFonts.DefaultFont;
            }
        }

        private void AddCostAnalyticsToStatsPanel()
        {
            try
            {
                if (_statsPanel == null) return;

                // Create cost analytics panel
                var costAnalyticsPanel = new Panel
                {
                    Name = "CostAnalyticsPanel",
                    Size = new Size(250, 180),
                    Location = new Point(530, 10),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Visible = true
                };

                // Title label
                var titleLabel = new Label
                {
                    Text = "💰 Cost Per Student",
                    Font = EnhancedThemeService.GetSafeFont(12, FontStyle.Bold),
                    ForeColor = EnhancedThemeService.HeaderColor,
                    Location = new Point(10, 10),
                    Size = new Size(230, 25),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                costAnalyticsPanel.Controls.Add(titleLabel);

                // Loading label (will be replaced with actual data)
                var loadingLabel = new Label
                {
                    Text = "Loading cost data...",
                    Font = EnhancedThemeService.GetSafeFont(9),
                    ForeColor = Color.Gray,
                    Location = new Point(10, 45),
                    Size = new Size(230, 120),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                costAnalyticsPanel.Controls.Add(loadingLabel);

                _statsPanel.Controls.Add(costAnalyticsPanel);

                // Load actual cost data asynchronously
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var routeAnalyticsService = new RouteAnalyticsService();
                        var endDate = DateTime.Now;
                        var startDate = endDate.AddDays(-30); // Last 30 days

                        var costMetrics = await routeAnalyticsService.CalculateCostPerStudentMetricsAsync(startDate, endDate);

                        // Update UI on main thread
                        this.Invoke(new System.Action(() =>
                        {
                            costAnalyticsPanel.Controls.Remove(loadingLabel);

                            var metricsText = $"📊 Last 30 Days\n\n" +
                                            $"Route Cost/Student/Day:\n${costMetrics.RouteCostPerStudentPerDay:F2}\n\n" +
                                            $"Sports Cost/Student:\n${costMetrics.SportsCostPerStudent:F2}\n\n" +
                                            $"Field Trip Cost/Student:\n${costMetrics.FieldTripCostPerStudent:F2}";

                            var metricsLabel = new Label
                            {
                                Text = metricsText,
                                Font = EnhancedThemeService.GetSafeFont(9),
                                ForeColor = Color.Black,
                                Location = new Point(10, 45),
                                Size = new Size(230, 120),
                                TextAlign = ContentAlignment.TopLeft
                            };
                            costAnalyticsPanel.Controls.Add(metricsLabel);
                        }));
                    }
                    catch (Exception ex)
                    {
                        this.Invoke(new System.Action(() =>
                        {
                            loadingLabel.Text = $"Error loading cost data:\n{ex.Message}";
                            loadingLabel.ForeColor = Color.Red;
                        }));
                    }
                });
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "Failed to add cost analytics panel", ex);
            }
        }

        private void CreateFallbackLayout()
        {
            Console.WriteLine("🆘 FALLBACK: Creating emergency layout");
            Log(LogLevel.Warning, "Creating fallback layout");

            this.Controls.Clear();
            var fallbackPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightGray,
                Padding = new Padding(20)
            };

            var statusMessage = _syncfusionRenderingValidated ?
                "BusBuddy Dashboard - Layout Failed\nUsing simplified layout." :
                "BusBuddy Dashboard - Fallback Mode\nSyncfusion controls unavailable.";

            var fallbackLabel = new Label
            {
                Text = statusMessage,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Black,
                BackColor = Color.White,
                Font = EnhancedThemeService.DefaultFont,
                BorderStyle = BorderStyle.FixedSingle
            };

            var navPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = EnhancedThemeService.HeaderColor
            };

            var titleLabel = new Label
            {
                Text = "🚌 BusBuddy - Safe Mode",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = EnhancedThemeService.HeaderTextColor,
                Font = EnhancedThemeService.HeaderFont
            };

            navPanel.Controls.Add(titleLabel);
            fallbackPanel.Controls.Add(fallbackLabel);
            this.Controls.Add(navPanel);
            this.Controls.Add(fallbackPanel);

            Console.WriteLine("✅ FALLBACK: Emergency layout created");
        }

        #endregion

        #region Enhanced Form Discovery

        private void ScanAndCacheFormsEnhanced()
        {
            _cachedForms = new List<FormInfo>();
            try
            {
                Console.WriteLine("🔍 SCAN: Discovering Syncfusion forms...");
                if (ENABLE_PERFORMANCE_CACHING && LoadFormsFromCache())
                {
                    Console.WriteLine($"📋 Loaded {_cachedForms.Count} forms from cache");
                    return;
                }

                IEnumerable<Type> syncfusionFormTypes;
                lock (_cacheInitLock)
                {
                    if (_formTypeCache.Count == 0)
                    {
                        Console.WriteLine("📋 Initializing form cache...");
                        var assembly = Assembly.GetExecutingAssembly();
                        var types = assembly.GetTypes()
                            .Where(type => type.Namespace == "BusBuddy.UI.Views" &&
                                          type.Name.EndsWith("Syncfusion") &&
                                          type.IsSubclassOf(typeof(Form)) &&
                                          !type.IsAbstract &&
                                          type != typeof(BusBuddyDashboardSyncfusion))
                            .ToList();
                        foreach (var type in types)
                            _formTypeCache[type.Name] = type;
                        Console.WriteLine($"📋 Cached {_formTypeCache.Count} form types");
                    }
                }
                syncfusionFormTypes = _formTypeCache.Values;

                foreach (var formType in syncfusionFormTypes)
                {
                    try
                    {
                        var formInfo = CreateFormInfoFromType(formType);
                        _cachedForms.Add(formInfo);
                        Console.WriteLine($"✅ Added: {formInfo.DisplayName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Failed to process {formType.Name}: {ex.Message}");
                    }
                }

                if (ENABLE_PERFORMANCE_CACHING && _cachedForms.Count > 0)
                    SaveFormsToCache();

                if (_cachedForms.Count == 0)
                {
                    Console.WriteLine("⚠️ Discovery failed, using manual list");
                    AddManualFormList();
                }

                Console.WriteLine($"📊 SCAN: Total forms: {_cachedForms.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SCAN ERROR: {ex.Message}");
                AddManualFormList();
            }
        }

        private bool LoadFormsFromCache()
        {
            try
            {
                if (!File.Exists(CACHE_FILE))
                    return false;
                var json = File.ReadAllText(CACHE_FILE);
                var cachedData = System.Text.Json.JsonSerializer.Deserialize<List<FormInfo>>(json);
                if (cachedData != null && cachedData.Count > 0)
                {
                    _cachedForms = cachedData;
                    Log(LogLevel.Info, $"Loaded {_cachedForms.Count} forms from cache");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log(LogLevel.Warning, "Failed to load cache", ex);
            }
            return false;
        }

        private void SaveFormsToCache()
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(_cachedForms, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(CACHE_FILE, json);
                Log(LogLevel.Debug, $"Saved {_cachedForms.Count} forms to cache");
            }
            catch (Exception ex)
            {
                Log(LogLevel.Warning, "Failed to save cache", ex);
            }
        }

        private FormInfo CreateFormInfoFromType(Type formType)
        {
            var formInfo = new FormInfo
            {
                Name = formType.Name,
                FormTypeName = formType.AssemblyQualifiedName,
                NavigationMethod = MapToNavigationMethod(formType.Name)
            };

            var displayNameAttribute = formType.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
            formInfo.DisplayName = displayNameAttribute != null ? displayNameAttribute.DisplayName : GenerateDisplayName(formType.Name);

            var descriptionAttribute = formType.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
            formInfo.Description = descriptionAttribute != null ? descriptionAttribute.Description : $"Manage {formInfo.DisplayName.ToLower()}";

            return formInfo;
        }

        private string GenerateDisplayName(string typeName)
        {
            var cleanName = typeName.Replace("FormSyncfusion", "").Replace("Form", "");
            var spacedName = Regex.Replace(cleanName, "(?<!^)([A-Z])", " $1");
            var emojiMap = new Dictionary<string, string>
            {
                { "Vehicle", "🚗" }, { "Driver", "👤" }, { "Route", "🚌" }, { "Activity", "🎯" },
                { "Fuel", "⛽" }, { "Maintenance", "🔧" }, { "School Calendar", "📅" }, { "Activity Schedule", "📋" }
            };
            foreach (var kvp in emojiMap)
                if (spacedName.Contains(kvp.Key))
                    return $"{kvp.Value} {spacedName}";
            return spacedName;
        }

        #endregion

        #region Enhanced Button Creation

        private Control CreateFormButtonEnhanced(FormInfo formInfo)
        {
            try
            {
                Button button = null;
                try
                {
                    button = (Button)Activator.CreateInstance(typeof(ButtonAdv));
                    Console.WriteLine($"✅ ButtonAdv for {formInfo.DisplayName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ ButtonAdv failed: {ex.Message}");
                    button = new Button();
                    Console.WriteLine($"✅ Standard Button for {formInfo.DisplayName}");
                }

                button.Text = formInfo.DisplayName;
                button.Size = new Size(220, 80);
                button.Margin = new Padding(10);
                button.BackColor = EnhancedThemeService.ButtonColor;
                button.ForeColor = EnhancedThemeService.ButtonTextColor;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.Font = EnhancedThemeService.ButtonFont;
                button.TextAlign = ContentAlignment.MiddleCenter;
                button.UseVisualStyleBackColor = false;
                button.Cursor = Cursors.Hand;
                button.Visible = true;

                button.MouseEnter += (s, e) => button.BackColor = EnhancedThemeService.PrimaryDarkColor;
                button.MouseLeave += (s, e) => button.BackColor = EnhancedThemeService.ButtonColor;
                button.Click += (s, e) => NavigateToForm(formInfo);

                if (ENABLE_DIAGNOSTICS)
                {
                    Console.WriteLine($"🔘 Button created: {formInfo.DisplayName}, Type: {button.GetType().Name}, Visible: {button.Visible}");
                }

                return button;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Button creation failed: {ex.Message}");
                var fallbackLabel = new Label
                {
                    Text = formInfo.DisplayName,
                    Size = new Size(220, 80),
                    Margin = new Padding(10),
                    BackColor = Color.LightBlue,
                    ForeColor = Color.Black,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = EnhancedThemeService.ButtonFont,
                    Cursor = Cursors.Hand,
                    Visible = true
                };
                fallbackLabel.Click += (s, e) => NavigateToForm(formInfo);
                Console.WriteLine($"🆘 Fallback label: {formInfo.DisplayName}");
                return fallbackLabel;
            }
        }

        #endregion

        #region Diagnostic Methods

        private void LogControlHierarchy()
        {
            Console.WriteLine($"📋 CONTROL HIERARCHY: Form: {this.Text} ({this.Size})");
            LogControlRecursive(this, 1);
        }

        private void LogControlRecursive(Control parent, int depth)
        {
            var indent = new string(' ', depth * 2);
            foreach (Control control in parent.Controls)
            {
                var info = $"{indent}- {control.GetType().Name}";
                if (!string.IsNullOrEmpty(control.Text))
                    info += $": '{control.Text}'";
                info += $" (Visible={control.Visible}, Size={control.Size}, HandleCreated={control.IsHandleCreated})";
                if (control.ForeColor != Color.Empty)
                    info += $" ForeColor={control.ForeColor}";
                if (control.BackColor != Color.Empty)
                    info += $" BackColor={control.BackColor}";
                if (!control.Visible)
                    info += " ⚠️ HIDDEN";
                if (control.Size.Width <= 0 || control.Size.Height <= 0)
                    info += " ⚠️ ZERO_SIZE";
                if (!control.IsHandleCreated)
                    info += " ⚠️ NO_HANDLE";
                Console.WriteLine(info);
                if (control.Controls.Count > 0 && depth < 3)
                    LogControlRecursive(control, depth + 1);
            }
        }

        private void SetControlVisibility(Control parent, bool visible)
        {
            if (parent == null) return;
            parent.Visible = visible;
            Console.WriteLine($"🔬 Setting {parent.GetType().Name} '{parent.Name}' visible = {visible}");
            foreach (Control child in parent.Controls)
                child.Visible = visible;
        }

        #endregion

        #region Helper Methods and Classes

        private class FormInfo
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string Icon { get; set; }
            public string FormTypeName { get; set; }
            public string NavigationMethod { get; set; }
            [System.Text.Json.Serialization.JsonIgnore]
            public Type FormType
            {
                get => !string.IsNullOrEmpty(FormTypeName) ? Type.GetType(FormTypeName) : null;
                set => FormTypeName = value?.AssemblyQualifiedName;
            }
        }

        private void NavigateToForm(FormInfo formInfo)
        {
            try
            {
                Log(LogLevel.Info, $"Navigating to {formInfo.DisplayName}...");
                var method = _navigationService.GetType().GetMethod(formInfo.NavigationMethod);
                if (method != null)
                {
                    Log(LogLevel.Debug, $"Using method: {formInfo.NavigationMethod}");
                    method.Invoke(_navigationService, null);
                }
                else
                {
                    Log(LogLevel.Info, $"Creating form: {formInfo.FormType.Name}");
                    var form = Activator.CreateInstance(formInfo.FormType) as Form;
                    if (form != null)
                    {
                        EnhancedThemeService.ApplyTheme(form);
                        Log(LogLevel.Info, $"Showing {formInfo.DisplayName}");
                        form.ShowDialog(this);
                    }
                    else
                    {
                        Log(LogLevel.Error, $"Failed to create {formInfo.FormType.Name}");
                        MessageBox.Show($"Failed to create {formInfo.DisplayName}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"Failed to open {formInfo.DisplayName}", ex);
                MessageBox.Show($"Failed to open {formInfo.DisplayName}: {ex.Message}", "Navigation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddManualFormList()
        {
            try
            {
                Log(LogLevel.Info, "Adding manual form list");
                var configurations = LoadFormConfigurations();
                foreach (var config in configurations.Where(c => c.IsEnabled))
                {
                    Type formType = null;
                    try
                    {
                        formType = Type.GetType($"BusBuddy.UI.Views.{config.Name}, {Assembly.GetExecutingAssembly().GetName().Name}");
                    }
                    catch
                    {
                        Log(LogLevel.Warning, $"Could not load type for {config.Name}");
                    }
                    var formInfo = new FormInfo
                    {
                        Name = config.Name,
                        DisplayName = config.DisplayName,
                        Description = config.Description,
                        NavigationMethod = config.NavigationMethod,
                        FormType = formType
                    };
                    _cachedForms.Add(formInfo);
                    Log(LogLevel.Debug, $"Added: {config.DisplayName}");
                }
                Log(LogLevel.Info, $"Added {_cachedForms.Count} forms");
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "Failed to add manual list", ex);
            }
        }

        private string MapToNavigationMethod(string formName)
        {
            var mappings = new Dictionary<string, string>
            {
                ["VehicleManagementFormSyncfusion"] = "ShowVehicleManagement",
                ["DriverManagementFormSyncfusion"] = "ShowDriverManagement",
                ["RouteManagementFormSyncfusion"] = "ShowRouteManagement",
                ["ActivityManagementFormSyncfusion"] = "ShowActivityManagement",
                ["FuelManagementFormSyncfusion"] = "ShowFuelManagement",
                ["MaintenanceManagementFormSyncfusion"] = "ShowMaintenanceManagement",
                ["SchoolCalendarManagementFormSyncfusion"] = "ShowSchoolCalendarManagement",
                ["ActivityScheduleManagementFormSyncfusion"] = "ShowActivityScheduleManagement"
            };
            return mappings.GetValueOrDefault(formName, "ShowForm");
        }

        #endregion

        #region Enhanced Error Handling and Logging

        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3
        }

        private static readonly LogLevel CURRENT_LOG_LEVEL = ENABLE_DIAGNOSTICS ? LogLevel.Debug : LogLevel.Warning;

        private static void Log(LogLevel level, string message, Exception ex = null)
        {
            if (level < CURRENT_LOG_LEVEL) return;
            var prefix = level switch
            {
                LogLevel.Debug => "🔍 DEBUG",
                LogLevel.Info => "ℹ️ INFO",
                LogLevel.Warning => "⚠️ WARNING",
                LogLevel.Error => "❌ ERROR",
                _ => "📝 LOG"
            };
            Console.WriteLine($"{prefix}: {message}");
            if (ex != null)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                if (level >= LogLevel.Error)
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                try
                {
                    File.AppendAllText("error.log", $"{DateTime.Now}: {prefix}: {message}\n{ex?.ToString()}\n\n");
                }
                catch { }
            }
        }

        private T ExecuteWithFallback<T>(Func<T> primaryAction, Func<T> fallbackAction, string operationName)
        {
            try
            {
                Log(LogLevel.Debug, $"Executing {operationName}...");
                var result = primaryAction();
                Log(LogLevel.Debug, $"{operationName} completed");
                return result;
            }
            catch (Exception ex)
            {
                Log(LogLevel.Warning, $"{operationName} failed, using fallback", ex);
                try
                {
                    var result = fallbackAction();
                    Log(LogLevel.Info, $"{operationName} fallback succeeded");
                    return result;
                }
                catch (Exception fallbackEx)
                {
                    Log(LogLevel.Error, $"{operationName} fallback failed", fallbackEx);
                    throw new AggregateException($"Both primary and fallback failed for {operationName}", ex, fallbackEx);
                }
            }
        }

        private bool ValidateSyncfusionLicense()
        {
            return true; // Licensing fixed as per user update
        }

        private bool ValidateFontRendering()
        {
            try
            {
                var testFonts = new[] { "Segoe UI", "Arial", "Microsoft Sans Serif" };
                foreach (var fontName in testFonts)
                {
                    using (var font = new Font(fontName, 10F))
                    using (var label = new AutoLabel { Text = "Test", Font = font })
                    {
                        label.CreateControl();
                        if (label.IsHandleCreated)
                        {
                            Log(LogLevel.Debug, $"Font test passed: {fontName}");
                            return true;
                        }
                    }
                }
                Log(LogLevel.Warning, "All font tests failed");
                return false;
            }
            catch (Exception ex)
            {
                Log(LogLevel.Warning, "Font validation failed", ex);
                return false;
            }
        }

        #endregion

        #region Configuration Support

        public class FormConfiguration
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string Icon { get; set; }
            public string NavigationMethod { get; set; }
            public bool IsEnabled { get; set; } = true;
            public int SortOrder { get; set; }
        }

        private List<FormConfiguration> LoadFormConfigurations()
        {
            return new List<FormConfiguration>
            {
                new FormConfiguration { Name = "VehicleManagementFormSyncfusion", DisplayName = "🚗 Vehicle Management", Description = "Manage vehicle fleet", NavigationMethod = "ShowVehicleManagement", SortOrder = 1 },
                new FormConfiguration { Name = "DriverManagementFormSyncfusion", DisplayName = "👤 Driver Management", Description = "Manage drivers", NavigationMethod = "ShowDriverManagement", SortOrder = 2 },
                new FormConfiguration { Name = "RouteManagementFormSyncfusion", DisplayName = "🚌 Route Management", Description = "Manage routes", NavigationMethod = "ShowRouteManagement", SortOrder = 3 },
                new FormConfiguration { Name = "ActivityManagementFormSyncfusion", DisplayName = "🎯 Activity Management", Description = "Manage activities", NavigationMethod = "ShowActivityManagement", SortOrder = 4 },
                new FormConfiguration { Name = "FuelManagementFormSyncfusion", DisplayName = "⛽ Fuel Management", Description = "Manage fuel records", NavigationMethod = "ShowFuelManagement", SortOrder = 5 },
                new FormConfiguration { Name = "MaintenanceManagementFormSyncfusion", DisplayName = "🔧 Maintenance Management", Description = "Manage maintenance records", NavigationMethod = "ShowMaintenanceManagement", SortOrder = 6 },
                new FormConfiguration { Name = "SchoolCalendarManagementFormSyncfusion", DisplayName = "📅 School Calendar", Description = "Manage school calendar", NavigationMethod = "ShowSchoolCalendarManagement", SortOrder = 7 },
                new FormConfiguration { Name = "ActivityScheduleManagementFormSyncfusion", DisplayName = "📋 Activity Schedule", Description = "Manage activity schedules", NavigationMethod = "ShowActivityScheduleManagement", SortOrder = 8 }
            };
        }

        #endregion

        private void PopulateFormButtonsEnhanced()
        {
            try
            {
                if (_formButtonsPanel == null)
                    throw new InvalidOperationException("_formButtonsPanel is null");
                _formButtonsPanel.Controls.Clear();
                Log(LogLevel.Info, $"Populating {_cachedForms.Count} form buttons...");

                var configurations = LoadFormConfigurations();
                var configDict = configurations.ToDictionary(c => c.Name, c => c);
                foreach (var formInfo in _cachedForms.OrderBy(f => configDict.ContainsKey(f.Name) ? configDict[f.Name].SortOrder : int.MaxValue))
                {
                    if (configDict.ContainsKey(formInfo.Name) && !configDict[formInfo.Name].IsEnabled)
                    {
                        Log(LogLevel.Debug, $"Skipping disabled: {formInfo.DisplayName}");
                        continue;
                    }
                    var button = ControlFactory.CreateButton(formInfo.DisplayName, new Size(200, 80), (s, e) => NavigateToForm(formInfo));
                    button.Margin = new Padding(10);
                    button.Cursor = Cursors.Hand;
                    _formButtonsPanel.Controls.Add(button);
                    Log(LogLevel.Debug, $"Added button: {formInfo.DisplayName}");
                }
                Log(LogLevel.Info, $"Added {_formButtonsPanel.Controls.Count} buttons");
                ExecuteWithFallback(
                    () =>
                    {
                        _formButtonsPanel.PerformLayout();
                        _contentPanel?.PerformLayout();
                        _dashboardMainPanel?.PerformLayout();
                        this.PerformLayout();
                        return true;
                    },
                    () => { _formButtonsPanel.Refresh(); return true; },
                    "Layout Refresh"
                );
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "Failed to populate buttons", ex);
                CreateEmergencyButtons();
            }
        }

        private void CreateEmergencyButtons()
        {
            Log(LogLevel.Warning, "Creating emergency buttons");
            if (_formButtonsPanel == null)
            {
                Console.WriteLine("⚠️ _formButtonsPanel null, creating fallback");
                _formButtonsPanel = new FlowLayoutPanel
                {
                    Name = "EmergencyQuickActionsFlowPanel",
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    AutoScroll = true,
                    Padding = new Padding(10),
                    BackColor = EnhancedThemeService.BackgroundColor,
                    MinimumSize = new Size(400, 200),
                    Visible = true
                };
                if (_contentPanel != null)
                    _contentPanel.Controls.Add(_formButtonsPanel);
                else
                    this.Controls.Add(_formButtonsPanel);
            }
            _formButtonsPanel.Controls.Clear();
            var emergencyButton = new Label
            {
                Text = "Dashboard Error\nCheck console",
                Size = new Size(300, 100),
                BackColor = EnhancedThemeService.ErrorColor,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = EnhancedThemeService.DefaultFont,
                BorderStyle = BorderStyle.FixedSingle
            };
            _formButtonsPanel.Controls.Add(emergencyButton);
            Console.WriteLine("✅ Emergency button added");
        }

        private void CompleteInitialization()
        {
            try
            {
                Log(LogLevel.Info, "Completing initialization...");
                PopulateFormButtonsEnhanced();
                Log(LogLevel.Info, "Initialization completed");
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "Initialization failed", ex);
                CreateEmergencyButtons();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CompleteInitialization();
        }

        private void BusBuddyDashboardSyncfusion_Load(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("🚀 Dashboard Load");
                var loadStartTime = DateTime.Now;
                var maxLoadTime = TimeSpan.FromSeconds(10);

                EnsureWindowControlsVisible();
                ApplyDpiAwarePositioning();

                if (_dashboardMainPanel != null && DateTime.Now - loadStartTime < maxLoadTime)
                {
                    SetControlVisibility(_dashboardMainPanel, true);
                    _dashboardMainPanel.Refresh();
                }

                this.Refresh();
                Application.DoEvents();

                this.Focus();
                this.BringToFront();

                if (ENABLE_DIAGNOSTICS && DateTime.Now - loadStartTime < maxLoadTime)
                {
                    Console.WriteLine("🔬 Final hierarchy:");
                    LogControlHierarchy();
                }

                var totalLoadTime = DateTime.Now - loadStartTime;
                Console.WriteLine($"✅ Load completed in {totalLoadTime.TotalMilliseconds:F0}ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Load failed: {ex.Message}");
                Log(LogLevel.Error, "Load failed", ex);
            }
        }

        private void ApplyDpiAwarePositioning()
        {
            try
            {
                using (var graphics = this.CreateGraphics())
                {
                    var dpiScale = graphics.DpiX / 96f;
                    var workingArea = Screen.PrimaryScreen.WorkingArea;
                    var centeredX = workingArea.X + (workingArea.Width - this.Width) / 2;
                    var centeredY = workingArea.Y + (workingArea.Height - this.Height) / 2;
                    centeredX = Math.Max(workingArea.X, Math.Min(centeredX, workingArea.Right - this.Width));
                    centeredY = Math.Max(workingArea.Y, Math.Min(centeredY, workingArea.Bottom - this.Height));
                    this.Location = new Point(centeredX, centeredY);
                    Console.WriteLine($"🎯 DPI positioning: Scale: {dpiScale:F2}x, Location: {this.Location}, Size: {this.Size}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DPI positioning failed: {ex.Message}");
                this.CenterToScreen();
            }
        }

        private void EnsureWindowControlsVisible()
        {
            try
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.ControlBox = true;
                this.MaximizeBox = true;
                this.MinimizeBox = true;
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
                this.Refresh();
                Console.WriteLine($"✅ Window controls: Border: {this.FormBorderStyle}, State: {this.WindowState}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Window controls failed: {ex.Message}");
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            try
            {
                if (keyData == (Keys.Control | Keys.Home))
                {
                    Console.WriteLine("🏠 Reset position (Ctrl+Home)");
                    this.WindowState = FormWindowState.Normal;
                    ApplyDpiAwarePositioning();
                    return true;
                }
                if (keyData == (Keys.Control | Keys.M))
                {
                    Console.WriteLine("🔄 Toggle state (Ctrl+M)");
                    this.WindowState = this.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Shortcut failed: {ex.Message}");
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void HandleSidebarModuleClick(string module)
        {
            try
            {
                Console.WriteLine($"🖱️ Sidebar clicked: {module}");
                var moduleName = module.Substring(module.IndexOf(' ') + 1).Trim();
                string navigationMethod = moduleName.ToLower() switch
                {
                    "vehicles" => "ShowVehicleManagement",
                    "drivers" => "ShowDriverManagement",
                    "routes" => "ShowRouteManagement",
                    "fuel" => "ShowFuelManagement",
                    "maintenance" => "ShowMaintenanceManagement",
                    "calendar" => "ShowSchoolCalendarManagement",
                    "reports" => null,
                    "settings" => null,
                    _ => null
                };
                if (navigationMethod == null)
                {
                    MessageBox.Show($"{moduleName} coming soon!", "Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                var method = _navigationService.GetType().GetMethod(navigationMethod);
                if (method != null)
                {
                    Console.WriteLine($"✅ Navigating: {navigationMethod}");
                    method.Invoke(_navigationService, null);
                }
                else
                {
                    Console.WriteLine($"⚠️ Method not found: {navigationMethod}");
                    MessageBox.Show($"Navigation for '{moduleName}' not available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Sidebar error: {ex.Message}");
                MessageBox.Show($"Error navigating to {module}: {ex.Message}", "Navigation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BusBuddyDashboardSyncfusion_WindowStateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized && this.Location.Y < 0)
            {
                Console.WriteLine("⚠️ Title bar hidden, adjusting");
                this.Location = new Point(this.Location.X, 0);
            }
        }

        private void BusBuddyDashboardSyncfusion_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            try
            {
                using (var graphics = this.CreateGraphics())
                {
                    float currentDpi = graphics.DpiX;
                    Log(LogLevel.Info, $"DPI changed: {currentDpi:F0}");
                    var scaleFactor = currentDpi / 96f;
                    this.AutoScaleDimensions = new SizeF(currentDpi, currentDpi);
                    _dashboardMainPanel?.PerformLayout();
                    _contentPanel?.PerformLayout();
                    _headerPanel?.PerformLayout();
                    _titleLabel?.PerformLayout();
                    _formButtonsPanel?.PerformLayout();
                    _statsPanel?.PerformLayout();
                    if (ENABLE_CONTROL_OVERLAP_DETECTION)
                        CheckForControlOverlaps(this);
                    if (ENABLE_ACCESSIBILITY_VALIDATION)
                        ValidateAccessibilityContrast();
                    Log(LogLevel.Info, "DPI handling completed");
                }
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "DPI change failed", ex);
            }
        }

        private void CheckForControlOverlaps(Control parent)
        {
            try
            {
                if (parent?.Controls == null || parent.Controls.Count < 2)
                    return;
                for (int i = 0; i < parent.Controls.Count; i++)
                {
                    for (int j = i + 1; j < parent.Controls.Count; j++)
                    {
                        var control1 = parent.Controls[i];
                        var control2 = parent.Controls[j];
                        if (control1.Bounds.IntersectsWith(control2.Bounds) && control1.Visible && control2.Visible &&
                            !string.IsNullOrEmpty(control1.Name) && !string.IsNullOrEmpty(control2.Name))
                        {
                            Log(LogLevel.Warning, $"Overlap: {control1.Name} and {control2.Name}");
                            if (parent is FlowLayoutPanel flowPanel)
                            {
                                flowPanel.PerformLayout();
                                Log(LogLevel.Info, "Resolved via FlowLayoutPanel");
                            }
                        }
                    }
                }
                foreach (Control control in parent.Controls)
                    if (control.HasChildren)
                        CheckForControlOverlaps(control);
            }
            catch (Exception ex)
            {
                Log(LogLevel.Warning, "Overlap detection failed", ex);
            }
        }

        private void ValidateAccessibilityContrast()
        {
            try
            {
                var controls = new List<Control>();
                CollectAllControls(this, controls);
                foreach (var control in controls)
                {
                    if (control.ForeColor != Color.Empty && control.BackColor != Color.Empty)
                    {
                        if (!IsAccessibleContrast(control.ForeColor, control.BackColor))
                        {
                            Log(LogLevel.Warning, $"Low contrast: {control.Name ?? control.GetType().Name} (Fore={control.ForeColor.Name}, Back={control.BackColor.Name})");
                            control.ForeColor = control.BackColor.GetBrightness() > 0.5 ? Color.Black : Color.White;
                        }
                    }
                }
                Log(LogLevel.Info, $"Accessibility validated for {controls.Count} controls");
            }
            catch (Exception ex)
            {
                Log(LogLevel.Warning, "Accessibility validation failed", ex);
            }
        }

        private bool IsAccessibleContrast(Color foreground, Color background)
        {
            double GetLuminance(Color c)
            {
                double GetLinearRGB(double value)
                {
                    value /= 255.0;
                    return value <= 0.03928 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
                }
                var r = GetLinearRGB(c.R);
                var g = GetLinearRGB(c.G);
                var b = GetLinearRGB(c.B);
                return 0.2126 * r + 0.7152 * g + 0.0722 * b;
            }
            double l1 = GetLuminance(foreground);
            double l2 = GetLuminance(background);
            double contrast = (Math.Max(l1, l2) + 0.05) / (Math.Min(l1, l2) + 0.05);
            if (contrast < 4.5)
                Log(LogLevel.Warning, $"Contrast ratio {contrast:F2} < 4.5: Fore={foreground.Name}, Back={background.Name}");
            return contrast >= 4.5;
        }

        private void CollectAllControls(Control parent, List<Control> collection)
        {
            if (parent == null) return;
            collection.Add(parent);
            foreach (Control child in parent.Controls)
                CollectAllControls(child, collection);
        }
    }

    public static class ControlExtensions
    {
        public static void SetDoubleBuffered(Control control, bool value)
        {
            try
            {
                typeof(Control).InvokeMember("DoubleBuffered",
                    BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, control, new object[] { value });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to set double buffering on {control.GetType().Name}: {ex.Message}");
            }
        }
    }
}
