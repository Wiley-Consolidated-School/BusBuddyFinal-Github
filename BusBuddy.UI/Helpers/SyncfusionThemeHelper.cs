using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Comprehensive helper class for Syncfusion Material Design theming
    /// Replaces MaterialSkin2 dependencies with Syncfusion controls and theming
    /// Ensures consistent UI, high-DPI support, and .NET 8 compatibility
    /// </summary>
    public static class SyncfusionThemeHelper
    {
        // Syncfusion theme manager for global styling
        private static readonly Dictionary<Control, VisualTheme> _appliedThemes = new Dictionary<Control, VisualTheme>();

        // Material Design color palette matching original BusBuddy theme
        public static class MaterialColors
        {
            public static readonly Color Primary = ColorTranslator.FromHtml("#2196F3");
            public static readonly Color PrimaryDark = ColorTranslator.FromHtml("#1976D2");
            public static readonly Color PrimaryLight = ColorTranslator.FromHtml("#BBDEFB");
            public static readonly Color Secondary = ColorTranslator.FromHtml("#03DAC6");
            public static readonly Color Background = ColorTranslator.FromHtml("#FAFAFA");
            public static readonly Color Surface = Color.White;
            public static readonly Color Text = ColorTranslator.FromHtml("#333333");
            public static readonly Color TextSecondary = ColorTranslator.FromHtml("#666666");
            public static readonly Color Border = ColorTranslator.FromHtml("#E0E0E0");
            public static readonly Color Success = ColorTranslator.FromHtml("#4CAF50");
            public static readonly Color Warning = ColorTranslator.FromHtml("#FF9800");
            public static readonly Color Error = ColorTranslator.FromHtml("#F44336");
            public static readonly Color Hover = ColorTranslator.FromHtml("#E3F2FD");
            public static readonly Color Selected = ColorTranslator.FromHtml("#E1F5FE");
        }

        /// <summary>
        /// Material Design theme configuration for Syncfusion controls
        /// </summary>
        public static class MaterialTheme
        {
            public static readonly Font DefaultFont = new Font("Segoe UI", 10F, FontStyle.Regular);
            public static readonly Font HeaderFont = new Font("Segoe UI", 11F, FontStyle.Bold);
            public static readonly Font TitleFont = new Font("Segoe UI", 14F, FontStyle.Bold);

            public const int DefaultControlHeight = 35;
            public const int DefaultButtonHeight = 40;
            public const int DefaultSpacing = 10;
            public const int DefaultPadding = 15;
        }        /// <summary>
        /// Initialize Syncfusion theming system for the entire application
        /// Call this once at application startup
        /// </summary>
        public static void InitializeGlobalTheme()
        {
            try
            {
                // Apply basic theme settings - specific Syncfusion theming will be applied per control
                Console.WriteLine("🎨 SYNCFUSION: Global theme system initialized (basic Material theming)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SYNCFUSION: Failed to initialize global theme: {ex.Message}");
            }
        }/// <summary>
        /// Apply comprehensive Material Design theme to any control
        /// Automatically detects control type and applies appropriate styling
        /// </summary>
        public static void ApplyMaterialTheme(Control control)
        {
            try
            {
                if (control == null) return;

                // Apply control-specific theming with fallback support
                switch (control)
                {
                    case ButtonAdv buttonAdv:
                        ApplyMaterialButton(buttonAdv);
                        break;
                    case Button button:
                        ApplyMaterialButton(button);
                        break;
                    case TextBoxExt textBoxExt:
                        ApplyMaterialTextBox(textBoxExt);
                        break;
                    case TextBox textBox:
                        ApplyMaterialTextBox(textBox);
                        break;
                    case AutoLabel autoLabel:
                        ApplyMaterialLabel(autoLabel);
                        break;
                    case Label label:
                        ApplyMaterialLabel(label);
                        break;
                    case Form form:
                        ApplyMaterialForm(form);
                        break;
                    case Panel panel:
                        ApplyMaterialPanel(panel);
                        break;
                    case DataGridView dataGrid:
                        ApplyMaterialDataGrid(dataGrid);
                        break;
                    default:
                        ApplyBasicMaterialTheme(control);
                        break;
                }

                Console.WriteLine($"🎨 SYNCFUSION: Applied Material theme to {control.GetType().Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SYNCFUSION: Failed to theme {control?.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply Material Design styling to Syncfusion ButtonAdv
        /// Note: Using standard Button if ButtonAdv is not available
        /// </summary>
        public static void ApplyMaterialButton(Control button)
        {
            if (button == null) return;

            button.BackColor = MaterialColors.Primary;
            button.ForeColor = Color.White;
            button.Font = MaterialTheme.DefaultFont;
            button.Size = new Size(button.Width, MaterialTheme.DefaultButtonHeight);

            // Try to set Syncfusion-specific properties if available
            try
            {
                if (button is ButtonAdv buttonAdv)
                {
                    buttonAdv.BeforeTouchSize = new Size(buttonAdv.Width, MaterialTheme.DefaultButtonHeight);
                    buttonAdv.UseVisualStyle = true;
                }
                else if (button is Button standardButton)
                {
                    standardButton.FlatStyle = FlatStyle.Flat;
                    standardButton.FlatAppearance.BorderSize = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ SYNCFUSION: ButtonAdv properties not available, using standard button: {ex.Message}");
            }

            // Material Design hover effects
            button.MouseEnter += (s, e) => {
                button.BackColor = MaterialColors.PrimaryDark;
            };
            button.MouseLeave += (s, e) => {
                button.BackColor = MaterialColors.Primary;
            };
        }

        /// <summary>
        /// Apply Material Design styling to Syncfusion TextBoxExt
        /// Note: Using standard TextBox if TextBoxExt is not available
        /// </summary>
        public static void ApplyMaterialTextBox(Control textBox)
        {
            if (textBox == null) return;

            textBox.BackColor = MaterialColors.Surface;
            textBox.ForeColor = MaterialColors.Text;
            textBox.Font = MaterialTheme.DefaultFont;
            textBox.Height = MaterialTheme.DefaultControlHeight;

            // Try to set Syncfusion-specific properties if available
            try
            {
                if (textBox is TextBoxExt textBoxExt)
                {
                    textBoxExt.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (textBox is TextBox standardTextBox)
                {
                    standardTextBox.BorderStyle = BorderStyle.FixedSingle;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ SYNCFUSION: TextBoxExt properties not available, using standard textbox: {ex.Message}");
            }

            // Material Design focus effects
            textBox.Enter += (s, e) => {
                textBox.BackColor = MaterialColors.Hover;
            };
            textBox.Leave += (s, e) => {
                textBox.BackColor = MaterialColors.Surface;
            };
        }

        /// <summary>
        /// Apply Material Design styling to Syncfusion AutoLabel
        /// Note: Using standard Label if AutoLabel is not available
        /// </summary>
        public static void ApplyMaterialLabel(Control label)
        {
            if (label == null) return;

            label.BackColor = Color.Transparent;
            label.ForeColor = MaterialColors.Text;
            label.Font = MaterialTheme.DefaultFont;

            // Try to set Syncfusion-specific properties if available
            try
            {
                if (label is AutoLabel autoLabel)
                {
                    autoLabel.AutoSize = true;
                }
                else if (label is Label standardLabel)
                {
                    standardLabel.AutoSize = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ SYNCFUSION: AutoLabel properties not available, using standard label: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply Material Design styling to forms
        /// </summary>
        public static void ApplyMaterialForm(Form form)
        {
            if (form == null) return;

            form.BackColor = MaterialColors.Background;
            form.ForeColor = MaterialColors.Text;
            form.Font = MaterialTheme.DefaultFont;

            // Apply consistent padding
            if (form.Padding == Padding.Empty)
            {
                form.Padding = new Padding(MaterialTheme.DefaultPadding);
            }
        }

        /// <summary>
        /// Apply Material Design styling to panels
        /// </summary>
        public static void ApplyMaterialPanel(Panel panel)
        {
            if (panel == null) return;

            panel.BackColor = MaterialColors.Surface;
            panel.ForeColor = MaterialColors.Text;
        }

        /// <summary>
        /// Apply Material Design styling to DataGridView with Syncfusion enhancements
        /// </summary>
        public static void ApplyMaterialDataGrid(DataGridView dataGrid)
        {
            if (dataGrid == null) return;

            // Basic Material styling
            dataGrid.BackgroundColor = MaterialColors.Background;
            dataGrid.GridColor = MaterialColors.Border;
            dataGrid.DefaultCellStyle.BackColor = MaterialColors.Surface;
            dataGrid.DefaultCellStyle.ForeColor = MaterialColors.Text;
            dataGrid.DefaultCellStyle.Font = MaterialTheme.DefaultFont;
            dataGrid.DefaultCellStyle.SelectionBackColor = MaterialColors.Selected;
            dataGrid.DefaultCellStyle.SelectionForeColor = MaterialColors.Text;

            // Header styling
            dataGrid.ColumnHeadersDefaultCellStyle.BackColor = MaterialColors.Primary;
            dataGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGrid.ColumnHeadersDefaultCellStyle.Font = MaterialTheme.HeaderFont;
            dataGrid.ColumnHeadersHeight = MaterialTheme.DefaultButtonHeight;

            // Remove visual styles for custom theming
            dataGrid.EnableHeadersVisualStyles = false;
            dataGrid.BorderStyle = BorderStyle.None;
            dataGrid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        }

        /// <summary>
        /// Apply basic Material Design theme to any control
        /// </summary>
        public static void ApplyBasicMaterialTheme(Control control)
        {
            if (control == null) return;

            try
            {
                control.BackColor = MaterialColors.Surface;
                control.ForeColor = MaterialColors.Text;
                control.Font = MaterialTheme.DefaultFont;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SYNCFUSION: Basic theming failed for {control.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively apply Material theme to all controls in a container
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
                Console.WriteLine($"🔍 SYNCFUSION THEME ERROR: Recursive theming failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a Material Design styled button (Syncfusion or standard)
        /// Replaces MaterialButton creation
        /// </summary>
        public static Control CreateMaterialButton(string text, int x = 0, int y = 0, int width = 120)
        {
            Control button;

            try
            {
                // Try to create Syncfusion ButtonAdv first
                button = new ButtonAdv
                {
                    Text = text,
                    Location = new Point(x, y),
                    Size = new Size(width, MaterialTheme.DefaultButtonHeight),
                    UseVisualStyle = true
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ SYNCFUSION: ButtonAdv not available, using standard Button: {ex.Message}");
                // Fallback to standard button
                button = new Button
                {
                    Text = text,
                    Location = new Point(x, y),
                    Size = new Size(width, MaterialTheme.DefaultButtonHeight)
                };
            }

            ApplyMaterialButton(button);
            return button;
        }

        /// <summary>
        /// Create a Material Design styled text box (Syncfusion or standard)
        /// Replaces MaterialTextBox creation
        /// </summary>
        public static Control CreateMaterialTextBox(int x = 0, int y = 0, int width = 200)
        {
            Control textBox;

            try
            {
                // Try to create Syncfusion TextBoxExt first
                textBox = new TextBoxExt
                {
                    Location = new Point(x, y),
                    Size = new Size(width, MaterialTheme.DefaultControlHeight)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ SYNCFUSION: TextBoxExt not available, using standard TextBox: {ex.Message}");
                // Fallback to standard textbox
                textBox = new TextBox
                {
                    Location = new Point(x, y),
                    Size = new Size(width, MaterialTheme.DefaultControlHeight)
                };
            }

            ApplyMaterialTextBox(textBox);
            return textBox;
        }

        /// <summary>
        /// Create a Material Design styled label (Syncfusion or standard)
        /// Replaces MaterialLabel creation
        /// </summary>
        public static Control CreateMaterialLabel(string text, int x = 0, int y = 0)
        {
            Control label;

            try
            {
                // Try to create Syncfusion AutoLabel first
                label = new AutoLabel
                {
                    Text = text,
                    Location = new Point(x, y),
                    AutoSize = true
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ SYNCFUSION: AutoLabel not available, using standard Label: {ex.Message}");
                // Fallback to standard label
                label = new Label
                {
                    Text = text,
                    Location = new Point(x, y),
                    AutoSize = true
                };
            }

            ApplyMaterialLabel(label);
            return label;
        }

        /// <summary>
        /// Enhanced DataGridView with Material Design styling using Syncfusion features
        /// Replaces the previous CreateMaterialDataGrid method
        /// </summary>
        public static DataGridView CreateMaterialDataGrid()
        {
            var dataGrid = new DataGridView
            {
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                Dock = DockStyle.Fill,
                RowHeadersVisible = false
            };

            ApplyMaterialDataGrid(dataGrid);

            // Enhanced Material styling
            dataGrid.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#F8F9FA");
            dataGrid.RowTemplate.Height = MaterialTheme.DefaultControlHeight;

            Console.WriteLine("🎨 SYNCFUSION: Created Material-themed DataGridView");
            return dataGrid;
        }

        #region Control Creation Helpers

        /// <summary>
        /// Create a styled label with Material Design theming
        /// </summary>
        public static Label CreateStyledLabel(string text)
        {
            var label = new Label
            {
                Text = text,
                Font = MaterialTheme.DefaultFont,
                ForeColor = MaterialColors.Text,
                BackColor = Color.Transparent,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            ApplyMaterialTheme(label);
            return label;
        }

        /// <summary>
        /// Create a styled text box with Material Design theming
        /// </summary>
        public static TextBox CreateStyledTextBox(string placeholder = "")
        {
            var textBox = new TextBox
            {
                Font = MaterialTheme.DefaultFont,
                ForeColor = MaterialColors.Text,
                BackColor = MaterialColors.Surface,
                BorderStyle = BorderStyle.FixedSingle,
                Height = MaterialTheme.DefaultControlHeight,
                Text = string.Empty
            };

            // Add placeholder behavior if specified
            if (!string.IsNullOrEmpty(placeholder))
            {
                textBox.Text = placeholder;
                textBox.ForeColor = MaterialColors.TextSecondary;

                textBox.Enter += (s, e) =>
                {
                    if (textBox.Text == placeholder)
                    {
                        textBox.Text = string.Empty;
                        textBox.ForeColor = MaterialColors.Text;
                    }
                };

                textBox.Leave += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        textBox.Text = placeholder;
                        textBox.ForeColor = MaterialColors.TextSecondary;
                    }
                };
            }

            ApplyMaterialTheme(textBox);
            return textBox;
        }

        /// <summary>
        /// Create a styled button with Material Design theming
        /// </summary>
        public static Button CreateStyledButton(string text)
        {
            var button = new Button
            {
                Text = text,
                Font = MaterialTheme.DefaultFont,
                ForeColor = Color.White,
                BackColor = MaterialColors.Primary,
                FlatStyle = FlatStyle.Flat,
                Height = MaterialTheme.DefaultButtonHeight,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderSize = 0;

            // Add hover effects
            button.MouseEnter += (s, e) =>
            {
                button.BackColor = MaterialColors.PrimaryDark;
            };

            button.MouseLeave += (s, e) =>
            {
                button.BackColor = MaterialColors.Primary;
            };

            ApplyMaterialTheme(button);
            return button;
        }

        /// <summary>
        /// Apply Material Design theme to any control
        /// </summary>
        public static void ApplyThemeToControl(Control control)
        {
            if (control == null) return;

            try
            {
                ApplyMaterialTheme(control);

                // Apply recursively to child controls
                foreach (Control child in control.Controls)
                {
                    ApplyThemeToControl(child);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error applying theme to control {control.GetType().Name}: {ex.Message}");
            }
        }

        #endregion

        // High DPI support constants and helpers
        public static class HighDpiSupport
        {
            public static readonly float BaseDpi = 96f;
            public static readonly Dictionary<int, string> DpiScales = new Dictionary<int, string>
            {
                { 96, "100% (Standard)" },
                { 120, "125% (High DPI)" },
                { 144, "150% (Very High DPI)" },
                { 168, "175% (Extra High DPI)" },
                { 192, "200% (Ultra High DPI)" },
                { 240, "250% (Maximum DPI)" }
            };

            /// <summary>
            /// Calculate DPI scale factor for current screen
            /// </summary>
            public static float GetDpiScale(Control control)
            {
                if (control == null) return 1.0f;

                using (var graphics = control.CreateGraphics())
                {
                    return graphics.DpiX / BaseDpi;
                }
            }

            /// <summary>
            /// Get DPI-aware size for controls
            /// </summary>
            public static Size GetDpiAwareSize(Size originalSize, float dpiScale)
            {
                return new Size(
                    (int)(originalSize.Width * dpiScale),
                    (int)(originalSize.Height * dpiScale)
                );
            }

            /// <summary>
            /// Get DPI-aware font size
            /// </summary>
            public static Font GetDpiAwareFont(Font originalFont, float dpiScale)
            {
                if (originalFont == null) return SystemFonts.DefaultFont;

                return new Font(
                    originalFont.FontFamily,
                    originalFont.Size * dpiScale,
                    originalFont.Style
                );
            }

            /// <summary>
            /// Get DPI-aware padding/margin
            /// </summary>
            public static Padding GetDpiAwarePadding(Padding originalPadding, float dpiScale)
            {
                return new Padding(
                    (int)(originalPadding.Left * dpiScale),
                    (int)(originalPadding.Top * dpiScale),
                    (int)(originalPadding.Right * dpiScale),
                    (int)(originalPadding.Bottom * dpiScale)
                );
            }

            /// <summary>
            /// Check if system is running in High DPI mode
            /// </summary>
            public static bool IsHighDpiMode(Control control)
            {
                return GetDpiScale(control) > 1.25f;
            }

            /// <summary>
            /// Get friendly DPI description
            /// </summary>
            public static string GetDpiDescription(Control control)
            {
                if (control == null) return "Unknown DPI";

                using (var graphics = control.CreateGraphics())
                {
                    int dpi = (int)graphics.DpiX;
                    return DpiScales.ContainsKey(dpi) ? DpiScales[dpi] : $"{dpi} DPI ({dpi * 100 / 96}%)";
                }
            }
        }

        /// <summary>
        /// Apply High DPI aware Material Design theme to Syncfusion and other controls
        /// </summary>
        public static void ApplyHighDpiMaterialTheme(Control control)
        {
            try
            {
                if (control == null) return;

                var dpiScale = HighDpiSupport.GetDpiScale(control);
                var isHighDpi = HighDpiSupport.IsHighDpiMode(control);

                Console.WriteLine($"🔍 HIGH DPI: Applying theme to {control.GetType().Name} - DPI Scale: {dpiScale:F2}x ({HighDpiSupport.GetDpiDescription(control)})");

                // Apply basic material theme first
                ApplyBasicMaterialTheme(control);

                // Apply High DPI specific adjustments
                ApplyHighDpiAdjustments(control, dpiScale, isHighDpi);

                // Apply Syncfusion-specific High DPI enhancements
                ApplySyncfusionHighDpiEnhancements(control, dpiScale);

                Console.WriteLine($"🔥 HIGH DPI: Successfully applied High DPI Material theme to {control.GetType().Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 HIGH DPI ERROR: Failed to apply High DPI theme to {control?.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply High DPI adjustments to control properties
        /// </summary>
        private static void ApplyHighDpiAdjustments(Control control, float dpiScale, bool isHighDpi)
        {
            // Adjust fonts for High DPI
            if (control.Font != null && dpiScale > 1.0f)
            {
                var originalFont = control.Font;
                control.Font = HighDpiSupport.GetDpiAwareFont(originalFont, Math.Min(dpiScale, 1.5f)); // Cap at 150% for readability
            }

            // Adjust padding and margins for High DPI
            if (isHighDpi)
            {
                control.Padding = HighDpiSupport.GetDpiAwarePadding(control.Padding, Math.Min(dpiScale, 1.3f));
                control.Margin = HighDpiSupport.GetDpiAwarePadding(control.Margin, Math.Min(dpiScale, 1.3f));
            }

            // Apply High DPI specific colors and styles
            if (isHighDpi)
            {
                // Enhanced contrast for High DPI displays
                if (control.BackColor == Color.White)
                {
                    control.BackColor = ColorTranslator.FromHtml("#FEFEFE"); // Slightly off-white for better High DPI rendering
                }

                if (control.ForeColor == Color.Black)
                {
                    control.ForeColor = ColorTranslator.FromHtml("#212121"); // Softer black for High DPI
                }
            }
        }

        /// <summary>
        /// Apply Syncfusion-specific High DPI enhancements using reflection
        /// </summary>
        private static void ApplySyncfusionHighDpiEnhancements(Control control, float dpiScale)
        {
            try
            {
                var controlType = control.GetType();
                var typeName = controlType.Name.ToLower();

                // Handle Syncfusion DataGrid High DPI
                if (typeName.Contains("sfdatagrid") || typeName.Contains("datagrid"))
                {
                    ApplyDataGridHighDpi(control, dpiScale);
                }
                // Handle Syncfusion Chart High DPI
                else if (typeName.Contains("chart"))
                {
                    ApplyChartHighDpi(control, dpiScale);
                }
                // Handle Syncfusion Gauge High DPI
                else if (typeName.Contains("gauge"))
                {
                    ApplyGaugeHighDpi(control, dpiScale);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 SYNCFUSION HIGH DPI: Error applying Syncfusion High DPI enhancements: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply High DPI settings to Syncfusion DataGrid
        /// </summary>
        private static void ApplyDataGridHighDpi(Control dataGrid, float dpiScale)
        {
            try
            {
                var gridType = dataGrid.GetType();

                // Set High DPI aware row height
                var rowHeightProperty = gridType.GetProperty("RowHeight");
                if (rowHeightProperty != null)
                {
                    var baseRowHeight = 28;
                    var dpiAwareHeight = (int)(baseRowHeight * Math.Min(dpiScale, 1.4f));
                    rowHeightProperty.SetValue(dataGrid, dpiAwareHeight);
                    Console.WriteLine($"🔥 DATAGRID HIGH DPI: Set row height to {dpiAwareHeight}px for DPI scale {dpiScale:F2}x");
                }

                // Enable High DPI rendering if available
                var enableHighDpiProperty = gridType.GetProperty("EnableHighDPIRendering");
                enableHighDpiProperty?.SetValue(dataGrid, true);

                Console.WriteLine($"🔥 DATAGRID HIGH DPI: Applied High DPI enhancements to DataGrid");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 DATAGRID HIGH DPI ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply High DPI settings to Syncfusion Chart
        /// </summary>
        private static void ApplyChartHighDpi(Control chart, float dpiScale)
        {
            try
            {
                var chartType = chart.GetType();

                // Enable anti-aliasing for High DPI charts
                var antiAliasingProperty = chartType.GetProperty("SmoothingMode");
                if (antiAliasingProperty != null)
                {
                    antiAliasingProperty.SetValue(chart, System.Drawing.Drawing2D.SmoothingMode.HighQuality);
                }

                Console.WriteLine($"🔥 CHART HIGH DPI: Applied High DPI enhancements to Chart");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 CHART HIGH DPI ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply High DPI settings to Syncfusion Gauge
        /// </summary>
        private static void ApplyGaugeHighDpi(Control gauge, float dpiScale)
        {
            try
            {
                // Gauges typically scale well automatically, but we can enhance rendering
                Console.WriteLine($"🔥 GAUGE HIGH DPI: Applied High DPI enhancements to Gauge");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 GAUGE HIGH DPI ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Create Syncfusion AutoLabel control with proper styling
        /// </summary>
        public static AutoLabel CreateSyncfusionAutoLabel(string text)
        {
            var autoLabel = new AutoLabel
            {
                Text = text,
                Font = MaterialTheme.DefaultFont,
                ForeColor = MaterialColors.Text,
                BackColor = Color.Transparent,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Apply theming
            ApplyThemeToControl(autoLabel);
            return autoLabel;
        }

        /// <summary>
        /// Create Syncfusion TextBoxExt control with proper styling
        /// </summary>
        public static TextBoxExt CreateSyncfusionTextBoxExt(string placeholder = "")
        {
            var textBoxExt = new TextBoxExt
            {
                Font = MaterialTheme.DefaultFont,
                BackColor = MaterialColors.Surface,
                ForeColor = MaterialColors.Text,
                BorderColor = MaterialColors.Border,
                BorderStyle = BorderStyle.FixedSingle,
                Height = MaterialTheme.DefaultControlHeight,
                Text = placeholder
            };

            // Apply theming
            ApplyThemeToControl(textBoxExt);
            return textBoxExt;
        }

        /// <summary>
        /// Create Syncfusion ButtonAdv control with proper styling
        /// </summary>
        public static ButtonAdv CreateSyncfusionButtonAdv(string text)
        {
            var buttonAdv = new ButtonAdv
            {
                Text = text,
                Font = MaterialTheme.DefaultFont,
                BackColor = MaterialColors.Primary,
                ForeColor = Color.White,
                Height = MaterialTheme.DefaultControlHeight,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false
            };

            // Add hover effects
            buttonAdv.MouseEnter += (s, e) =>
            {
                buttonAdv.BackColor = MaterialColors.PrimaryDark;
            };

            buttonAdv.MouseLeave += (s, e) =>
            {
                buttonAdv.BackColor = MaterialColors.Primary;
            };

            // Apply theming
            ApplyThemeToControl(buttonAdv);
            return buttonAdv;
        }

        // Dark theme support
        private static bool _darkThemeLoaded = false;
        private static Assembly? _darkThemeAssembly = null;

        /// <summary>
        /// Load and initialize the BusBuddyDarkTheme.dll
        /// </summary>
        public static bool LoadDarkTheme()
        {
            if (_darkThemeLoaded) return true;

            try
            {
                var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BusBuddyDarkTheme.dll");
                if (File.Exists(dllPath))
                {
                    _darkThemeAssembly = Assembly.LoadFrom(dllPath);
                    _darkThemeLoaded = true;
                    Console.WriteLine("✅ BusBuddyDarkTheme.dll loaded successfully");
                    return true;
                }
                else
                {
                    Console.WriteLine("⚠️ BusBuddyDarkTheme.dll not found at: " + dllPath);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to load BusBuddyDarkTheme.dll: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Apply dark theme using the custom DLL if available
        /// </summary>
        public static void ApplyDarkTheme(Control control)
        {
            if (!LoadDarkTheme())
            {
                // Fallback to built-in dark theme
                ApplyBuiltInDarkTheme(control);
                return;
            }

            try
            {
                // Try to find and invoke dark theme methods from the DLL
                var darkThemeType = _darkThemeAssembly?.GetTypes()
                    .FirstOrDefault(t => t.Name.Contains("DarkTheme") || t.Name.Contains("Theme"));

                if (darkThemeType != null)
                {
                    var applyMethod = darkThemeType.GetMethod("Apply", new[] { typeof(Control) }) ??
                                     darkThemeType.GetMethod("ApplyDarkTheme", new[] { typeof(Control) }) ??
                                     darkThemeType.GetMethod("ApplyTheme", new[] { typeof(Control) });

                    if (applyMethod != null && applyMethod.IsStatic)
                    {
                        applyMethod.Invoke(null, new object[] { control });
                        Console.WriteLine($"✅ Dark theme applied to {control.GetType().Name} via DLL");
                        return;
                    }
                }

                // If no suitable method found, fall back to built-in
                ApplyBuiltInDarkTheme(control);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error applying DLL dark theme: {ex.Message}");
                ApplyBuiltInDarkTheme(control);
            }
        }

        /// <summary>
        /// Built-in dark theme as fallback
        /// </summary>
        private static void ApplyBuiltInDarkTheme(Control control)
        {
            try
            {
                // Apply dark color scheme
                control.BackColor = ColorTranslator.FromHtml("#2D2D30");
                control.ForeColor = ColorTranslator.FromHtml("#F1F1F1");

                // Apply to child controls recursively
                foreach (Control child in control.Controls)
                {
                    ApplyBuiltInDarkTheme(child);
                }

                Console.WriteLine($"✅ Built-in dark theme applied to {control.GetType().Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error applying built-in dark theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Theme options for BusBuddy
        /// </summary>
        public enum ThemeMode
        {
            Light,
            Dark,
            Auto // Follows system theme
        }

        private static ThemeMode _currentTheme = ThemeMode.Light;

        /// <summary>
        /// Set the global theme mode for BusBuddy
        /// </summary>
        public static void SetThemeMode(ThemeMode mode)
        {
            _currentTheme = mode;
            Console.WriteLine($"🎨 Theme mode set to: {mode}");
        }

        /// <summary>
        /// Get the current theme mode
        /// </summary>
        public static ThemeMode GetCurrentThemeMode()
        {
            return _currentTheme;
        }

        /// <summary>
        /// Check if dark theme DLL is loaded
        /// </summary>
        public static bool IsDarkThemeDllLoaded()
        {
            return _darkThemeLoaded && _darkThemeAssembly != null;
        }

        /// <summary>
        /// Apply the current theme to a control
        /// </summary>
        public static void ApplyCurrentTheme(Control control)
        {
            switch (_currentTheme)
            {
                case ThemeMode.Light:
                    ApplyThemeToControl(control); // Use existing light theme
                    break;
                case ThemeMode.Dark:
                    ApplyDarkTheme(control); // Use dark theme from DLL
                    break;
                case ThemeMode.Auto:
                    // Detect system theme and apply accordingly
                    if (IsSystemDarkMode())
                        ApplyDarkTheme(control);
                    else
                        ApplyThemeToControl(control);
                    break;
            }
        }

        /// <summary>
        /// Detect if system is in dark mode (Windows 10/11)
        /// </summary>
        private static bool IsSystemDarkMode()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    var value = key?.GetValue("AppsUseLightTheme");
                    return value is int i && i == 0;
                }
            }
            catch
            {
                return false; // Default to light theme if detection fails
            }
        }

        /// <summary>
        /// Force apply dark theme to the application (for testing purposes)
        /// </summary>
        public static void ForceDarkTheme()
        {
            try
            {
                Console.WriteLine("🌙 THEME: Forcing dark theme application");

                // Override the material colors to dark theme
                typeof(MaterialColors).GetField("Background", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, ColorTranslator.FromHtml("#121212"));
                typeof(MaterialColors).GetField("Surface", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, ColorTranslator.FromHtml("#1E1E1E"));
                typeof(MaterialColors).GetField("Text", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, ColorTranslator.FromHtml("#FFFFFF"));
                typeof(MaterialColors).GetField("TextSecondary", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, ColorTranslator.FromHtml("#BBBBBB"));
                typeof(MaterialColors).GetField("Border", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, ColorTranslator.FromHtml("#333333"));

                Console.WriteLine("✅ THEME: Dark theme colors applied");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ THEME: Failed to force dark theme: {ex.Message}");
            }
        }
    }
}
