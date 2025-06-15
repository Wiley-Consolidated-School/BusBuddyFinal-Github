using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Svg;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Enhanced helper class for DPI-aware vector graphics and SVG icon rendering
    /// Provides comprehensive SVG support with dynamic DPI scaling and caching
    /// </summary>
    public static class VectorGraphicsHelper
    {
        /// <summary>
        /// DPI-aware image collection for dynamic scaling
        /// </summary>
        public class DpiAwareImageCollection
        {
            private readonly Dictionary<string, SvgDocument> _svgCache = new();
            private readonly Dictionary<string, Dictionary<int, Image>> _bitmapCache = new();

            /// <summary>
            /// Add an SVG to the collection
            /// </summary>
            /// <param name="key">Unique identifier for the SVG</param>
            /// <param name="svgContent">SVG content string</param>
            public void AddSvg(string key, string svgContent)
            {
                try
                {
                    var svgDoc = SvgDocument.FromSvg<SvgDocument>(svgContent);
                    _svgCache[key] = svgDoc;

                    // Initialize bitmap cache for this SVG
                    if (!_bitmapCache.ContainsKey(key))
                    {
                        _bitmapCache[key] = new Dictionary<int, Image>();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to parse SVG '{key}': {ex.Message}");
                }
            }

            /// <summary>
            /// Load SVG from file
            /// </summary>
            /// <param name="key">Unique identifier for the SVG</param>
            /// <param name="filePath">Path to SVG file</param>
            public void LoadSvgFromFile(string key, string filePath)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        string svgContent = File.ReadAllText(filePath);
                        AddSvg(key, svgContent);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load SVG file '{filePath}': {ex.Message}");
                }
            }

            /// <summary>
            /// Get DPI-aware bitmap from SVG
            /// </summary>
            /// <param name="key">SVG identifier</param>
            /// <param name="size">Desired size</param>
            /// <param name="scaleFactor">DPI scale factor</param>
            /// <param name="color">Optional color override</param>
            /// <returns>Scaled bitmap</returns>
            public Image? GetScaledImage(string key, int size, DpiScaleHelper.ScaleFactor scaleFactor, Color? color = null)
            {
                if (!_svgCache.ContainsKey(key))
                    return null;

                int scaledSize = DpiScaleHelper.ScaleSize(size, scaleFactor);

                // Check cache first
                if (_bitmapCache[key].ContainsKey(scaledSize))
                {
                    return _bitmapCache[key][scaledSize];
                }

                try
                {
                    var svgDoc = _svgCache[key];

                    // Apply color if specified
                    if (color.HasValue)
                    {
                        ApplyColorToSvg(svgDoc, color.Value);
                    }

                    // Render at scaled size
                    var bitmap = svgDoc.Draw(scaledSize, scaledSize);

                    // Cache the result
                    _bitmapCache[key][scaledSize] = bitmap;

                    return bitmap;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to render SVG '{key}': {ex.Message}");
                    return CreateFallbackIcon(scaledSize, color ?? Color.Gray);
                }
            }

            /// <summary>
            /// Get image for a control with automatic DPI scaling
            /// </summary>
            /// <param name="key">SVG identifier</param>
            /// <param name="size">Base size at 96 DPI</param>
            /// <param name="control">Control for DPI context</param>
            /// <param name="color">Optional color override</param>
            /// <returns>DPI-scaled image</returns>
            public Image? GetImageForControl(string key, int size, Control control, Color? color = null)
            {
                var scaleFactor = DpiScaleHelper.GetControlScaleFactor(control);
                return GetScaledImage(key, size, scaleFactor, color);
            }

            /// <summary>
            /// Clear cached bitmaps (useful for theme changes)
            /// </summary>
            public void ClearBitmapCache()
            {
                foreach (var cache in _bitmapCache.Values)
                {
                    foreach (var image in cache.Values)
                    {
                        image?.Dispose();
                    }
                    cache.Clear();
                }
            }

            /// <summary>
            /// Clear all caches and dispose resources
            /// </summary>
            public void Dispose()
            {
                ClearBitmapCache();
                _svgCache.Clear();
                _bitmapCache.Clear();
            }
        }

        // Static instance for application-wide use
        private static readonly Lazy<DpiAwareImageCollection> _globalCollection =
            new Lazy<DpiAwareImageCollection>(() => new DpiAwareImageCollection());

        /// <summary>
        /// Global DPI-aware image collection
        /// </summary>
        public static DpiAwareImageCollection GlobalImageCollection => _globalCollection.Value;

        /// <summary>
        /// Apply color tinting to SVG elements
        /// </summary>
        private static void ApplyColorToSvg(SvgDocument svgDoc, Color color)
        {
            try
            {
                // Recursively apply color to all path elements
                ApplyColorToElement(svgDoc, color);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply color to SVG: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively apply color to SVG elements
        /// </summary>
        private static void ApplyColorToElement(SvgElement element, Color color)
        {
            try
            {
                // Apply color to paths and shapes
                if (element is SvgPath || element is SvgRectangle || element is SvgCircle || element is SvgPolygon)
                {
                    element.Fill = new SvgColourServer(color);
                    element.Stroke = new SvgColourServer(color);
                }

                // Recursively process children
                foreach (var child in element.Children)
                {
                    if (child is SvgElement svgChild)
                    {
                        ApplyColorToElement(svgChild, color);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply color to SVG element: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a high-DPI aware icon from SVG content with automatic scaling
        /// </summary>
        /// <param name="svgContent">SVG content string</param>
        /// <param name="baseSize">Base size at 96 DPI</param>
        /// <param name="control">Control for DPI context</param>
        /// <param name="color">Optional color override</param>
        /// <returns>DPI-scaled image</returns>
        public static Image CreateDpiAwareIconFromSvg(string svgContent, int baseSize, Control? control = null, Color? color = null)
        {
            try
            {
                var svgDoc = SvgDocument.FromSvg<SvgDocument>(svgContent);
                var scaleFactor = control != null ? DpiScaleHelper.GetControlScaleFactor(control) : DpiScaleHelper.GetSystemScaleFactor();

                int scaledSize = DpiScaleHelper.ScaleSize(baseSize, scaleFactor);

                // Apply color if specified
                if (color.HasValue)
                {
                    ApplyColorToSvg(svgDoc, color.Value);
                }

                // Render to bitmap with DPI-aware size
                var bitmap = svgDoc.Draw(scaledSize, scaledSize);
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create DPI-aware icon: {ex.Message}");
                // Fallback to geometric icon
                var scaleFactor = control != null ? DpiScaleHelper.GetControlScaleFactor(control) : DpiScaleHelper.GetSystemScaleFactor();
                int scaledSize = DpiScaleHelper.ScaleSize(baseSize, scaleFactor);
                return CreateFallbackIcon(scaledSize, color ?? Color.Gray);
            }
        }

        /// <summary>
        /// Load SVG from file with DPI awareness
        /// </summary>
        /// <param name="filePath">Path to SVG file</param>
        /// <param name="baseSize">Base size at 96 DPI</param>
        /// <param name="control">Control for DPI context</param>
        /// <param name="color">Optional color override</param>
        /// <returns>DPI-scaled image</returns>
        public static Image? LoadSvgFromFile(string filePath, int baseSize, Control? control = null, Color? color = null)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                string svgContent = File.ReadAllText(filePath);
                return CreateDpiAwareIconFromSvg(svgContent, baseSize, control, color);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load SVG file '{filePath}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create a high-DPI aware icon from SVG content
        /// </summary>
        public static Image CreateIconFromSvg(string svgContent, int size, Color? color = null)
        {
            try
            {
                // Create SVG document from string
                var svgDoc = SvgDocument.FromSvg<SvgDocument>(svgContent);

                // Calculate DPI-aware size
                using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
                {
                    float dpiScale = graphics.DpiX / 96.0f;
                    int scaledSize = (int)(size * dpiScale);

                    // Set document DPI
                    svgDoc.Ppi = (int)graphics.DpiX;

                    // Apply color if specified
                    if (color.HasValue)
                    {
                        ApplyColorToSvg(svgDoc, color.Value);
                    }

                    // Render to bitmap with proper size
                    var bitmap = svgDoc.Draw(scaledSize, scaledSize);
                    return bitmap;
                }
            }
            catch (Exception)
            {
                // Fallback to a simple geometric icon
                return CreateFallbackIcon(size, color ?? Color.Gray);
            }
        }

        /// <summary>
        /// Create a fallback geometric icon when SVG fails
        /// </summary>
        private static Image CreateFallbackIcon(int size, Color color)
        {
            var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // Create a simple geometric shape as fallback
                var rect = new Rectangle(size / 4, size / 4, size / 2, size / 2);
                using (var brush = new SolidBrush(color))
                {
                    graphics.FillEllipse(brush, rect);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Load SVG from embedded resource
        /// </summary>
        public static Image LoadSvgFromResource(string resourceName, int size, Color? color = null)
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            string svgContent = reader.ReadToEnd();
                            return CreateIconFromSvg(svgContent, size, color);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load SVG resource '{resourceName}': {ex.Message}");
            }

            // Fallback
            return CreateFallbackIcon(size, color ?? Color.Gray);
        }

        /// <summary>
        /// Create a simple arrow icon programmatically
        /// </summary>
        public static Image CreateArrowIcon(int size, ArrowDirection direction, Color color)
        {
            var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.CompositingQuality = CompositingQuality.HighQuality;

                var points = CreateArrowPoints(size, direction);
                using (var brush = new SolidBrush(color))
                {
                    graphics.FillPolygon(brush, points);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Arrow direction enumeration
        /// </summary>
        public enum ArrowDirection
        {
            Up, Down, Left, Right
        }

        /// <summary>
        /// Create arrow points based on direction
        /// </summary>
        private static PointF[] CreateArrowPoints(int size, ArrowDirection direction)
        {
            var margin = size * 0.2f;
            var center = size / 2f;

            return direction switch
            {
                ArrowDirection.Up => new PointF[]
                {
                    new PointF(center, margin),
                    new PointF(size - margin, size - margin),
                    new PointF(margin, size - margin)
                },
                ArrowDirection.Down => new PointF[]
                {
                    new PointF(margin, margin),
                    new PointF(size - margin, margin),
                    new PointF(center, size - margin)
                },
                ArrowDirection.Left => new PointF[]
                {
                    new PointF(margin, center),
                    new PointF(size - margin, margin),
                    new PointF(size - margin, size - margin)
                },
                ArrowDirection.Right => new PointF[]
                {
                    new PointF(margin, margin),
                    new PointF(size - margin, center),
                    new PointF(margin, size - margin)
                },
                _ => new PointF[0]
            };
        }

        /// <summary>
        /// Apply high-quality rendering settings to graphics object
        /// </summary>
        public static void ApplyHighQualitySettings(Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.CompositingMode = CompositingMode.SourceOver;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        }

        /// <summary>
        /// Scale image with high quality
        /// </summary>
        public static Image ScaleImage(Image source, int width, int height)
        {
            var scaled = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(scaled))
            {
                ApplyHighQualitySettings(graphics);
                graphics.DrawImage(source, 0, 0, width, height);
            }
            return scaled;
        }

        /// <summary>
        /// Common SVG icons for application use
        /// </summary>
        public static class CommonIcons
        {
            public static readonly string Save = @"
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'>
                    <path d='M17 3H5c-1.11 0-2 .9-2 2v14c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V7l-4-4zm-5 16c-1.66 0-3-1.34-3-3s1.34-3 3-3 3 1.34 3 3-1.34 3-3 3zm3-10H5V5h10v4z'/>
                </svg>";

            public static readonly string Edit = @"
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'>
                    <path d='M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z'/>
                </svg>";

            public static readonly string Plus = @"
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'>
                    <path d='M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z'/>
                </svg>";

            public static readonly string Delete = @"
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'>
                    <path d='M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z'/>
                </svg>";

            public static readonly string Search = @"
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'>
                    <path d='M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z'/>
                </svg>";

            public static readonly string Settings = @"
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'>
                    <path d='M19.14,12.94c0.04-0.3,0.06-0.61,0.06-0.94c0-0.32-0.02-0.64-0.07-0.94l2.03-1.58c0.18-0.14,0.23-0.41,0.12-0.61 l-1.92-3.32c-0.12-0.22-0.37-0.29-0.59-0.22l-2.39,0.96c-0.5-0.38-1.03-0.7-1.62-0.94L14.4,2.81c-0.04-0.24-0.24-0.41-0.48-0.41 h-3.84c-0.24,0-0.43,0.17-0.47,0.41L9.25,5.35C8.66,5.59,8.12,5.92,7.63,6.29L5.24,5.33c-0.22-0.08-0.47,0-0.59,0.22L2.74,8.87 C2.62,9.08,2.66,9.34,2.86,9.48l2.03,1.58C4.84,11.36,4.8,11.69,4.8,12s0.02,0.64,0.07,0.94l-2.03,1.58 c-0.18,0.14-0.23,0.41-0.12,0.61l1.92,3.32c0.12,0.22,0.37,0.29,0.59,0.22l2.39-0.96c0.5,0.38,1.03,0.7,1.62,0.94l0.36,2.54 c0.05,0.24,0.24,0.41,0.48,0.41h3.84c0.24,0,0.44-0.17,0.47-0.41l0.36-2.54c0.59-0.24,1.13-0.56,1.62-0.94l2.39,0.96 c0.22,0.08,0.47,0,0.59-0.22l1.92-3.32c0.12-0.22,0.07-0.47-0.12-0.61L19.14,12.94z M12,15.6c-1.98,0-3.6-1.62-3.6-3.6 s1.62-3.6,3.6-3.6s3.6,1.62,3.6,3.6S13.98,15.6,12,15.6z'/>
                </svg>";

            public static readonly string Home = @"
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'>
                    <path d='M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z'/>
                </svg>";

            public static readonly string Menu = @"
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'>
                    <path d='M3 18h18v-2H3v2zm0-5h18v-2H3v2zm0-7v2h18V6H3z'/>
                </svg>";

            public static readonly string Close = @"
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'>
                    <path d='M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z'/>
                </svg>";

            public static readonly string Check = @"
                <svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'>
                    <path d='M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z'/>
                </svg>";
        }
    }
}
