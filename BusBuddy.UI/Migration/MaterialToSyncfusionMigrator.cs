using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using MaterialSkin.Controls;
using Syncfusion.Windows.Forms.Tools;

namespace BusBuddy.UI.Migration
{
    /// <summary>
    /// Utility class to help migrate forms from MaterialSkin2 to Syncfusion
    /// Provides automated control replacement and theme migration
    /// </summary>
    public static class MaterialToSyncfusionMigrator
    {        /// <summary>
        /// Control mapping from MaterialSkin to Syncfusion equivalents
        /// </summary>
        public static readonly Dictionary<Type, Type> ControlMappings = new Dictionary<Type, Type>
        {
            { typeof(MaterialLabel), typeof(Label) },
            { typeof(MaterialTextBox), typeof(TextBox) },
            { typeof(MaterialButton), typeof(Button) },
            { typeof(MaterialForm), typeof(SyncfusionBaseForm) }
        };

        /// <summary>
        /// Migration report for tracking changes
        /// </summary>
        public class MigrationReport
        {
            public List<string> ConvertedControls { get; set; } = new List<string>();
            public List<string> Warnings { get; set; } = new List<string>();
            public List<string> Errors { get; set; } = new List<string>();
            public int TotalControls { get; set; }
            public int ConvertedCount { get; set; }

            public double SuccessRate => TotalControls > 0 ? (double)ConvertedCount / TotalControls * 100 : 0;

            public override string ToString()
            {
                return $"Migration Report: {ConvertedCount}/{TotalControls} controls converted ({SuccessRate:F1}%)";
            }
        }

        /// <summary>
        /// Analyze a form for MaterialSkin dependencies
        /// </summary>
        public static MigrationReport AnalyzeForm(Form form)
        {
            var report = new MigrationReport();

            try
            {
                var allControls = GetAllControls(form).ToList();
                report.TotalControls = allControls.Count;

                foreach (var control in allControls)
                {
                    var controlType = control.GetType();

                    if (IsMaterialSkinControl(controlType))
                    {
                        if (ControlMappings.ContainsKey(controlType))
                        {
                            report.ConvertedControls.Add($"{controlType.Name} → {ControlMappings[controlType].Name}");
                        }
                        else
                        {
                            report.Warnings.Add($"No direct mapping for {controlType.Name}");
                        }
                    }
                }

                report.ConvertedCount = report.ConvertedControls.Count;

                Console.WriteLine($"📊 MIGRATION ANALYSIS: {report}");
            }
            catch (Exception ex)
            {
                report.Errors.Add($"Analysis failed: {ex.Message}");
                Console.WriteLine($"❌ MIGRATION ANALYSIS ERROR: {ex.Message}");
            }

            return report;
        }

        /// <summary>
        /// Apply Syncfusion theming to an existing form without changing control types
        /// Useful for gradual migration
        /// </summary>
        public static MigrationReport ApplySyncfusionThemeToExistingForm(Form form)
        {
            var report = new MigrationReport();

            try
            {
                // Apply form-level theming
                SyncfusionThemeHelper.ApplyMaterialForm(form);
                report.ConvertedControls.Add($"Form: {form.GetType().Name} themed");

                // Apply theme to all controls recursively
                var allControls = GetAllControls(form).ToList();
                report.TotalControls = allControls.Count;

                foreach (var control in allControls)
                {
                    try
                    {
                        ApplySyncfusionThemeToControl(control);
                        report.ConvertedCount++;
                        report.ConvertedControls.Add($"Themed: {control.GetType().Name}");
                    }
                    catch (Exception ex)
                    {
                        report.Warnings.Add($"Failed to theme {control.GetType().Name}: {ex.Message}");
                    }
                }

                Console.WriteLine($"🎨 THEMING COMPLETE: {report}");
            }
            catch (Exception ex)
            {
                report.Errors.Add($"Theming failed: {ex.Message}");
                Console.WriteLine($"❌ THEMING ERROR: {ex.Message}");
            }

            return report;
        }

        /// <summary>
        /// Create Syncfusion replacement for MaterialSkin control
        /// </summary>
        public static Control CreateSyncfusionReplacement(Control materialControl)
        {
            try
            {
                var materialType = materialControl.GetType();

                if (!ControlMappings.ContainsKey(materialType))
                {
                    Console.WriteLine($"⚠️ MIGRATION: No mapping for {materialType.Name}, applying basic theme");
                    SyncfusionThemeHelper.ApplyBasicMaterialTheme(materialControl);
                    return materialControl;
                }

                var syncfusionType = ControlMappings[materialType];
                var syncfusionControl = CreateSyncfusionControl(syncfusionType, materialControl);

                // Copy common properties
                CopyControlProperties(materialControl, syncfusionControl);

                Console.WriteLine($"✅ MIGRATION: Created {syncfusionType.Name} to replace {materialType.Name}");
                return syncfusionControl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ MIGRATION ERROR: Failed to create replacement for {materialControl.GetType().Name}: {ex.Message}");
                return materialControl;
            }
        }

        /// <summary>
        /// Generate migration instructions for a form
        /// </summary>
        public static List<string> GenerateMigrationInstructions(Form form)
        {
            var instructions = new List<string>();
            var report = AnalyzeForm(form);

            instructions.Add($"# Migration Instructions for {form.GetType().Name}");
            instructions.Add("");
            instructions.Add("## Summary");
            instructions.Add($"- Total controls to migrate: {report.TotalControls}");
            instructions.Add($"- Direct mappings available: {report.ConvertedCount}");
            instructions.Add($"- Success rate: {report.SuccessRate:F1}%");
            instructions.Add("");

            if (report.ConvertedControls.Any())
            {
                instructions.Add("## Control Mappings");
                foreach (var mapping in report.ConvertedControls)
                {
                    instructions.Add($"- {mapping}");
                }
                instructions.Add("");
            }

            if (report.Warnings.Any())
            {
                instructions.Add("## Warnings");
                foreach (var warning in report.Warnings)
                {
                    instructions.Add($"- ⚠️ {warning}");
                }
                instructions.Add("");
            }

            instructions.Add("## Migration Steps");
            instructions.Add("1. Change form inheritance from MaterialForm to SyncfusionBaseForm");
            instructions.Add("2. Replace MaterialSkin using statements with Syncfusion equivalents");
            instructions.Add("3. Update control creation methods to use Syncfusion helpers");
            instructions.Add("4. Remove MaterialSkinManager initialization");
            instructions.Add("5. Test functionality and visual appearance");

            return instructions;
        }

        #region Private Helper Methods

        private static bool IsMaterialSkinControl(Type controlType)
        {
            return controlType.Namespace?.Contains("MaterialSkin") == true ||
                   ControlMappings.ContainsKey(controlType);
        }

        private static IEnumerable<Control> GetAllControls(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                yield return control;

                foreach (var child in GetAllControls(control))
                {
                    yield return child;
                }
            }
        }

        private static void ApplySyncfusionThemeToControl(Control control)
        {
            // Apply appropriate Syncfusion theming based on control type
            SyncfusionThemeHelper.ApplyMaterialTheme(control);
        }

        private static Control CreateSyncfusionControl(Type syncfusionType, Control sourceControl)
        {
            // Create instance of Syncfusion control
            var syncfusionControl = Activator.CreateInstance(syncfusionType) as Control;

            if (syncfusionControl == null)
            {
                throw new InvalidOperationException($"Failed to create instance of {syncfusionType.Name}");
            }

            // Apply Material theming
            SyncfusionThemeHelper.ApplyMaterialTheme(syncfusionControl);

            return syncfusionControl;
        }

        private static void CopyControlProperties(Control source, Control target)
        {
            try
            {
                // Copy basic properties
                target.Name = source.Name;
                target.Text = source.Text;
                target.Location = source.Location;
                target.Size = source.Size;
                target.Anchor = source.Anchor;
                target.Dock = source.Dock;
                target.Enabled = source.Enabled;
                target.Visible = source.Visible;
                target.TabIndex = source.TabIndex;
                target.TabStop = source.TabStop;

                // Copy event handlers would require more complex reflection
                // For now, we'll document that these need manual migration
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ MIGRATION: Failed to copy some properties: {ex.Message}");
            }
        }

        #endregion
    }
}
