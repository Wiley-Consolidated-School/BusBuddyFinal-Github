using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using BusBuddy.UI.Theme;

namespace BusBuddy.UI.Validation
{
    /// <summary>
    /// Enhanced validation service with Material Design feedback
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validate a model and display Material Design error messages
        /// </summary>
        public static bool ValidateModel<T>(T model, Control parentControl) where T : class
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);

            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

            if (!isValid)
            {
                ShowValidationErrors(validationResults, parentControl);
            }

            return isValid;
        }

        /// <summary>
        /// Show validation errors using Material Design components
        /// </summary>
        private static void ShowValidationErrors(List<ValidationResult> validationResults, Control parentControl)
        {
            var errorMessage = string.Join("\n", validationResults.Select(r => r.ErrorMessage));

            // Create Material Design error dialog
            var errorDialog = new Form
            {
                Text = "Validation Errors",
                Size = new Size(400, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = MaterialDesignThemeManager.DarkTheme.Surface
            };

            var errorLabel = new MaterialLabel
            {
                Text = errorMessage,
                Dock = DockStyle.Fill,
                ForeColor = MaterialDesignThemeManager.DarkTheme.Error,
                Font = MaterialDesignThemeManager.Typography.GetBodyMedium(errorDialog),
                Padding = new Padding(16)
            };

            var okButton = new MaterialButton
            {
                Text = "OK",
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true,
                Size = new Size(100, 36),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(280, 220)
            };

            okButton.Click += (s, e) => errorDialog.Close();

            errorDialog.Controls.Add(errorLabel);
            errorDialog.Controls.Add(okButton);
            errorDialog.ShowDialog(parentControl.FindForm());
        }

        /// <summary>
        /// Validate individual control and show inline error
        /// </summary>
        public static bool ValidateControl(Control control, Func<string, bool> validator, string errorMessage)
        {
            var isValid = true;

            if (control is MaterialTextBox textBox)
            {
                isValid = validator(textBox.Text);
                if (!isValid)
                {
                    // Add error styling to MaterialTextBox
                    textBox.BackColor = MaterialDesignThemeManager.DarkTheme.ErrorContainer;
                    textBox.ForeColor = MaterialDesignThemeManager.DarkTheme.OnErrorContainer;

                    // Show tooltip with error
                    var toolTip = new ToolTip();
                    toolTip.SetToolTip(textBox, errorMessage);
                }
                else
                {
                    // Reset to normal styling
                    textBox.BackColor = MaterialDesignThemeManager.DarkTheme.Surface;
                    textBox.ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurface;
                }
            }

            return isValid;
        }

        /// <summary>
        /// Show success message using Material Design
        /// </summary>
        public static void ShowSuccessMessage(string message, Control parentControl)
        {
            var successDialog = new Form
            {
                Text = "Success",
                Size = new Size(350, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = MaterialDesignThemeManager.DarkTheme.Surface
            };

            var successLabel = new MaterialLabel
            {
                Text = message,
                Dock = DockStyle.Fill,
                ForeColor = MaterialDesignThemeManager.DarkTheme.Success,
                Font = MaterialDesignThemeManager.Typography.GetBodyMedium(successDialog),
                Padding = new Padding(16)
            };

            var okButton = new MaterialButton
            {
                Text = "OK",
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true,
                Size = new Size(100, 36),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(230, 120)
            };

            okButton.Click += (s, e) => successDialog.Close();

            successDialog.Controls.Add(successLabel);
            successDialog.Controls.Add(okButton);
            successDialog.ShowDialog(parentControl.FindForm());
        }
    }
}
