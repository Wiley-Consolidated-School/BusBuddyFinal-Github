using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows.Forms;
using BusBuddy.Business;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Theme;
using BusBuddy.UI.Services;
using ThemeService = BusBuddy.UI.Theme.EnhancedThemeService;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;

namespace BusBuddy.UI.Base
{
    /// <summary>
    /// Native Syncfusion base form using SfForm for official theme support
    /// Provides standardized theming with proper Office2016Black/White support
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class SyncfusionBaseForm : SfForm
    {
        protected readonly ErrorProvider _errorProvider;
        protected readonly BusBuddy.Business.IDatabaseHelperService _databaseService;
        // BannerTextProvider removed - causes license popups, not needed per official Syncfusion docs

        // Common UI elements
        protected Panel _mainPanel;
        protected Panel _buttonPanel;

        // Theme and DPI support
        protected float _dpiScale;
        protected bool _isHighDpi;

        // Static initialization guard for Syncfusion components - removed unused field
        private static readonly object _initLock = new object();

        // Test mode support - prevents dialog boxes during testing
        private static bool _testModeEnabled = false;

        // Theme management
        private string _currentTheme = "Office2016Black";

        /// <summary>
        /// Enable test mode to redirect message boxes to console output
        /// </summary>
        public static void EnableTestMode()
        {
            _testModeEnabled = true;
        }

        /// <summary>
        /// Disable test mode to restore normal message box behavior
        /// </summary>
        public static void DisableTestMode()
        {
            _testModeEnabled = false;
        }

        public SyncfusionBaseForm()
        {
            // Syncfusion license is already registered in Program.cs Main() method
            // No additional license validation should be performed here
            Console.WriteLine("✅ SyncfusionBaseForm: License already registered at application startup");

            // Set consistent initialization before component initialization
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            // Initialize common components
            _errorProvider = new ErrorProvider();
            _databaseService = UnifiedServiceManager.Instance.GetService<BusBuddy.Business.IDatabaseHelperService>();
            // BannerTextProvider removed - causes license popups, not needed per official Syncfusion docs

            // Initialize DPI awareness for proper scaling
            InitializeDpiAwareness();

            // Initialize Syncfusion theming
            InitializeSyncfusionDesign();

            // Set common form properties with dark theme graphics-friendly defaults
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.KeyPreview = true;
            this.Size = GetDpiAwareSize(new Size(800, 600));
            this.MinimumSize = new Size(1024, 600);
            this.BackColor = BusBuddyThemeManager.DarkTheme.BackgroundColor; // Dark graphics-friendly background

            // Initialize layout
            InitializeLayout();

            // Register with shutdown manager for proper cleanup
            try
            {
                // Use reflection to find and call TestSafeApplicationShutdownManager.RegisterForm
                var shutdownManagerType = Type.GetType("BusBuddy.UI.Services.TestSafeApplicationShutdownManager");
                if (shutdownManagerType != null)
                {
                    var registerMethod = shutdownManagerType.GetMethod("RegisterForm",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    registerMethod?.Invoke(null, new object[] { this });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Could not register form with shutdown manager: {ex.Message}");
            }
        }

        #region Initialization

        private void InitializeDpiAwareness()
        {
            _dpiScale = BusBuddyThemeManager.GetDpiScale(this);
            _isHighDpi = BusBuddyThemeManager.IsHighDpiMode(this);
        }

        private void InitializeSyncfusionDesign()
        {
            // Syncfusion license is already registered in Program.cs Main() method
            // No additional license validation is needed - it should be done only once in Main()
            Console.WriteLine("✅ SyncfusionBaseForm: Using pre-registered license for theme initialization");

            // Apply proper Syncfusion theming following official documentation
            // Reference: https://help.syncfusion.com/windowsforms/form/themes
            try
            {
                // Apply Office2016Black theme using ThemeName property as documented
                this.ThemeName = "Office2016Black";
                _currentTheme = "Office2016Black";
                Console.WriteLine("✅ Applied Office2016Black theme using official ThemeName property");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Theme application error: {ex.Message}");
                // Apply minimal styling as fallback
                ApplyMinimalStyling();
            }
        }

        /// <summary>
        /// Controls whether the base form should automatically create standard panels.
        /// Override this property to return false in forms that manage their own layout (like Dashboard).
        /// </summary>
        protected virtual bool ShouldCreateStandardPanels => true;

        protected virtual void InitializeLayout()
        {
            // Only create standard panels if the derived form wants them
            if (ShouldCreateStandardPanels)
            {
                CreateStandardPanels();
            }
        }

        /// <summary>
        /// Creates the standard main panel and button panel.
        /// Call this method from derived forms if they want the standard layout.
        /// </summary>
        protected void CreateStandardPanels()
        {
            // Create main panel with NO custom styling - standard Windows appearance
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = GetDpiAwarePadding(new Padding(10)),
                BackColor = SystemColors.Control, // Standard Windows background
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(_mainPanel);

            // Create button panel with standard Windows styling
            _buttonPanel = new Panel
            {
                Height = GetDpiAwareHeight(60),
                Dock = DockStyle.Bottom,
                BackColor = SystemColors.Control,
                Padding = GetDpiAwarePadding(new Padding(20, 10, 20, 10)),
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(_buttonPanel);
        }

        /// <summary>
        /// Apply absolutely minimal styling for graphics troubleshooting
        /// Completely disable any theming that could interfere with graphics rendering
        /// </summary>
        private void ApplyMinimalStyling()
        {
            try
            {
                // Use standard Windows colors - no custom theming at all
                this.BackColor = SystemColors.Control;
                this.ForeColor = SystemColors.ControlText;

                // Enable double buffering explicitly for graphics compatibility
                this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                this.SetStyle(ControlStyles.UserPaint, true);
                this.SetStyle(ControlStyles.DoubleBuffer, true);
                this.SetStyle(ControlStyles.ResizeRedraw, true);

                // Basic form properties only
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.MaximizeBox = true;
                this.MinimizeBox = true;

                Console.WriteLine("✨ Minimal styling applied - all theming disabled for graphics troubleshooting");
                Console.WriteLine("✨ Double buffering enabled for graphics compatibility");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Minimal styling error: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply simple button styling that won't interfere with graphics
        /// </summary>
        protected virtual void ApplyEnhancedButtonStyling(Control buttonContainer)
        {
            try
            {
                if (buttonContainer == null) return;

                foreach (Control control in buttonContainer.Controls)
                {
                    if (control is Button button)
                    {
                        // Apply dark theme button styling
                        button.BackColor = BusBuddyThemeManager.DarkTheme.PrimaryColor; // Darker blue for dark theme
                        button.ForeColor = Color.White;
                        button.FlatStyle = FlatStyle.Flat;
                        button.FlatAppearance.BorderSize = 1;
                        button.FlatAppearance.BorderColor = BusBuddyThemeManager.ThemeColors.GetMutedColor(BusBuddyThemeManager.SupportedThemes.Office2016Black);
                        button.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
                    }
                }

                Console.WriteLine("✨ Dark theme button styling applied");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Button styling error: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply simple grid theming that won't interfere with graphics
        /// </summary>
        protected virtual void ApplyEnhancedGridTheming()
        {
            try
            {
                // Find and enhance any SfDataGrid controls in the form
                EnhanceGridControlsRecursive(this.Controls);
                Console.WriteLine("✨ Simple grid theming applied");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Grid theming error: {ex.Message}");
            }
        }

        private void EnhanceGridControlsRecursive(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is Syncfusion.WinForms.DataGrid.SfDataGrid sfGrid)
                {
                    // Apply simple grid enhancements for dark theme
                    sfGrid.AllowEditing = false;
                    sfGrid.AllowSorting = true;
                    sfGrid.ShowToolTip = true;

                    // Use dark theme colors that won't interfere with graphics
                    if (sfGrid.Style?.HeaderStyle != null)
                    {
                        sfGrid.Style.HeaderStyle.BackColor = BusBuddyThemeManager.DarkTheme.PrimaryColor;
                        sfGrid.Style.HeaderStyle.TextColor = Color.White;
                    }
                }

                if (control.HasChildren)
                {
                    EnhanceGridControlsRecursive(control.Controls);
                }
            }
        }

        /// <summary>
        /// Safely creates BannerTextProvider with protection against window handle conflicts
        /// Essential for UI testing scenarios where multiple forms may be created
        /// </summary>
        /// <summary>
        /// Apply consistent Syncfusion theme across all forms to prevent theme inconsistencies
        /// Removed to prevent graphics display issues
        /// </summary>
        private void ApplyConsistentSyncfusionTheme()
        {
            // Method disabled to prevent graphics interference
            Console.WriteLine("✨ Consistent theme application skipped for graphics compatibility");
        }

        #endregion

        #region DPI Awareness Helpers

        protected Size GetDpiAwareSize(Size originalSize) => ThemeService.GetDpiAwareSize(originalSize, _dpiScale);
        protected Padding GetDpiAwarePadding(Padding originalPadding) => ThemeService.GetDpiAwarePadding(originalPadding, _dpiScale);
        protected int GetDpiAwareX(int x) => ThemeService.ScaleByDpi(x, _dpiScale);
        protected int GetDpiAwareY(int y) => ThemeService.ScaleByDpi(y, _dpiScale);
        protected int GetDpiAwareWidth(int width) => ThemeService.ScaleByDpi(width, _dpiScale);
        protected int GetDpiAwareHeight(int height) => ThemeService.ScaleByDpi(height, _dpiScale);

        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Keep consistent theme - don't override what was set in constructor
            Console.WriteLine($"✅ SyncfusionBaseForm loaded with theme: {_currentTheme}");

            // Apply enhanced features without changing the base theme
            ApplyEnhancedButtonStyling(_buttonPanel);
            ApplyEnhancedGridTheming();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                Console.WriteLine($"🧽 SyncfusionBaseForm closing: {this.GetType().Name}");

                // Enhanced cleanup for all Syncfusion components
                PerformEnhancedSyncfusionCleanup();

                Console.WriteLine($"✅ SyncfusionBaseForm cleanup completed: {this.GetType().Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during SyncfusionBaseForm closing: {ex.Message}");
            }
            finally
            {
                base.OnFormClosing(e);
                _errorProvider?.Dispose();
            }
        }

        /// <summary>
        /// Perform enhanced cleanup of Syncfusion components to prevent process lingering
        /// </summary>
        protected virtual void PerformEnhancedSyncfusionCleanup()
        {
            try
            {
                // BannerTextProvider disposal removed - not needed per official Syncfusion docs

                // Database service cleanup (if it implements IDisposable)
                if (_databaseService != null)
                {
                    try
                    {
                        // Check if the service implements IDisposable
                        if (_databaseService is IDisposable disposableService)
                        {
                            disposableService.Dispose();
                            Console.WriteLine("🧽 DatabaseService disposed");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error disposing DatabaseService: {ex.Message}");
                    }
                }

                // Clean up all Syncfusion controls recursively
                CleanupSyncfusionControlsRecursively(this.Controls);

                // Force garbage collection to help cleanup
                GC.SuppressFinalize(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error in PerformEnhancedSyncfusionCleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively cleanup Syncfusion controls to prevent process lingering
        /// </summary>
        private void CleanupSyncfusionControlsRecursively(Control.ControlCollection controls)
        {
            try
            {
                var controlsToCleanup = new List<Control>();
                foreach (Control control in controls)
                {
                    controlsToCleanup.Add(control);
                }

                // Process in reverse order (children first)
                for (int i = controlsToCleanup.Count - 1; i >= 0; i--)
                {
                    var control = controlsToCleanup[i];

                    try
                    {
                        // Recursively cleanup child controls first
                        if (control.HasChildren)
                        {
                            CleanupSyncfusionControlsRecursively(control.Controls);
                        }

                        // Special cleanup for Syncfusion controls
                        if (control.GetType().FullName?.Contains("Syncfusion") == true)
                        {
                            // Special handling for ChartControl
                            if (control is Syncfusion.Windows.Forms.Chart.ChartControl chartControl)
                            {
                                CleanupChartControlSafely(chartControl);
                            }
                            else if (control is Syncfusion.WinForms.DataGrid.SfDataGrid dataGrid)
                            {
                                // Clear data sources for data controls
                                dataGrid.DataSource = null;
                            }

                            // Suppress finalization for all Syncfusion controls
                            GC.SuppressFinalize(control);
                            Console.WriteLine($"🧽 Cleaned up Syncfusion control: {control.GetType().Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error cleaning up control {control?.GetType().Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error in CleanupSyncfusionControlsRecursively: {ex.Message}");
            }
        }

        /// <summary>
        /// Safely cleanup ChartControl to prevent ChartToolBar issues during cleanup
        /// </summary>
        private void CleanupChartControlSafely(Syncfusion.Windows.Forms.Chart.ChartControl chartControl)
        {
            try
            {
                if (chartControl == null) return;

                // Clear chart data first
                if (chartControl.Series != null)
                {
                    chartControl.Series.Clear();
                }

                // Chart areas are not available in Syncfusion ChartControl
                // The control handles chart layout internally

                // Hide the chart to prevent drawing during cleanup
                chartControl.Visible = false;

                Console.WriteLine("🧽 ChartControl cleaned up safely");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ ChartControl cleanup warning: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current theme name
        /// </summary>
        public string CurrentTheme => _currentTheme;

        /// <summary>
        /// Applies the specified Syncfusion visual theme to the form.
        /// Uses Office2016Black (dark) and Office2016White (light) as documented themes.
        /// </summary>
        /// <param name="themeName">The theme to apply ("Office2016Black" or "Office2016White")</param>
        public void ApplyTheme(string themeName)
        {
            try
            {
                // Validate theme name against documented Syncfusion themes
                if (themeName != "Office2016Black" && themeName != "Office2016White")
                {
                    throw new ArgumentException($"Unsupported theme: {themeName}. Use 'Office2016Black' or 'Office2016White'.");
                }

                _currentTheme = themeName;

                // Apply theme using SfSkinManager as documented
                SfSkinManager.SetVisualStyle(this, themeName);

                // Load appropriate theme DLL
                if (themeName == "Office2016Black")
                {
                    BusBuddyThemeManager.LoadDarkTheme();
                }

                // Update form background colors based on theme
                UpdateFormColorsForTheme(themeName);

                // Refresh form to reflect theme changes
                this.Refresh();

                LogInfo($"Applied theme: {themeName}");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Failed to apply theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggles between Office2016Black (dark) and Office2016White (light) themes
        /// </summary>
        public void ToggleTheme()
        {
            try
            {
                string newTheme = _currentTheme == "Office2016Black" ? "Office2016White" : "Office2016Black";
                ApplyTheme(newTheme);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Failed to toggle theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates form colors based on the selected theme
        /// </summary>
        private void UpdateFormColorsForTheme(string themeName)
        {
            try
            {
                if (themeName == "Office2016Black")
                {
                    // Dark theme colors
                    this.BackColor = BusBuddyThemeManager.DarkTheme.BackgroundColor;
                    this.ForeColor = Color.White;
                    if (_mainPanel != null)
                        _mainPanel.BackColor = BusBuddyThemeManager.DarkTheme.BackgroundColor;
                    if (_buttonPanel != null)
                        _buttonPanel.BackColor = BusBuddyThemeManager.DarkTheme.BackgroundColor;
                }
                else if (themeName == "Office2016White")
                {
                    // Light theme colors
                    this.BackColor = Color.White;
                    this.ForeColor = Color.Black;
                    if (_mainPanel != null)
                        _mainPanel.BackColor = Color.White;
                    if (_buttonPanel != null)
                        _buttonPanel.BackColor = Color.White;
                }
            }
            catch (Exception ex)
            {
                LogInfo($"Theme color update error: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs information messages to console
        /// </summary>
        private void LogInfo(string message)
        {
            Console.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }

        #endregion

        #region Disposal and Cleanup

        /// <summary>
        /// Clean up any resources being used with proper Syncfusion disposal
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    // Dispose of managed resources
                    _errorProvider?.Dispose();

                    // Clear Syncfusion theme resources for this form
                    try
                    {
                        SfSkinManager.SetVisualStyle(this, "Default");
                    }
                    catch (Exception ex)
                    {
                        LogInfo($"Syncfusion theme cleanup warning: {ex.Message}");
                    }

                    // Dispose Syncfusion controls recursively
                    DisposeSyncfusionControlsRecursive(this.Controls);
                }
            }
            catch (Exception ex)
            {
                LogInfo($"Disposal error: {ex.Message}");
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Recursively dispose Syncfusion controls to prevent memory leaks
        /// </summary>
        private void DisposeSyncfusionControlsRecursive(Control.ControlCollection controls)
        {
            try
            {
                var controlsList = new List<Control>();
                foreach (Control control in controls)
                {
                    controlsList.Add(control);
                }

                foreach (Control control in controlsList)
                {
                    try
                    {
                        // Dispose child controls first
                        if (control.HasChildren)
                        {
                            DisposeSyncfusionControlsRecursive(control.Controls);
                        }

                        // Handle specific Syncfusion controls that need special disposal
                        if (control.GetType().Namespace?.StartsWith("Syncfusion") == true)
                        {
                            // Special handling for ChartControl to prevent ChartToolBar null reference
                            if (control is Syncfusion.Windows.Forms.Chart.ChartControl chartControl)
                            {
                                DisposeChartControlSafely(chartControl);
                            }
                            else
                            {
                                // Standard Syncfusion control disposal
                                control.Dispose();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogInfo($"Individual control disposal warning for {control?.GetType().Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogInfo($"Syncfusion control disposal warning: {ex.Message}");
            }
        }

        /// <summary>
        /// Safely dispose ChartControl to prevent ChartToolBar null reference exceptions
        /// This is a known issue with Syncfusion ChartControl disposal
        /// </summary>
        private void DisposeChartControlSafely(Syncfusion.Windows.Forms.Chart.ChartControl chartControl)
        {
            try
            {
                if (chartControl == null) return;

                // Step 1: Disable the toolbar first to prevent ChartToolBar disposal issues
                try
                {
                    chartControl.ShowToolbar = false;
                }
                catch (Exception ex)
                {
                    LogInfo($"ChartControl toolbar disable warning: {ex.Message}");
                }

                // Step 2: Clear chart data to prevent disposal issues
                try
                {
                    if (chartControl.Series != null)
                    {
                        chartControl.Series.Clear();
                    }
                }
                catch (Exception ex)
                {
                    LogInfo($"ChartControl series clear warning: {ex.Message}");
                }

                // Step 3: Disable tooltips and other features that might cause disposal issues
                try
                {
                    chartControl.ShowToolTips = false;
                    chartControl.ShowLegend = false;
                }
                catch (Exception ex)
                {
                    LogInfo($"ChartControl features disable warning: {ex.Message}");
                }

                // Step 4: Hide the chart to prevent drawing during disposal
                try
                {
                    chartControl.Visible = false;
                }
                catch (Exception ex)
                {
                    LogInfo($"ChartControl hide warning: {ex.Message}");
                }

                // Step 5: Remove from parent to prevent cascading disposal issues
                try
                {
                    if (chartControl.Parent != null)
                    {
                        chartControl.Parent.Controls.Remove(chartControl);
                    }
                }
                catch (Exception ex)
                {
                    LogInfo($"ChartControl parent removal warning: {ex.Message}");
                }

                // Step 6: Finally dispose the chart control (this may still throw, so we catch it)
                try
                {
                    chartControl.Dispose();
                    LogInfo("ChartControl disposed safely");
                }
                catch (Exception ex)
                {
                    LogInfo($"ChartControl disposal warning (known Syncfusion issue): {ex.Message}");
                    // Don't rethrow - this is a known Syncfusion ChartToolBar disposal issue
                    // The control is already removed from parent and hidden, so it's effectively disposed
                }
            }
            catch (Exception ex)
            {
                LogInfo($"ChartControl safe disposal outer warning: {ex.Message}");
                // Don't rethrow - we're in cleanup, best effort only
            }
        }

        #endregion

        #region Validation and Messaging

        /// <summary>
        /// Override this method to implement form-specific validation
        /// </summary>
        protected virtual bool ValidateForm() => true;

        /// <summary>
        /// Clear all validation errors from the form
        /// </summary>
        protected virtual void ClearAllValidationErrors()
        {
            _errorProvider?.Clear();
        }

        /// <summary>
        /// Set validation error for a specific control
        /// </summary>
        protected virtual void SetValidationError(Control control, string message)
        {
            _errorProvider?.SetError(control, message);
        }

        /// <summary>
        /// Show validation errors to the user
        /// </summary>
        protected void ShowValidationErrors(List<string> errors)
        {
            if (errors?.Count > 0)
            {
                string message = "Please correct the following errors:\n\n" + string.Join("\n", errors);
                if (_testModeEnabled)
                {
                    Console.WriteLine($"[VALIDATION ERROR] {message}");
                }
                else
                {
                    MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        /// <summary>
        /// Show error message to the user
        /// </summary>
        protected void ShowErrorMessage(string message)
        {
            if (_testModeEnabled)
            {
                Console.WriteLine($"[ERROR] {message}");
            }
            else
            {
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Show success message to the user
        /// </summary>
        protected void ShowSuccessMessage(string message)
        {
            if (_testModeEnabled)
            {
                Console.WriteLine($"[SUCCESS] {message}");
            }
            else
            {
                MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Show confirmation dialog
        /// </summary>
        protected bool ConfirmDelete(string itemType)
        {
            if (_testModeEnabled)
            {
                Console.WriteLine($"[CONFIRMATION] Are you sure you want to delete this {itemType}? (Test mode: returning true)");
                return true; // Auto-confirm in test mode
            }
            else
            {
                return MessageBox.Show($"Are you sure you want to delete this {itemType}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
            }
        }

        #endregion
    }
}
