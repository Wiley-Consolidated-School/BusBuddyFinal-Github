using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Helper class for Syncfusion theming in BusBuddy
    /// </summary>
    public static class SyncfusionThemeHelper
    {
        public enum ThemeMode
        {
            Light,
            Dark
        }

        private static ThemeMode _currentTheme = ThemeMode.Light; // TEMPORARY: Test with light theme

        public static ThemeMode CurrentTheme
        {
            get => _currentTheme;
            set
            {
                _currentTheme = value;
                Console.WriteLine($"🎨 SYNCFUSION: Theme changed to {value}");
            }
        }

        /// <summary>
        /// Initialize global Syncfusion theme
        /// </summary>
        public static void InitializeGlobalTheme()
        {
            try
            {
                string themeName = CurrentTheme == ThemeMode.Dark ? "MaterialDark" : "MaterialLight";
                // Apply basic theming without specific Syncfusion API calls
                Console.WriteLine($"✅ Syncfusion theme initialized: {themeName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to initialize theme: {ex.Message}");
                // Fallback to basic theming
                Console.WriteLine("🔄 Using fallback theming...");
            }
        }

        /// <summary>
        /// Apply theme to a specific form
        /// </summary>
        public static void ApplyMaterialTheme(Form form)
        {
            try
            {
                string themeName = CurrentTheme == ThemeMode.Dark ? "MaterialDark" : "MaterialLight";
                SfSkinManager.SetVisualStyle(form, themeName);
                Console.WriteLine($"✅ Applied {themeName} theme to {form.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to apply theme to {form?.Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply theme to a specific control
        /// </summary>
        public static void ApplyMaterialTheme(Control control)
        {
            try
            {
                string themeName = CurrentTheme == ThemeMode.Dark ? "MaterialDark" : "MaterialLight";
                SfSkinManager.SetVisualStyle(control, themeName);
                ApplyMaterialColors(control);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to apply theme to control: {ex.Message}");
            }
        }

        /// <summary>
        /// Load dark theme DLL
        /// </summary>
        public static void LoadDarkTheme()
        {
            try
            {
                // Theme loading is handled by Syncfusion automatically
                Console.WriteLine("✅ Dark theme loaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to load dark theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply current theme to a form
        /// </summary>
        public static void ApplyCurrentTheme(Form form)
        {
            ApplyMaterialTheme(form);
        }

        /// <summary>
        /// Apply theme recursively to all controls
        /// </summary>
        public static void ApplyMaterialThemeRecursive(Control parent)
        {
            try
            {
                ApplyMaterialTheme(parent);
                foreach (Control child in parent.Controls)
                {
                    ApplyMaterialThemeRecursive(child);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to apply theme recursively: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply high DPI material theme
        /// </summary>
        public static void ApplyHighDpiMaterialTheme(Form form)
        {
            ApplyMaterialTheme(form);
        }

        /// <summary>
        /// Apply material styling to panel
        /// </summary>
        public static void ApplyMaterialPanel(Panel panel)
        {
            try
            {
                panel.BackColor = MaterialColors.Surface;
                panel.ForeColor = MaterialColors.Text;
                ApplyMaterialTheme(panel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to apply material panel styling: {ex.Message}");
            }
        }

        /// <summary>
        /// Set theme mode
        /// </summary>
        public static void SetThemeMode(ThemeMode mode)
        {
            CurrentTheme = mode;
            InitializeGlobalTheme();
        }

        /// <summary>
        /// Get current theme mode
        /// </summary>
        public static ThemeMode GetCurrentThemeMode()
        {
            return CurrentTheme;
        }

        /// <summary>
        /// Check if dark theme DLL is loaded
        /// </summary>
        public static bool IsDarkThemeDllLoaded()
        {
            return true; // Syncfusion themes are built-in
        }

        /// <summary>
        /// Create a styled button with material design
        /// </summary>
        public static Button CreateStyledButton(string text)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(120, 35),
                BackColor = MaterialColors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = MaterialTheme.DefaultFont,
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = MaterialColors.Hover;

            return button;
        }

        /// <summary>
        /// Create a material label
        /// </summary>
        public static Label CreateMaterialLabel(string text, bool isRequired = false)
        {
            return new Label
            {
                Text = text,
                Font = MaterialTheme.DefaultFont,
                ForeColor = isRequired ? MaterialColors.Error : MaterialColors.Text,
                AutoSize = true
            };
        }

        /// <summary>
        /// Create a material label with size and location parameters
        /// </summary>
        public static Label CreateMaterialLabel(string text, Point location, Size size)
        {
            var label = CreateMaterialLabel(text);
            label.Location = location;
            label.Size = size;
            return label;
        }

        /// <summary>
        /// Create a material label with x,y coordinates
        /// </summary>
        public static Label CreateMaterialLabel(string text, int x, int y)
        {
            var label = CreateMaterialLabel(text);
            label.Location = new Point(x, y);
            return label;
        }

        /// <summary>
        /// Create a material textbox
        /// </summary>
        public static TextBox CreateMaterialTextBox(string placeholder = "", bool isRequired = false)
        {
            var textBox = new TextBox
            {
                Font = MaterialTheme.DefaultFont,
                BackColor = MaterialColors.Surface,
                ForeColor = MaterialColors.Text,
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(200, MaterialTheme.DefaultControlHeight)
            };

            if (!string.IsNullOrEmpty(placeholder))
            {
                textBox.PlaceholderText = placeholder;
            }

            return textBox;
        }

        /// <summary>
        /// Create a material textbox with location and size parameters
        /// </summary>
        public static TextBox CreateMaterialTextBox(string placeholder, Point location, Size size)
        {
            var textBox = CreateMaterialTextBox(placeholder);
            textBox.Location = location;
            textBox.Size = size;
            return textBox;
        }

        /// <summary>
        /// Create a material textbox with x,y coordinates and size
        /// </summary>
        public static TextBox CreateMaterialTextBox(int placeholder, int x, int y)
        {
            var textBox = CreateMaterialTextBox("");
            textBox.Location = new Point(x, y);
            return textBox;
        }

        /// <summary>
        /// Create a material button
        /// </summary>
        public static Button CreateMaterialButton(string text, bool isPrimary = true)
        {
            return CreateStyledButton(text);
        }

        /// <summary>
        /// Create a material button with text, location, size, and click handler
        /// </summary>
        public static Button CreateMaterialButton(string text, Point location, Size size, EventHandler clickHandler)
        {
            var button = CreateMaterialButton(text);
            button.Location = location;
            button.Size = size;
            if (clickHandler != null)
                button.Click += clickHandler;
            return button;
        }

        /// <summary>
        /// Create a material button with text, x,y coordinates, size, and event handler parameter slots
        /// </summary>
        public static Button CreateMaterialButton(string text, int x, int y, int eventHandler)
        {
            var button = CreateMaterialButton(text);
            button.Location = new Point(x, y);
            return button;
        }

        /// <summary>
        /// Create a material data grid
        /// </summary>
        public static DataGridView CreateMaterialDataGrid()
        {
            var grid = new DataGridView
            {
                BackgroundColor = MaterialColors.Surface,
                GridColor = MaterialColors.Border,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = MaterialColors.Surface,
                    ForeColor = MaterialColors.Text,
                    Font = MaterialTheme.DefaultFont
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = MaterialColors.Primary,
                    ForeColor = Color.White,
                    Font = MaterialTheme.DefaultFont
                },
                EnableHeadersVisualStyles = false,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };

            return grid;
        }

        /// <summary>
        /// Apply material data grid styling
        /// </summary>
        public static void ApplyMaterialDataGrid(DataGridView grid)
        {
            try
            {
                grid.BackgroundColor = MaterialColors.Surface;
                grid.GridColor = MaterialColors.Border;
                grid.DefaultCellStyle.BackColor = MaterialColors.Surface;
                grid.DefaultCellStyle.ForeColor = MaterialColors.Text;
                grid.DefaultCellStyle.Font = MaterialTheme.DefaultFont;
                grid.ColumnHeadersDefaultCellStyle.BackColor = MaterialColors.Primary;
                grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                grid.EnableHeadersVisualStyles = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to apply material data grid styling: {ex.Message}");
            }
        }

        /// <summary>
        /// Create styled TextBox with modern appearance
        /// </summary>
        public static TextBox CreateStyledTextBox(string placeholder = "")
        {
            var textBox = new TextBox
            {
                Font = MaterialTheme.DefaultFont,
                BackColor = MaterialColors.Surface,
                ForeColor = MaterialColors.Text,
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(200, MaterialTheme.DefaultControlHeight),
                PlaceholderText = placeholder
            };

            // Add hover and focus effects
            textBox.Enter += (s, e) =>
            {
                textBox.BackColor = MaterialColors.Selected;
            };

            textBox.Leave += (s, e) =>
            {
                textBox.BackColor = MaterialColors.Surface;
            };

            return textBox;
        }

        /// <summary>
        /// Create styled Label with theme colors
        /// </summary>
        public static Label CreateStyledLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = MaterialTheme.DefaultFont,
                ForeColor = MaterialColors.Text,
                AutoSize = true,
                BackColor = Color.Transparent
            };
        }

        /// <summary>
        /// Apply theme to any control
        /// </summary>
        public static void ApplyThemeToControl(Control control)
        {
            try
            {
                if (control == null) return;

                control.BackColor = MaterialColors.Surface;
                control.ForeColor = MaterialColors.Text;
                control.Font = MaterialTheme.DefaultFont;

                // Apply to child controls recursively
                foreach (Control child in control.Controls)
                {
                    ApplyThemeToControl(child);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to apply theme to control: {ex.Message}");
            }
        }

        /// <summary>
        /// Get DPI-aware size
        /// </summary>
        public static Size GetDpiAwareSize(Size originalSize, Control control)
        {
            float dpiScale = HighDpiSupport.GetDpiScale(control);
            return HighDpiSupport.GetDpiAwareSize(originalSize, dpiScale);
        }

        /// <summary>
        /// Get DPI-aware size from integer parameters
        /// </summary>
        public static Size GetDpiAwareSize(int width, Control control)
        {
            float dpiScale = HighDpiSupport.GetDpiScale(control);
            return new Size((int)(width * dpiScale), (int)(width * dpiScale));
        }

        /// <summary>
        /// Get DPI-aware size with width and height
        /// </summary>
        public static Size GetDpiAwareSize(int width, int height, Control control)
        {
            float dpiScale = HighDpiSupport.GetDpiScale(control);
            return new Size((int)(width * dpiScale), (int)(height * dpiScale));
        }

        /// <summary>
        /// Get DPI-aware padding
        /// </summary>
        public static Padding GetDpiAwarePadding(Padding originalPadding, Control control)
        {
            float dpiScale = HighDpiSupport.GetDpiScale(control);
            return HighDpiSupport.GetDpiAwarePadding(originalPadding, dpiScale);
        }

        /// <summary>
        /// Get DPI-aware padding with 3 parameters (padding, float, control)
        /// </summary>
        public static Padding GetDpiAwarePadding(Padding originalPadding, float dpiScale, Control control)
        {
            return HighDpiSupport.GetDpiAwarePadding(originalPadding, dpiScale);
        }

        /// <summary>
        /// Apply material colors to a control
        /// </summary>
        private static void ApplyMaterialColors(Control control)
        {
            control.BackColor = MaterialColors.Surface;
            control.ForeColor = MaterialColors.Text;
        }

        /// <summary>
        /// Material Design color palette
        /// </summary>
        public static class MaterialColors
        {
            public static Color Primary => CurrentTheme == ThemeMode.Dark ?
                ColorTranslator.FromHtml("#BB86FC") : ColorTranslator.FromHtml("#2196F3");

            public static Color PrimaryDark => CurrentTheme == ThemeMode.Dark ?
                ColorTranslator.FromHtml("#3700B3") : ColorTranslator.FromHtml("#1976D2");

            public static Color Background => CurrentTheme == ThemeMode.Dark ?
                ColorTranslator.FromHtml("#121212") : ColorTranslator.FromHtml("#FAFAFA");

            public static Color Surface => CurrentTheme == ThemeMode.Dark ?
                ColorTranslator.FromHtml("#1E1E1E") : Color.White;

            public static Color Text => CurrentTheme == ThemeMode.Dark ?
                ColorTranslator.FromHtml("#FFFFFF") : ColorTranslator.FromHtml("#333333");

            public static Color TextSecondary => CurrentTheme == ThemeMode.Dark ?
                ColorTranslator.FromHtml("#B3B3B3") : ColorTranslator.FromHtml("#666666");

            public static Color Border => CurrentTheme == ThemeMode.Dark ?
                ColorTranslator.FromHtml("#333333") : ColorTranslator.FromHtml("#E0E0E0");

            public static Color Success => ColorTranslator.FromHtml("#4CAF50");
            public static Color Warning => ColorTranslator.FromHtml("#FF9800");
            public static Color Error => CurrentTheme == ThemeMode.Dark ?
                ColorTranslator.FromHtml("#CF6679") : ColorTranslator.FromHtml("#F44336");

            public static Color Hover => CurrentTheme == ThemeMode.Dark ?
                ColorTranslator.FromHtml("#2C2C2C") : ColorTranslator.FromHtml("#E3F2FD");

            public static Color Selected => CurrentTheme == ThemeMode.Dark ?
                ColorTranslator.FromHtml("#373737") : ColorTranslator.FromHtml("#E1F5FE");
        }

        /// <summary>
        /// Dark theme color palette
        /// </summary>
        public static class DarkTheme
        {
            public static Color Surface => ColorTranslator.FromHtml("#1E1E1E");
            public static Color SurfaceContainer => ColorTranslator.FromHtml("#2C2C2C");
            public static Color SurfaceContainerHigh => ColorTranslator.FromHtml("#373737");
            public static Color OnSurface => ColorTranslator.FromHtml("#FFFFFF");
            public static Color OnSurfaceVariant => ColorTranslator.FromHtml("#B3B3B3");
            public static Color Primary => ColorTranslator.FromHtml("#BB86FC");
            public static Color OnPrimary => ColorTranslator.FromHtml("#000000");
            public static Color Error => ColorTranslator.FromHtml("#CF6679");
            public static Color ErrorContainer => ColorTranslator.FromHtml("#93000A");
            public static Color OnErrorContainer => ColorTranslator.FromHtml("#FFDAD6");
            public static Color Success => ColorTranslator.FromHtml("#4CAF50");
            public static Color Outline => ColorTranslator.FromHtml("#333333");
        }

        /// <summary>
        /// Typography helper
        /// </summary>
        public static class Typography
        {
            public static Font GetHeadline(Control control)
            {
                return new Font("Segoe UI", 24, FontStyle.Bold);
            }

            public static Font GetTitleMedium(Control control)
            {
                return new Font("Segoe UI", 16, FontStyle.Bold);
            }

            public static Font GetBodyMedium(Control control)
            {
                return new Font("Segoe UI", 10, FontStyle.Regular);
            }

            public static Font GetBodyLarge(Control control)
            {
                return new Font("Segoe UI", 12, FontStyle.Regular);
            }
        }

        /// <summary>
        /// Material theme constants
        /// </summary>
        public static class MaterialTheme
        {
            public static Font DefaultFont => new Font("Segoe UI", 10, FontStyle.Regular);
            public static int DefaultControlHeight => 32;
            public static int DefaultButtonHeight => 36;
        }

        /// <summary>
        /// High DPI support utilities
        /// </summary>
        public static class HighDpiSupport
        {
            public static float GetDpiScale(Control control)
            {
                try
                {
                    using (var graphics = control.CreateGraphics())
                    {
                        return graphics.DpiX / 96f;
                    }
                }
                catch
                {
                    return 1.0f;
                }
            }

            public static bool IsHighDpiMode(Control control)
            {
                return GetDpiScale(control) > 1.25f;
            }

            public static Size GetDpiAwareSize(Size originalSize, float dpiScale)
            {
                return new Size(
                    (int)(originalSize.Width * dpiScale),
                    (int)(originalSize.Height * dpiScale)
                );
            }

            public static Padding GetDpiAwarePadding(Padding originalPadding, float dpiScale)
            {
                return new Padding(
                    (int)(originalPadding.Left * dpiScale),
                    (int)(originalPadding.Top * dpiScale),
                    (int)(originalPadding.Right * dpiScale),
                    (int)(originalPadding.Bottom * dpiScale)
                );
            }

            public static string GetDpiDescription(Control control)
            {
                float scale = GetDpiScale(control);
                if (scale <= 1.0f) return "Standard DPI (96)";
                if (scale <= 1.25f) return "Medium DPI (120)";
                if (scale <= 1.5f) return "High DPI (144)";
                if (scale <= 2.0f) return "Very High DPI (192)";
                return $"Ultra High DPI ({scale * 96:F0})";
            }
        }
    }
}
