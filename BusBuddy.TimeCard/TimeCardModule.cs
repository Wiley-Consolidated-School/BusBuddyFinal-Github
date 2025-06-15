using System;
using System.Windows.Forms;
using BusBuddy.TimeCard.Views;
using BusBuddy.Data;

namespace BusBuddy.TimeCard
{
    /// <summary>
    /// Entry point for the TimeCard module
    /// Provides static methods to launch TimeCard functionality
    /// </summary>
    public static class TimeCardModule
    {
        /// <summary>
        /// Launch the TimeCard Management Form as a dialog
        /// </summary>
        public static void LaunchTimeCardManagementForm()
        {
            try
            {
                using var form = new TimeCardManagementForm();
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error launching TimeCard Management: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Launch the TimeCard Management Form with specific repositories (for dependency injection)
        /// </summary>
        public static void LaunchTimeCardManagementForm(ITimeCardRepository timeCardRepository, IDriverRepository driverRepository)
        {
            try
            {
                using var form = new TimeCardManagementForm(timeCardRepository, driverRepository);
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error launching TimeCard Management: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Create a new TimeCard Management Form instance (for hosting in other containers)
        /// </summary>
        public static TimeCardManagementForm CreateTimeCardManagementForm()
        {
            return new TimeCardManagementForm();
        }

        /// <summary>
        /// Create a new TimeCard Management Form instance with specific repositories
        /// </summary>
        public static TimeCardManagementForm CreateTimeCardManagementForm(ITimeCardRepository timeCardRepository, IDriverRepository driverRepository)
        {
            return new TimeCardManagementForm(timeCardRepository, driverRepository);
        }
    }
}
