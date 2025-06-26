using System;
using System.Windows.Forms;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Production implementation of IMessageService using Windows Forms MessageBox
    /// </summary>
    public class MessageBoxService : IMessageService
    {
        public void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowInfo(string message, string title = "Information")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public bool ShowConfirmation(string message, string title = "Confirm")
        {
            var result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }

        public void ShowWarning(string message, string title = "Warning")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
