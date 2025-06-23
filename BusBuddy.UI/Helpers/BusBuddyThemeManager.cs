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
                Console.WriteLine($"🎨 BusBuddy theme changed to {value}");
            }
        }

        /// <summary>
        /// Apply theme to SfForm using official Syncfusion ThemeName property
        /// </summary>
        public static void ApplyTheme(SfForm form, SupportedThemes theme)
        {
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
        /// </summary>
        public static void ApplyTheme(Syncfusion.WinForms.DataGrid.SfDataGrid dataGrid, SupportedThemes theme)
        {
            try
            {
                var themeName = GetThemeName(theme);
                dataGrid.ThemeName = themeName;
                Console.WriteLine($"✅ Applied {themeName} theme to data grid");
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
        /// </summary>
        public static SfDataGrid CreateEnhancedMaterialSfDataGrid()
        {
            var grid = new SfDataGrid();
            SfDataGridEnhancements(grid);
            ApplyTheme(grid, CurrentTheme);
            return grid;
        }

        /// <summary>
        /// Apply enhancements to SfDataGrid
        /// </summary>
        public static void SfDataGridEnhancements(SfDataGrid grid)
        {
            try
            {
                grid.AllowEditing = false;
                grid.AllowSorting = true;
                grid.AllowFiltering = true;
                grid.ShowGroupDropArea = true;
                grid.Style.HeaderStyle.BackColor = ThemeColors.GetPrimaryColor(CurrentTheme);
                grid.Style.HeaderStyle.TextColor = Color.White;
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

        /// <summary>
        /// Apply theme to ChartControl using manual styling
        /// ChartControl doesn't have ThemeName property, so we apply colors manually
        /// TEMPORARILY DISABLED - Testing graphics rendering issues
        /// </summary>
        public static void ApplyTheme(Syncfusion.Windows.Forms.Chart.ChartControl chart, SupportedThemes theme)
        {
            try
            {
                // DISABLE all ChartControl theming for graphics troubleshooting
                Console.WriteLine($"⚠️ ChartControl theming DISABLED for graphics troubleshooting - skipping theme: {theme}");
                return;

                // Original theming code commented out for troubleshooting
                /*
                var bgColor = ThemeColors.GetBackgroundColor(theme);
                var primaryColor = ThemeColors.GetPrimaryColor(theme);
                var textColor = ThemeColors.GetTextColor(theme);

                chart.BackColor = bgColor;
                chart.BackInterior = new Syncfusion.Drawing.BrushInfo(bgColor);

                // Apply theme to chart area
                if (chart.ChartArea != null)
                {
                    chart.ChartArea.BackInterior = new Syncfusion.Drawing.BrushInfo(bgColor);
                    chart.ChartArea.BorderColor = textColor;
                }

                // Apply theme to legend if present
                if (chart.Legend != null)
                {
                    chart.Legend.BackInterior = new Syncfusion.Drawing.BrushInfo(bgColor);
                    chart.Legend.Font = new Font("Segoe UI", 9);
                }

                // Apply theme to title
                if (chart.Title != null)
                {
                    chart.Title.ForeColor = textColor;
                    chart.Title.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                }

                // Apply theme to axes
                if (chart.PrimaryXAxis != null)
                {
                    chart.PrimaryXAxis.TitleColor = textColor;
                    chart.PrimaryXAxis.GridLineType.ForeColor = Color.FromArgb(50, textColor.R, textColor.G, textColor.B);
                }

                if (chart.PrimaryYAxis != null)
                {
                    chart.PrimaryYAxis.TitleColor = textColor;
                    chart.PrimaryYAxis.GridLineType.ForeColor = Color.FromArgb(50, textColor.R, textColor.G, textColor.B);
                }

                // Apply theme to series
                foreach (var series in chart.Series)
                {
                    if (series is Syncfusion.Windows.Forms.Chart.ChartSeries chartSeries)
                    {
                        chartSeries.Style.Interior = new Syncfusion.Drawing.BrushInfo(primaryColor);
                        chartSeries.Style.Border.Color = primaryColor;
                    }
                }

                Console.WriteLine($"✅ Applied {theme} theme to chart control");
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error applying theme to chart control: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply theme to RadialGauge using manual styling
        /// RadialGauge doesn't have ThemeName property, so we apply colors manually
        /// TEMPORARILY DISABLED - Testing graphics rendering issues
        /// </summary>
        public static void ApplyTheme(Syncfusion.Windows.Forms.Gauge.RadialGauge gauge, SupportedThemes theme)
        {
            try
            {
                // DISABLE all RadialGauge theming for graphics troubleshooting
                Console.WriteLine($"⚠️ RadialGauge theming DISABLED for graphics troubleshooting - skipping theme: {theme}");
                return;

                // Original theming code commented out
                /*
                var bgColor = ThemeColors.GetBackgroundColor(theme);
                var primaryColor = ThemeColors.GetPrimaryColor(theme);
                var textColor = ThemeColors.GetTextColor(theme);

                gauge.BackgroundGradientStartColor = bgColor;
                gauge.BackgroundGradientEndColor = bgColor;
                gauge.ForeColor = textColor;

                // Apply theme to frame
                try
                {
                    gauge.OuterFrameGradientStartColor = Color.FromArgb(100, textColor.R, textColor.G, textColor.B);
                    gauge.OuterFrameGradientEndColor = Color.FromArgb(50, textColor.R, textColor.G, textColor.B);
                }
                catch
                {
                    // Frame properties may not be available on all gauge configurations
                }

                Console.WriteLine($"✅ Applied {theme} theme to radial gauge");
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error applying theme to radial gauge: {ex.Message}");
            }
        }

        #endregion
    }
}
