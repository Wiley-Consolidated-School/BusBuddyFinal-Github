using System;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Interface for handling user messages and dialogs
    /// Allows for testable error handling without UI dependencies
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Show an error message to the user
        /// </summary>
        void ShowError(string message, string title = "Error");

        /// <summary>
        /// Show an information message to the user
        /// </summary>
        void ShowInfo(string message, string title = "Information");

        /// <summary>
        /// Show a confirmation dialog and return the user's choice
        /// </summary>
        bool ShowConfirmation(string message, string title = "Confirm");

        /// <summary>
        /// Show a warning message to the user
        /// </summary>
        void ShowWarning(string message, string title = "Warning");
    }
}
