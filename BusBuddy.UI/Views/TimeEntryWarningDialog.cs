using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Business;

namespace BusBuddy.UI.Views
{
    public class TimeEntryWarningDialog : Form
    {
        private List<TimeEntryWarning> _warnings;
        private ListView _warningsList;
        private Button _continueButton;
        private Button _cancelButton;
        private Button _fixIssuesButton;
        private Label _summaryLabel;

        public bool ContinueWithSave { get; private set; } = false;
        public bool ShouldFixIssues { get; private set; } = false;

        public TimeEntryWarningDialog(List<TimeEntryWarning> warnings)
        {
            _warnings = warnings ?? new List<TimeEntryWarning>();
            InitializeComponent();
            LoadWarnings();
        }

        private void InitializeComponent()
        {
            this.Text = "Time Entry Warnings";
            this.Size = new Size(700, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.ShowInTaskbar = false;
            this.MinimumSize = new Size(600, 400);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            // Summary label
            _summaryLabel = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(this.ClientSize.Width - 40, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                Text = "Potential issues found with your time entry:"
            };
            this.Controls.Add(_summaryLabel);

            // Warnings list
            _warningsList = new ListView
            {
                Location = new Point(20, 70),
                Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 250),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };

            _warningsList.Columns.Add("Type", 140);
            _warningsList.Columns.Add("Issue", 360);
            _warningsList.Columns.Add("Severity", 100);

            this.Controls.Add(_warningsList);

            // Details text box for selected warning
            var detailsLabel = new Label
            {
                Location = new Point(20, this.ClientSize.Height - 175),
                Size = new Size(150, 20),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Text = "Suggested Action:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            this.Controls.Add(detailsLabel);

            var detailsTextBox = new TextBox
            {
                Location = new Point(20, this.ClientSize.Height - 150),
                Size = new Size(this.ClientSize.Width - 40, 60),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.LightYellow,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(detailsTextBox);

            // Update details when selection changes
            _warningsList.SelectedIndexChanged += (s, e) =>
            {
                if (_warningsList.SelectedItems.Count > 0)
                {
                    var selectedIndex = _warningsList.SelectedItems[0].Index;
                    detailsTextBox.Text = _warnings[selectedIndex].SuggestedAction;
                }
                else
                {
                    detailsTextBox.Text = "Select a warning to see suggested actions.";
                }
            };

            // Buttons - positioned lower to be visible
            _fixIssuesButton = new Button
            {
                Location = new Point(this.ClientSize.Width - 370, this.ClientSize.Height - 65),
                Size = new Size(120, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Text = "Auto-Fix",
                DialogResult = DialogResult.Retry,
                BackColor = Color.LightBlue,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            _fixIssuesButton.Click += (s, e) => ShouldFixIssues = true;

            _continueButton = new Button
            {
                Location = new Point(this.ClientSize.Width - 240, this.ClientSize.Height - 65),
                Size = new Size(120, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Text = "Accept As-Is",
                DialogResult = DialogResult.OK,
                BackColor = Color.LightGreen,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            _continueButton.Click += (s, e) => ContinueWithSave = true;

            _cancelButton = new Button
            {
                Location = new Point(this.ClientSize.Width - 110, this.ClientSize.Height - 65),
                Size = new Size(100, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                BackColor = Color.LightCoral,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            this.Controls.AddRange(new Control[] { _fixIssuesButton, _continueButton, _cancelButton });

            // Set default buttons
            this.AcceptButton = _continueButton;
            this.CancelButton = _cancelButton;

            // Select first item if any
            if (_warningsList.Items.Count > 0)
            {
                _warningsList.Items[0].Selected = true;
            }

            // Add resize event handler
            this.Resize += TimeEntryWarningDialog_Resize;
        }

        private void TimeEntryWarningDialog_Resize(object sender, EventArgs e)
        {
            // Adjust control positions and sizes when form is resized
            if (_summaryLabel != null)
            {
                _summaryLabel.Size = new Size(this.ClientSize.Width - 40, 40);
            }

            if (_warningsList != null)
            {
                _warningsList.Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 250);
            }

            // Find and adjust details controls
            foreach (Control control in this.Controls)
            {
                if (control is Label label && label.Text == "Suggested Action:")
                {
                    label.Location = new Point(20, this.ClientSize.Height - 175);
                }
                else if (control is TextBox textBox && textBox.BackColor == Color.LightYellow)
                {
                    textBox.Location = new Point(20, this.ClientSize.Height - 150);
                    textBox.Size = new Size(this.ClientSize.Width - 40, 60);
                }
            }

            // Adjust button positions
            if (_fixIssuesButton != null)
                _fixIssuesButton.Location = new Point(this.ClientSize.Width - 370, this.ClientSize.Height - 65);
            if (_continueButton != null)
                _continueButton.Location = new Point(this.ClientSize.Width - 240, this.ClientSize.Height - 65);
            if (_cancelButton != null)
                _cancelButton.Location = new Point(this.ClientSize.Width - 110, this.ClientSize.Height - 65);
        }

        private void LoadWarnings()
        {
            _warningsList.Items.Clear();

            if (_warnings.Count == 0)
            {
                _summaryLabel.Text = "No issues found with your time entry.";
                _summaryLabel.ForeColor = Color.DarkGreen;
                _fixIssuesButton.Visible = false;
                _continueButton.Text = "OK";
                return;
            }

            // Count warnings by severity
            var highCount = _warnings.Count(w => w.Severity == WarningSeverity.High);
            var mediumCount = _warnings.Count(w => w.Severity == WarningSeverity.Medium);
            var lowCount = _warnings.Count(w => w.Severity == WarningSeverity.Low);

            _summaryLabel.Text = $"Found {_warnings.Count} potential issue(s): " +
                               $"{highCount} high, {mediumCount} medium, {lowCount} low priority";

            foreach (var warning in _warnings.OrderByDescending(w => w.Severity))
            {
                var item = new ListViewItem(new[]
                {
                    GetWarningTypeDisplayName(warning.Type),
                    warning.Message,
                    warning.Severity.ToString()
                });

                // Color code by severity
                switch (warning.Severity)
                {
                    case WarningSeverity.High:
                        item.BackColor = Color.MistyRose;
                        item.ForeColor = Color.DarkRed;
                        break;
                    case WarningSeverity.Medium:
                        item.BackColor = Color.LightYellow;
                        item.ForeColor = Color.DarkOrange;
                        break;
                    case WarningSeverity.Low:
                        item.BackColor = Color.LightBlue;
                        item.ForeColor = Color.DarkBlue;
                        break;
                }

                _warningsList.Items.Add(item);
            }

            // Auto-resize columns
            _warningsList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private string GetWarningTypeDisplayName(WarningType type)
        {
            return type switch
            {
                WarningType.MissingPreviousClockOut => "Missing Clock Out",
                WarningType.MissingClockIn => "Missing Clock In",
                WarningType.ShortLunchBreak => "Short Lunch",
                WarningType.LongLunchBreak => "Long Lunch",
                WarningType.VeryEarlyClockIn => "Early Clock In",
                WarningType.VeryLateClockOut => "Late Clock Out",
                WarningType.IncompleteRouteEntry => "Incomplete Route",
                WarningType.ExcessiveWorkHours => "Long Work Day",
                _ => type.ToString()
            };
        }

        public static DialogResult ShowWarnings(List<TimeEntryWarning> warnings, out bool shouldFixIssues)
        {
            shouldFixIssues = false;

            if (warnings == null || warnings.Count == 0)
            {
                return DialogResult.OK; // No warnings, proceed normally
            }

            using (var dialog = new TimeEntryWarningDialog(warnings))
            {
                var result = dialog.ShowDialog();
                shouldFixIssues = dialog.ShouldFixIssues;
                return result;
            }
        }
    }
}
