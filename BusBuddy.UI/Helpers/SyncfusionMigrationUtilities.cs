using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.Input;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Utilities to help with migrating Material controls to Syncfusion controls
    /// Provides mappings, helper methods, and compatibility functions
    /// </summary>
    public static class SyncfusionMigrationUtilities
    {
        /// <summary>
        /// Material to Syncfusion control type mappings
        /// </summary>
        public static readonly Dictionary<Type, Type> ControlMappings = new Dictionary<Type, Type>
        {
            // Note: These would be the actual mappings if Material types were available
            // For now, providing conceptual mappings for the migration script
        };

        /// <summary>
        /// Get the Syncfusion equivalent for a Material control type name
        /// </summary>
        public static string GetSyncfusionEquivalent(string materialControlName)
        {
            var mappings = new Dictionary<string, string>
            {
                {"Form", "SfForm"},
                {"MaterialSkinManager", "SfSkinManager"},
                {"MaterialRaisedButton", "SfButton"},
                {"MaterialFlatButton", "SfButton"},
                {"MaterialSingleLineTextField", "SfTextBox"},
                {"Label", "Label"},
                {"TabControl", "SfTabControl"},
                {"TabPage", "TabPageAdv"},
                {"ComboBox", "SfComboBox"},
                {"MaterialDateTimePicker", "SfDateTimeEdit"},
                {"DataGridView", "SfDataGrid"},
                {"CheckBox", "SfCheckBox"},
                {"RadioButton", "SfRadioButton"},
                {"MaterialContextMenuStrip", "SfContextMenu"},
                {"ProgressBar", "SfProgressBar"},
                {"Panel", "SfGradientPanel"},
                {"MaterialDivider", "SfSeparator"},
                {"StandardMaterialForm", "StandardSyncfusionForm"},
                {"StandardMaterialManagementForm", "StandardSyncfusionManagementForm"}
            };

            return mappings.ContainsKey(materialControlName) ? mappings[materialControlName] : materialControlName;
        }

        /// <summary>
        /// Get required using statements for Syncfusion controls
        /// </summary>
        public static List<string> GetRequiredUsingStatements()
        {
            return new List<string>
            {
                "using Syncfusion.Windows.Forms;",
                "using Syncfusion.Windows.Forms.Tools;",
                "using Syncfusion.Windows.Forms.Grid;",
                "using Syncfusion.WinForms.Controls;",
                "using Syncfusion.WinForms.DataGrid;",
                "using Syncfusion.WinForms.Input;",
                "using Syncfusion.WinForms.ListView;",
                "using BusBuddy.UI.Helpers;"
            };
        }

        /// <summary>
        /// Get Material using statements that should be removed
        /// </summary>
        public static List<string> GetMaterialUsingStatements()
        {
            return new List<string>
            {
                "// MaterialSkin removed - using Syncfusion theming",
                "// MaterialSkin.Controls removed - using standard controls with Syncfusion theming",
                "// MaterialSkin.Animations removed"
            };
        }

        /// <summary>
        /// Apply Syncfusion theme to a form (replacement for MaterialSkinManager)
        /// </summary>
        public static void ApplyTheme(Form form)
        {
            if (form == null) return;

            try
            {
                // Use the existing SyncfusionThemeHelper
                SyncfusionThemeHelper.ApplyMaterialTheme(form);

                // Apply theme to all child controls
                ApplyThemeToControls(form.Controls);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error applying Syncfusion theme to {form.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively apply theme to all controls in a collection
        /// </summary>
        private static void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                try
                {
                    SyncfusionThemeHelper.ApplyMaterialTheme(control);

                    // Recursively apply to child controls
                    if (control.HasChildren)
                    {
                        ApplyThemeToControls(control.Controls);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error applying theme to {control.GetType().Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Convert Material property names to Syncfusion equivalents
        /// </summary>
        public static string ConvertPropertyName(string materialProperty)
        {
            var propertyMappings = new Dictionary<string, string>
            {
                {"MaterialSkinManager.Instance", "SyncfusionHelper"},
                {"Theme", "VisualTheme"},
                {"ColorScheme", "ColorPalette"},
                {"AddFormToManage", "ApplyTheme"}
            };

            return propertyMappings.ContainsKey(materialProperty) ? propertyMappings[materialProperty] : materialProperty;
        }

        /// <summary>
        /// Generate replacement code for Material-specific initialization
        /// </summary>
        public static string GetSyncfusionInitializationCode()
        {
            return @"
            // Initialize Syncfusion theme
            SyncfusionHelper.ApplyTheme(this);

            // Apply material design styling
            SyncfusionThemeHelper.ApplyMaterialTheme(this);";
        }

        /// <summary>
        /// Check if a control type is a Syncfusion control
        /// </summary>
        public static bool IsSyncfusionControl(Type controlType)
        {
            return controlType.Namespace?.StartsWith("Syncfusion") ?? false;
        }

        /// <summary>
        /// Get migration notes for specific control types
        /// </summary>
        public static string GetMigrationNotes(string controlType)
        {
            var notes = new Dictionary<string, string>
            {
                {"Form", "Replace with SfForm or use standard Form with SyncfusionThemeHelper.ApplyMaterialTheme()"},
                {"MaterialRaisedButton", "Replace with SfButton or standard Button with material styling"},
                {"MaterialSingleLineTextField", "Replace with SfTextBox or standard TextBox with material styling"},
                {"TabControl", "Replace with SfTabControl for enhanced features"},
                {"DataGridView", "Consider upgrading to SfDataGrid for better performance and features"},
                {"ComboBox", "Replace with SfComboBox for consistent styling"},
                {"MaterialDateTimePicker", "Replace with SfDateTimeEdit for better date/time handling"}
            };

            return notes.ContainsKey(controlType) ? notes[controlType] : "Standard migration - replace with Syncfusion equivalent";
        }

        /// <summary>
        /// Validate that all required Syncfusion assemblies are available
        /// </summary>
        public static bool ValidateSyncfusionReferences()
        {
            try
            {
                // Check for core Syncfusion assemblies that we know exist
                var syncfusionAssemblies = new[]
                {
                    "Syncfusion.Shared.Base",
                    "Syncfusion.Tools.Windows",
                    "Syncfusion.SfDataGrid.WinForms"
                };

                foreach (var assemblyName in syncfusionAssemblies)
                {
                    try
                    {
                        var assembly = System.Reflection.Assembly.Load(assemblyName);
                        if (assembly == null)
                        {
                            Console.WriteLine($"❌ Required Syncfusion assembly not found: {assemblyName}");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error loading Syncfusion assembly {assemblyName}: {ex.Message}");
                        return false;
                    }
                }

                Console.WriteLine("✅ All required Syncfusion references validated");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error validating Syncfusion references: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get recommended migration order for forms
        /// </summary>
        public static List<string> GetRecommendedMigrationOrder()
        {
            return new List<string>
            {
                // Phase 1: Core Entity Forms
                "ActivityEditForm.cs",
                "VehicleForm.cs",
                "FuelEditForm.cs",
                "MaintenanceEditForm.cs",
                "SchoolCalendarEditForm.cs",

                // Phase 2: Management Forms
                "ActivityManagementForm.cs",
                "VehicleManagementForm.cs",
                "FuelManagementForm.cs",
                "MaintenanceManagementForm.cs",
                "SchoolCalendarManagementForm.cs",

                // Phase 3: Advanced Forms
                "RouteManagementForm.cs",
                "DriverManagementForm.cs",
                "ActivityScheduleEditForm.cs",
                "ActivityScheduleManagementForm.cs"
            };
        }

        /// <summary>
        /// Create a migration report for a specific form
        /// </summary>
        public static string CreateMigrationReport(string formName, bool success, List<string> changes, List<string> warnings)
        {
            var report = $@"
=== MIGRATION REPORT: {formName} ===
Status: {(success ? "✅ SUCCESS" : "❌ FAILED")}
Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

Changes Made:
{string.Join("\n", changes.Select(c => $"  - {c}"))}

{(warnings.Any() ? $@"
Warnings:
{string.Join("\n", warnings.Select(w => $"  ⚠️  {w}"))}" : "")}

Next Steps:
  1. Review the generated Syncfusion form
  2. Test the form manually
  3. Run automated tests
  4. Update any remaining references
  5. Consider cleanup of old Material files

";
            return report;
        }
    }

    /// <summary>
    /// Helper class for backward compatibility during migration
    /// Provides shims for Material controls that don't have direct Syncfusion equivalents
    /// </summary>
    public static class SyncfusionCompatibilityHelper
    {
        /// <summary>
        /// Create a Syncfusion-styled button that behaves like MaterialRaisedButton
        /// </summary>
        public static Button CreateMaterialButton(string text = "")
        {
            var button = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                BackColor = SyncfusionThemeHelper.MaterialColors.Primary,
                ForeColor = Color.White,
                Font = SyncfusionThemeHelper.MaterialTheme.DefaultFont,
                Height = SyncfusionThemeHelper.MaterialTheme.DefaultButtonHeight
            };

            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        /// <summary>
        /// Create a Syncfusion-styled text box that behaves like MaterialSingleLineTextField
        /// </summary>
        public static TextBox CreateMaterialTextBox(string hint = "")
        {
            var textBox = new TextBox
            {
                Font = SyncfusionThemeHelper.MaterialTheme.DefaultFont,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ForeColor = SyncfusionThemeHelper.MaterialColors.Text,
                Height = SyncfusionThemeHelper.MaterialTheme.DefaultControlHeight
            };

            if (!string.IsNullOrEmpty(hint))
            {
                // Add placeholder text behavior
                textBox.Text = hint;
                textBox.ForeColor = SyncfusionThemeHelper.MaterialColors.TextSecondary;

                textBox.GotFocus += (s, e) =>
                {
                    if (textBox.Text == hint)
                    {
                        textBox.Text = "";
                        textBox.ForeColor = SyncfusionThemeHelper.MaterialColors.Text;
                    }
                };

                textBox.LostFocus += (s, e) =>
                {
                    if (string.IsNullOrEmpty(textBox.Text))
                    {
                        textBox.Text = hint;
                        textBox.ForeColor = SyncfusionThemeHelper.MaterialColors.TextSecondary;
                    }
                };
            }

            return textBox;
        }
    }
}
