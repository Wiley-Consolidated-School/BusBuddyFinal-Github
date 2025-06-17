using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Theme;
using MaterialSkin.Controls;

namespace BusBuddy.UI.Diagnostics
{
    /// <summary>
    /// Diagnostic utility for identifying and fixing BusBuddyDashboard distortion issues
    /// Provides methods to detect DPI scaling problems, layout issues, and rendering inconsistencies
    /// </summary>
    public static class FormDistortionDiagnostics
    {
        /// <summary>
        /// Comprehensive distortion analysis result
        /// </summary>
        public class DistortionAnalysisResult
        {
            public bool HasDistortionIssues { get; set; }
            public List<string> Issues { get; set; } = new List<string>();
            public List<string> Recommendations { get; set; } = new List<string>();
            public DpiScaleInfo DpiInfo { get; set; } = new DpiScaleInfo();
            public LayoutAnalysis Layout { get; set; } = new LayoutAnalysis();
        }

        public class DpiScaleInfo
        {
            public float SystemDpiX { get; set; }
            public float SystemDpiY { get; set; }
            public float ScaleFactorX { get; set; }
            public float ScaleFactorY { get; set; }
            public bool IsHighDpi => ScaleFactorX > 1.0f || ScaleFactorY > 1.0f;
            public bool HasInconsistentScaling => Math.Abs(ScaleFactorX - ScaleFactorY) > 0.1f;
        }

        public class LayoutAnalysis
        {
            public List<ControlLayoutInfo> ControlIssues { get; set; } = new List<ControlLayoutInfo>();
            public bool HasOverlappingControls { get; set; }
            public bool HasClippedControls { get; set; }
            public bool HasInvalidSizes { get; set; }
        }

        public class ControlLayoutInfo
        {
            public string ControlName { get; set; } = string.Empty;
            public string ControlType { get; set; } = string.Empty;
            public Rectangle Bounds { get; set; }
            public string Issue { get; set; } = string.Empty;
            public string Recommendation { get; set; } = string.Empty;
        }

        /// <summary>
        /// Analyze form for distortion issues
        /// </summary>
        /// <param name="form">Form to analyze</param>
        /// <returns>Comprehensive analysis result</returns>
        public static DistortionAnalysisResult AnalyzeFormDistortion(Form form)
        {
            var result = new DistortionAnalysisResult();

            try
            {
                // Analyze DPI scaling
                AnalyzeDpiScaling(form, result);

                // Analyze layout issues
                AnalyzeLayout(form, result);

                // Analyze control sizing
                AnalyzeControlSizing(form, result);

                // Analyze Material Design implementation
                AnalyzeMaterialDesignConsistency(form, result);

                // Determine overall distortion status
                result.HasDistortionIssues = result.Issues.Count > 0;
            }
            catch (Exception ex)
            {
                result.Issues.Add($"Analysis failed: {ex.Message}");
                result.HasDistortionIssues = true;
            }

            return result;
        }

        /// <summary>
        /// Apply automatic fixes for common distortion issues
        /// </summary>
        /// <param name="form">Form to fix</param>
        /// <returns>List of fixes applied</returns>
        public static List<string> ApplyAutomaticFixes(Form form)
        {
            var appliedFixes = new List<string>();

            try
            {
                // Fix 1: Ensure proper DPI awareness
                if (FixDpiAwareness(form))
                {
                    appliedFixes.Add("Applied proper DPI awareness settings");
                }

                // Fix 2: Correct control anchoring and docking
                if (FixControlAnchoring(form))
                {
                    appliedFixes.Add("Fixed control anchoring and docking issues");
                }

                // Fix 3: Standardize Material Design scaling
                if (FixMaterialDesignScaling(form))
                {
                    appliedFixes.Add("Standardized Material Design control scaling");
                }

                // Fix 4: Correct layout panel configuration
                if (FixLayoutPanelConfiguration(form))
                {
                    appliedFixes.Add("Fixed TableLayoutPanel configuration");
                }

                // Fix 5: Ensure minimum control sizes
                if (FixMinimumControlSizes(form))
                {
                    appliedFixes.Add("Applied minimum control sizes to prevent clipping");
                }

                // Refresh form layout
                form.SuspendLayout();
                form.PerformLayout();
                form.ResumeLayout(true);
                form.Refresh();

                appliedFixes.Add("Refreshed form layout");
            }
            catch (Exception ex)
            {
                appliedFixes.Add($"Fix application failed: {ex.Message}");
            }

            return appliedFixes;
        }

        private static void AnalyzeDpiScaling(Form form, DistortionAnalysisResult result)
        {
            // Get system DPI information
            using (var graphics = Graphics.FromHwnd(form.Handle))
            {
                result.DpiInfo.SystemDpiX = graphics.DpiX;
                result.DpiInfo.SystemDpiY = graphics.DpiY;
                result.DpiInfo.ScaleFactorX = graphics.DpiX / 96.0f;
                result.DpiInfo.ScaleFactorY = graphics.DpiY / 96.0f;
            }

            // Check for DPI scaling issues
            if (result.DpiInfo.HasInconsistentScaling)
            {
                result.Issues.Add($"Inconsistent DPI scaling detected: X={result.DpiInfo.ScaleFactorX:F2}, Y={result.DpiInfo.ScaleFactorY:F2}");
                result.Recommendations.Add("Ensure uniform DPI scaling across both axes");
            }

            // Check AutoScaleMode
            if (form.AutoScaleMode != AutoScaleMode.Dpi)
            {
                result.Issues.Add($"Form AutoScaleMode is {form.AutoScaleMode}, should be AutoScaleMode.Dpi");
                result.Recommendations.Add("Set form.AutoScaleMode = AutoScaleMode.Dpi");
            }

            // Check AutoScaleDimensions
            var expectedDimensions = new SizeF(96F, 96F);
            if (form.AutoScaleDimensions != expectedDimensions)
            {
                result.Issues.Add($"Form AutoScaleDimensions is {form.AutoScaleDimensions}, should be {expectedDimensions}");
                result.Recommendations.Add("Set form.AutoScaleDimensions = new SizeF(96F, 96F)");
            }
        }

        private static void AnalyzeLayout(Form form, DistortionAnalysisResult result)
        {
            var allControls = GetAllControls(form).ToList();

            // Check for overlapping controls
            for (int i = 0; i < allControls.Count; i++)
            {
                for (int j = i + 1; j < allControls.Count; j++)
                {
                    if (allControls[i].Parent == allControls[j].Parent &&
                        allControls[i].Bounds.IntersectsWith(allControls[j].Bounds) &&
                        !IsValidOverlap(allControls[i], allControls[j]))
                    {
                        result.Layout.HasOverlappingControls = true;
                        result.Issues.Add($"Overlapping controls detected: {allControls[i].Name} and {allControls[j].Name}");
                    }
                }
            }

            // Check for clipped controls
            foreach (var control in allControls)
            {
                if (control.Parent != null && IsControlClipped(control))
                {
                    result.Layout.HasClippedControls = true;
                    result.Layout.ControlIssues.Add(new ControlLayoutInfo
                    {
                        ControlName = control.Name,
                        ControlType = control.GetType().Name,
                        Bounds = control.Bounds,
                        Issue = "Control appears to be clipped by parent",
                        Recommendation = "Check parent container size and control positioning"
                    });
                }
            }
        }

        private static void AnalyzeControlSizing(Form form, DistortionAnalysisResult result)
        {
            var allControls = GetAllControls(form).ToList();

            foreach (var control in allControls)
            {
                // Check for invalid sizes
                if (control.Width <= 0 || control.Height <= 0)
                {
                    result.Layout.HasInvalidSizes = true;
                    result.Issues.Add($"Control {control.Name} has invalid size: {control.Size}");
                    result.Recommendations.Add($"Set proper size for control {control.Name}");
                }

                // Check for extremely small or large controls that might indicate scaling issues
                if (control.Width > 0 && control.Height > 0)
                {
                    var scaleFactor = DpiScaleHelper.GetControlScaleFactor(control);
                    var expectedMinSize = DpiScaleHelper.ScaleSize(10, scaleFactor);
                    var expectedMaxSize = DpiScaleHelper.ScaleSize(2000, scaleFactor);

                    if (control.Width < expectedMinSize || control.Height < expectedMinSize)
                    {
                        result.Issues.Add($"Control {control.Name} may be too small: {control.Size}");
                        result.Recommendations.Add($"Verify DPI scaling for control {control.Name}");
                    }
                    else if (control.Width > expectedMaxSize || control.Height > expectedMaxSize)
                    {
                        result.Issues.Add($"Control {control.Name} may be too large: {control.Size}");
                        result.Recommendations.Add($"Check if control {control.Name} is properly constrained");
                    }
                }
            }
        }

        private static void AnalyzeMaterialDesignConsistency(Form form, DistortionAnalysisResult result)
        {
            var materialButtons = GetControlsByType<MaterialButton>(form).ToList();

            // Check Material Design button consistency
            if (materialButtons.Count > 1)
            {
                var buttonSizes = materialButtons.Select(b => b.Size).Distinct().ToList();
                if (buttonSizes.Count > 3) // Allow for some size variation
                {
                    result.Issues.Add($"Inconsistent Material Design button sizes detected: {buttonSizes.Count} different sizes");
                    result.Recommendations.Add("Standardize Material Design button sizes using consistent DPI scaling");
                }
            }

            // Check for DataGridView Material Design compliance
            var dataGridViews = GetControlsByType<DataGridView>(form).ToList();
            foreach (var grid in dataGridViews)
            {
                if (grid.RowTemplate.Height < 40) // Material Design minimum touch target
                {
                    result.Issues.Add($"DataGridView {grid.Name} row height too small for Material Design: {grid.RowTemplate.Height}");
                    result.Recommendations.Add($"Set minimum row height of 48dp (DPI-scaled) for DataGridView {grid.Name}");
                }
            }
        }

        private static bool FixDpiAwareness(Form form)
        {
            bool changed = false;

            if (form.AutoScaleMode != AutoScaleMode.Dpi)
            {
                form.AutoScaleMode = AutoScaleMode.Dpi;
                changed = true;
            }

            var expectedDimensions = new SizeF(96F, 96F);
            if (form.AutoScaleDimensions != expectedDimensions)
            {
                form.AutoScaleDimensions = expectedDimensions;
                changed = true;
            }

            return changed;
        }

        private static bool FixControlAnchoring(Form form)
        {
            bool changed = false;
            var allControls = GetAllControls(form).ToList();

            foreach (var control in allControls)
            {
                // Fix common anchoring issues
                if (control.Parent is TableLayoutPanel && control.Anchor == AnchorStyles.None)
                {
                    control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    changed = true;
                }
            }

            return changed;
        }

        private static bool FixMaterialDesignScaling(Form form)
        {
            bool changed = false;
            var materialButtons = GetControlsByType<MaterialButton>(form).ToList();

            foreach (var button in materialButtons)
            {
                var scaleFactor = DpiScaleHelper.GetControlScaleFactor(button);
                var standardSize = new Size(
                    DpiScaleHelper.ScaleSize(170, scaleFactor),
                    DpiScaleHelper.ScaleSize(52, scaleFactor)
                );

                if (button.Size != standardSize && button.AutoSize == false)
                {
                    button.Size = standardSize;
                    changed = true;
                }
            }

            return changed;
        }

        private static bool FixLayoutPanelConfiguration(Form form)
        {
            bool changed = false;
            var layoutPanels = GetControlsByType<TableLayoutPanel>(form).ToList();

            foreach (var panel in layoutPanels)
            {
                // Ensure proper row and column styles
                if (panel.RowStyles.Count != panel.RowCount)
                {
                    panel.RowStyles.Clear();
                    for (int i = 0; i < panel.RowCount; i++)
                    {
                        if (i == panel.RowCount - 1)
                            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                        else
                            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    }
                    changed = true;
                }
            }

            return changed;
        }

        private static bool FixMinimumControlSizes(Form form)
        {
            bool changed = false;
            var allControls = GetAllControls(form).ToList();

            foreach (var control in allControls)
            {
                if (control.Width > 0 && control.Height > 0)
                {
                    var scaleFactor = DpiScaleHelper.GetControlScaleFactor(control);
                    var minSize = DpiScaleHelper.ScaleSize(20, scaleFactor);

                    if (control.Width < minSize && control.AutoSize == false)
                    {
                        control.Width = minSize;
                        changed = true;
                    }

                    if (control.Height < minSize && control.AutoSize == false)
                    {
                        control.Height = minSize;
                        changed = true;
                    }
                }
            }

            return changed;
        }

        private static IEnumerable<Control> GetAllControls(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                yield return control;
                foreach (var child in GetAllControls(control))
                    yield return child;
            }
        }

        private static IEnumerable<T> GetControlsByType<T>(Control parent) where T : Control
        {
            foreach (Control control in parent.Controls)
            {
                if (control is T targetControl)
                    yield return targetControl;

                foreach (var found in GetControlsByType<T>(control))
                    yield return found;
            }
        }

        private static bool IsValidOverlap(Control control1, Control control2)
        {
            // Allow overlaps for certain control types (like labels over panels)
            return (control1 is Label || control2 is Label) ||
                   (control1.Parent != control2.Parent);
        }

        private static bool IsControlClipped(Control control)
        {
            if (control.Parent == null) return false;

            var parentBounds = new Rectangle(0, 0, control.Parent.Width, control.Parent.Height);
            return !parentBounds.Contains(control.Bounds);
        }
    }
}
