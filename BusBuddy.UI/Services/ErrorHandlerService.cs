using System;
using System.Windows.Forms;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Concrete implementation of IErrorHandlerService for centralized error handling
    /// </summary>
    public class ErrorHandlerService : IErrorHandlerService
    {
        public void HandleError(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void HandleException(Exception exception, string context)
        {
            MessageBox.Show($"{context}: {exception.Message}", "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            // Optionally log exception details here
        }

        public void LogError(string message, string context)
        {
            // Simple logging to debug output; replace with real logging as needed
            System.Diagnostics.Debug.WriteLine($"[ERROR] {context}: {message}");
        }
    }
}

