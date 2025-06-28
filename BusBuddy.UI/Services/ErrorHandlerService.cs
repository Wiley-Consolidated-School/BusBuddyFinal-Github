// Task 7: Create Error Handler Service (DashboardRedesign.md)
using System;
using System.Windows.Forms;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Centralized error handling service for the BusBuddy dashboard
    /// Provides user feedback via MessageBox and optional logging support
    /// </summary>
    public class ErrorHandlerService : IErrorHandlerService
    {
        /// <summary>
        /// Handles an error with a user-friendly message and title
        /// </summary>
        /// <param name="message">Error message to display to user</param>
        /// <param name="title">Title for the error dialog</param>
        public void HandleError(string message, string title)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = "An unexpected error occurred.";
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                title = "Error";
            }

            try
            {
                // Display user-friendly error message
                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Log the error for debugging
                LogError(message, title);
            }
            catch (Exception ex)
            {
                // Fallback error handling if MessageBox fails
                System.Diagnostics.Debug.WriteLine($"Error in ErrorHandlerService.HandleError: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Original error: {message}");
            }
        }

        /// <summary>
        /// Handles an exception with user feedback and optional logging
        /// </summary>
        /// <param name="exception">Exception to handle</param>
        /// <param name="context">Context where the error occurred</param>
        public void HandleException(Exception exception, string context)
        {
            if (exception == null)
            {
                HandleError("An unknown error occurred.", context ?? "Unknown Context");
                return;
            }

            var userMessage = GetUserFriendlyMessage(exception);
            var title = $"Error in {context ?? "Application"}";

            try
            {
                // Display user-friendly error message
                MessageBox.Show(userMessage, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Log detailed exception information
                LogError($"Exception: {exception.Message}\nStack Trace: {exception.StackTrace}", context ?? "Unknown");
            }
            catch (Exception ex)
            {
                // Fallback error handling
                System.Diagnostics.Debug.WriteLine($"Error in ErrorHandlerService.HandleException: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Original exception: {exception}");
            }
        }

        /// <summary>
        /// Logs an error message without displaying UI
        /// </summary>
        /// <param name="message">Error message to log</param>
        /// <param name="context">Context where the error occurred</param>
        public void LogError(string message, string context)
        {
            try
            {
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{context ?? "Unknown"}] {message}";

                // Log to Debug output (visible in VS Output window)
                System.Diagnostics.Debug.WriteLine($"ERROR: {logMessage}");

                // Log to Console (visible in console applications)
                Console.WriteLine($"ERROR: {logMessage}");

                // TODO: Add file logging or database logging as needed
                // For now, Debug and Console logging provide sufficient visibility
            }
            catch (Exception ex)
            {
                // Fallback - write to Debug only
                System.Diagnostics.Debug.WriteLine($"Error in LogError: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Original message: {message}");
            }
        }

        /// <summary>
        /// Converts technical exceptions to user-friendly messages
        /// </summary>
        /// <param name="exception">Exception to convert</param>
        /// <returns>User-friendly error message</returns>
        private string GetUserFriendlyMessage(Exception exception)
        {
            // Convert common technical exceptions to user-friendly messages
            return exception switch
            {
                ArgumentNullException => "A required value was not provided.",
                ArgumentException => "An invalid value was provided.",
                InvalidOperationException => "The requested operation cannot be completed at this time.",
                UnauthorizedAccessException => "Access denied. Please check your permissions.",
                System.IO.FileNotFoundException => "A required file could not be found.",
                System.IO.DirectoryNotFoundException => "A required directory could not be found.",
                System.Data.SqlClient.SqlException => "Database connection error. Please try again later.",
                TimeoutException => "The operation timed out. Please try again.",
                NotImplementedException => "This feature is not yet implemented.",
                _ => $"An error occurred: {exception.Message}"
            };
        }
    }
}

