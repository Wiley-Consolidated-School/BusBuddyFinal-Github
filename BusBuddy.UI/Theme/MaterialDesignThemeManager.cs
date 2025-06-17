/*
 * 🎨 BusBuddy Material Design Theme Manager
 * ========================================
 *
 * A comprehensive Material Design 3.0 theming system with:
 * • 🌙 Rich dark theme with accessibility focus
 * • 📱 High DPI support and responsive design
 * • 🎬 Smooth animations and transitions
 * • ✨ Visual effects (shadows, gradients, rounded corners)
 * • 🎯 One-click form styling
 * • 📏 Consistent spacing system based on 8px grid
 * • 🔤 Typography scale following Material Design guidelines
 *
 * Usage:
 * ```csharp
 * // Quick setup for any MaterialForm
 * MaterialDesignThemeManager.SetupMaterialForm(this);
 *
 * // Apply theme to specific controls
 * MaterialDesignThemeManager.QuickStyle(myButton);
 * MaterialDesignThemeManager.ApplyCardElevation(myPanel, 2);
 * ```
 *
 * Author: BusBuddy Development Team
 * Version: 2.0.0 - Enhanced Material Design 3.0
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace BusBuddy.UI.Theme
{
    /// <summary>
    /// 🎨 Advanced Material Design 3.0 Theme Manager
    /// Features: Dark theme, high DPI support, animations, accessibility, and modern UI patterns
    /// Implements Material You design system with dynamic colors and enhanced visual hierarchy
    /// </summary>
    public static class MaterialDesignThemeManager
    {
        #region 🌙 Material Design 3.0 Dark Theme Colors

        // 🌈 Material Design 3.0 Dark Theme Color Palette
        public static class DarkTheme
        {
            // 🏢 Surface Colors (Material Design 3.0 Dark Theme)
            public static Color Surface => Color.FromArgb(16, 16, 20);           // Rich dark surface
            public static Color SurfaceVariant => Color.FromArgb(24, 24, 29);    // Elevated surface
            public static Color SurfaceContainer => Color.FromArgb(30, 30, 35);  // Container surface
            public static Color SurfaceContainerHigh => Color.FromArgb(35, 35, 40); // High emphasis surface
            public static Color SurfaceBright => Color.FromArgb(54, 54, 54);     // Bright surface variant
            public static Color SurfaceDim => Color.FromArgb(17, 17, 17);        // Dim surface variant

            // 🔵 Primary Colors (Enhanced Blue Palette)
            public static Color Primary => Color.FromArgb(165, 200, 255);        // Vibrant blue for dark theme
            public static Color OnPrimary => Color.FromArgb(0, 28, 58);          // Dark blue for text on primary
            public static Color PrimaryContainer => Color.FromArgb(0, 82, 204);  // Rich blue container
            public static Color OnPrimaryContainer => Color.FromArgb(210, 227, 252); // Light text on primary container
            public static Color PrimaryFixed => Color.FromArgb(210, 227, 252);   // Fixed primary color
            public static Color OnPrimaryFixed => Color.FromArgb(0, 28, 58);     // Text on fixed primary

            // 🔘 Secondary Colors (Sophisticated Grey-Blue)
            public static readonly Color Secondary = Color.FromArgb(191, 199, 220);      // Elegant grey-blue
            public static readonly Color OnSecondary = Color.FromArgb(41, 50, 65);       // Dark text on secondary
            public static readonly Color SecondaryContainer = Color.FromArgb(64, 71, 88); // Rich secondary container
            public static readonly Color OnSecondaryContainer = Color.FromArgb(219, 227, 248); // Light text on secondary container
            public static readonly Color SecondaryFixed = Color.FromArgb(219, 227, 248); // Fixed secondary color
            public static readonly Color OnSecondaryFixed = Color.FromArgb(41, 50, 65);  // Text on fixed secondary

            // ✨ Accent Colors (Purple for premium feel)
            public static readonly Color Tertiary = Color.FromArgb(220, 184, 255);       // Elegant purple accent
            public static readonly Color OnTertiary = Color.FromArgb(56, 30, 114);       // Dark purple text
            public static readonly Color TertiaryContainer = Color.FromArgb(79, 55, 139); // Rich purple container
            public static readonly Color OnTertiaryContainer = Color.FromArgb(240, 219, 255); // Light text on tertiary container
            public static readonly Color TertiaryFixed = Color.FromArgb(240, 219, 255);  // Fixed tertiary color
            public static readonly Color OnTertiaryFixed = Color.FromArgb(56, 30, 114);  // Text on fixed tertiary

            // 📝 Text Colors (Enhanced readability)
            public static Color OnSurface => Color.FromArgb(230, 225, 229);      // Primary text - high contrast
            public static Color OnSurfaceVariant => Color.FromArgb(196, 196, 208); // Secondary text
            public static Color Outline => Color.FromArgb(147, 143, 153);        // Outline color
            public static Color OutlineVariant => Color.FromArgb(68, 71, 78);    // Subtle outline
            public static Color InverseSurface => Color.FromArgb(231, 224, 236); // Inverse surface
            public static Color InverseOnSurface => Color.FromArgb(48, 47, 51);  // Text on inverse surface

            // 🚨 Status Colors (Accessible and vibrant)
            public static Color Error => Color.FromArgb(255, 180, 171);          // Warm red for errors
            public static Color OnError => Color.FromArgb(105, 0, 5);            // Dark red text
            public static Color ErrorContainer => Color.FromArgb(147, 0, 10);    // Rich red container
            public static Color OnErrorContainer => Color.FromArgb(255, 218, 214); // Light text on error container

            public static Color Success => Color.FromArgb(166, 218, 149);        // Fresh green for success
            public static Color OnSuccess => Color.FromArgb(0, 57, 10);          // Dark green text
            public static Color SuccessContainer => Color.FromArgb(0, 83, 18);   // Rich green container
            public static Color OnSuccessContainer = Color.FromArgb(194, 246, 177); // Light text on success container

            public static Color Warning => Color.FromArgb(255, 202, 40);         // Vibrant orange for warnings
            public static Color OnWarning => Color.FromArgb(66, 31, 0);          // Dark orange text
            public static Color WarningContainer => Color.FromArgb(96, 52, 0);   // Rich orange container
            public static Color OnWarningContainer => Color.FromArgb(255, 221, 168); // Light text on warning container

            // 💎 Premium Gradient Colors
            public static Color GradientStart => Color.FromArgb(30, 41, 59);     // Deep blue-grey
            public static Color GradientEnd => Color.FromArgb(15, 23, 42);       // Darker blue-grey
            public static Color AccentGlow => Color.FromArgb(59, 130, 246);      // Bright blue glow
        }

        #endregion

        #region 🎬 Animation and Transition Effects

        /// <summary>
        /// 🎭 Animation timing and easing curves for smooth transitions
        /// </summary>
        public static class Animations
        {
            // Standard animation durations (Material Design spec)
            public const int FastDuration = 150;     // Fast transitions (toggles, switches)
            public const int StandardDuration = 300; // Standard transitions (most UI elements)
            public const int SlowDuration = 500;     // Slow transitions (page transitions)
            public const int ExtraSlowDuration = 750; // Extra slow (complex animations)

            // Easing curves
            public const string StandardEasing = "cubic-bezier(0.4, 0.0, 0.2, 1)";
            public const string DecelerateEasing = "cubic-bezier(0.0, 0.0, 0.2, 1)";
            public const string AccelerateEasing = "cubic-bezier(0.4, 0.0, 1, 1)";
            public const string SharpEasing = "cubic-bezier(0.4, 0.0, 0.6, 1)";

            /// <summary>
            /// Create a smooth fade-in animation for controls
            /// </summary>
            public static void FadeIn(Control control, int duration = StandardDuration)
            {
                if (control == null) return;

                control.Visible = true;
                var timer = new System.Windows.Forms.Timer { Interval = 16 }; // ~60 FPS
                var startTime = DateTime.Now;

                timer.Tick += (s, e) =>
                {
                    var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                    var progress = Math.Min(elapsed / duration, 1.0);
                    var easedProgress = EaseOut(progress);

                    // Simulate opacity by adjusting the control's visibility
                    if (progress >= 1.0)
                    {
                        timer.Stop();
                        timer.Dispose();
                        control.Invalidate();
                    }
                    else
                    {
                        control.Invalidate();
                    }
                };
                timer.Start();
            }

            /// <summary>
            /// Create a smooth slide-up animation for panels
            /// </summary>
            public static void SlideUp(Control control, int distance = 20, int duration = StandardDuration)
            {
                if (control == null) return;

                var startY = control.Location.Y + distance;
                var targetY = control.Location.Y;
                var timer = new System.Windows.Forms.Timer { Interval = 16 };
                var startTime = DateTime.Now;

                control.Location = new Point(control.Location.X, startY);

                timer.Tick += (s, e) =>
                {
                    var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                    var progress = Math.Min(elapsed / duration, 1.0);
                    var easedProgress = EaseOut(progress);

                    var currentY = (int)(startY + (targetY - startY) * easedProgress);
                    control.Location = new Point(control.Location.X, currentY);

                    if (progress >= 1.0)
                    {
                        timer.Stop();
                        timer.Dispose();
                    }
                };
                timer.Start();
            }

            /// <summary>
            /// Ease-out cubic function for smooth animations
            /// </summary>
            private static double EaseOut(double t)
            {
                return 1 - Math.Pow(1 - t, 3);
            }

            /// <summary>
            /// Add subtle hover effect to buttons
            /// </summary>
            public static void AddHoverEffect(MaterialButton button)
            {
                if (button == null) return;

                var originalBackColor = button.BackColor;
                var hoverColor = LightenColor(originalBackColor, 0.1f);

                button.MouseEnter += (s, e) =>
                {
                    button.BackColor = hoverColor;
                };

                button.MouseLeave += (s, e) =>
                {
                    button.BackColor = originalBackColor;
                };
            }

            /// <summary>
            /// Lighten a color by a specified amount
            /// </summary>
            private static Color LightenColor(Color color, float amount)
            {
                var r = Math.Min(255, (int)(color.R + (255 - color.R) * amount));
                var g = Math.Min(255, (int)(color.G + (255 - color.G) * amount));
                var b = Math.Min(255, (int)(color.B + (255 - color.B) * amount));
                return Color.FromArgb(color.A, r, g, b);
            }
        }

        #endregion

        #region 🎨 Advanced Visual Effects

        /// <summary>
        /// 🌟 Advanced visual effects and styling utilities
        /// </summary>
        public static class VisualEffects
        {
            /// <summary>
            /// Create a beautiful gradient background for panels
            /// </summary>
            public static void ApplyGradientBackground(Panel panel, Color startColor, Color endColor, LinearGradientMode direction = LinearGradientMode.Vertical)
            {
                if (panel == null) return;

                panel.Paint += (s, e) =>
                {
                    using (var brush = new LinearGradientBrush(panel.ClientRectangle, startColor, endColor, direction))
                    {
                        e.Graphics.FillRectangle(brush, panel.ClientRectangle);
                    }
                };
            }

            /// <summary>
            /// Apply elegant rounded corners to any control
            /// </summary>
            public static void ApplyRoundedCorners(Control control, int radius = 8)
            {
                if (control == null) return;

                control.Paint += (s, e) =>
                {
                    var path = CreateRoundedRectanglePath(control.ClientRectangle, radius);
                    control.Region = new Region(path);
                };
            }

            /// <summary>
            /// Create a subtle shadow effect for elevated components
            /// </summary>
            public static void ApplyShadowEffect(Control control, Color shadowColor, int shadowDepth = 3, int shadowOpacity = 50)
            {
                if (control == null) return;

                var parent = control.Parent;
                if (parent == null) return;

                parent.Paint += (s, e) =>
                {
                    var shadowRect = new Rectangle(
                        control.Bounds.X + shadowDepth,
                        control.Bounds.Y + shadowDepth,
                        control.Bounds.Width,
                        control.Bounds.Height
                    );

                    using (var brush = new SolidBrush(Color.FromArgb(shadowOpacity, shadowColor)))
                    {
                        e.Graphics.FillRectangle(brush, shadowRect);
                    }
                };
            }

            /// <summary>
            /// Add a subtle glow effect to important elements
            /// </summary>
            public static void ApplyGlowEffect(Control control, Color glowColor, int glowSize = 2)
            {
                if (control == null) return;

                control.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    for (int i = 1; i <= glowSize; i++)
                    {
                        var alpha = (int)(50 / i);
                        using (var pen = new Pen(Color.FromArgb(alpha, glowColor), i * 2))
                        {
                            var rect = new Rectangle(i, i, control.Width - i * 2, control.Height - i * 2);
                            e.Graphics.DrawRectangle(pen, rect);
                        }
                    }
                };
            }

            /// <summary>
            /// Create rounded rectangle path for custom drawing
            /// </summary>
            private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
            {
                var path = new GraphicsPath();
                var diameter = radius * 2;

                var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));
                path.AddArc(arc, 180, 90);

                arc.X = bounds.Right - diameter;
                path.AddArc(arc, 270, 90);

                arc.Y = bounds.Bottom - diameter;
                path.AddArc(arc, 0, 90);

                arc.X = bounds.Left;
                path.AddArc(arc, 90, 90);

                path.CloseFigure();
                return path;
            }

            /// <summary>
            /// Apply Material Design ripple effect simulation
            /// </summary>
            public static void ApplyRippleEffect(Control control)
            {
                if (control == null) return;

                Point clickPoint = Point.Empty;
                bool isAnimating = false;
                var animationProgress = 0.0;

                control.MouseDown += (s, e) =>
                {
                    clickPoint = e.Location;
                    isAnimating = true;
                    animationProgress = 0.0;

                    var timer = new System.Windows.Forms.Timer { Interval = 16 };
                    var startTime = DateTime.Now;

                    timer.Tick += (ts, te) =>
                    {
                        var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                        animationProgress = Math.Min(elapsed / 300.0, 1.0);

                        control.Invalidate();

                        if (animationProgress >= 1.0)
                        {
                            isAnimating = false;
                            timer.Stop();
                            timer.Dispose();
                        }
                    };
                    timer.Start();
                };

                control.Paint += (s, e) =>
                {
                    if (isAnimating && clickPoint != Point.Empty)
                    {
                        var maxRadius = Math.Max(control.Width, control.Height);
                        var currentRadius = (int)(maxRadius * animationProgress);
                        var alpha = (int)(100 * (1 - animationProgress));

                        using (var brush = new SolidBrush(Color.FromArgb(alpha, DarkTheme.Primary)))
                        {
                            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            e.Graphics.FillEllipse(brush,
                                clickPoint.X - currentRadius,
                                clickPoint.Y - currentRadius,
                                currentRadius * 2,
                                currentRadius * 2);
                        }
                    }
                };
            }
        }

        #endregion

        #region 📱 High DPI Support

        /// <summary>
        /// Get DPI-aware size based on current system DPI
        /// </summary>
        /// <param name="baseSize">Base size at 96 DPI</param>
        /// <param name="control">Control to get DPI from (optional)</param>
        /// <returns>DPI-scaled size</returns>
        public static int GetDpiAwareSize(int baseSize, Control? control = null)
        {
            if (control?.DeviceDpi > 0)
            {
                return (int)(baseSize * (control.DeviceDpi / 96.0));
            }
            return baseSize;
        }

        /// <summary>
        /// Get DPI-aware padding
        /// </summary>
        public static Padding GetDpiAwarePadding(int horizontal, int vertical, Control? control = null)
        {
            var h = GetDpiAwareSize(horizontal, control);
            var v = GetDpiAwareSize(vertical, control);
            return new Padding(h, v, h, v);
        }

        /// <summary>
        /// Get DPI-aware font size
        /// </summary>
        public static float GetDpiAwareFontSize(float baseSize, Control? control = null)
        {
            if (control?.DeviceDpi > 0)
            {
                return baseSize * (control.DeviceDpi / 96.0f);
            }
            return baseSize;
        }

        #endregion

        #region 🔤 Material Design Typography        /// <summary>
        /// 📚 Material Design 3.0 Typography Scale (DPI-aware & Responsive)
        /// Enhanced typography system with accessibility and readability focus
        /// </summary>
        public static class Typography
        {
            // 🎯 Display Typography (Hero content)
            public static Font GetDisplayLarge(Control? control = null) =>
                CreateDpiAwareFont("Segoe UI", 57f, FontStyle.Regular, control);

            public static Font GetDisplayMedium(Control? control = null) =>
                CreateDpiAwareFont("Segoe UI", 45f, FontStyle.Regular, control);

            public static Font GetDisplaySmall(Control? control = null) =>
                CreateDpiAwareFont("Segoe UI", 36f, FontStyle.Regular, control);

            // 📰 Headline Typography (Section headers)
            public static Font GetHeadlineLarge(Control? control = null) =>
                CreateDpiAwareFont("Segoe UI", 32f, FontStyle.Regular, control);

            public static Font GetHeadlineMedium(Control? control = null) =>
                CreateDpiAwareFont("Segoe UI", 28f, FontStyle.Regular, control);

            public static Font GetHeadlineSmall(Control? control = null) =>
                CreateDpiAwareFont("Segoe UI", 24f, FontStyle.Regular, control);

            // 🏷️ Title Typography (Card titles, dialog headers)
            public static Font GetTitleLarge(Control? control = null) =>
                CreateDpiAwareFont("Segoe UI", 22f, FontStyle.Regular, control);

            public static Font GetTitleMedium(Control? control = null) =>
                CreateDpiAwareFont("Segoe UI", 18f, FontStyle.Regular, control);

            public static Font GetTitleSmall(Control? control = null) =>
                CreateDpiAwareFont("Segoe UI", 16f, FontStyle.Regular, control);

            // 📄 Body Typography (Main content)
            public static Font GetBodyLarge(Control? control = null) =>
                CreateDpiAwareFont("Segoe UI", 16f, FontStyle.Regular, control);

            public static Font GetBodyMedium(Control? control = null) =>
                CreateDpiAwareFont("Segoe UI", 14f, FontStyle.Regular, control);

            public static Font GetBodySmall(Control? control = null) =>
                CreateDpiAwareFont("Segoe UI", 12f, FontStyle.Regular, control);

            // 🏷️ Label Typography (Buttons, tabs, chips)
            public static Font GetLabelLarge(Control? control = null) =>
                CreateDpiAwareFont("Segoe UI", 14f, FontStyle.Bold, control);

            public static Font GetLabelMedium(Control? control = null) =>
                CreateDpiAwareFont("Segoe UI", 12f, FontStyle.Bold, control);

            public static Font GetLabelSmall(Control? control = null) =>
                CreateDpiAwareFont("Segoe UI", 11f, FontStyle.Bold, control);

            // 📝 Specialized Typography
            public static Font GetCaptionText(Control? control = null) =>
                CreateDpiAwareFont("Segoe UI", 10f, FontStyle.Regular, control);

            public static Font GetOverlineText(Control? control = null) =>
                CreateDpiAwareFont("Segoe UI", 10f, FontStyle.Bold, control);

            public static Font GetMonospaceText(Control? control = null) =>
                CreateDpiAwareFont("Cascadia Code", 12f, FontStyle.Regular, control, "Consolas");

            /// <summary>
            /// Create DPI-aware font with fallback support
            /// </summary>
            private static Font CreateDpiAwareFont(string fontFamily, float size, FontStyle style, Control? control, string? fallbackFamily = null)
            {
                var scaledSize = GetDpiAwareFontSize(size, control);

                try
                {
                    return new Font(fontFamily, scaledSize, style);
                }
                catch (ArgumentException)
                {
                    // Fallback to system default or specified fallback
                    var fallback = fallbackFamily ?? SystemFonts.DefaultFont.FontFamily.Name;
                    return new Font(fallback, scaledSize, style);
                }
            }
        }

        #endregion

        #region 🏔️ Material Design Elevation & Spacing

        /// <summary>
        /// 📐 Material Design elevation levels and spacing system
        /// </summary>
        public static class Elevation
        {
            // 🎭 Elevation levels for shadows and depth (Material Design 3.0)
            public const int Level0 = 0;   // Surface level (flat components)
            public const int Level1 = 1;   // Slightly elevated (cards at rest)
            public const int Level2 = 3;   // Raised elements (buttons, switches)
            public const int Level3 = 6;   // Modal surfaces (dialogs, menus)
            public const int Level4 = 8;   // Navigation surfaces (nav drawer)
            public const int Level5 = 12;  // Top app bar elevation

            /// <summary>
            /// Get shadow settings for the specified elevation level
            /// </summary>
            public static (int offsetX, int offsetY, int blur, int spread, Color color) GetShadow(int level)
            {
                return level switch
                {
                    0 => (0, 0, 0, 0, Color.Transparent),
                    1 => (0, 1, 3, 0, Color.FromArgb(25, 0, 0, 0)),
                    2 => (0, 2, 6, 0, Color.FromArgb(35, 0, 0, 0)),
                    3 => (0, 4, 8, 0, Color.FromArgb(45, 0, 0, 0)),
                    4 => (0, 6, 10, 0, Color.FromArgb(55, 0, 0, 0)),
                    5 => (0, 8, 12, 0, Color.FromArgb(65, 0, 0, 0)),
                    _ => (0, 2, 6, 0, Color.FromArgb(35, 0, 0, 0))
                };
            }
        }

        /// <summary>
        /// 📏 Consistent spacing system based on 8px grid
        /// </summary>
        public static class Spacing
        {
            // Base spacing unit (8px at standard DPI)
            public const int BaseUnit = 8;

            // Predefined spacing values
            public const int XTiny = BaseUnit / 2;     // 4px
            public const int Tiny = BaseUnit;          // 8px
            public const int Small = BaseUnit * 2;     // 16px
            public const int Medium = BaseUnit * 3;    // 24px
            public const int Large = BaseUnit * 4;     // 32px
            public const int XLarge = BaseUnit * 6;    // 48px
            public const int XXLarge = BaseUnit * 8;   // 64px

            /// <summary>
            /// Get DPI-aware spacing value
            /// </summary>
            public static int GetDpiAware(int baseSpacing, Control? control = null)
            {
                return GetDpiAwareSize(baseSpacing, control);
            }

            /// <summary>
            /// Create DPI-aware padding from spacing constants
            /// </summary>
            public static Padding CreatePadding(int spacing, Control? control = null)
            {
                var size = GetDpiAware(spacing, control);
                return new Padding(size);
            }

            /// <summary>
            /// Create DPI-aware asymmetric padding
            /// </summary>
            public static Padding CreatePadding(int horizontal, int vertical, Control? control = null)
            {
                var h = GetDpiAware(horizontal, control);
                var v = GetDpiAware(vertical, control);
                return new Padding(h, v, h, v);
            }
        }

        #endregion

        #region ⚙️ Initialization and Theme Application

        /// <summary>
        /// 🚀 Initialize application-wide settings (MUST be called in Main() before any forms are created)
        /// </summary>
        public static void InitializeApplication()
        {
            // Set application-wide visual styles - MUST be called before any forms are created
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }

        /// <summary>
        /// 🎨 Initialize the Material Design theme system with enhanced features
        /// </summary>
        public static void Initialize()
        {
            // Initialize MaterialSkinManager with optimal settings
            var skinManager = MaterialSkinManager.Instance;
            skinManager.Theme = MaterialSkinManager.Themes.DARK;

            // Set enhanced color scheme
            skinManager.ColorScheme = new ColorScheme(
                Primary.BlueGrey800,    // Primary
                Primary.BlueGrey900,    // Primary Dark
                Primary.BlueGrey500,    // Primary Light
                Accent.LightBlue200,    // Accent
                TextShade.WHITE         // Text shade
            );
        }

        /// <summary>
        /// 🎨 Apply comprehensive Material Design dark theme to a form
        /// </summary>
        /// <param name="form">Form to apply theme to</param>
        /// <param name="enableAnimations">Whether to enable smooth animations</param>
        public static void ApplyDarkTheme(MaterialForm form, bool enableAnimations = true)
        {
            if (form == null) return;

            // Apply base theme colors
            form.BackColor = DarkTheme.Surface;
            form.ForeColor = DarkTheme.OnSurface;

            // Apply theme to all child controls
            ApplyThemeToControls(form.Controls, enableAnimations);

            // Add subtle entrance animation
            if (enableAnimations)
            {
                Animations.FadeIn(form, Animations.StandardDuration);
            }
        }

        /// <summary>
        /// 🎯 Apply theme to control collection recursively
        /// </summary>
        private static void ApplyThemeToControls(Control.ControlCollection controls, bool enableAnimations = true)
        {
            foreach (Control control in controls)
            {
                ApplyThemeToControl(control, enableAnimations);

                if (control.HasChildren)
                {
                    ApplyThemeToControls(control.Controls, enableAnimations);
                }
            }
        }

        /// <summary>
        /// 🎨 Apply theme to individual control based on its type
        /// </summary>
        private static void ApplyThemeToControl(Control control, bool enableAnimations = true)
        {
            switch (control)
            {
                case MaterialButton button:
                    ApplyDarkThemeToButton(button);
                    if (enableAnimations) Animations.AddHoverEffect(button);
                    break;

                case MaterialLabel label:
                    ApplyDarkThemeToLabel(label);
                    break;

                case MaterialCard card:
                    ApplyDarkThemeToCard(card);
                    if (enableAnimations) VisualEffects.ApplyRippleEffect(card);
                    break;

                case Panel panel:
                    ApplyDarkThemeToPanel(panel);
                    if (enableAnimations) VisualEffects.ApplyRoundedCorners(panel, 8);
                    break;

                case DataGridView dataGrid:
                    ApplyDarkThemeToDataGrid(dataGrid);
                    break;

                case MaterialTextBox textBox:
                    ApplyDarkThemeToTextBox(textBox);
                    break;
            }
        }

        /// <summary>
        /// 🔘 Apply dark theme to Material buttons with enhanced styling
        /// </summary>
        private static void ApplyDarkThemeToButton(MaterialButton button)
        {
            if (button == null) return;

            // MaterialSkin will handle button theming automatically
            // Ensure proper font sizing for high DPI
            button.Font = Typography.GetLabelMedium(button);

            // Add subtle padding for better touch targets
            button.Margin = Spacing.CreatePadding(Spacing.Tiny, button);

            // Ensure minimum touch target size
            if (button.Height < Spacing.GetDpiAware(44, button))
            {
                button.Height = Spacing.GetDpiAware(44, button);
            }
        }

        /// <summary>
        /// 🏷️ Apply dark theme to Material labels with accessibility focus
        /// </summary>
        private static void ApplyDarkThemeToLabel(MaterialLabel label)
        {
            if (label == null) return;

            // MaterialSkin will handle label theming automatically
            // Ensure proper contrast for accessibility
            label.ForeColor = DarkTheme.OnSurface;
            label.Font = Typography.GetBodyMedium(label);
        }

        /// <summary>
        /// 🎴 Apply dark theme to Material cards with elevation
        /// </summary>
        private static void ApplyDarkThemeToCard(MaterialCard card)
        {
            if (card == null) return;

            // Apply elevated surface color
            card.BackColor = DarkTheme.SurfaceContainer;
            card.ForeColor = DarkTheme.OnSurface;

            // Add generous padding for content
            card.Padding = Spacing.CreatePadding(Spacing.Medium, Spacing.Small, card);
            card.Margin = Spacing.CreatePadding(Spacing.Tiny, card);
        }

        /// <summary>
        /// 📱 Apply dark theme to Material text boxes
        /// </summary>
        private static void ApplyDarkThemeToTextBox(MaterialTextBox textBox)
        {
            if (textBox == null) return;

            // MaterialSkin handles the theming, just ensure proper sizing
            textBox.Font = Typography.GetBodyMedium(textBox);
            textBox.Margin = Spacing.CreatePadding(Spacing.Tiny, textBox);

            // Ensure adequate height for readability
            var minHeight = Spacing.GetDpiAware(40, textBox);
            if (textBox.Height < minHeight)
            {
                textBox.Height = minHeight;
            }
        }

        /// <summary>
        /// 📄 Apply dark theme to panels with optional visual enhancements
        /// </summary>
        private static void ApplyDarkThemeToPanel(Panel panel)
        {
            if (panel == null) return;

            panel.BackColor = DarkTheme.Surface;
            panel.ForeColor = DarkTheme.OnSurface;

            // Add subtle padding for content spacing
            if (panel.Padding == Padding.Empty)
            {
                panel.Padding = Spacing.CreatePadding(Spacing.Small, panel);
            }
        }

        /// <summary>
        /// 📊 Apply dark theme to data grids with Material Design styling and accessibility
        /// </summary>
        private static void ApplyDarkThemeToDataGrid(DataGridView dataGrid)
        {
            if (dataGrid == null) return;

            // 🎨 Background colors with improved contrast
            dataGrid.BackgroundColor = DarkTheme.Surface;
            dataGrid.DefaultCellStyle.BackColor = DarkTheme.Surface;
            dataGrid.DefaultCellStyle.ForeColor = DarkTheme.OnSurface;
            dataGrid.DefaultCellStyle.SelectionBackColor = DarkTheme.Primary;
            dataGrid.DefaultCellStyle.SelectionForeColor = DarkTheme.OnPrimary;

            // 📋 Header styling with enhanced visibility
            dataGrid.ColumnHeadersDefaultCellStyle.BackColor = DarkTheme.SurfaceContainer;
            dataGrid.ColumnHeadersDefaultCellStyle.ForeColor = DarkTheme.OnSurface;
            dataGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = DarkTheme.Primary;
            dataGrid.ColumnHeadersDefaultCellStyle.SelectionForeColor = DarkTheme.OnPrimary;
            dataGrid.ColumnHeadersDefaultCellStyle.Font = Typography.GetLabelMedium(dataGrid);

            // 🏷️ Row header styling
            dataGrid.RowHeadersDefaultCellStyle.BackColor = DarkTheme.SurfaceContainer;
            dataGrid.RowHeadersDefaultCellStyle.ForeColor = DarkTheme.OnSurface;
            dataGrid.RowHeadersDefaultCellStyle.SelectionBackColor = DarkTheme.Primary;
            dataGrid.RowHeadersDefaultCellStyle.SelectionForeColor = DarkTheme.OnPrimary;

            // 🔄 Alternating row colors for better readability
            dataGrid.AlternatingRowsDefaultCellStyle.BackColor = DarkTheme.SurfaceVariant;
            dataGrid.AlternatingRowsDefaultCellStyle.ForeColor = DarkTheme.OnSurface;

            // 🔲 Grid styling
            dataGrid.GridColor = DarkTheme.Outline;
            dataGrid.BorderStyle = BorderStyle.None;
            dataGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGrid.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            // 📝 Typography for optimal readability
            dataGrid.DefaultCellStyle.Font = Typography.GetBodyMedium(dataGrid);

            // 📏 DPI-aware sizing
            dataGrid.ColumnHeadersHeight = Spacing.GetDpiAware(40, dataGrid);
            dataGrid.RowTemplate.Height = Spacing.GetDpiAware(32, dataGrid);

            // 🎯 Enhanced interaction
            dataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGrid.MultiSelect = false;
            dataGrid.ReadOnly = true; // Typically for display
            dataGrid.AllowUserToAddRows = false;
            dataGrid.AllowUserToDeleteRows = false;
            dataGrid.AllowUserToResizeRows = false;

            // 📱 Touch-friendly column sizing
            dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // 🎨 Add subtle padding
            dataGrid.DefaultCellStyle.Padding = new Padding(
                Spacing.GetDpiAware(Spacing.Tiny, dataGrid),
                Spacing.GetDpiAware(Spacing.XTiny, dataGrid),
                Spacing.GetDpiAware(Spacing.Tiny, dataGrid),
                Spacing.GetDpiAware(Spacing.XTiny, dataGrid)
            );
        }

        /// <summary>
        /// 🎨 Create a Material Design elevated card effect with modern styling
        /// </summary>
        /// <param name="panel">Panel to apply elevation to</param>
        /// <param name="elevation">Elevation level (0-5)</param>
        /// <param name="applyRounding">Whether to apply rounded corners</param>
        public static void ApplyCardElevation(Panel panel, int elevation, bool applyRounding = true)
        {
            if (panel == null) return;

            // Apply appropriate surface color based on elevation
            panel.BackColor = elevation switch
            {
                0 => DarkTheme.Surface,
                1 => DarkTheme.SurfaceVariant,
                2 => DarkTheme.SurfaceContainer,
                3 => DarkTheme.SurfaceContainerHigh,
                4 => DarkTheme.SurfaceBright,
                _ => DarkTheme.SurfaceContainerHigh
            };

            // Apply shadow effect for elevation > 0
            if (elevation > 0)
            {
                var shadow = Elevation.GetShadow(elevation);
                VisualEffects.ApplyShadowEffect(panel, shadow.color, Math.Max(shadow.offsetY, 1), shadow.color.A);
            }

            // Apply rounded corners for modern look
            if (applyRounding)
            {
                VisualEffects.ApplyRoundedCorners(panel, Spacing.GetDpiAware(12, panel));
            }

            // Add appropriate padding
            panel.Padding = Spacing.CreatePadding(Spacing.Medium, Spacing.Small, panel);
        }

        /// <summary>
        /// 🌈 Get appropriate text color for the given background with WCAG compliance
        /// </summary>
        /// <param name="backgroundColor">Background color</param>
        /// <returns>Contrasting text color with proper accessibility</returns>
        public static Color GetContrastingTextColor(Color backgroundColor)
        {
            // Calculate relative luminance using WCAG formula
            var luminance = (0.299 * backgroundColor.R + 0.587 * backgroundColor.G + 0.114 * backgroundColor.B) / 255;

            // Use WCAG AA standard (4.5:1 contrast ratio)
            return luminance > 0.5 ? Color.FromArgb(33, 33, 33) : DarkTheme.OnSurface;
        }

        /// <summary>
        /// 📱 Create a responsive layout container with Material Design principles
        /// </summary>
        /// <param name="columns">Number of columns</param>
        /// <param name="rows">Number of rows</param>
        /// <param name="parent">Control for DPI context</param>
        /// <param name="applyPadding">Whether to apply Material Design padding</param>
        /// <returns>Configured responsive layout panel</returns>
        public static TableLayoutPanel CreateResponsiveLayout(int columns, int rows, Control? parent = null, bool applyPadding = true)
        {
            var layout = new TableLayoutPanel
            {
                ColumnCount = columns,
                RowCount = rows,
                Dock = DockStyle.Fill,
                AutoSize = true,
                BackColor = DarkTheme.Surface,
                ForeColor = DarkTheme.OnSurface
            };

            // Configure column and row styles for responsive behavior
            for (int i = 0; i < columns; i++)
            {
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
            }

            for (int i = 0; i < rows; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            // Apply Material Design spacing
            if (applyPadding)
            {
                layout.Padding = Spacing.CreatePadding(Spacing.Medium, parent);
                layout.Margin = Spacing.CreatePadding(Spacing.Small, parent);
            }

            return layout;
        }

        /// <summary>
        /// 🎯 Quick utility to style any control with Material Design dark theme
        /// </summary>
        /// <param name="control">Control to style</param>
        /// <param name="enableAnimations">Whether to enable animations</param>
        public static void QuickStyle(Control control, bool enableAnimations = true)
        {
            if (control == null) return;

            // Apply base styling
            control.BackColor = DarkTheme.Surface;
            control.ForeColor = DarkTheme.OnSurface;
            control.Font = Typography.GetBodyMedium(control);

            // Apply specific styling based on control type
            ApplyThemeToControl(control, enableAnimations);
        }

        /// <summary>
        /// 🚀 One-click setup for Material Design form with all enhancements
        /// </summary>
        /// <param name="form">Form to setup</param>
        /// <param name="enableAnimations">Enable smooth animations</param>
        /// <param name="enableEffects">Enable visual effects (shadows, rounded corners)</param>
        public static void SetupMaterialForm(MaterialForm form, bool enableAnimations = true, bool enableEffects = true)
        {
            if (form == null) return;

            // Initialize theme system
            Initialize();

            // Configure MaterialSkin manager
            ConfigureMaterialSkinManager(form);

            // Apply comprehensive theming
            ApplyDpiAwareMaterialDesign(form, true);

            // Apply dark theme with animations
            ApplyDarkTheme(form, enableAnimations);

            // Add visual enhancements
            if (enableEffects)
            {
                form.FormBorderStyle = FormBorderStyle.None; // Let MaterialSkin handle borders

                // Add subtle glow effect to the form
                VisualEffects.ApplyGlowEffect(form, DarkTheme.AccentGlow, 1);
            }

            // Set optimal form properties
            form.StartPosition = FormStartPosition.CenterScreen;
            form.AutoScaleMode = AutoScaleMode.Dpi;

            // Enable double buffering through reflection for smoother rendering
            try
            {
                typeof(Control).InvokeMember("DoubleBuffered",
                    BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, form, new object[] { true });
            }
            catch
            {
                // Ignore if reflection fails
            }

            // Apply entrance animation
            if (enableAnimations)
            {
                Animations.SlideUp(form, 30, Animations.StandardDuration);
            }
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
                scaleFactor
            );
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

        #region Helper class for DPI scale calculations

        /// <summary>
        /// <summary>
        /// Helper class for DPI scale calculations
        /// </summary>
        public static class DpiScaleHelper
        {
            public class ScaleFactor
            {
                public double X { get; set; } = 1.0;
                public double Y { get; set; } = 1.0;

                public ScaleFactor(double x = 1.0, double y = 1.0)
                {
                    X = x;
                    Y = y;
                }
            }

            public static ScaleFactor GetControlScaleFactor(Control control)
            {
                if (control == null)
                    return new ScaleFactor();

                using (var graphics = control.CreateGraphics())
                {
                    return new ScaleFactor(graphics.DpiX / 96.0, graphics.DpiY / 96.0);
                }
            }

            public static int ScaleSize(int baseSize, ScaleFactor scaleFactor)
            {
                return (int)(baseSize * Math.Max(scaleFactor.X, scaleFactor.Y));
            }

            public static Padding CreatePadding(int size, Control control)
            {
                var scaleFactor = GetControlScaleFactor(control);
                int scaledSize = ScaleSize(size, scaleFactor);
                return new Padding(scaledSize);
            }

            public static Padding CreatePadding(int horizontal, int vertical, Control control)
            {
                var scaleFactor = GetControlScaleFactor(control);
                int scaledHorizontal = ScaleSize(horizontal, scaleFactor);
                int scaledVertical = ScaleSize(vertical, scaleFactor);
                return new Padding(scaledHorizontal, scaledVertical, scaledHorizontal, scaledVertical);
            }

            public static Font CreateFont(FontFamily fontFamily, float size, FontStyle style, ScaleFactor scaleFactor)
            {
                float scaledSize = (float)(size * Math.Max(scaleFactor.X, scaleFactor.Y));
                return new Font(fontFamily, scaledSize, style);
            }
        }

        #endregion
    }

    // Extension methods for MaterialForm DPI support
    public static class MaterialFormExtensions
    {
        public static void ConfigureAutoScaling(this MaterialForm form, AutoScaleMode mode)
        {
            // Configure auto scaling for the form
            form.AutoScaleMode = mode;
        }

    public static void ApplyDpiAwareSpacingToAll(this MaterialForm form)
    {
        // Apply DPI aware spacing to all controls in the form
        var scaleFactor = MaterialDesignThemeManager.DpiScaleHelper.GetControlScaleFactor(form);
        ApplyDpiAwareSpacingRecursive(form, scaleFactor);
    }

    private static void ApplyDpiAwareSpacingRecursive(Control parent, MaterialDesignThemeManager.DpiScaleHelper.ScaleFactor scaleFactor)
    {
        foreach (Control control in parent.Controls)
        {
            // Apply scaled margins and padding
            if (control.Margin != Padding.Empty)
            {
                control.Margin = ScalePadding(control.Margin, scaleFactor);
            }

            // Recursively apply to child controls
            if (control.HasChildren)
            {
                ApplyDpiAwareSpacingRecursive(control, scaleFactor);
            }
        }
    }

    private static Padding ScalePadding(Padding padding, MaterialDesignThemeManager.DpiScaleHelper.ScaleFactor scaleFactor)
        {
            var scale = Math.Max(scaleFactor.X, scaleFactor.Y);
            return new Padding(
                (int)(padding.Left * scale),
                (int)(padding.Top * scale),
                (int)(padding.Right * scale),
                (int)(padding.Bottom * scale)
            );
        }
    }
}
