using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Extensions;

namespace BusBuddy.UI.Theme
{
    /// <summary>
    /// Advanced Material Design theme manager with dark theme, high DPI, and vector graphics support
    /// Implements Material Design 3.0 principles with accessibility and modern UI patterns
    /// </summary>
    public static class MaterialDesignThemeManager
    {
        #region Material Design Dark Theme Colors

        // Material Design Dark Theme Color Palette
        public static class DarkTheme
        {
            // Surface Colors (Material Design 3.0 Dark Theme)
            public static readonly Color Surface = Color.FromArgb(16, 16, 20);           // Pure dark surface
            public static readonly Color SurfaceVariant = Color.FromArgb(28, 27, 31);    // Elevated surface
            public static readonly Color SurfaceContainer = Color.FromArgb(33, 33, 38);  // Container surface
            public static readonly Color SurfaceContainerHigh = Color.FromArgb(40, 40, 46); // High emphasis surface

            // Primary Colors (Modern Blue-Grey Palette)
            public static readonly Color Primary = Color.FromArgb(138, 180, 248);        // Light blue for dark theme
            public static readonly Color OnPrimary = Color.FromArgb(0, 30, 66);          // Dark blue for text on primary
            public static readonly Color PrimaryContainer = Color.FromArgb(0, 51, 102);  // Dark blue container
            public static readonly Color OnPrimaryContainer = Color.FromArgb(195, 225, 255); // Light text on primary container

            // Secondary Colors (Complementary Grey-Blue)
            public static readonly Color Secondary = Color.FromArgb(191, 199, 220);      // Light grey-blue
            public static readonly Color OnSecondary = Color.FromArgb(41, 50, 65);       // Dark text on secondary
            public static readonly Color SecondaryContainer = Color.FromArgb(64, 71, 88); // Dark grey-blue container
            public static readonly Color OnSecondaryContainer = Color.FromArgb(219, 227, 248); // Light text on secondary container

            // Accent Colors (Cyan for highlights)
            public static readonly Color Tertiary = Color.FromArgb(220, 184, 255);       // Light purple accent
            public static readonly Color OnTertiary = Color.FromArgb(56, 30, 114);       // Dark purple text
            public static readonly Color TertiaryContainer = Color.FromArgb(79, 55, 139); // Dark purple container
            public static readonly Color OnTertiaryContainer = Color.FromArgb(240, 219, 255); // Light text on tertiary container

            // Text Colors
            public static readonly Color OnSurface = Color.FromArgb(230, 225, 229);      // Primary text
            public static readonly Color OnSurfaceVariant = Color.FromArgb(196, 196, 208); // Secondary text
            public static readonly Color Outline = Color.FromArgb(147, 143, 153);        // Outline color
            public static readonly Color OutlineVariant = Color.FromArgb(68, 71, 78);    // Subtle outline

            // Status Colors (Accessible in dark theme)
            public static readonly Color Error = Color.FromArgb(242, 184, 181);          // Light red for errors
            public static readonly Color OnError = Color.FromArgb(96, 20, 16);           // Dark red text
            public static readonly Color ErrorContainer = Color.FromArgb(140, 29, 24);   // Dark red container
            public static readonly Color OnErrorContainer = Color.FromArgb(249, 222, 220); // Light text on error container

            public static readonly Color Success = Color.FromArgb(166, 218, 149);        // Light green for success
            public static readonly Color OnSuccess = Color.FromArgb(0, 57, 10);          // Dark green text
            public static readonly Color SuccessContainer = Color.FromArgb(0, 83, 18);   // Dark green container
            public static readonly Color OnSuccessContainer = Color.FromArgb(194, 246, 177); // Light text on success container

            public static readonly Color Warning = Color.FromArgb(255, 204, 128);        // Light orange for warnings
            public static readonly Color OnWarning = Color.FromArgb(66, 31, 0);          // Dark orange text
            public static readonly Color WarningContainer = Color.FromArgb(96, 52, 0);   // Dark orange container
            public static readonly Color OnWarningContainer = Color.FromArgb(255, 221, 168); // Light text on warning container
        }

        #endregion

        #region High DPI Support

        /// <summary>
        /// Get DPI-aware size based on current system DPI
        /// </summary>
        /// <param name="baseSize">Base size at 96 DPI</param>
        /// <param name="control">Control to get DPI from (optional)</param>
        /// <returns>DPI-scaled size</returns>
        public static int GetDpiAwareSize(int baseSize, Control? control = null)
        {
            float dpiScale = 1.0f;

            if (control != null)
            {
                dpiScale = control.DeviceDpi / 96.0f;
            }
            else
            {
                using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
                {
                    dpiScale = graphics.DpiX / 96.0f;
                }
            }

            return (int)Math.Round(baseSize * dpiScale);
        }

        /// <summary>
        /// Get DPI-aware padding
        /// </summary>
        public static Padding GetDpiAwarePadding(int horizontal, int vertical, Control? control = null)
        {
            int h = GetDpiAwareSize(horizontal, control);
            int v = GetDpiAwareSize(vertical, control);
            return new Padding(h, v, h, v);
        }

        /// <summary>
        /// Get DPI-aware font size
        /// </summary>
        public static float GetDpiAwareFontSize(float baseSize, Control? control = null)
        {
            float dpiScale = 1.0f;

            if (control != null)
            {
                dpiScale = control.DeviceDpi / 96.0f;
            }
            else
            {
                using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
                {
                    dpiScale = graphics.DpiX / 96.0f;
                }
            }

            return baseSize * dpiScale;
        }

        #endregion

        #region Material Design Typography        /// <summary>
        /// Material Design 3.0 Typography Scale (DPI-aware)
        /// </summary>
        public static class Typography
        {
            public static Font GetDisplayLarge(Control? control = null) =>
                new Font(FontFamily.GenericSansSerif, GetDpiAwareFontSize(57f, control), FontStyle.Regular);

            public static Font GetDisplayMedium(Control? control = null) =>
                new Font(FontFamily.GenericSansSerif, GetDpiAwareFontSize(45f, control), FontStyle.Regular);

            public static Font GetDisplaySmall(Control? control = null) =>
                new Font(FontFamily.GenericSansSerif, GetDpiAwareFontSize(36f, control), FontStyle.Regular);

            public static Font GetHeadlineLarge(Control? control = null) =>
                new Font(FontFamily.GenericSansSerif, GetDpiAwareFontSize(32f, control), FontStyle.Regular);

            public static Font GetHeadlineMedium(Control? control = null) =>
                new Font(FontFamily.GenericSansSerif, GetDpiAwareFontSize(28f, control), FontStyle.Regular);

            public static Font GetHeadlineSmall(Control? control = null) =>
                new Font(FontFamily.GenericSansSerif, GetDpiAwareFontSize(24f, control), FontStyle.Regular);

            public static Font GetTitleLarge(Control? control = null) =>
                new Font(FontFamily.GenericSansSerif, GetDpiAwareFontSize(22f, control), FontStyle.Regular);

            public static Font GetTitleMedium(Control? control = null) =>
                new Font(FontFamily.GenericSansSerif, GetDpiAwareFontSize(16f, control), FontStyle.Bold);

            public static Font GetTitleSmall(Control? control = null) =>
                new Font(FontFamily.GenericSansSerif, GetDpiAwareFontSize(14f, control), FontStyle.Bold);

            public static Font GetBodyLarge(Control? control = null) =>
                new Font(FontFamily.GenericSansSerif, GetDpiAwareFontSize(16f, control), FontStyle.Regular);

            public static Font GetBodyMedium(Control? control = null) =>
                new Font(FontFamily.GenericSansSerif, GetDpiAwareFontSize(14f, control), FontStyle.Regular);

            public static Font GetBodySmall(Control? control = null) =>
                new Font(FontFamily.GenericSansSerif, GetDpiAwareFontSize(12f, control), FontStyle.Regular);

            public static Font GetLabelLarge(Control? control = null) =>
                new Font(FontFamily.GenericSansSerif, GetDpiAwareFontSize(14f, control), FontStyle.Bold);

            public static Font GetLabelMedium(Control? control = null) =>
                new Font(FontFamily.GenericSansSerif, GetDpiAwareFontSize(12f, control), FontStyle.Bold);

            public static Font GetLabelSmall(Control? control = null) =>
                new Font(FontFamily.GenericSansSerif, GetDpiAwareFontSize(11f, control), FontStyle.Bold);
        }

        #endregion

        #region Material Design Elevation

        /// <summary>
        /// Material Design elevation levels for shadows and depth
        /// </summary>
        public static class Elevation
        {
            public static readonly int Level0 = 0;   // Surface level
            public static readonly int Level1 = 1;   // Elevated surface
            public static readonly int Level2 = 3;   // Card elevation
            public static readonly int Level3 = 6;   // Modal elevation
            public static readonly int Level4 = 8;   // Navigation elevation
            public static readonly int Level5 = 12;  // App bar elevation
        }

        #endregion

        #region Initialization and Configuration

        /// <summary>
        /// Initialize the Material Design theme system with dark theme and high DPI support
        /// </summary>
        public static void Initialize()
        {
            try
            {
                var materialSkinManager = MaterialSkinManager.Instance;

                // Configure dark theme
                materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;

                // Set advanced dark color scheme
                materialSkinManager.ColorScheme = CreateDarkColorScheme();

                // Enable high DPI support
                materialSkinManager.EnforceBackcolorOnAllComponents = false;

                // Configure text rendering for clarity
                ConfigureTextRendering();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing MaterialDesignThemeManager: {ex.Message}");
                // Continue without material design rather than hanging
            }
        }

        /// <summary>
        /// Create a Material Design 3.0 compliant dark color scheme
        /// </summary>
        private static ColorScheme CreateDarkColorScheme()
        {
            return new ColorScheme(
                // Primary colors
                Primary.Blue400,      // Primary - bright blue for dark theme
                Primary.Blue600,      // Primary Dark
                Primary.Blue200,      // Primary Light
                Accent.Cyan200,       // Accent - cyan for highlights
                TextShade.WHITE       // Text shade - white for dark theme
            );
        }

        /// <summary>
        /// Configure text rendering for optimal clarity on high DPI displays
        /// </summary>
        private static void ConfigureTextRendering()
        {
            // Enable ClearType text rendering
            Application.SetCompatibleTextRenderingDefault(false);
        }

        /// <summary>
        /// Apply Material Design dark theme to a form
        /// </summary>
        /// <param name="form">Form to apply theme to</param>
        public static void ApplyDarkTheme(MaterialForm form)
        {
            if (form == null) return;

            // Add form to MaterialSkin manager
            MaterialSkinManager.Instance.AddFormToManage(form);

            // Configure form properties for dark theme
            form.BackColor = DarkTheme.Surface;
            form.ForeColor = DarkTheme.OnSurface;

            // Apply high DPI awareness
            EnableHighDpiForForm(form);

            // Apply Material Design styling to child controls
            ApplyDarkThemeToControls(form.Controls);
        }

        /// <summary>
        /// Enable high DPI support for a specific form
        /// </summary>
        /// <param name="form">Form to enable high DPI for</param>
        private static void EnableHighDpiForForm(Form form)
        {
            form.AutoScaleMode = AutoScaleMode.Dpi;
            form.AutoScaleDimensions = new SizeF(96F, 96F);

            // Set minimum size based on DPI
            if (form.MinimumSize != Size.Empty)
            {
                form.MinimumSize = new Size(
                    GetDpiAwareSize(form.MinimumSize.Width, form),
                    GetDpiAwareSize(form.MinimumSize.Height, form)
                );
            }
        }

        /// <summary>
        /// Apply dark theme styling to a collection of controls
        /// </summary>
        /// <param name="controls">Controls to apply styling to</param>
        private static void ApplyDarkThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                ApplyDarkThemeToControl(control);

                // Recursively apply to child controls
                if (control.HasChildren)
                {
                    ApplyDarkThemeToControls(control.Controls);
                }
            }
        }

        /// <summary>
        /// Apply dark theme styling to a specific control
        /// </summary>
        /// <param name="control">Control to apply styling to</param>
        private static void ApplyDarkThemeToControl(Control control)
        {
            switch (control)
            {
                case MaterialButton button:
                    ApplyDarkThemeToButton(button);
                    break;

                case MaterialLabel label:
                    ApplyDarkThemeToLabel(label);
                    break;

                case Panel panel:
                    ApplyDarkThemeToPanel(panel);
                    break;

                case DataGridView dataGrid:
                    ApplyDarkThemeToDataGrid(dataGrid);
                    break;

                default:
                    // Apply general dark theme properties
                    if (!(control is MaterialSkin.Controls.MaterialForm))
                    {
                        control.BackColor = DarkTheme.Surface;
                        control.ForeColor = DarkTheme.OnSurface;
                    }
                    break;
            }
        }

        /// <summary>
        /// Apply dark theme to Material buttons
        /// </summary>
        private static void ApplyDarkThemeToButton(MaterialButton button)
        {
            // MaterialSkin will handle button theming automatically
            // Just ensure proper font sizing for high DPI
            button.Font = Typography.GetLabelMedium(button);
        }

        /// <summary>
        /// Apply dark theme to Material labels
        /// </summary>
        private static void ApplyDarkThemeToLabel(MaterialLabel label)
        {
            // MaterialSkin will handle label theming automatically
            // Ensure proper contrast for accessibility
            label.ForeColor = DarkTheme.OnSurface;
        }

        /// <summary>
        /// Apply dark theme to panels
        /// </summary>
        private static void ApplyDarkThemeToPanel(Panel panel)
        {
            panel.BackColor = DarkTheme.Surface;
            panel.ForeColor = DarkTheme.OnSurface;
        }

        /// <summary>
        /// Apply dark theme to data grids with Material Design styling
        /// </summary>
        private static void ApplyDarkThemeToDataGrid(DataGridView dataGrid)
        {
            // Background colors
            dataGrid.BackgroundColor = DarkTheme.Surface;
            dataGrid.DefaultCellStyle.BackColor = DarkTheme.Surface;
            dataGrid.DefaultCellStyle.ForeColor = DarkTheme.OnSurface;
            dataGrid.DefaultCellStyle.SelectionBackColor = DarkTheme.Primary;
            dataGrid.DefaultCellStyle.SelectionForeColor = DarkTheme.OnPrimary;

            // Header styling
            dataGrid.ColumnHeadersDefaultCellStyle.BackColor = DarkTheme.SurfaceContainer;
            dataGrid.ColumnHeadersDefaultCellStyle.ForeColor = DarkTheme.OnSurface;
            dataGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = DarkTheme.Primary;
            dataGrid.ColumnHeadersDefaultCellStyle.SelectionForeColor = DarkTheme.OnPrimary;

            // Row header styling
            dataGrid.RowHeadersDefaultCellStyle.BackColor = DarkTheme.SurfaceContainer;
            dataGrid.RowHeadersDefaultCellStyle.ForeColor = DarkTheme.OnSurface;
            dataGrid.RowHeadersDefaultCellStyle.SelectionBackColor = DarkTheme.Primary;
            dataGrid.RowHeadersDefaultCellStyle.SelectionForeColor = DarkTheme.OnPrimary;

            // Alternating row colors for better readability
            dataGrid.AlternatingRowsDefaultCellStyle.BackColor = DarkTheme.SurfaceVariant;
            dataGrid.AlternatingRowsDefaultCellStyle.ForeColor = DarkTheme.OnSurface;

            // Grid line color
            dataGrid.GridColor = DarkTheme.Outline;

            // Border style
            dataGrid.BorderStyle = BorderStyle.None;
            dataGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            // Font for high DPI
            dataGrid.DefaultCellStyle.Font = Typography.GetBodyMedium(dataGrid);
            dataGrid.ColumnHeadersDefaultCellStyle.Font = Typography.GetLabelMedium(dataGrid);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Create a Material Design elevated card effect
        /// </summary>
        /// <param name="panel">Panel to apply elevation to</param>
        /// <param name="elevation">Elevation level (0-5)</param>
        public static void ApplyCardElevation(Panel panel, int elevation = 2)
        {
            panel.BackColor = DarkTheme.SurfaceContainer;

            // Apply subtle border for elevation effect in dark theme
            switch (elevation)
            {
                case 0:
                    panel.BackColor = DarkTheme.Surface;
                    break;
                case 1:
                    panel.BackColor = DarkTheme.SurfaceVariant;
                    break;
                case 2:
                    panel.BackColor = DarkTheme.SurfaceContainer;
                    break;
                case 3:
                case 4:
                case 5:
                    panel.BackColor = DarkTheme.SurfaceContainerHigh;
                    break;
            }
        }

        /// <summary>
        /// Get appropriate text color for the given background
        /// </summary>
        /// <param name="backgroundColor">Background color</param>
        /// <returns>Contrasting text color</returns>
        public static Color GetContrastingTextColor(Color backgroundColor)
        {
            // Calculate relative luminance
            double luminance = (0.299 * backgroundColor.R + 0.587 * backgroundColor.G + 0.114 * backgroundColor.B) / 255;

            // Return white for dark backgrounds, dark for light backgrounds
            return luminance > 0.5 ? DarkTheme.OnSurface : Color.White;
        }

        #endregion

        #region Comprehensive DPI-aware Configuration

        /// <summary>
        /// Apply comprehensive DPI-aware Material Design configuration to a form
        /// </summary>
        /// <param name="form">Form to configure</param>
        /// <param name="applyToAllControls">Whether to apply to all child controls recursively</param>
        public static void ApplyDpiAwareMaterialDesign(MaterialForm form, bool applyToAllControls = true)
        {
            if (form == null) return;

            try
            {
                // Configure auto-scaling for optimal DPI handling
                form.ConfigureAutoScaling(AutoScaleMode.Dpi);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configuring auto-scaling: {ex.Message}");
            }

            try
            {
                // Apply Material Design theme
                ApplyDarkTheme(form);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying dark theme: {ex.Message}");
            }

            if (applyToAllControls)
            {
                try
                {
                    // Apply DPI-aware spacing to all MaterialSkin controls
                    form.ApplyDpiAwareSpacingToAll();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error applying DPI-aware spacing: {ex.Message}");
                }
            }

            try
            {
                // Configure form-specific DPI settings
                ConfigureFormForHighDpi(form);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configuring form for high DPI: {ex.Message}");
            }
        }

        /// <summary>
        /// Configure form-specific high DPI settings
        /// </summary>
        /// <param name="form">Form to configure</param>
        private static void ConfigureFormForHighDpi(Form form)
        {
            var scaleFactor = DpiScaleHelper.GetControlScaleFactor(form);

            // Set minimum form size based on DPI
            int baseMinWidth = 800;
            int baseMinHeight = 600;
            form.MinimumSize = new Size(
                DpiScaleHelper.ScaleSize(baseMinWidth, scaleFactor),
                DpiScaleHelper.ScaleSize(baseMinHeight, scaleFactor)
            );

            // Set high-quality font scaling
            float baseFontSize = 8.25f; // Standard WinForms base font size
            form.Font = DpiScaleHelper.CreateFont(
                form.Font?.FontFamily ?? SystemFonts.DefaultFont.FontFamily,
                baseFontSize,
                form.Font?.Style ?? FontStyle.Regular,
                form
            );
        }

        /// <summary>
        /// Create a responsive layout container with DPI awareness
        /// </summary>
        /// <param name="columns">Number of columns</param>
        /// <param name="rows">Number of rows</param>
        /// <param name="control">Control for DPI context</param>
        /// <returns>Configured responsive layout panel</returns>
        public static TableLayoutPanel CreateResponsiveLayout(int columns, int rows, Control? control = null)
        {
            try
            {
                var layoutPanel = new TableLayoutPanel
                {
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    ColumnCount = columns,
                    RowCount = rows,
                    Dock = DockStyle.Fill,
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.None
                };

                // Configure column styles for proportional sizing
                for (int i = 0; i < columns; i++)
                {
                    layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
                }

                // Configure row styles for proportional sizing
                for (int i = 0; i < rows; i++)
                {
                    layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));
                }

                // Apply DPI-aware spacing
                try
                {
                    layoutPanel.Padding = DpiScaleHelper.CreatePadding(8, control);
                    layoutPanel.Margin = DpiScaleHelper.CreatePadding(4, control);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating DPI-aware padding: {ex.Message}");
                    // Fallback to default padding
                    layoutPanel.Padding = new Padding(8);
                    layoutPanel.Margin = new Padding(4);
                }

                // Set background to match Material Design surface
                layoutPanel.BackColor = DarkTheme.Surface;

                return layoutPanel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating responsive layout: {ex.Message}");
                // Return a basic layout as fallback
                return new TableLayoutPanel
                {
                    AutoSize = true,
                    ColumnCount = columns,
                    RowCount = rows,
                    Dock = DockStyle.Fill
                };
            }
        }

        /// <summary>
        /// Configure MaterialSkin manager with enhanced DPI-aware settings
        /// </summary>
        /// <param name="form">Form to apply theme to</param>
        public static void ConfigureMaterialSkinManager(MaterialForm form)
        {
            var skinManager = MaterialSkinManager.Instance;
            skinManager.AddFormToManage(form);
            skinManager.Theme = MaterialSkinManager.Themes.DARK;

            // Set enhanced color scheme with accessibility considerations
            skinManager.ColorScheme = new ColorScheme(
                Primary.BlueGrey800,    // Primary
                Primary.BlueGrey900,    // Primary Dark
                Primary.BlueGrey500,    // Primary Light
                Accent.LightBlue200,    // Accent
                TextShade.WHITE         // Text shade
            );

            // Apply DPI-aware control adjustments
            ApplyDpiAwareControlAdjustments(form);
        }

        /// <summary>
        /// Apply DPI-aware adjustments to MaterialSkin controls globally
        /// </summary>
        /// <param name="container">Container to process</param>
        private static void ApplyDpiAwareControlAdjustments(Control container)
        {
            var scaleFactor = DpiScaleHelper.GetControlScaleFactor(container);

            foreach (Control control in container.Controls)
            {
                // Apply specific adjustments based on control type
                switch (control)
                {
                    case MaterialButton button:
                        AdjustMaterialButton(button, scaleFactor);
                        break;
                    case MaterialTextBox textBox:
                        AdjustMaterialTextBox(textBox, scaleFactor);
                        break;
                    case MaterialCard card:
                        AdjustMaterialCard(card, scaleFactor);
                        break;
                }

                // Recursively process child controls
                if (control.HasChildren)
                {
                    ApplyDpiAwareControlAdjustments(control);
                }
            }
        }

        /// <summary>
        /// Adjust MaterialButton for DPI scaling
        /// </summary>
        private static void AdjustMaterialButton(MaterialButton button, DpiScaleHelper.ScaleFactor scaleFactor)
        {
            // Set DPI-aware margins and padding
            button.Margin = DpiScaleHelper.CreatePadding(8, button);
            button.Padding = DpiScaleHelper.CreatePadding(4, button);

            // Ensure minimum touch target size (44px at standard DPI)
            int minTouchTarget = DpiScaleHelper.ScaleSize(44, scaleFactor);
            if (button.Height < minTouchTarget)
            {
                button.Height = minTouchTarget;
            }
        }

        /// <summary>
        /// Adjust MaterialTextBox for DPI scaling
        /// </summary>
        private static void AdjustMaterialTextBox(MaterialTextBox textBox, DpiScaleHelper.ScaleFactor scaleFactor)
        {
            // Set DPI-aware margins
            textBox.Margin = DpiScaleHelper.CreatePadding(8, textBox);

            // Ensure adequate height for readability
            int minHeight = DpiScaleHelper.ScaleSize(32, scaleFactor);
            if (textBox.Height < minHeight)
            {
                textBox.Height = minHeight;
            }
        }

        /// <summary>
        /// Adjust MaterialCard for DPI scaling
        /// </summary>
        private static void AdjustMaterialCard(MaterialCard card, DpiScaleHelper.ScaleFactor scaleFactor)
        {
            // Set generous DPI-aware padding for content
            card.Padding = DpiScaleHelper.CreatePadding(16, 12, card);
            card.Margin = DpiScaleHelper.CreatePadding(8, card);
        }

        #endregion
    }
}
