using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BusBuddy.UI.Services;
using BusBuddy.UI.Helpers;
using BusBuddy.Business;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Enhanced BusBuddy Dashboard with comprehensive "no rendered text" fixes
    /// Features: Diagnostic testing, multiple fallback strategies, enhanced error handling
    /// </summary>
    public partial class BusBuddyDashboardSyncfusionFixed : Form
    {
        private readonly INavigationService _navigationService;
        private readonly IDatabaseHelperService _databaseHelperService;
        private List<FormInfo> _cachedForms;
        private Panel? _mainPanel;
        private Panel? _headerPanel;
        private Control _titleLabel; // Changed to Control to support both AutoLabel and Label
        private Panel? _contentPanel;
        private FlowLayoutPanel _formButtonsPanel;

        // Configuration flags - make these configurable for production
        private static readonly bool ENABLE_DIAGNOSTICS = true;
        private static readonly bool USE_ENHANCED_LAYOUT = true;
        private static readonly bool USE_ENHANCED_FORM_DISCOVERY = true;
        private static readonly bool ENABLE_DPI_LOGGING = true;
        private static readonly bool ENABLE_PERFORMANCE_CACHING = true;

        // Performance optimization: Static form cache to avoid repeated reflection
        private static readonly Dictionary<string, Type> _formTypeCache = new Dictionary<string, Type>();
        private static readonly object _cacheInitLock = new object();

        public BusBuddyDashboardSyncfusionFixed(INavigationService navigationService, IDatabaseHelperService databaseHelperService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _databaseHelperService = databaseHelperService ?? throw new ArgumentNullException(nameof(databaseHelperService));

            InitializeComponent();
            ConfigureWindow();

            // Choose form discovery strategy
            if (USE_ENHANCED_FORM_DISCOVERY)
            {
                ScanAndCacheFormsEnhanced();
            }
            else
            {
                ScanAndCacheForms();
            }

            // Choose layout strategy
            if (USE_ENHANCED_LAYOUT)
            {
                Console.WriteLine("🔬 Using ENHANCED layout with fallbacks and diagnostics");
                CreateMainLayoutEnhanced();
            }
            else
            {
                Console.WriteLine("🔧 Using ORIGINAL layout");
                CreateMainLayout();
            }
        }

        #region Initialization Methods

        private void InitializeComponent()
        {
            this.Text = "BusBuddy Dashboard - Enhanced";
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
            this.Text = "BusBuddy Dashboard - Enhanced";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;
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
        /// Original form scanning method for fallback compatibility
        /// </summary>
        private void ScanAndCacheForms()
        {
            Console.WriteLine("⚠️ Using original form scanning - consider switching to enhanced discovery");
            AddManualFormList(); // Use manual list as fallback
        }

        #endregion

        #region Enhanced Layout Creation

        /// <summary>
        /// PROPOSED FIX: Enhanced CreateMainLayout with better error handling and fallbacks
        /// </summary>
        private void CreateMainLayoutEnhanced()
        {
            try
            {
                Log(LogLevel.Info, "Creating enhanced main layout with fallbacks...");
                this.SuspendLayout();

                // Validate Syncfusion license first
                var licenseValid = ValidateSyncfusionLicense();
                if (!licenseValid)
                {
                    Log(LogLevel.Warning, "Syncfusion license invalid - some features may not work correctly");
                }

                // Create main panel with explicit background
                _mainPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(20),
                    BackColor = ColorTranslator.FromHtml("#FAFAFA") // Explicit color instead of theme reference
                };

                // Create header panel with explicit primary color
                _headerPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 96,
                    Padding = new Padding(20),
                    BackColor = ColorTranslator.FromHtml("#2196F3") // Explicit blue instead of theme reference
                };

                // Create title label with multiple fallbacks using enhanced error handling
                _titleLabel = ExecuteWithFallback(
                    () => CreateTitleLabelWithFallbacks(),
                    () => CreateBasicTitleLabel(),
                    "Title Label Creation"
                );

                // Set location after creation
                _titleLabel.Location = new Point(24, 25);

                // Create content panel
                _contentPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(20),
                    BackColor = ColorTranslator.FromHtml("#FAFAFA") // Explicit color
                };

                // Create form buttons panel with explicit styling
                _formButtonsPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    AutoScroll = true,
                    Padding = new Padding(10),
                    BackColor = ColorTranslator.FromHtml("#FAFAFA") // Explicit color
                };

                // Add controls to hierarchy
                _headerPanel.Controls.Add(_titleLabel);
                _contentPanel.Controls.Add(_formButtonsPanel);
                _mainPanel.Controls.Add(_headerPanel);
                _mainPanel.Controls.Add(_contentPanel);
                this.Controls.Add(_mainPanel);

                // Force immediate layout and refresh
                _titleLabel.PerformLayout();
                _headerPanel.PerformLayout();
                _contentPanel.PerformLayout();
                _mainPanel.PerformLayout();
                this.PerformLayout();

                // Log the final state
                Log(LogLevel.Info, "Enhanced layout created with explicit colors and fallbacks");
                if (ENABLE_DIAGNOSTICS)
                {
                    LogControlHierarchy();
                }

                this.ResumeLayout(false);
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "Enhanced layout creation failed", ex);
                CreateFallbackLayout();
            }
        }

        /// <summary>
        /// Create a basic title label as last resort fallback
        /// </summary>
        private Control CreateBasicTitleLabel()
        {
            Log(LogLevel.Warning, "Creating basic title label as emergency fallback");
            var label = new Label
            {
                Text = "BusBuddy Dashboard",
                Location = new Point(24, 25),
                ForeColor = Color.White,
                AutoSize = true,
                Font = new Font("Arial", 18F, FontStyle.Bold)
            };
            return label;
        }        /// <summary>
        /// Create title label with multiple fallback strategies using factory
        /// </summary>
        private Control CreateTitleLabelWithFallbacks()
        {
            return ControlFactory.CreateLabel(
                "🚌 BusBuddy Dashboard",
                EnhancedThemeService.HeaderFont,
                Color.White,
                true
            );
        }

        /// <summary>
        /// Get a safe font with fallback options
        /// </summary>
        private Font GetSafeFontWithFallback(string preferredFontName, float size, FontStyle style)
        {
            try
            {
                // Try preferred font
                var font = new Font(preferredFontName, size, style);
                Console.WriteLine($"✅ Using font: {font.FontFamily.Name}");
                return font;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Preferred font '{preferredFontName}' failed: {ex.Message}");

                // Try common fallbacks
                string[] fallbackFonts = { "Arial", "Microsoft Sans Serif", "Tahoma" };

                foreach (var fallbackFont in fallbackFonts)
                {
                    try
                    {
                        var font = new Font(fallbackFont, size, style);
                        Console.WriteLine($"✅ Using fallback font: {font.FontFamily.Name}");
                        return font;
                    }
                    catch (Exception fallbackEx)
                    {
                        Console.WriteLine($"⚠️ Fallback font '{fallbackFont}' failed: {fallbackEx.Message}");
                    }
                }

                // Last resort: system default
                Console.WriteLine("⚠️ Using system default font");
                return SystemFonts.DefaultFont;
            }
        }

        /// <summary>
        /// Create a simple fallback layout if all else fails
        /// </summary>
        private void CreateFallbackLayout()
        {
            Console.WriteLine("🆘 FALLBACK LAYOUT: Creating emergency layout");

            this.Controls.Clear();

            var fallbackPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightGray
            };

            var fallbackLabel = new Label
            {
                Text = "BusBuddy Dashboard - Fallback Mode\\nSome display issues detected. Check console for details.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Black,
                BackColor = Color.White,
                Font = new Font("Arial", 12F, FontStyle.Bold)
            };

            fallbackPanel.Controls.Add(fallbackLabel);
            this.Controls.Add(fallbackPanel);

            Console.WriteLine("✅ FALLBACK LAYOUT: Emergency layout created");
        }

        #endregion

        #region Enhanced Form Discovery

        /// <summary>
        /// REFACTORING: Dynamic form discovery instead of hardcoded lists
        /// </summary>
        private void ScanAndCacheFormsEnhanced()
        {
            _cachedForms = new List<FormInfo>();
            try
            {
                Console.WriteLine("🔍 ENHANCED SCAN: Dynamically discovering Syncfusion forms with caching...");

                // Use cached types if available for performance
                IEnumerable<Type> syncfusionFormTypes;

                if (ENABLE_PERFORMANCE_CACHING && _formTypeCache.Count > 0)
                {
                    Console.WriteLine("📋 Using cached form types for improved performance");
                    syncfusionFormTypes = _formTypeCache.Values;
                }
                else
                {
                    // Thread-safe cache initialization
                    lock (_cacheInitLock)
                    {
                        if (_formTypeCache.Count == 0)
                        {
                            Console.WriteLine("📋 Initializing form type cache...");
                            var assembly = Assembly.GetExecutingAssembly();
                            var types = assembly.GetTypes()
                                .Where(type => type.Name.EndsWith("Syncfusion") &&
                                              type.IsSubclassOf(typeof(Form)) &&
                                              !type.IsAbstract &&
                                              type != typeof(BusBuddyDashboardSyncfusionFixed)) // Exclude self
                                .ToList();

                            foreach (var type in types)
                            {
                                _formTypeCache[type.Name] = type;
                            }

                            Console.WriteLine($"📋 Cached {_formTypeCache.Count} form types");
                        }
                    }
                    syncfusionFormTypes = _formTypeCache.Values;
                }

                Console.WriteLine($"📋 Processing {syncfusionFormTypes.Count()} Syncfusion form types");

                foreach (var formType in syncfusionFormTypes)
                {
                    try
                    {
                        var formInfo = CreateFormInfoFromType(formType);
                        _cachedForms.Add(formInfo);
                        Console.WriteLine($"   ✅ Added: {formInfo.DisplayName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ❌ Failed to process {formType.Name}: {ex.Message}");
                    }
                }

                // Fallback to manual list if dynamic discovery fails
                if (_cachedForms.Count == 0)
                {
                    Console.WriteLine("⚠️ Dynamic discovery failed, using manual fallback list");
                    AddManualFormList();
                }

                Console.WriteLine($"📊 ENHANCED SCAN: Total forms available: {_cachedForms.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ENHANCED SCAN ERROR: {ex.Message}");
                AddManualFormList(); // Fallback to original method
            }
        }

        /// <summary>
        /// Create FormInfo from Type using reflection and attributes
        /// </summary>
        private FormInfo CreateFormInfoFromType(Type formType)
        {
            var formInfo = new FormInfo
            {
                Name = formType.Name,
                FormType = formType,
                NavigationMethod = MapToNavigationMethod(formType.Name)
            };

            // Try to get display name from attributes or generate from type name
            var displayNameAttribute = formType.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>();
            if (displayNameAttribute != null)
            {
                formInfo.DisplayName = displayNameAttribute.DisplayName;
            }
            else
            {
                formInfo.DisplayName = GenerateDisplayName(formType.Name);
            }

            // Try to get description from attributes or generate
            var descriptionAttribute = formType.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
            if (descriptionAttribute != null)
            {
                formInfo.Description = descriptionAttribute.Description;
            }
            else
            {
                formInfo.Description = $"Manage {formInfo.DisplayName.ToLower()}";
            }

            return formInfo;
        }

        /// <summary>
        /// Generate user-friendly display name from type name
        /// </summary>
        private string GenerateDisplayName(string typeName)
        {
            // Remove "FormSyncfusion" suffix
            var cleanName = typeName.Replace("FormSyncfusion", "").Replace("Form", "");

            // Add spaces before capital letters
            var spacedName = Regex.Replace(cleanName, "(?<!^)([A-Z])", " $1");

            // Add appropriate emoji
            var emojiMap = new Dictionary<string, string>
            {
                { "Vehicle", "🚗" },
                { "Driver", "👤" },
                { "Route", "🚌" },
                { "Activity", "🎯" },
                { "Fuel", "⛽" },
                { "Maintenance", "🔧" },
                { "School Calendar", "📅" },
                { "Activity Schedule", "📋" }
            };

            foreach (var kvp in emojiMap)
            {
                if (spacedName.Contains(kvp.Key))
                {
                    return $"{kvp.Value} {spacedName}";
                }
            }

            return spacedName;
        }

        #endregion

        #region Enhanced Button Creation

        /// <summary>
        /// REFACTORING: Enhanced button creation with better theming and error handling
        /// </summary>
        private Control CreateFormButtonEnhanced(FormInfo formInfo)
        {
            try
            {
                // Try Syncfusion ButtonAdv first, fallback to standard Button
                Button button = null;

                try
                {
                    // Attempt to create Syncfusion ButtonAdv if available
                    var buttonAdvType = typeof(ButtonAdv);
                    button = (Button)Activator.CreateInstance(buttonAdvType);
                    Console.WriteLine($"✅ Using Syncfusion ButtonAdv for {formInfo.DisplayName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ ButtonAdv failed for {formInfo.DisplayName}: {ex.Message}");
                    button = new Button();
                    Console.WriteLine($"✅ Using standard Button for {formInfo.DisplayName}");
                }

                // Configure button with explicit colors and safe font
                button.Text = formInfo.DisplayName;
                button.Size = new Size(220, 120);
                button.Margin = new Padding(10);
                button.BackColor = ColorTranslator.FromHtml("#2196F3"); // Explicit primary blue
                button.ForeColor = Color.White;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.Font = EnhancedThemeService.GetSafeFont(10F, FontStyle.Bold);
                button.TextAlign = ContentAlignment.MiddleCenter;
                button.UseVisualStyleBackColor = false;
                button.Cursor = Cursors.Hand;

                // Add hover effects with safe color transitions
                button.MouseEnter += (s, e) => {
                    button.BackColor = ColorTranslator.FromHtml("#1976D2"); // Darker blue
                };
                button.MouseLeave += (s, e) => {
                    button.BackColor = ColorTranslator.FromHtml("#2196F3"); // Original blue
                };

                // Add click handler
                button.Click += (s, e) => NavigateToForm(formInfo);

                // Log button creation if diagnostics enabled
                if (ENABLE_DIAGNOSTICS)
                {
                    Console.WriteLine($"🔘 Enhanced button created for: {formInfo.DisplayName}");
                    Console.WriteLine($"   Type: {button.GetType().Name}");
                    Console.WriteLine($"   Colors: ForeColor={button.ForeColor}, BackColor={button.BackColor}");
                    Console.WriteLine($"   Font: {button.Font}");
                }

                return button;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Enhanced button creation failed for {formInfo.DisplayName}: {ex.Message}");

                // Emergency fallback: create a simple label
                var fallbackLabel = new Label
                {
                    Text = formInfo.DisplayName,
                    Size = new Size(220, 120),
                    Margin = new Padding(10),
                    BackColor = Color.LightBlue,
                    ForeColor = Color.Black,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Arial", 10F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };

                fallbackLabel.Click += (s, e) => NavigateToForm(formInfo);

                Console.WriteLine($"🆘 Emergency fallback label created for: {formInfo.DisplayName}");
                return fallbackLabel;
            }
        }

        #endregion

        #region Diagnostic Methods

        /// <summary>
        /// Log the complete control hierarchy for debugging
        /// </summary>
        private void LogControlHierarchy()
        {
            Console.WriteLine("📋 CONTROL HIERARCHY:");
            Console.WriteLine($"  Form: {this.Text} ({this.Size})");
            LogControlRecursive(this, 1);
        }

        /// <summary>
        /// Recursively log control hierarchy
        /// </summary>
        private void LogControlRecursive(Control parent, int depth)
        {
            var indent = new string(' ', depth * 2);

            foreach (Control control in parent.Controls)
            {
                var info = $"{indent}- {control.GetType().Name}";

                if (!string.IsNullOrEmpty(control.Text))
                    info += $": '{control.Text}'";

                info += $" (Visible={control.Visible}, Size={control.Size})";

                if (control.ForeColor != Color.Empty)
                    info += $" ForeColor={control.ForeColor}";

                if (control.BackColor != Color.Empty)
                    info += $" BackColor={control.BackColor}";

                Console.WriteLine(info);

                if (control.Controls.Count > 0 && depth < 3) // Limit depth
                {
                    LogControlRecursive(control, depth + 1);
                }
            }
        }

        #endregion

        #region Helper Methods and Classes

        private class FormInfo
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string Icon { get; set; }
            public Type FormType { get; set; }
            public string NavigationMethod { get; set; }
        }

        /// <summary>
        /// Navigate to the specified form with enhanced error handling
        /// </summary>
        private void NavigateToForm(FormInfo formInfo)
        {
            try
            {
                Log(LogLevel.Info, $"Navigating to {formInfo.DisplayName}...");

                // Try navigation service first
                var method = _navigationService.GetType().GetMethod(formInfo.NavigationMethod);
                if (method != null)
                {
                    Log(LogLevel.Debug, $"Using navigation service method: {formInfo.NavigationMethod}");
                    method.Invoke(_navigationService, null);
                }
                else
                {
                    // Fallback: Create form directly
                    Log(LogLevel.Info, $"Creating form directly: {formInfo.FormType.Name}");
                    var form = Activator.CreateInstance(formInfo.FormType) as Form;
                    if (form != null)
                    {
                        // Apply consistent theming
                        EnhancedThemeService.ApplyTheme(form);
                        Log(LogLevel.Info, $"Showing {formInfo.DisplayName}");
                        form.ShowDialog(this);
                    }
                    else
                    {
                        Log(LogLevel.Error, $"Failed to create instance of {formInfo.FormType.Name}");
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
            // Implementation would include the manual form list as fallback
            // This ensures the application works even if dynamic discovery fails
            try
            {
                Log(LogLevel.Info, "Adding manual form list as fallback");
                var configurations = LoadFormConfigurations();

                foreach (var config in configurations.Where(c => c.IsEnabled))
                {
                    // Try to find the actual type, fallback to basic info if not found
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
                        FormType = formType // May be null, will be handled in navigation
                    };

                    _cachedForms.Add(formInfo);
                    Log(LogLevel.Debug, $"Added manual form: {config.DisplayName}");
                }

                Log(LogLevel.Info, $"Added {_cachedForms.Count} forms from manual configuration");
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "Failed to add manual form list", ex);
            }
        }

        /// <summary>
        /// Map form names to navigation methods with fallback
        /// </summary>
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

        /// <summary>
        /// Log levels for production filtering
        /// </summary>
        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3
        }

        private static readonly LogLevel CURRENT_LOG_LEVEL = ENABLE_DIAGNOSTICS ? LogLevel.Debug : LogLevel.Warning;

        /// <summary>
        /// Enhanced logging with levels
        /// </summary>
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
                Console.WriteLine($"   Exception: {ex.Message}");
                if (level >= LogLevel.Error)
                {
                    Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Enhanced exception handling with progressive fallbacks
        /// </summary>
        private T ExecuteWithFallback<T>(Func<T> primaryAction, Func<T> fallbackAction, string operationName)
        {
            try
            {
                Log(LogLevel.Debug, $"Executing {operationName}...");
                var result = primaryAction();
                Log(LogLevel.Debug, $"{operationName} completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                Log(LogLevel.Warning, $"{operationName} failed, attempting fallback", ex);
                try
                {
                    var result = fallbackAction();
                    Log(LogLevel.Info, $"{operationName} fallback succeeded");
                    return result;
                }
                catch (Exception fallbackEx)
                {
                    Log(LogLevel.Error, $"{operationName} fallback also failed", fallbackEx);
                    throw new AggregateException($"Both primary and fallback failed for {operationName}", ex, fallbackEx);
                }
            }
        }

        /// <summary>
        /// Validate Syncfusion license with fallback notification
        /// </summary>
        private bool ValidateSyncfusionLicense()
        {
            try
            {
                // This would check if Syncfusion controls are working properly
                var testLabel = new AutoLabel { Text = "Test" };
                testLabel.Dispose();
                Log(LogLevel.Info, "Syncfusion license validation passed");
                return true;
            }
            catch (Exception ex)
            {
                Log(LogLevel.Warning, "Syncfusion license validation failed - using fallback mode", ex);
                return false;
            }
        }

        #endregion

        #region Theme Service and Configuration Support

        /// <summary>
        /// Centralized theme service for consistent styling
        /// </summary>
        public static class EnhancedThemeService
        {
            // Material Design Colors - can be loaded from configuration
            public static readonly Color PrimaryColor = ColorTranslator.FromHtml("#2196F3");
            public static readonly Color PrimaryDarkColor = ColorTranslator.FromHtml("#1976D2");
            public static readonly Color BackgroundColor = ColorTranslator.FromHtml("#FAFAFA");
            public static readonly Color SurfaceColor = Color.White;
            public static readonly Color TextColor = ColorTranslator.FromHtml("#333333");
            public static readonly Color ErrorColor = ColorTranslator.FromHtml("#F44336");

            // Font definitions with fallbacks
            public static readonly string[] PreferredFonts = { "Segoe UI", "Arial", "Microsoft Sans Serif", "Tahoma" };
            public static readonly Font DefaultFont = GetSafeFont(10F, FontStyle.Regular);
            public static readonly Font HeaderFont = GetSafeFont(18F, FontStyle.Bold);
            public static readonly Font ButtonFont = GetSafeFont(10F, FontStyle.Bold);

            /// <summary>
            /// Get a safe font with automatic fallbacks
            /// </summary>
            public static Font GetSafeFont(float size, FontStyle style = FontStyle.Regular)
            {
                foreach (var fontName in PreferredFonts)
                {
                    try
                    {
                        return new Font(fontName, size, style);
                    }
                    catch
                    {
                        // Continue to next fallback
                    }
                }
                // Last resort
                return new Font(SystemFonts.DefaultFont.FontFamily, size, style);
            }

            /// <summary>
            /// Apply consistent theming to any control
            /// </summary>
            public static void ApplyTheme(Control control)
            {
                if (control == null) return;

                control.BackColor = SurfaceColor;
                control.ForeColor = TextColor;
                control.Font = DefaultFont;

                // Specific control theming
                switch (control)
                {
                    case Button button:
                        ApplyButtonTheme(button);
                        break;
                    case Label label:
                        ApplyLabelTheme(label);
                        break;
                    case Panel panel:
                        ApplyPanelTheme(panel);
                        break;
                }
            }

            private static void ApplyButtonTheme(Button button)
            {
                button.BackColor = PrimaryColor;
                button.ForeColor = Color.White;
                button.Font = ButtonFont;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;

                // Add hover effects
                button.MouseEnter += (s, e) => button.BackColor = PrimaryDarkColor;
                button.MouseLeave += (s, e) => button.BackColor = PrimaryColor;
            }

            private static void ApplyLabelTheme(Label label)
            {
                label.BackColor = Color.Transparent;
                label.ForeColor = TextColor;
                label.Font = DefaultFont;
            }

            private static void ApplyPanelTheme(Panel panel)
            {
                panel.BackColor = SurfaceColor;
            }
        }

        /// <summary>
        /// Configuration-driven form metadata
        /// </summary>
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

        /// <summary>
        /// Load form configurations from external source (JSON, XML, etc.)
        /// </summary>
        private List<FormConfiguration> LoadFormConfigurations()
        {
            // This could load from a JSON file, database, etc.
            // For now, return a default configuration
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

        #region Control Factory Pattern

        /// <summary>
        /// Factory for creating controls with built-in fallback strategies
        /// </summary>
        public static class ControlFactory
        {
            /// <summary>
            /// Create a label control with fallback strategies
            /// </summary>
            public static Control CreateLabel(string text, Font font = null, Color? foreColor = null, bool autoSize = true)
            {
                return ExecuteWithFallback(
                    () =>
                    {
                        var autoLabel = new AutoLabel
                        {
                            Text = text,
                            Font = font ?? EnhancedThemeService.DefaultFont,
                            ForeColor = foreColor ?? EnhancedThemeService.TextColor,
                            AutoSize = autoSize,
                            BackColor = Color.Transparent
                        };
                        Log(LogLevel.Debug, $"Created Syncfusion AutoLabel: {text}");
                        return autoLabel;
                    },
                    () =>
                    {
                        var label = new Label
                        {
                            Text = text,
                            Font = font ?? EnhancedThemeService.DefaultFont,
                            ForeColor = foreColor ?? EnhancedThemeService.TextColor,
                            AutoSize = autoSize,
                            BackColor = Color.Transparent
                        };
                        Log(LogLevel.Info, $"Created standard Label fallback: {text}");
                        return label;
                    },
                    "Label Creation"
                );
            }

            /// <summary>
            /// Create a button control with fallback strategies
            /// </summary>
            public static Control CreateButton(string text, Size size, EventHandler clickHandler = null)
            {
                return ExecuteWithFallback(
                    () =>
                    {
                        var buttonAdv = new ButtonAdv
                        {
                            Text = text,
                            Size = size,
                            Font = EnhancedThemeService.ButtonFont,
                            BackColor = EnhancedThemeService.PrimaryColor,
                            ForeColor = Color.White,
                            UseVisualStyle = true
                        };

                        if (clickHandler != null)
                            buttonAdv.Click += clickHandler;

                        Log(LogLevel.Debug, $"Created Syncfusion ButtonAdv: {text}");
                        return buttonAdv;
                    },
                    () =>
                    {
                        var button = new Button
                        {
                            Text = text,
                            Size = size,
                            Font = EnhancedThemeService.ButtonFont,
                            BackColor = EnhancedThemeService.PrimaryColor,
                            ForeColor = Color.White,
                            FlatStyle = FlatStyle.Flat,
                            UseVisualStyleBackColor = false
                        };

                        button.FlatAppearance.BorderSize = 0;

                        if (clickHandler != null)
                            button.Click += clickHandler;

                        // Apply hover effects
                        button.MouseEnter += (s, e) => button.BackColor = EnhancedThemeService.PrimaryDarkColor;
                        button.MouseLeave += (s, e) => button.BackColor = EnhancedThemeService.PrimaryColor;

                        Log(LogLevel.Info, $"Created standard Button fallback: {text}");
                        return button;
                    },
                    "Button Creation"
                );
            }

            /// <summary>
            /// Helper method for ExecuteWithFallback in static context
            /// </summary>
            private static T ExecuteWithFallback<T>(Func<T> primaryAction, Func<T> fallbackAction, string operationName)
            {
                try
                {
                    return primaryAction();
                }
                catch (Exception ex)
                {
                    Log(LogLevel.Warning, $"{operationName} primary failed, using fallback", ex);
                    try
                    {
                        return fallbackAction();
                    }
                    catch (Exception fallbackEx)
                    {
                        Log(LogLevel.Error, $"{operationName} fallback also failed", fallbackEx);
                        throw new AggregateException($"Both primary and fallback failed for {operationName}", ex, fallbackEx);
                    }
                }
            }
        }

        /// <summary>
        /// Enhanced PopulateFormButtons using control factory and configuration
        /// </summary>
        private void PopulateFormButtonsEnhanced()
        {
            try
            {
                _formButtonsPanel.Controls.Clear();
                Log(LogLevel.Info, $"Populating {_cachedForms.Count} form buttons using enhanced factory...");

                // Load configurations for form metadata
                var configurations = LoadFormConfigurations();
                var configDict = configurations.ToDictionary(c => c.Name, c => c);

                foreach (var formInfo in _cachedForms.OrderBy(f =>
                    configDict.ContainsKey(f.Name) ? configDict[f.Name].SortOrder : int.MaxValue))
                {
                    // Skip disabled forms
                    if (configDict.ContainsKey(formInfo.Name) && !configDict[formInfo.Name].IsEnabled)
                    {
                        Log(LogLevel.Debug, $"Skipping disabled form: {formInfo.DisplayName}");
                        continue;
                    }

                    var button = ControlFactory.CreateButton(
                        formInfo.DisplayName,
                        new Size(220, 120),
                        (s, e) => NavigateToForm(formInfo)
                    );

                    button.Margin = new Padding(10);
                    button.Cursor = Cursors.Hand;

                    _formButtonsPanel.Controls.Add(button);
                    Log(LogLevel.Debug, $"Added enhanced button: {formInfo.DisplayName}");
                }

                Log(LogLevel.Info, $"Successfully added {_formButtonsPanel.Controls.Count} buttons to panel");

                // Force layout update with error handling
                ExecuteWithFallback(
                    () => {
                        _formButtonsPanel.PerformLayout();
                        _contentPanel?.PerformLayout();
                        _mainPanel?.PerformLayout();
                        this.PerformLayout();
                        return true;
                    },
                    () => {
                        // Minimal layout refresh as fallback
                        _formButtonsPanel.Refresh();
                        return true;
                    },
                    "Layout Refresh"
                );
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "Failed to populate form buttons", ex);
                CreateEmergencyButtons();
            }
        }

        /// <summary>
        /// Create emergency buttons as last resort
        /// </summary>
        private void CreateEmergencyButtons()
        {
            Log(LogLevel.Warning, "Creating emergency buttons");
            _formButtonsPanel.Controls.Clear();

            var emergencyButton = new Label
            {
                Text = "Dashboard Error\nCheck console for details",
                Size = new Size(300, 100),
                BackColor = EnhancedThemeService.ErrorColor,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = EnhancedThemeService.DefaultFont,
                BorderStyle = BorderStyle.FixedSingle
            };

            _formButtonsPanel.Controls.Add(emergencyButton);
        }

        #endregion

        /// <summary>
        /// Complete form initialization with button population
        /// </summary>
        private void CompleteInitialization()
        {
            try
            {
                Log(LogLevel.Info, "Completing form initialization...");

                // Populate form buttons using enhanced method
                PopulateFormButtonsEnhanced();

                Log(LogLevel.Info, "Form initialization completed successfully");
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "Form initialization failed", ex);
                CreateEmergencyButtons();
            }
        }

        /// <summary>
        /// Override OnLoad to complete initialization
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CompleteInitialization();
        }
    }
}
