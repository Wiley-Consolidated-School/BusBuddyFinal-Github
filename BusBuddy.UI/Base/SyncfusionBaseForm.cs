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
    /// Native Syncfusion base form using SfButton, SfTextBox, SfComboBox, and AutoLabel
    /// Provides standardized MaterialLight UI with proper theming and DPI support
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class SyncfusionBaseForm : Form
    {
        protected readonly ErrorProvider _errorProvider;
        protected readonly BusBuddy.Business.DatabaseHelperService _databaseService;
        protected readonly BannerTextProvider _bannerTextProvider;

        // Common UI elements
        protected Panel _mainPanel;
        protected Panel _buttonPanel;

        // Theme and DPI support
        protected float _dpiScale;
        protected bool _isHighDpi;

        // Static initialization guard for Syncfusion components
        private static bool _syncfusionInitialized = false;
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
            // Set consistent initialization before component initialization
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            // Initialize common components
            _errorProvider = new ErrorProvider();
            _databaseService = new BusBuddy.Business.DatabaseHelperService();
            _bannerTextProvider = CreateBannerTextProviderSafely();

            // Initialize DPI awareness for proper scaling
            InitializeDpiAwareness();

            // Initialize Syncfusion theming
            InitializeSyncfusionDesign();

            // Set common form properties
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.KeyPreview = true;
            this.Size = GetDpiAwareSize(new Size(800, 600));
            this.MinimumSize = new Size(1024, 600);
            this.BackColor = Color.FromArgb(255, 248, 248);

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
            _dpiScale = SyncfusionThemeHelper.HighDpiSupport.GetDpiScale(this);
            _isHighDpi = SyncfusionThemeHelper.HighDpiSupport.IsHighDpiMode(this);
        }

        private void InitializeSyncfusionDesign()
        {
            // Task 8: Update to Office2016Black theme for BusBuddy dashboard redesign
            // Load dark theme DLL if available
            SyncfusionThemeHelper.LoadDarkTheme();

            // Apply Office2016Black theme using SfSkinManager
            SfSkinManager.SetVisualStyle(this, "Office2016Black");

            // Enhanced Office2016Black title bar styling
            ApplyMaterialTitleBarStyling();
        }

        protected virtual void InitializeLayout()
        {
            // Create main panel with DPI-aware sizing and solid background
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = GetDpiAwarePadding(new Padding(20)),
                BackColor = ThemeService.SurfaceColor
            };
            this.Controls.Add(_mainPanel);

            // Create button panel with DPI-aware sizing and solid background
            _buttonPanel = new Panel
            {
                Height = GetDpiAwareHeight(60),
                Dock = DockStyle.Bottom,
                BackColor = ThemeService.SurfaceColor,
                Padding = GetDpiAwarePadding(new Padding(20, 10, 20, 10))
            };
            this.Controls.Add(_buttonPanel);
        }

        /// <summary>
        /// Apply Material Design title bar styling for consistent branding
        /// </summary>
        private void ApplyMaterialTitleBarStyling()
        {
            try
            {
                // Set form background for Syncfusion MaterialLight consistency
                this.BackColor = SyncfusionThemeHelper.MaterialColors.Background;

                // Apply consistent form styling with Syncfusion Material Design colors
                this.ForeColor = SyncfusionThemeHelper.MaterialColors.TextPrimary;

                // Enhanced title bar styling for professional appearance
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.MaximizeBox = true;
                this.MinimizeBox = true;

                // Apply Syncfusion Material Design accent as visual enhancement
                this.Padding = new Padding(1);

                Console.WriteLine("✨ Syncfusion MaterialLight title bar styling applied");
            }
            catch (Exception ex)
            {
                // Fallback to basic styling if Syncfusion features fail
                Console.WriteLine($"Syncfusion title bar styling fallback: {ex.Message}");
                this.BackColor = Color.FromArgb(245, 245, 245); // Safe fallback
            }
        }

        /// <summary>
        /// Apply enhanced button styling using Syncfusion Material theme
        /// </summary>
        protected virtual void ApplyEnhancedButtonStyling(Control buttonContainer)
        {
            try
            {
                foreach (Control control in buttonContainer.Controls)
                {
                    if (control is SfButton sfButton)
                    {
                        // Apply Syncfusion Material button styling
                        sfButton.Style.BackColor = SyncfusionThemeHelper.MaterialColors.Primary;
                        sfButton.Style.ForeColor = Color.White;
                        sfButton.Style.HoverBackColor = ControlPaint.Light(SyncfusionThemeHelper.MaterialColors.Primary, 0.2f);
                        sfButton.Style.PressedBackColor = ControlPaint.Dark(SyncfusionThemeHelper.MaterialColors.Primary, 0.1f);
                        sfButton.Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 10f, FontStyle.Regular);
                    }
                    else if (control is Button stdButton)
                    {
                        // Apply Material styling to standard buttons as fallback
                        stdButton.BackColor = SyncfusionThemeHelper.MaterialColors.Primary;
                        stdButton.ForeColor = Color.White;
                        stdButton.FlatStyle = FlatStyle.Flat;
                        stdButton.FlatAppearance.BorderSize = 0;
                        stdButton.Font = SyncfusionThemeHelper.GetSafeFont("Segoe UI", 10f, FontStyle.Regular);
                    }
                }

                Console.WriteLine("✨ Enhanced Syncfusion button styling applied");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Button styling error: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply enhanced grid theming for SfDataGrid controls
        /// </summary>
        protected virtual void ApplyEnhancedGridTheming()
        {
            try
            {
                // Find and enhance any SfDataGrid controls in the form
                EnhanceGridControlsRecursive(this.Controls);
                Console.WriteLine("✨ Enhanced Syncfusion grid theming applied");
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
                    SyncfusionThemeHelper.SfDataGridEnhancements(sfGrid);

                    // Add tooltip support for better UX
                    sfGrid.ShowToolTip = true;

                    // Apply Material colors
                    if (sfGrid.Style?.HeaderStyle != null)
                    {
                        sfGrid.Style.HeaderStyle.BackColor = SyncfusionThemeHelper.MaterialColors.Primary;
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
        private BannerTextProvider CreateBannerTextProviderSafely()
        {
            lock (_initLock)
            {
                try
                {
                    // Check if we're in a testing environment
                    bool isTestEnvironment = AppDomain.CurrentDomain.GetAssemblies()
                        .Any(a => a.FullName.Contains("xunit") || a.FullName.Contains("Test"));

                    if (isTestEnvironment && _syncfusionInitialized)
                    {
                        // In test scenarios, return null to avoid window handle conflicts
                        Console.WriteLine("⚠️ Skipping BannerTextProvider creation in test environment to prevent handle conflicts");
                        return null;
                    }

                    var provider = new BannerTextProvider();
                    _syncfusionInitialized = true;
                    return provider;
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("Window handle already exists"))
                {
                    Console.WriteLine($"⚠️ Window handle conflict prevented: {ex.Message}");
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ BannerTextProvider creation failed: {ex.Message}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Apply consistent Syncfusion theme across all forms to prevent theme inconsistencies
        /// </summary>
        private void ApplyConsistentSyncfusionTheme()
        {
            try
            {
                // Force MaterialLight theme application
                SfSkinManager.SetVisualStyle(this, "MaterialLight");

                // Reset current theme to Light to ensure consistency
                SyncfusionThemeHelper.CurrentTheme = SyncfusionThemeHelper.ThemeMode.Light;
                SyncfusionThemeHelper.MaterialTheme.IsDarkMode = false;

                // Apply consistent colors to form
                this.BackColor = SyncfusionThemeHelper.MaterialColors.Background;
                this.ForeColor = SyncfusionThemeHelper.MaterialColors.TextPrimary;

                Console.WriteLine("✨ Consistent Syncfusion MaterialLight theme applied");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Theme consistency error: {ex.Message}");
                // Fallback to basic light theme
                this.BackColor = Color.White;
                this.ForeColor = Color.Black;
            }
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

            // Apply consistent MaterialLight theme to all forms
            SfSkinManager.SetVisualStyle(this, "MaterialLight");

            // Apply high DPI theming if necessary
            if (_isHighDpi) SyncfusionThemeHelper.ApplyHighDpiMaterialTheme(this);

            // Apply enhanced theming features
            ApplyEnhancedButtonStyling(_buttonPanel);
            ApplyEnhancedGridTheming();

            // Force consistent theme colors
            ApplyConsistentSyncfusionTheme();
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
                // Dispose banner text provider safely
                if (_bannerTextProvider != null)
                {
                    try
                    {
                        _bannerTextProvider.Dispose();
                        Console.WriteLine("🧽 BannerTextProvider disposed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error disposing BannerTextProvider: {ex.Message}");
                    }
                }

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
                            // Clear data sources for data controls
                            if (control is Syncfusion.WinForms.DataGrid.SfDataGrid dataGrid)
                            {
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
                    SyncfusionThemeHelper.LoadDarkTheme();
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
                    this.BackColor = Color.FromArgb(68, 68, 68);
                    this.ForeColor = Color.White;
                    if (_mainPanel != null)
                        _mainPanel.BackColor = Color.FromArgb(68, 68, 68);
                    if (_buttonPanel != null)
                        _buttonPanel.BackColor = Color.FromArgb(68, 68, 68);
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
                foreach (Control control in controls)
                {
                    // Dispose child controls first
                    if (control.HasChildren)
                    {
                        DisposeSyncfusionControlsRecursive(control.Controls);
                    }

                    // Dispose Syncfusion-specific controls
                    if (control.GetType().Namespace?.StartsWith("Syncfusion") == true)
                    {
                        control.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                LogInfo($"Syncfusion control disposal warning: {ex.Message}");
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
