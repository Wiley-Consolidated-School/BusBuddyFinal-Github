using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Svg;
using BusBuddy.UI.Theme;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Advanced SVG graphics manager optimized for high DPI displays and Material Design dark theme
    /// Provides crisp, scalable vector graphics with proper caching and theme integration
    /// </summary>
    public static class AdvancedSvgGraphicsManager
    {
        #region Cache Management

        private static readonly Dictionary<string, SvgDocument> _svgCache = new();
        private static readonly Dictionary<string, Image> _imageCache = new();

        /// <summary>
        /// Clear all cached SVG documents and rendered images
        /// </summary>
        public static void ClearCache()
        {
            // Dispose cached images
            foreach (var image in _imageCache.Values)
            {
                image?.Dispose();
            }

            _svgCache.Clear();
            _imageCache.Clear();
        }

        #endregion

        #region Material Design Icon Library

        /// <summary>
        /// Material Design icon SVG definitions optimized for dark theme
        /// All icons use outline style for better visibility on dark backgrounds
        /// </summary>
        public static class MaterialIcons
        {
            public const string Dashboard = @"
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <path d='M3 13h8V3H3v10zm0 8h8v-6H3v6zm10 0h8V11h-8v10zm0-18v6h8V3h-8z'
                          stroke='currentColor' stroke-width='2' fill='none'/>
                </svg>";

            public const string Vehicle = @"
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <path d='M18.92 6.01C18.72 5.42 18.16 5 17.5 5h-11c-.66 0-1.22.42-1.42 1.01L3 12v8c0 .55.45 1 1 1h1c.55 0 1-.45 1-1v-1h12v1c0 .55.45 1 1 1h1c.55 0 1-.45 1-1v-8l-2.08-5.99zM6.5 16c-.83 0-1.5-.67-1.5-1.5S5.67 13 6.5 13s1.5.67 1.5 1.5S7.33 16 6.5 16zm11 0c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5 1.5zM5 11l1.5-4.5h11L19 11H5z'
                          stroke='currentColor' stroke-width='1.5' fill='none'/>
                </svg>";

            public const string Driver = @"
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <path d='M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z'
                          stroke='currentColor' stroke-width='1.5' fill='none'/>
                </svg>";

            public const string Route = @"
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <path d='M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z'
                          stroke='currentColor' stroke-width='1.5' fill='none'/>
                    <path d='M9 11h6m-3-3v6' stroke='currentColor' stroke-width='1.5'/>
                </svg>";

            public const string Schedule = @"
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <rect x='3' y='4' width='18' height='18' rx='2' ry='2' stroke='currentColor' stroke-width='1.5' fill='none'/>
                    <line x1='16' y1='2' x2='16' y2='6' stroke='currentColor' stroke-width='1.5'/>
                    <line x1='8' y1='2' x2='8' y2='6' stroke='currentColor' stroke-width='1.5'/>
                    <line x1='3' y1='10' x2='21' y2='10' stroke='currentColor' stroke-width='1.5'/>
                </svg>";

            public const string Maintenance = @"
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <path d='M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z'
                          stroke='currentColor' stroke-width='1.5' fill='none'/>
                </svg>";

            public const string Fuel = @"
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <path d='M3 7v10a2 2 0 0 0 2 2h9a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2H5a2 2 0 0 0-2 2z'
                          stroke='currentColor' stroke-width='1.5' fill='none'/>
                    <path d='M16 6l4 4v5h-2V9l-2-2' stroke='currentColor' stroke-width='1.5' fill='none'/>
                </svg>";

            public const string Activity = @"
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <polyline points='22,12 18,12 15,21 9,3 6,12 2,12' stroke='currentColor' stroke-width='1.5' fill='none'/>
                </svg>";

            public const string Settings = @"
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <circle cx='12' cy='12' r='3' stroke='currentColor' stroke-width='1.5' fill='none'/>
                    <path d='M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z'
                          stroke='currentColor' stroke-width='1.5' fill='none'/>
                </svg>";

            public const string Reports = @"
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <path d='M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z'
                          stroke='currentColor' stroke-width='1.5' fill='none'/>
                    <polyline points='14,2 14,8 20,8' stroke='currentColor' stroke-width='1.5' fill='none'/>
                    <line x1='16' y1='13' x2='8' y2='13' stroke='currentColor' stroke-width='1.5'/>
                    <line x1='16' y1='17' x2='8' y2='17' stroke='currentColor' stroke-width='1.5'/>
                    <polyline points='10,9 9,9 8,9' stroke='currentColor' stroke-width='1.5' fill='none'/>
                </svg>";

            public const string Calendar = @"
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <rect x='3' y='4' width='18' height='18' rx='2' ry='2' stroke='currentColor' stroke-width='1.5' fill='none'/>
                    <line x1='16' y1='2' x2='16' y2='6' stroke='currentColor' stroke-width='1.5'/>
                    <line x1='8' y1='2' x2='8' y2='6' stroke='currentColor' stroke-width='1.5'/>
                    <line x1='3' y1='10' x2='21' y2='10' stroke='currentColor' stroke-width='1.5'/>
                    <circle cx='12' cy='16' r='2' stroke='currentColor' stroke-width='1.5' fill='none'/>
                </svg>";

            public const string TimeCard = @"
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <circle cx='12' cy='12' r='10' stroke='currentColor' stroke-width='1.5' fill='none'/>
                    <polyline points='12,6 12,12 16,14' stroke='currentColor' stroke-width='1.5' fill='none'/>
                </svg>";

            public const string Demo = @"
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <polygon points='5,3 19,12 5,21' stroke='currentColor' stroke-width='1.5' fill='none'/>
                </svg>";
        }

        #endregion

        #region SVG Rendering Methods

        /// <summary>
        /// Create a high-DPI aware icon from SVG content with Material Design dark theme support
        /// </summary>
        /// <param name="svgContent">SVG content string</param>
        /// <param name="size">Base size (will be scaled for DPI)</param>
        /// <param name="color">Icon color (uses theme color if null)</param>
        /// <param name="control">Control for DPI context</param>
        /// <returns>Rendered bitmap optimized for high DPI</returns>
        public static Bitmap CreateMaterialIcon(string svgContent, int size, Color? color = null, Control? control = null)
        {
            try
            {
                // Get DPI-aware size
                int dpiAwareSize = MaterialDesignThemeManager.GetDpiAwareSize(size, control);

                // Use theme color if not specified
                Color iconColor = color ?? MaterialDesignThemeManager.DarkTheme.OnSurface;

                // Create cache key
                string cacheKey = $"{svgContent.GetHashCode()}_{dpiAwareSize}_{iconColor.ToArgb()}";

                // Check cache first
                if (_imageCache.TryGetValue(cacheKey, out Image? cachedImage))
                {
                    return new Bitmap(cachedImage);
                }

                // Apply color to SVG content
                string coloredSvgContent = ApplyColorToSvgContent(svgContent, iconColor);

                // Parse SVG document
                var svgDoc = SvgDocument.FromSvg<SvgDocument>(coloredSvgContent);

                // Configure for high-quality rendering
                ConfigureSvgForHighQuality(svgDoc, control);

                // Render to high-quality bitmap
                var bitmap = RenderSvgToHighQualityBitmap(svgDoc, dpiAwareSize, dpiAwareSize);

                // Cache the result
                _imageCache[cacheKey] = new Bitmap(bitmap);

                return bitmap;
            }
            catch (Exception)
            {
                // Return fallback icon
                return CreateFallbackMaterialIcon(size, color ?? Color.Gray, control);
            }
        }

        /// <summary>
        /// Create an icon button with Material Design styling and SVG icon
        /// </summary>
        /// <param name="svgContent">SVG icon content</param>
        /// <param name="size">Button size</param>
        /// <param name="iconSize">Icon size within button</param>
        /// <param name="color">Icon color</param>
        /// <param name="control">Control for DPI context</param>
        /// <returns>Styled bitmap for button</returns>
        public static Bitmap CreateMaterialIconButton(string svgContent, int size, int iconSize, Color? color = null, Control? control = null)
        {
            int dpiAwareSize = MaterialDesignThemeManager.GetDpiAwareSize(size, control);
            int dpiAwareIconSize = MaterialDesignThemeManager.GetDpiAwareSize(iconSize, control);

            var bitmap = new Bitmap(dpiAwareSize, dpiAwareSize, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                // Configure high-quality rendering
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;

                // Create rounded background (Material Design style)
                using (var backgroundBrush = new SolidBrush(MaterialDesignThemeManager.DarkTheme.SurfaceContainer))
                {
                    var backgroundRect = new Rectangle(0, 0, dpiAwareSize, dpiAwareSize);
                    graphics.FillEllipse(backgroundBrush, backgroundRect);
                }

                // Add subtle border
                using (var borderPen = new Pen(MaterialDesignThemeManager.DarkTheme.Outline, 1))
                {
                    var borderRect = new Rectangle(0, 0, dpiAwareSize - 1, dpiAwareSize - 1);
                    graphics.DrawEllipse(borderPen, borderRect);
                }

                // Create and draw icon
                using (var icon = CreateMaterialIcon(svgContent, iconSize, color, control))
                {
                    int iconX = (dpiAwareSize - dpiAwareIconSize) / 2;
                    int iconY = (dpiAwareSize - dpiAwareIconSize) / 2;
                    graphics.DrawImage(icon, iconX, iconY, dpiAwareIconSize, dpiAwareIconSize);
                }
            }

            return bitmap;
        }

        /// <summary>
        /// Create a Material Design card with icon and text
        /// </summary>
        /// <param name="svgContent">SVG icon content</param>
        /// <param name="title">Card title</param>
        /// <param name="subtitle">Card subtitle</param>
        /// <param name="width">Card width</param>
        /// <param name="height">Card height</param>
        /// <param name="control">Control for DPI context</param>
        /// <returns>Rendered card bitmap</returns>
        public static Bitmap CreateMaterialCard(string svgContent, string title, string subtitle, int width, int height, Control? control = null)
        {
            int dpiAwareWidth = MaterialDesignThemeManager.GetDpiAwareSize(width, control);
            int dpiAwareHeight = MaterialDesignThemeManager.GetDpiAwareSize(height, control);

            var bitmap = new Bitmap(dpiAwareWidth, dpiAwareHeight, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                // Configure high-quality rendering
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // Draw card background with elevation
                using (var backgroundBrush = new SolidBrush(MaterialDesignThemeManager.DarkTheme.SurfaceContainer))
                {
                    var cardRect = new RectangleF(0, 0, dpiAwareWidth, dpiAwareHeight);
                    graphics.FillRoundedRectangle(backgroundBrush, cardRect, 12);
                }

                // Draw subtle border
                using (var borderPen = new Pen(MaterialDesignThemeManager.DarkTheme.Outline, 1))
                {
                    var borderRect = new RectangleF(0.5f, 0.5f, dpiAwareWidth - 1, dpiAwareHeight - 1);
                    graphics.DrawRoundedRectangle(borderPen, borderRect, 12);
                }

                // Draw icon
                int iconSize = MaterialDesignThemeManager.GetDpiAwareSize(24, control);
                int padding = MaterialDesignThemeManager.GetDpiAwareSize(16, control);

                using (var icon = CreateMaterialIcon(svgContent, 24, MaterialDesignThemeManager.DarkTheme.Primary, control))
                {
                    graphics.DrawImage(icon, padding, padding, iconSize, iconSize);
                }

                // Draw title
                using (var titleFont = MaterialDesignThemeManager.Typography.GetTitleMedium(control))
                using (var titleBrush = new SolidBrush(MaterialDesignThemeManager.DarkTheme.OnSurface))
                {
                    var titleRect = new RectangleF(padding * 2 + iconSize, padding,
                        dpiAwareWidth - (padding * 3 + iconSize), titleFont.Height);
                    graphics.DrawString(title, titleFont, titleBrush, titleRect);
                }

                // Draw subtitle
                if (!string.IsNullOrEmpty(subtitle))
                {
                    using (var subtitleFont = MaterialDesignThemeManager.Typography.GetBodyMedium(control))
                    using (var subtitleBrush = new SolidBrush(MaterialDesignThemeManager.DarkTheme.OnSurfaceVariant))
                    {
                        var subtitleRect = new RectangleF(padding * 2 + iconSize, padding * 2 + iconSize,
                            dpiAwareWidth - (padding * 3 + iconSize), subtitleFont.Height);
                        graphics.DrawString(subtitle, subtitleFont, subtitleBrush, subtitleRect);
                    }
                }
            }

            return bitmap;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Apply color to SVG content by replacing 'currentColor' with specified color
        /// </summary>
        private static string ApplyColorToSvgContent(string svgContent, Color color)
        {
            string hexColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            return svgContent.Replace("currentColor", hexColor);
        }

        /// <summary>
        /// Configure SVG document for high-quality rendering
        /// </summary>
        private static void ConfigureSvgForHighQuality(SvgDocument svgDoc, Control? control)
        {
            // Set DPI for proper scaling
            if (control != null)
            {
                svgDoc.Ppi = (int)control.DeviceDpi;
            }
            else
            {
                using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
                {
                    svgDoc.Ppi = (int)graphics.DpiX;
                }
            }

            // Configure for crisp rendering
            svgDoc.ShapeRendering = SvgShapeRendering.CrispEdges;
        }

        /// <summary>
        /// Render SVG to high-quality bitmap
        /// </summary>
        private static Bitmap RenderSvgToHighQualityBitmap(SvgDocument svgDoc, int width, int height)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                // Configure for highest quality rendering
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // Clear with transparent background
                graphics.Clear(Color.Transparent);

                // Render SVG
                svgDoc.Draw(graphics, new SizeF(width, height));
            }

            return bitmap;
        }

        /// <summary>
        /// Create a fallback Material Design icon when SVG rendering fails
        /// </summary>
        private static Bitmap CreateFallbackMaterialIcon(int size, Color color, Control? control)
        {
            int dpiAwareSize = MaterialDesignThemeManager.GetDpiAwareSize(size, control);
            var bitmap = new Bitmap(dpiAwareSize, dpiAwareSize, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Draw simple geometric icon
                using (var brush = new SolidBrush(color))
                {
                    int centerX = dpiAwareSize / 2;
                    int centerY = dpiAwareSize / 2;
                    int radius = dpiAwareSize / 4;

                    graphics.FillEllipse(brush, centerX - radius, centerY - radius, radius * 2, radius * 2);
                }
            }

            return bitmap;
        }

        #endregion

        #region Extension Methods

        /// <summary>
        /// Extension method to draw rounded rectangles
        /// </summary>
        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, RectangleF rect, float radius)
        {
            using (var path = CreateRoundedRectanglePath(rect, radius))
            {
                graphics.FillPath(brush, path);
            }
        }

        /// <summary>
        /// Extension method to draw rounded rectangle borders
        /// </summary>
        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, RectangleF rect, float radius)
        {
            using (var path = CreateRoundedRectanglePath(rect, radius))
            {
                graphics.DrawPath(pen, path);
            }
        }

        /// <summary>
        /// Create a rounded rectangle graphics path
        /// </summary>
        private static GraphicsPath CreateRoundedRectanglePath(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            // Ensure radius doesn't exceed rectangle dimensions
            radius = Math.Min(radius, Math.Min(rect.Width / 2, rect.Height / 2));

            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();

            return path;
        }

        #endregion
    }
}
