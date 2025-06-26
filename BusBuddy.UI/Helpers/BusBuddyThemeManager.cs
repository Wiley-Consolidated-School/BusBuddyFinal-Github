using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Complete theme manager for BusBuddy using ThemeName properties
    /// Based on official Syncfusion documentation patterns
    /// Reference: https://help.syncfusion.com/windowsforms/form/themes
    /// </summary>
    public static class BusBuddyThemeManager
    {
        /// <summary>
        /// Test mode flag to prevent UI control creation during unit tests
        /// </summary>
        private static bool _testMode = false;

        /// <summary>
        /// Enable test mode to prevent UI control instantiation
        /// </summary>
        public static void EnableTestMode()
        {
            _testMode = true;
            Console.WriteLine("🧪 BusBuddyThemeManager: Test mode enabled - UI controls will not be created");
        }

        /// <summary>
        /// Disable test mode to allow normal UI control creation
        /// </summary>
        public static void DisableTestMode()
        {
            _testMode = false;
            Console.WriteLine("🎨 BusBuddyThemeManager: Test mode disabled - UI controls enabled");
        }

        /// <summary>
        /// Check if currently in test mode
        /// </summary>
        public static bool IsTestMode => _testMode;

        public enum SupportedThemes
        {
            Office2016White,
            Office2016Black,
            Office2016DarkGray,
            Office2016Colorful,
            MaterialLight
        }

        private static SupportedThemes _currentTheme = SupportedThemes.Office2016White;

        public static SupportedThemes CurrentTheme
        {
            get => _currentTheme;
            set
            {
                _currentTheme = value;
                if (!_testMode)
                    Console.WriteLine($"🎨 BusBuddy theme changed to {value}");
            }
        }

        /// <summary>
        /// Apply theme to SfForm using official Syncfusion ThemeName property
        /// </summary>
        public static void ApplyTheme(SfForm form, SupportedThemes theme)
        {
            if (_testMode) return;

            try
            {
                var themeName = GetThemeName(theme);
                form.ThemeName = themeName;
                CurrentTheme = theme;
                Console.WriteLine($"✅ Applied {themeName} theme to form: {form.Text}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error applying theme to form: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply theme to regular Windows Forms controls (fallback)
        /// </summary>
        public static void ApplyTheme(Control control, SupportedThemes theme)
        {
            try
            {
                if (control is SfForm sfForm)
                {
                    ApplyTheme(sfForm, theme);
                    return;
                }

                // For non-Syncfusion controls, apply basic styling
                var bgColor = ThemeColors.GetBackgroundColor(theme);
                var textColor = ThemeColors.GetTextColor(theme);

                control.BackColor = bgColor;
                control.ForeColor = textColor;
                CurrentTheme = theme;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error applying theme to control: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply theme to SfDataGrid using official Syncfusion ThemeName property
        /// ENHANCED: Added null safety and test mode checks
        /// </summary>
        public static void ApplyTheme(Syncfusion.WinForms.DataGrid.SfDataGrid dataGrid, SupportedThemes theme)
        {
            if (_testMode || dataGrid == null)
            {
                if (_testMode)
                    Console.WriteLine("🧪 BusBuddyThemeManager: Skipping SfDataGrid theming - test mode enabled");
                if (dataGrid == null)
                    Console.WriteLine("⚠️ BusBuddyThemeManager: SfDataGrid is null - skipping theming");
                return;
            }

            try
            {
                // Check if the dataGrid is properly initialized
                if (dataGrid.IsDisposed)
                {
                    Console.WriteLine("⚠️ BusBuddyThemeManager: SfDataGrid is disposed - skipping theming");
                    return;
                }

                var themeName = GetThemeName(theme);
                dataGrid.ThemeName = themeName;
                Console.WriteLine($"✅ Applied {themeName} theme to data grid");
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine($"❌ NullReferenceException applying theme to data grid: {ex.Message}");
                Console.WriteLine("🔧 This may indicate the SfDataGrid is not fully initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error applying theme to data grid: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply theme to NavigationDrawer using official Syncfusion ThemeName property
        /// </summary>
        public static void ApplyTheme(Syncfusion.Windows.Forms.Tools.NavigationDrawer navigationDrawer, SupportedThemes theme)
        {
            try
            {
                var themeName = GetThemeName(theme);
                navigationDrawer.ThemeName = themeName;
                Console.WriteLine($"✅ Applied {themeName} theme to navigation drawer");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error applying theme to navigation drawer: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply theme to TabControlAdv using official Syncfusion ThemeName property
        /// </summary>
        public static void ApplyTheme(Syncfusion.Windows.Forms.Tools.TabControlAdv tabControl, SupportedThemes theme)
        {
            try
            {
                var themeName = GetThemeName(theme);
                tabControl.ThemeName = themeName;
                Console.WriteLine($"✅ Applied {themeName} theme to tab control");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error applying theme to tab control: {ex.Message}");
            }
        }

        /// <summary>
        /// Get the official Syncfusion theme name string
        /// </summary>
        private static string GetThemeName(SupportedThemes theme)
        {
            return theme switch
            {
                SupportedThemes.Office2016White => "Office2016White",
                SupportedThemes.Office2016Black => "Office2016Black",
                SupportedThemes.Office2016DarkGray => "Office2016DarkGray",
                SupportedThemes.Office2016Colorful => "Office2016Colorful",
                SupportedThemes.MaterialLight => "MaterialLight",
                _ => "Office2016White" // Safe default
            };
        }

        /// <summary>
        /// Get theme-appropriate colors for manual styling when needed
        /// Based on Syncfusion theme color schemes
        /// </summary>
        public static class ThemeColors
        {
            public static Color GetPrimaryColor(SupportedThemes theme)
            {
                return theme switch
                {
                    SupportedThemes.Office2016White => Color.FromArgb(43, 87, 154),
                    SupportedThemes.Office2016Black => Color.FromArgb(43, 87, 154),
                    SupportedThemes.Office2016DarkGray => Color.FromArgb(43, 87, 154),
                    SupportedThemes.Office2016Colorful => Color.FromArgb(43, 87, 154),
                    SupportedThemes.MaterialLight => Color.FromArgb(33, 150, 243),
                    _ => Color.FromArgb(43, 87, 154)
                };
            }

            public static Color GetBackgroundColor(SupportedThemes theme)
            {
                return theme switch
                {
                    SupportedThemes.Office2016White => Color.White,
                    SupportedThemes.Office2016Black => Color.FromArgb(54, 54, 54),
                    SupportedThemes.Office2016DarkGray => Color.FromArgb(71, 71, 71),
                    SupportedThemes.Office2016Colorful => Color.White,
                    SupportedThemes.MaterialLight => Color.White,
                    _ => Color.White
                };
            }

            public static Color GetTextColor(SupportedThemes theme)
            {
                return theme switch
                {
                    SupportedThemes.Office2016White => Color.FromArgb(68, 68, 68),
                    SupportedThemes.Office2016Black => Color.White,
                    SupportedThemes.Office2016DarkGray => Color.White,
                    SupportedThemes.Office2016Colorful => Color.FromArgb(68, 68, 68),
                    SupportedThemes.MaterialLight => Color.FromArgb(33, 33, 33),
                    _ => Color.FromArgb(68, 68, 68)
                };
            }

            /// <summary>
            /// Get theme-appropriate success color (green)
            /// </summary>
            public static Color GetSuccessColor(SupportedThemes theme)
            {
                return theme switch
                {
                    SupportedThemes.Office2016White => Color.FromArgb(46, 204, 113),
                    SupportedThemes.Office2016Black => Color.FromArgb(76, 175, 80),
                    SupportedThemes.Office2016DarkGray => Color.FromArgb(76, 175, 80),
                    SupportedThemes.Office2016Colorful => Color.FromArgb(46, 204, 113),
                    SupportedThemes.MaterialLight => Color.FromArgb(76, 175, 80),
                    _ => Color.FromArgb(46, 204, 113)
                };
            }

            /// <summary>
            /// Get theme-appropriate error color (red)
            /// </summary>
            public static Color GetErrorColor(SupportedThemes theme)
            {
                return theme switch
                {
                    SupportedThemes.Office2016White => Color.FromArgb(231, 76, 60),
                    SupportedThemes.Office2016Black => Color.FromArgb(244, 67, 54),
                    SupportedThemes.Office2016DarkGray => Color.FromArgb(244, 67, 54),
                    SupportedThemes.Office2016Colorful => Color.FromArgb(231, 76, 60),
                    SupportedThemes.MaterialLight => Color.FromArgb(244, 67, 54),
                    _ => Color.FromArgb(231, 76, 60)
                };
            }

            /// <summary>
            /// Get theme-appropriate warning color (orange)
            /// </summary>
            public static Color GetWarningColor(SupportedThemes theme)
            {
                return theme switch
                {
                    SupportedThemes.Office2016White => Color.FromArgb(230, 126, 34),
                    SupportedThemes.Office2016Black => Color.FromArgb(255, 152, 0),
                    SupportedThemes.Office2016DarkGray => Color.FromArgb(255, 152, 0),
                    SupportedThemes.Office2016Colorful => Color.FromArgb(230, 126, 34),
                    SupportedThemes.MaterialLight => Color.FromArgb(255, 152, 0),
                    _ => Color.FromArgb(230, 126, 34)
                };
            }

            /// <summary>
            /// Get theme-appropriate info color (blue)
            /// </summary>
            public static Color GetInfoColor(SupportedThemes theme)
            {
                return theme switch
                {
                    SupportedThemes.Office2016White => Color.FromArgb(52, 152, 219),
                    SupportedThemes.Office2016Black => Color.FromArgb(33, 150, 243),
                    SupportedThemes.Office2016DarkGray => Color.FromArgb(33, 150, 243),
                    SupportedThemes.Office2016Colorful => Color.FromArgb(52, 152, 219),
                    SupportedThemes.MaterialLight => Color.FromArgb(33, 150, 243),
                    _ => Color.FromArgb(52, 152, 219)
                };
            }

            /// <summary>
            /// Get theme-appropriate secondary color (purple)
            /// </summary>
            public static Color GetSecondaryColor(SupportedThemes theme)
            {
                return theme switch
                {
                    SupportedThemes.Office2016White => Color.FromArgb(155, 89, 182),
                    SupportedThemes.Office2016Black => Color.FromArgb(156, 39, 176),
                    SupportedThemes.Office2016DarkGray => Color.FromArgb(156, 39, 176),
                    SupportedThemes.Office2016Colorful => Color.FromArgb(155, 89, 182),
                    SupportedThemes.MaterialLight => Color.FromArgb(156, 39, 176),
                    _ => Color.FromArgb(155, 89, 182)
                };
            }

            /// <summary>
            /// Get theme-appropriate muted color (gray)
            /// </summary>
            public static Color GetMutedColor(SupportedThemes theme)
            {
                return theme switch
                {
                    SupportedThemes.Office2016White => Color.FromArgb(149, 165, 166),
                    SupportedThemes.Office2016Black => Color.FromArgb(158, 158, 158),
                    SupportedThemes.Office2016DarkGray => Color.FromArgb(158, 158, 158),
                    SupportedThemes.Office2016Colorful => Color.FromArgb(149, 165, 166),
                    SupportedThemes.MaterialLight => Color.FromArgb(158, 158, 158),
                    _ => Color.FromArgb(149, 165, 166)
                };
            }

            /// <summary>
            /// Get theme-appropriate dark color (crimson)
            /// </summary>
            public static Color GetDarkColor(SupportedThemes theme)
            {
                return theme switch
                {
                    SupportedThemes.Office2016White => Color.FromArgb(192, 57, 43),
                    SupportedThemes.Office2016Black => Color.FromArgb(211, 47, 47),
                    SupportedThemes.Office2016DarkGray => Color.FromArgb(211, 47, 47),
                    SupportedThemes.Office2016Colorful => Color.FromArgb(192, 57, 43),
                    SupportedThemes.MaterialLight => Color.FromArgb(211, 47, 47),
                    _ => Color.FromArgb(192, 57, 43)
                };
            }
        }

        /// <summary>
        /// Apply theme to any control (compatibility method)
        /// </summary>
        public static void ApplyThemeToControl(Control control, SupportedThemes theme)
        {
            ApplyTheme(control, theme);
        }

        /// <summary>
        /// Create enhanced SfDataGrid with Material styling
        /// ENHANCED: Added comprehensive safety checks and deferred initialization
        /// </summary>
        public static SfDataGrid? CreateEnhancedMaterialSfDataGrid()
        {
            // Always check for test mode using both the static flag and environment
            if (_testMode || IsTestEnvironment())
            {
                Console.WriteLine("🧪 BusBuddyThemeManager: Skipping SfDataGrid creation - test mode enabled");
                return null;
            }

            try
            {
                // Reference: https://help.syncfusion.com/windowsforms/datagrid/getting-started
                var grid = new SfDataGrid();

                // Check if grid was created successfully
                if (grid == null)
                {
                    Console.WriteLine("❌ Failed to create SfDataGrid instance");
                    return null;
                }

                // Apply basic initialization first
                Console.WriteLine("🔧 Initializing SfDataGrid with basic settings");

                // Defer enhancements and theming until the grid is properly initialized
                // This prevents NullReferenceExceptions during control creation
                grid.HandleCreated += (sender, e) =>
                {
                    try
                    {
                        Console.WriteLine("🔧 SfDataGrid handle created - applying enhancements");
                        SfDataGridEnhancements(grid);
                        ApplyTheme(grid, CurrentTheme);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error in deferred SfDataGrid initialization: {ex.Message}");
                    }
                };

                return grid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Critical error creating SfDataGrid: {ex.Message}");
                return null;
            }
        }

        private static bool IsTestEnvironment()
        {
            // Detect test environment by process name or command line
            return Environment.CommandLine.Contains("testhost") ||
                   Environment.CommandLine.Contains("vstest") ||
                   AppDomain.CurrentDomain.FriendlyName.Contains("test", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Apply enhancements to SfDataGrid
        /// ENHANCED: Added comprehensive null safety and initialization checks
        /// </summary>
        public static void SfDataGridEnhancements(SfDataGrid grid)
        {
            if (_testMode || grid == null)
            {
                if (_testMode)
                    Console.WriteLine("🧪 BusBuddyThemeManager: Skipping SfDataGrid enhancements - test mode enabled");
                if (grid == null)
                    Console.WriteLine("⚠️ BusBuddyThemeManager: SfDataGrid is null - skipping enhancements");
                return;
            }

            try
            {
                // Check if the grid is properly initialized and not disposed
                if (grid.IsDisposed)
                {
                    Console.WriteLine("⚠️ BusBuddyThemeManager: SfDataGrid is disposed - skipping enhancements");
                    return;
                }

                // Check if the grid is created but not yet initialized
                if (!grid.IsHandleCreated)
                {
                    Console.WriteLine("⚠️ BusBuddyThemeManager: SfDataGrid handle not created - deferring enhancements");
                    return;
                }

                // Apply basic settings first
                grid.AllowEditing = false;
                grid.AllowSorting = true;
                grid.AllowFiltering = true;
                grid.ShowGroupDropArea = true;

                // Apply styling with additional null checks
                if (grid.Style?.HeaderStyle != null)
                {
                    grid.Style.HeaderStyle.BackColor = ThemeColors.GetPrimaryColor(CurrentTheme);
                    grid.Style.HeaderStyle.TextColor = Color.White;
                    Console.WriteLine("✅ Applied SfDataGrid style enhancements");
                }
                else
                {
                    Console.WriteLine("⚠️ BusBuddyThemeManager: SfDataGrid Style or HeaderStyle is null - skipping style enhancements");
                }
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine($"❌ NullReferenceException in SfDataGrid enhancements: {ex.Message}");
                Console.WriteLine("🔧 This indicates the SfDataGrid internal state is not properly initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error applying SfDataGrid enhancements: {ex.Message}");
            }
        }

        /// <summary>
        /// Create styled label
        /// </summary>
        public static Label CreateStyledLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = Typography.GetLabelFont(),
                ForeColor = ThemeColors.GetTextColor(CurrentTheme),
                AutoSize = true
            };
        }

        /// <summary>
        /// Create styled text box
        /// </summary>
        public static TextBox CreateStyledTextBox(string placeholder = "")
        {
            var textBox = new TextBox
            {
                Font = Typography.GetDefaultFont(),
                BackColor = ThemeColors.GetBackgroundColor(CurrentTheme),
                ForeColor = ThemeColors.GetTextColor(CurrentTheme)
            };

            if (!string.IsNullOrEmpty(placeholder))
            {
                textBox.Text = placeholder;
                textBox.ForeColor = ThemeColors.GetTextColor(CurrentTheme);
            }

            return textBox;
        }

        /// <summary>
        /// Create styled button
        /// </summary>
        public static Button CreateStyledButton(string text)
        {
            return new Button
            {
                Text = text,
                Font = Typography.GetDefaultFont(),
                BackColor = ThemeColors.GetPrimaryColor(CurrentTheme),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 35)
            };
        }

        /// <summary>
        /// Dark theme compatibility properties (extended)
        /// </summary>
        public static class DarkTheme
        {
            public static Color BackgroundColor => ThemeColors.GetBackgroundColor(SupportedThemes.Office2016Black);
            public static Color TextColor => ThemeColors.GetTextColor(SupportedThemes.Office2016Black);
            public static Color PrimaryColor => ThemeColors.GetPrimaryColor(SupportedThemes.Office2016Black);
            public static Color Surface => ThemeColors.GetBackgroundColor(SupportedThemes.Office2016Black);
            public static Color Error => Color.FromArgb(244, 67, 54);
            public static Color ErrorContainer => Color.FromArgb(211, 47, 47);
            public static Color OnErrorContainer => Color.White;
            public static Color OnSurface => ThemeColors.GetTextColor(SupportedThemes.Office2016Black);
            public static Color Success => Color.FromArgb(76, 175, 80);
        }

        /// <summary>
        /// Material theme compatibility (extended)
        /// </summary>
        public static class MaterialTheme
        {
            public static Font DefaultFont => Typography.GetDefaultFont();
            public static bool IsDarkMode { get; set; } = false;
            public static int DefaultButtonHeight => 35;
            public static int DefaultControlHeight => 35;
            public static Color Primary => Color.FromArgb(33, 150, 243);
            public static Color Background => IsDarkMode ? Color.FromArgb(48, 48, 48) : Color.White;
            public static Color TextPrimary => IsDarkMode ? Color.White : Color.Black;
        }

        /// <summary>
        /// Typography helpers (extended)
        /// </summary>
        public static class Typography
        {
            public static Font GetDefaultFont() => new Font("Segoe UI", 9, FontStyle.Regular);
            public static Font GetLabelFont() => new Font("Segoe UI", 9, FontStyle.Regular);
            public static Font GetHeaderFont() => new Font("Segoe UI", 12, FontStyle.Bold);
            public static Font GetBodyMedium() => new Font("Segoe UI", 9, FontStyle.Regular);
        }

        #region Syncfusion Control Safety Validation

        /// <summary>
        /// Comprehensive safety check for Syncfusion controls before theming
        /// Prevents NullReferenceExceptions during control initialization
        /// </summary>
        public static bool IsSyncfusionControlSafe(Control control)
        {
            if (control == null)
            {
                Console.WriteLine("⚠️ Control is null");
                return false;
            }

            if (control.IsDisposed)
            {
                Console.WriteLine("⚠️ Control is disposed");
                return false;
            }

            if (!control.IsHandleCreated)
            {
                Console.WriteLine("⚠️ Control handle not created - may cause NullReferenceException");
                return false;
            }

            // Additional checks for specific Syncfusion controls
            if (control is SfDataGrid dataGrid)
            {
                try
                {
                    // Test if the internal state is accessible
                    var _ = dataGrid.AllowEditing;
                    return true;
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine("⚠️ SfDataGrid internal state not initialized");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Safe apply theme method that validates control state first
        /// </summary>
        public static bool SafeApplyTheme(Control control, SupportedThemes theme)
        {
            if (!IsSyncfusionControlSafe(control))
            {
                Console.WriteLine($"🔧 Deferring theme application for {control?.GetType().Name ?? "null control"}");
                return false;
            }

            try
            {
                // Apply theme based on control type
                switch (control)
                {
                    case SfForm sfForm:
                        ApplyTheme(sfForm, theme);
                        break;
                    case SfDataGrid dataGrid:
                        ApplyTheme(dataGrid, theme);
                        break;
                    case Syncfusion.Windows.Forms.Tools.NavigationDrawer navDrawer:
                        ApplyTheme(navDrawer, theme);
                        break;
                    case Syncfusion.Windows.Forms.Tools.TabControlAdv tabControl:
                        ApplyTheme(tabControl, theme);
                        break;
                    default:
                        ApplyTheme(control, theme);
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Safe theme application failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Initialize Syncfusion control with proper error handling
        /// </summary>
        public static void SafeInitializeSyncfusionControl(Control control, Action<Control> initializationAction)
        {
            if (control == null || initializationAction == null)
                return;

            try
            {
                if (control.IsHandleCreated)
                {
                    // Control is ready for initialization
                    initializationAction(control);
                }
                else
                {
                    // Defer initialization until handle is created
                    control.HandleCreated += (sender, e) =>
                    {
                        try
                        {
                            initializationAction(control);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Deferred initialization failed: {ex.Message}");
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Safe initialization setup failed: {ex.Message}");
            }
        }

        #endregion

        #region DPI and Legacy Compatibility Methods

        /// <summary>
        /// Get DPI scale factor for the form
        /// </summary>
        public static float GetDpiScale(Form form)
        {
            using (var graphics = form.CreateGraphics())
            {
                return graphics.DpiX / 96f;
            }
        }

        /// <summary>
        /// Check if form is in high DPI mode
        /// </summary>
        public static bool IsHighDpiMode(Form form)
        {
            return GetDpiScale(form) > 1.0f;
        }

        /// <summary>
        /// Load dark theme - compatibility method
        /// </summary>
        public static void LoadDarkTheme()
        {
            Console.WriteLine("🎨 BusBuddy: Dark theme loading handled by Syncfusion");
        }

        /// <summary>
        /// Apply high DPI material theme - compatibility method
        /// </summary>
        public static void ApplyHighDpiMaterialTheme(Form form)
        {
            try
            {
                float dpiScale = GetDpiScale(form);
                if (dpiScale > 1.0f)
                {
                    Console.WriteLine($"🎨 BusBuddy: Applying high DPI theme adjustments (scale: {dpiScale:F2})");
                    // High DPI adjustments would go here if needed
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error applying high DPI theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Get safe font with fallback
        /// </summary>
        public static Font GetSafeFont(string fontName, float size, FontStyle style)
        {
            try
            {
                return new Font(fontName, size, style);
            }
            catch
            {
                return new Font(FontFamily.GenericSansSerif, size, style);
            }
        }

        #endregion
    }
}
