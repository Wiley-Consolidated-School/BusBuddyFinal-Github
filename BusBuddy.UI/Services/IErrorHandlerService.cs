// Task 7: Create Error Handler Service (DashboardRedesign.md)
using System;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Provides centralized error handling with user feedback and logging support
    /// Ensures robust error management for dashboard services and UI components
    /// </summary>
    public interface IErrorHandlerService
    {
        /// <summary>
        /// Handles an error with a user-friendly message and title
        /// </summary>
        /// <param name="message">Error message to display to user</param>
        /// <param name="title">Title for the error dialog</param>
        void HandleError(string message, string title);

        /// <summary>
        /// Handles an exception with user feedback and optional logging
        /// </summary>
        /// <param name="exception">Exception to handle</param>
        /// <param name="context">Context where the error occurred</param>
        void HandleException(Exception exception, string context);

        /// <summary>
        /// Logs an error message without displaying UI
        /// </summary>
        /// <param name="message">Error message to log</param>
        /// <param name="context">Context where the error occurred</param>
        void LogError(string message, string context);
    }
}
