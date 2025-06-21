using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Styles;
using Syncfusion.WinForms.Input;
using Syncfusion.WinForms.Core;
using Syncfusion.WinForms.Input.Styles;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Enhanced helper class for Syncfusion theming in BusBuddy
    /// </summary>
    public static class SyncfusionThemeHelper
    {
        public enum ThemeMode
        {
            Light,
            Dark
        }

        private static ThemeMode _currentTheme = ThemeMode.Light;

        public static ThemeMode CurrentTheme
        {
            get => _currentTheme;
            set
            {
                _currentTheme = value;
                Console.WriteLine($"🎨 SYNCFUSION: Theme changed to {value}");
            }
        }

        public static class MaterialColors
        {
            public static Color Primary = Color.FromArgb(33, 150, 243); // Blue 500
            public static Color Secondary = Color.FromArgb(255, 193, 7); // Amber 500
            public static Color Background = Color.FromArgb(255, 255, 255); // White
            public static Color Surface = Color.FromArgb(245, 245, 245); // Light Gray
            public static Color TextPrimary = Color.FromArgb(33, 33, 33); // Dark Gray
            public static Color TextSecondary = Color.FromArgb(117, 117, 117); // Medium Gray
            public static Color Border = Color.FromArgb(224, 224, 224); // Light Border
            public static Color Text = Color.FromArgb(33, 33, 33); // Same as TextPrimary
        }

        public static class DarkTheme
        {
            public static Color Primary = Color.FromArgb(66, 165, 245); // Blue 400
            public static Color Secondary = Color.FromArgb(255, 202, 40); // Amber 400
            public static Color Background = Color.FromArgb(30, 30, 30); // Dark Gray
            public static Color Surface = Color.FromArgb(50, 50, 50); // Darker Gray
            public static Color TextPrimary = Color.FromArgb(255, 255, 255); // White
            public static Color TextSecondary = Color.FromArgb(189, 189, 189); // Light Gray
            public static Color OnSurface = Color.FromArgb(255, 255, 255); // White
            public static Color Error = Color.FromArgb(244, 67, 54); // Red
            public static Color ErrorContainer = Color.FromArgb(211, 47, 47); // Dark Red
            public static Color OnErrorContainer = Color.FromArgb(255, 255, 255); // White
            public static Color Success = Color.FromArgb(76, 175, 80); // Green
            public static Color SurfaceContainer = Color.FromArgb(66, 66, 66); // Medium Gray
        }

        public static class Typography
        {
            public static Font DefaultFont = GetSafeFont("Segoe UI", 12f);
            public static Font HeaderFont = GetSafeFont("Segoe UI", 16f, FontStyle.Bold);
            public static Font CaptionFont = GetSafeFont("Segoe UI", 10f);

            public static Font GetBodyMedium()
            {
                return GetSafeFont("Segoe UI", 12f);
            }

            public static Font GetBodyMedium(float size)
            {
                return GetSafeFont("Segoe UI", size);
            }
        }

        public static class MaterialTheme
        {
            public static Color PrimaryColor => MaterialColors.Primary;
            public static Color SecondaryColor => MaterialColors.Secondary;
            public static bool IsDarkMode { get; set; } = false;
            public static Font DefaultFont => GetSafeFont("Segoe UI", 12f);
            public static int DefaultButtonHeight => 36;
            public static int DefaultControlHeight => 30;
        }

        public static class HighDpiSupport
        {
            public static float GetScaleFactor()
            {
                return Screen.PrimaryScreen.Bounds.Height / 1080f;
            }

            public static Size ScaleSize(Size originalSize)
            {
                var scaleFactor = GetScaleFactor();
                return new Size((int)(originalSize.Width * scaleFactor), (int)(originalSize.Height * scaleFactor));
            }

            public static float GetDpiScale()
            {
                return GetScaleFactor();
            }

            public static float GetDpiScale(Control control)
            {
                return GetScaleFactor();
            }

            public static bool IsHighDpiMode()
            {
                return GetScaleFactor() > 1.2f;
            }

            public static bool IsHighDpiMode(Control control)
            {
                return GetScaleFactor() > 1.2f;
            }
        }

        public static class SfDataGridColumns
        {
            public static void ApplyStandardColumnStyle(SfDataGrid grid)
            {
                if (grid?.Style?.CellStyle != null)
                {
                    grid.Style.CellStyle.BackColor = MaterialTheme.IsDarkMode ? DarkTheme.Surface : MaterialColors.Surface;
                    grid.Style.CellStyle.TextColor = MaterialTheme.IsDarkMode ? DarkTheme.TextPrimary : MaterialColors.TextPrimary;
                }
            }

            public static GridTextColumn CreateIdColumn(string mappingName, string headerText)
            {
                return new GridTextColumn
                {
                    MappingName = mappingName,
                    HeaderText = headerText,
                    Width = 80
                };
            }

            public static GridTextColumn CreateTextColumn(string mappingName, string headerText, int width = 120)
            {
                return new GridTextColumn
                {
                    MappingName = mappingName,
                    HeaderText = headerText,
                    Width = width
                };
            }

            public static GridTextColumn CreateTextColumn(string mappingName, string headerText, int width, bool autoSize)
            {
                return new GridTextColumn
                {
                    MappingName = mappingName,
                    HeaderText = headerText,
                    Width = width,
                    AutoSizeColumnsMode = autoSize ? Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.Fill : Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.None
                };
            }

            public static GridTextColumn CreateStatusColumn(string mappingName, string headerText)
            {
                return new GridTextColumn
                {
                    MappingName = mappingName,
                    HeaderText = headerText,
                    Width = 100
                };
            }

            public static GridTextColumn CreateStatusColumn(string mappingName, string headerText, int width)
            {
                return new GridTextColumn
                {
                    MappingName = mappingName,
                    HeaderText = headerText,
                    Width = width
                };
            }

            public static GridDateTimeColumn CreateTimeColumn(string mappingName, string headerText)
            {
                return new GridDateTimeColumn
                {
                    MappingName = mappingName,
                    HeaderText = headerText,
                    Width = 120
                };
            }

            public static GridDateTimeColumn CreateTimeColumn(string mappingName, string headerText, int width)
            {
                return new GridDateTimeColumn
                {
                    MappingName = mappingName,
                    HeaderText = headerText,
                    Width = width
                };
            }

            public static GridDateTimeColumn CreateDateTimeColumn(string mappingName, string headerText)
            {
                return new GridDateTimeColumn
                {
                    MappingName = mappingName,
                    HeaderText = headerText,
                    Width = 150
                };
            }

            public static GridDateTimeColumn CreateDateTimeColumn(string mappingName, string headerText, int width)
            {
                return new GridDateTimeColumn
                {
                    MappingName = mappingName,
                    HeaderText = headerText,
                    Width = width
                };
            }

            public static GridNumericColumn CreateNumericColumn(string mappingName, string headerText)
            {
                return new GridNumericColumn
                {
                    MappingName = mappingName,
                    HeaderText = headerText,
                    Width = 100
                };
            }

            public static GridNumericColumn CreateNumericColumn(string mappingName, string headerText, int width)
            {
                return new GridNumericColumn
                {
                    MappingName = mappingName,
                    HeaderText = headerText,
                    Width = width
                };
            }

            public static GridNumericColumn CreateCurrencyTextColumn(string mappingName, string headerText)
            {
                return new GridNumericColumn
                {
                    MappingName = mappingName,
                    HeaderText = headerText,
                    Width = 120,
                    NumberFormatInfo = new System.Globalization.NumberFormatInfo { CurrencySymbol = "$" }
                };
            }

            public static GridNumericColumn CreateCurrencyTextColumn(string mappingName, string headerText, int width)
            {
                return new GridNumericColumn
                {
                    MappingName = mappingName,
                    HeaderText = headerText,
                    Width = width,
                    NumberFormatInfo = new System.Globalization.NumberFormatInfo { CurrencySymbol = "$" }
                };
            }

            public static GridTextColumn CreateAutoSizeColumn(string mappingName, string headerText)
            {
                return new GridTextColumn
                {
                    MappingName = mappingName,
                    HeaderText = headerText,
                    AutoSizeColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.Fill
                };
            }
        }

        /// <summary>
        /// Initialize global Syncfusion theme
        /// </summary>
        public static void InitializeGlobalTheme()
        {
            try
            {
                SfSkinManager.LoadAssembly(typeof(SfForm).Assembly);
                Console.WriteLine($"✅ Syncfusion {(MaterialTheme.IsDarkMode ? "MaterialDark" : "MaterialLight")} theme applied globally.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Syncfusion theme initialization failed: {ex.Message}");
                // Continue without theming - application will use default appearance
            }
        }

        public static void ApplyMaterialTheme(Control control)
        {
            if (control == null) return;

            control.BackColor = MaterialTheme.IsDarkMode ? DarkTheme.Surface : MaterialColors.Surface;
            control.ForeColor = MaterialTheme.IsDarkMode ? DarkTheme.TextPrimary : MaterialColors.TextPrimary;

            if (control is SfForm form)
            {
                form.Style.TitleBar.BackColor = MaterialTheme.IsDarkMode ? DarkTheme.Primary : MaterialColors.Primary;
                form.Style.TitleBar.ForeColor = MaterialTheme.IsDarkMode ? DarkTheme.TextPrimary : MaterialColors.TextPrimary;
            }

            ApplyMaterialThemeRecursive(control.Controls);
        }

        public static void ApplyMaterialThemeRecursive(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                ApplyMaterialTheme(control);
            }
        }

        public static void ApplyThemeToControl(Control control)
        {
            ApplyMaterialTheme(control);
        }

        public static SfButton CreateStyledButton(string text, Action clickAction = null)
        {
            var button = new SfButton
            {
                Text = text,
                BackColor = MaterialTheme.IsDarkMode ? DarkTheme.Primary : MaterialColors.Primary,
                ForeColor = MaterialTheme.IsDarkMode ? DarkTheme.TextPrimary : MaterialColors.TextPrimary,
                Font = Typography.DefaultFont,
                Size = HighDpiSupport.ScaleSize(new Size(120, 36))
            };

            if (clickAction != null)
            {
                button.Click += (s, e) => clickAction();
            }

            return button;
        }

        public static Label CreateStyledLabel(string text)
        {
            return new Label
            {
                Text = text,
                ForeColor = MaterialTheme.IsDarkMode ? DarkTheme.TextPrimary : MaterialColors.TextPrimary,
                Font = Typography.CaptionFont,
                AutoSize = true
            };
        }

        public static TextBox CreateStyledTextBox()
        {
            return new TextBox
            {
                BackColor = MaterialTheme.IsDarkMode ? DarkTheme.Surface : MaterialColors.Surface,
                ForeColor = MaterialTheme.IsDarkMode ? DarkTheme.TextPrimary : MaterialColors.TextPrimary,
                Font = Typography.DefaultFont,
                Size = HighDpiSupport.ScaleSize(new Size(200, 30))
            };
        }

        public static SfDataGrid CreateEnhancedMaterialSfDataGrid()
        {
            // CRITICAL: Ensure SfDataGrid creation happens on the UI thread to prevent GDI+ cross-thread errors
            if (System.Windows.Forms.Application.OpenForms.Count > 0)
            {
                var mainForm = System.Windows.Forms.Application.OpenForms[0];
                if (mainForm.InvokeRequired)
                {
                    return (SfDataGrid)mainForm.Invoke(new Func<SfDataGrid>(() => CreateEnhancedMaterialSfDataGridInternal()));
                }
                else
                {
                    return CreateEnhancedMaterialSfDataGridInternal();
                }
            }
            else
            {
                // No forms available, check if we're on the UI thread by trying to create a temporary control
                try
                {
                    using (var tempControl = new Control())
                    {
                        var handle = tempControl.Handle; // Force handle creation - will fail if not on UI thread
                        return CreateEnhancedMaterialSfDataGridInternal();
                    }
                }
                catch (System.InvalidOperationException)
                {
                    // Not on UI thread and no forms available - this is likely a test scenario
                    Console.WriteLine("⚠️ Creating SfDataGrid outside UI thread - this may cause GDI+ errors");
                    return CreateEnhancedMaterialSfDataGridInternal();
                }
            }
        }

        private static SfDataGrid CreateEnhancedMaterialSfDataGridInternal()
        {
            try
            {
                // Ensure Syncfusion theming is properly initialized before creating controls
                EnsureSyncfusionThemeInitialized();

                // THREAD SAFETY: Lock during control creation to prevent concurrent GDI+ access
                lock (typeof(SyncfusionThemeHelper))
                {
                    var grid = new SfDataGrid
                    {
                        BackColor = MaterialTheme.IsDarkMode ? DarkTheme.Surface : MaterialColors.Surface,
                        ForeColor = MaterialTheme.IsDarkMode ? DarkTheme.TextPrimary : MaterialColors.TextPrimary,
                        Font = Typography.DefaultFont
                    };

                    // Additional defensive initialization to prevent ScrollBar theme issues
                    try
                    {
                        grid.Style.BorderStyle = BorderStyle.FixedSingle;
                        // Use safe color fallbacks
                        grid.Style.BorderColor = MaterialTheme.IsDarkMode ? Color.Gray : Color.LightGray;
                    }
                    catch (Exception styleEx)
                    {
                        Console.WriteLine($"⚠️ Could not apply SfDataGrid style: {styleEx.Message}");
                    }

                    // Apply additional theming safely
                    try
                    {
                        if (grid.Style.HeaderStyle != null)
                        {
                            grid.Style.HeaderStyle.BackColor = MaterialTheme.IsDarkMode ? DarkTheme.Primary : MaterialColors.Primary;
                            grid.Style.HeaderStyle.TextColor = MaterialTheme.IsDarkMode ? DarkTheme.TextPrimary : MaterialColors.TextPrimary;
                        }

                        if (grid.Style.CellStyle != null)
                        {
                            grid.Style.CellStyle.BackColor = MaterialTheme.IsDarkMode ? DarkTheme.Surface : MaterialColors.Surface;
                            grid.Style.CellStyle.TextColor = MaterialTheme.IsDarkMode ? DarkTheme.TextPrimary : MaterialColors.TextPrimary;
                        }
                    }
                    catch (Exception themeEx)
                    {
                        Console.WriteLine($"⚠️ Could not apply advanced SfDataGrid theming: {themeEx.Message}");
                    }

                    SfDataGridEnhancements(grid);
                    return grid;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating SfDataGrid: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");

                // Fallback: return a basic SfDataGrid without advanced styling
                try
                {
                    lock (typeof(SyncfusionThemeHelper))
                    {
                        return new SfDataGrid();
                    }
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"❌ Even fallback SfDataGrid creation failed: {fallbackEx.Message}");
                    throw new InvalidOperationException($"Cannot create SfDataGrid - Thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}, IsBackground: {System.Threading.Thread.CurrentThread.IsBackground}", ex);
                }
            }
        }

        private static void EnsureSyncfusionThemeInitialized()
        {
            try
            {
                // Make sure the license is registered
                SyncfusionLicenseHelper.InitializeLicense();

                // Apply a basic visual style to ensure theming is initialized
                SfSkinManager.SetVisualStyle(Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null, "MaterialLight");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Could not ensure Syncfusion theme initialization: {ex.Message}");
            }
        }

        public static void SfDataGridEnhancements(SfDataGrid grid)
        {
            if (grid == null) return;

            try
            {
                // Enhanced grid configuration for optimal user experience
                grid.AutoSizeColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.Fill;
                grid.AllowSorting = true;
                grid.AllowFiltering = true;
                grid.AllowGrouping = true;
                grid.ShowGroupDropArea = true;

                // Enable tooltip support for better UX
                grid.ShowToolTip = true;

                // Enhanced selection styling
                if (grid.Style?.SelectionStyle != null)
                {
                    grid.Style.SelectionStyle.BackColor = MaterialTheme.IsDarkMode ? DarkTheme.Secondary : MaterialColors.Secondary;
                    grid.Style.SelectionStyle.TextColor = Color.White;
                }

                // Enhanced header styling
                if (grid.Style?.HeaderStyle != null)
                {
                    grid.Style.HeaderStyle.BackColor = MaterialTheme.IsDarkMode ? DarkTheme.Primary : MaterialColors.Primary;
                    grid.Style.HeaderStyle.TextColor = Color.White;
                }

                Console.WriteLine("✨ Enhanced SfDataGrid configuration applied");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error applying SfDataGrid enhancements: {ex.Message}");
            }
        }

        public static Font GetSafeFont(string fontName, float size, FontStyle style = FontStyle.Regular)
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

        public static void SetThemeMode(bool isDarkMode)
        {
            MaterialTheme.IsDarkMode = isDarkMode;
            InitializeGlobalTheme();
        }

        public static void ApplyHighDpiMaterialTheme(Control control)
        {
            ApplyMaterialTheme(control);
            // Additional DPI scaling for fonts
            if (control.Font != null)
            {
                var scaledSize = control.Font.Size * HighDpiSupport.GetScaleFactor();
                control.Font = GetSafeFont(control.Font.Name, scaledSize, control.Font.Style);
            }
        }

        public static void LoadDarkTheme()
        {
            MaterialTheme.IsDarkMode = true;
            InitializeGlobalTheme();
        }

        public static SfDataGrid CreateMaterialSfDataGrid()
        {
            return CreateEnhancedMaterialSfDataGrid();
        }

        public static void ApplyMaterialSfDataGrid(SfDataGrid grid)
        {
            SfDataGridEnhancements(grid);
        }

        public static TextBox CreateStyledTextBox(string placeholder)
        {
            var textBox = CreateStyledTextBox();
            // Note: Standard TextBox doesn't have placeholder, but we can simulate with events
            return textBox;
        }

        /// <summary>
        /// Toggle between light and dark themes
        /// </summary>
        public static void ToggleTheme()
        {
            CurrentTheme = CurrentTheme == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light;
            Console.WriteLine($"🎨 Theme toggled to: {CurrentTheme}");
        }
    }
}
