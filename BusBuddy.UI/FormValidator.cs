using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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
        }
        
        public static bool ValidatePhoneNumber(TextBox textBox, string fieldName, ErrorProvider errorProvider)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                return true; // Empty is valid for non-required phone fields
            }
            
            // Phone number can be in different formats, this is a basic validation
            string digitsOnly = Regex.Replace(textBox.Text, @"[^\d]", "");
            if (digitsOnly.Length < 10)
            {
                errorProvider.SetError(textBox, $"{fieldName} must be a valid phone number");
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
    }
}