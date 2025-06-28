using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools; // Added for Syncfusion controls

namespace BusBuddy.UI
{
    public static class FormValidator
    {
        public static bool ValidateRequiredField(Control control, string fieldName, ErrorProvider errorProvider)
        {
            if (control is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
            {
                errorProvider.SetError(control, $"{fieldName} is required");
                return false;
            }

            if (control is ComboBox comboBox && (comboBox.SelectedItem == null || string.IsNullOrWhiteSpace(comboBox.Text)))
            {
                errorProvider.SetError(control, $"{fieldName} is required");
                return false;
            }

            errorProvider.SetError(control, "");
            return true;
        }

        public static bool ValidateNumericField(TextBox textBox, string fieldName, ErrorProvider errorProvider)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                return true; // Empty is valid for non-required numeric fields
            }

            if (!double.TryParse(textBox.Text, out _))
            {
                errorProvider.SetError(textBox, $"{fieldName} must be a valid number");
                return false;
            }

            errorProvider.SetError(textBox, "");
            return true;
        }

        public static bool ValidateIntegerField(TextBox textBox, string fieldName, ErrorProvider errorProvider)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                return true; // Empty is valid for non-required numeric fields
            }

            if (!int.TryParse(textBox.Text, out _))
            {
                errorProvider.SetError(textBox, $"{fieldName} must be a valid integer");
                return false;
            }

            errorProvider.SetError(textBox, "");
            return true;
        }

        public static bool ValidateDateField(TextBox textBox, string fieldName, ErrorProvider errorProvider)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                return true; // Empty is valid for non-required date fields
            }

            if (!DateTime.TryParse(textBox.Text, out _))
            {
                errorProvider.SetError(textBox, $"{fieldName} must be a valid date");
                return false;
            }

            errorProvider.SetError(textBox, "");
            return true;
        }

        public static bool ValidateEmail(TextBox textBox, string fieldName, ErrorProvider errorProvider)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                return true; // Empty is valid for non-required email fields
            }

            // Simple email pattern validation
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(textBox.Text, pattern))
            {
                errorProvider.SetError(textBox, $"{fieldName} must be a valid email address");
                return false;
            }

            errorProvider.SetError(textBox, "");
            return true;
        }        public static bool ValidatePhoneNumber(TextBox textBox, string fieldName, ErrorProvider errorProvider)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                return true; // Empty is valid for non-required phone fields
            }

            // Phone number can be in different formats, this is a basic validation
            string digitsOnly = Regex.Replace(textBox.Text, @"[^\d]", "");
            if (digitsOnly.Length < 10 || digitsOnly.Length > 11)
            {
                errorProvider.SetError(textBox, $"{fieldName} must be a valid phone number (10-11 digits)");
                return false;
            }

            errorProvider.SetError(textBox, "");
            return true;
        }

        public static bool ValidateTime(TextBox textBox, string fieldName, ErrorProvider errorProvider)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                return true; // Empty is valid for non-required time fields
            }

            if (!TimeSpan.TryParse(textBox.Text, out _))
            {
                errorProvider.SetError(textBox, $"{fieldName} must be a valid time in HH:MM format");
                return false;
            }

            errorProvider.SetError(textBox, "");
            return true;
        }

        public static bool ValidateCurrency(TextBox textBox, string fieldName, ErrorProvider errorProvider)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                return true; // Empty is valid for non-required currency fields
            }

            // Remove currency symbols for validation
            string cleanValue = textBox.Text.Replace("$", "").Replace(",", "");

            if (!decimal.TryParse(cleanValue, out decimal value) || value < 0)
            {
                errorProvider.SetError(textBox, $"{fieldName} must be a valid positive currency amount");
                return false;
            }

            errorProvider.SetError(textBox, "");
            return true;
        }

        public static bool ValidateVINNumber(TextBox textBox, string fieldName, ErrorProvider errorProvider)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                return true; // Empty is valid for non-required VIN fields
            }

            // VIN numbers are typically 17 characters
            if (textBox.Text.Length != 17)
            {
                errorProvider.SetError(textBox, $"{fieldName} must be exactly 17 characters long");
                return false;
            }

            // VIN cannot contain I, O, or Q
            if (textBox.Text.ToUpper().Contains("I") || textBox.Text.ToUpper().Contains("O") || textBox.Text.ToUpper().Contains("Q"))
            {
                errorProvider.SetError(textBox, $"{fieldName} cannot contain the letters I, O, or Q");
                return false;
            }

            errorProvider.SetError(textBox, "");
            return true;
        }

        public static bool ValidateRange(TextBox textBox, string fieldName, double min, double max, ErrorProvider errorProvider)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                return true; // Empty is valid for non-required range fields
            }

            if (!double.TryParse(textBox.Text, out double value))
            {
                errorProvider.SetError(textBox, $"{fieldName} must be a valid number");
                return false;
            }

            if (value < min || value > max)
            {
                errorProvider.SetError(textBox, $"{fieldName} must be between {min} and {max}");
                return false;
            }

            errorProvider.SetError(textBox, "");
            return true;
        }

        public static bool ValidateTimeRange(TextBox startTime, TextBox endTime, ErrorProvider errorProvider)
        {
            if (string.IsNullOrWhiteSpace(startTime.Text) || string.IsNullOrWhiteSpace(endTime.Text))
            {
                return true; // Empty is valid for non-required time range fields
            }

            if (!TimeSpan.TryParse(startTime.Text, out TimeSpan start) || !TimeSpan.TryParse(endTime.Text, out TimeSpan end))
            {
                errorProvider.SetError(endTime, "Both start and end times must be valid");
                return false;
            }

            if (start >= end)
            {
                errorProvider.SetError(endTime, "End time must be after start time");
                return false;
            }

            errorProvider.SetError(endTime, "");
            return true;
        }

        public static void ClearAllErrors(ErrorProvider errorProvider, params Control[] controls)
        {
            foreach (Control control in controls)
            {
                errorProvider.SetError(control, "");
            }
        }

        // Syncfusion-specific validation methods
        public static bool ValidateRequiredField(TextBoxExt textBox, string fieldName, ErrorProvider errorProvider)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                errorProvider.SetError(textBox, $"{fieldName} is required");
                return false;
            }

            errorProvider.SetError(textBox, "");
            return true;
        }

        public static bool ValidateNumericField(TextBoxExt textBox, string fieldName, ErrorProvider errorProvider)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                return true; // Empty is valid for non-required numeric fields
            }

            if (!double.TryParse(textBox.Text, out _))
            {
                errorProvider.SetError(textBox, $"{fieldName} must be a valid number");
                return false;
            }

            errorProvider.SetError(textBox, "");
            return true;
        }

        public static bool ValidateIntegerField(TextBoxExt textBox, string fieldName, ErrorProvider errorProvider)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                return true; // Empty is valid for non-required numeric fields
            }

            if (!int.TryParse(textBox.Text, out _))
            {
                errorProvider.SetError(textBox, $"{fieldName} must be a valid integer");
                return false;
            }

            errorProvider.SetError(textBox, "");
            return true;
        }

        public static bool ValidateEmail(TextBoxExt textBox, string fieldName, ErrorProvider errorProvider)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                return true; // Empty is valid for non-required email fields
            }

            // Simple email pattern validation
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(textBox.Text, pattern))
            {
                errorProvider.SetError(textBox, $"{fieldName} must be a valid email address");
                return false;
            }

            errorProvider.SetError(textBox, "");
            return true;
        }
    }
}

