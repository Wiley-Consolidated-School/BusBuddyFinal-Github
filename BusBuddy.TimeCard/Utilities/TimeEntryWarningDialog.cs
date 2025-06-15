using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.TimeCard.Services;
using MaterialSkin.Controls;

namespace BusBuddy.TimeCard.Utilities
{
    /// <summary>
    /// Dialog for displaying time entry warnings
    /// </summary>
    public static class TimeEntryWarningDialog
    {
        /// <summary>
        /// Show warnings dialog and allow user to choose how to proceed
        /// </summary>
        public static DialogResult ShowWarnings(List<TimeEntryWarning> warnings, out bool shouldFixIssues)
        {
            shouldFixIssues = false;

            if (warnings == null || !warnings.Any())
            {
                return DialogResult.OK;
            }

            using var dialog = CreateWarningDialog(warnings);
            var result = dialog.ShowDialog();

            if (result == DialogResult.Yes)
            {
                // Ask if user wants to auto-fix issues
                var fixableWarnings = warnings.Where(w => w.Severity != WarningSeverity.High).ToList();
                if (fixableWarnings.Any())
                {
                    shouldFixIssues = MessageBox.Show(
                        "Would you like to automatically fix some of these issues?",
                        "Auto-Fix Issues",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes;
                }
            }

            return result;
        }

        /// <summary>
        /// Create the warning dialog form
        /// </summary>
        private static Form CreateWarningDialog(List<TimeEntryWarning> warnings)
        {
            var dialog = new Form();
            dialog.Text = "Time Entry Warnings";
            dialog.Size = new Size(500, 400);
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.MaximizeBox = false;
            dialog.MinimizeBox = false;
            dialog.ShowInTaskbar = false;

            // Set high DPI support
            dialog.AutoScaleMode = AutoScaleMode.Dpi;
            dialog.Font = new Font("Segoe UI", 9F);

            // Create main panel
            var mainPanel = new TableLayoutPanel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(10);
            mainPanel.RowCount = 3;
            mainPanel.ColumnCount = 1;

            // Row 0: Header (auto-size)
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            // Row 1: Content (fill)
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            // Row 2: Buttons (auto-size)
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Header
            var headerLabel = new Label();
            headerLabel.Text = "The following issues were found with this time entry:";
            headerLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            headerLabel.AutoSize = true;
            headerLabel.Dock = DockStyle.Top;
            mainPanel.Controls.Add(headerLabel, 0, 0);

            // Warning list
            var warningListView = CreateWarningListView(warnings);
            mainPanel.Controls.Add(warningListView, 0, 1);

            // Button panel
            var buttonPanel = CreateButtonPanel(dialog, warnings);
            mainPanel.Controls.Add(buttonPanel, 0, 2);

            dialog.Controls.Add(mainPanel);
            return dialog;
        }

        /// <summary>
        /// Create the warning list view
        /// </summary>
        private static ListView CreateWarningListView(List<TimeEntryWarning> warnings)
        {
            var listView = new ListView();
            listView.Dock = DockStyle.Fill;
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.MultiSelect = false;

            // Add columns
            listView.Columns.Add("Severity", 80);
            listView.Columns.Add("Issue", 350);

            // Add warnings
            foreach (var warning in warnings.OrderByDescending(w => w.Severity))
            {
                var item = new ListViewItem(GetSeverityText(warning.Severity));
                item.SubItems.Add(warning.Message);

                // Color code by severity
                switch (warning.Severity)
                {
                    case WarningSeverity.High:
                        item.BackColor = Color.FromArgb(255, 235, 235); // Light red
                        item.ForeColor = Color.DarkRed;
                        break;
                    case WarningSeverity.Medium:
                        item.BackColor = Color.FromArgb(255, 248, 220); // Light orange
                        item.ForeColor = Color.DarkOrange;
                        break;
                    case WarningSeverity.Low:
                        item.BackColor = Color.FromArgb(240, 255, 240); // Light green
                        item.ForeColor = Color.DarkGreen;
                        break;
                }

                listView.Items.Add(item);
            }

            return listView;
        }

        /// <summary>
        /// Create the button panel
        /// </summary>
        private static Panel CreateButtonPanel(Form dialog, List<TimeEntryWarning> warnings)
        {
            var panel = new Panel();
            panel.Height = 50;
            panel.Dock = DockStyle.Bottom;

            var hasHighSeverity = warnings.Any(w => w.Severity == WarningSeverity.High);

            if (hasHighSeverity)
            {
                // For high severity issues, only allow Fix or Cancel
                var fixButton = new Button();
                fixButton.Text = "Fix Issues";
                fixButton.Size = new Size(80, 30);
                fixButton.Location = new Point(10, 10);
                fixButton.DialogResult = DialogResult.Retry;
                fixButton.BackColor = Color.Orange;
                fixButton.ForeColor = Color.White;
                panel.Controls.Add(fixButton);

                var cancelButton = new Button();
                cancelButton.Text = "Cancel";
                cancelButton.Size = new Size(80, 30);
                cancelButton.Location = new Point(100, 10);
                cancelButton.DialogResult = DialogResult.Cancel;
                panel.Controls.Add(cancelButton);

                dialog.AcceptButton = fixButton;
                dialog.CancelButton = cancelButton;
            }
            else
            {
                // For lower severity issues, allow Continue, Fix, or Cancel
                var continueButton = new Button();
                continueButton.Text = "Continue";
                continueButton.Size = new Size(80, 30);
                continueButton.Location = new Point(10, 10);
                continueButton.DialogResult = DialogResult.Yes;
                continueButton.BackColor = Color.Green;
                continueButton.ForeColor = Color.White;
                panel.Controls.Add(continueButton);

                var fixButton = new Button();
                fixButton.Text = "Fix Issues";
                fixButton.Size = new Size(80, 30);
                fixButton.Location = new Point(100, 10);
                fixButton.DialogResult = DialogResult.Retry;
                fixButton.BackColor = Color.Orange;
                fixButton.ForeColor = Color.White;
                panel.Controls.Add(fixButton);

                var cancelButton = new Button();
                cancelButton.Text = "Cancel";
                cancelButton.Size = new Size(80, 30);
                cancelButton.Location = new Point(190, 10);
                cancelButton.DialogResult = DialogResult.Cancel;
                panel.Controls.Add(cancelButton);

                dialog.AcceptButton = continueButton;
                dialog.CancelButton = cancelButton;
            }

            return panel;
        }

        /// <summary>
        /// Get display text for severity level
        /// </summary>
        private static string GetSeverityText(WarningSeverity severity)
        {
            return severity switch
            {
                WarningSeverity.High => "❌ High",
                WarningSeverity.Medium => "⚠️ Medium",
                WarningSeverity.Low => "ℹ️ Low",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Show a simple message for warnings
        /// </summary>
        public static DialogResult ShowSimpleWarnings(List<TimeEntryWarning> warnings)
        {
            if (warnings == null || !warnings.Any())
            {
                return DialogResult.OK;
            }

            var message = "Issues found:\n\n" + string.Join("\n", warnings.Select(w => $"• {w.Message}"));

            var hasHighSeverity = warnings.Any(w => w.Severity == WarningSeverity.High);

            if (hasHighSeverity)
            {
                message += "\n\nPlease fix these issues before continuing.";
                return MessageBox.Show(message, "Time Entry Issues", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                message += "\n\nDo you want to continue anyway?";
                return MessageBox.Show(message, "Time Entry Warnings", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            }
        }
    }
}
