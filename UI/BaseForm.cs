using System;
using System.Windows.Forms;
using BusBuddy.Business;
using BusBuddy.Models;

namespace BusBuddy.UI
{
    public class BaseForm : Form
    {
        protected bool ValidateRequiredField(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        protected bool ValidateNumericField(string value, out int result)
        {
            return int.TryParse(value, out result);
        }

        protected bool ValidateDateField(string value, out DateTime result)
        {
            return DateTime.TryParse(value, out result);
        }
    }
}