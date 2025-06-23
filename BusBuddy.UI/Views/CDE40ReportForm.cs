using System;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.UI.Base;
using Syncfusion.WinForms.Controls;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// CDE-40 Report Form for Colorado Department of Education transportation reporting
    /// </summary>
    public partial class CDE40ReportForm : SyncfusionBaseForm
    {
        private Panel headerPanel;
        private Label titleLabel;
        private SfButton generateReportButton;
        private SfButton closeButton;
        private Label infoLabel;

        public CDE40ReportForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "CDE-40 Transportation Report";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(800, 600); // Override base class minimum size
            this.StartPosition = FormStartPosition.CenterParent;

            // Header panel
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = ColorTranslator.FromHtml("#212121")
            };

            titleLabel = new Label
            {
                Text = "CDE-40 Transportation Report",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 25)
            };

            headerPanel.Controls.Add(titleLabel);
            this.Controls.Add(headerPanel);

            // Info label
            infoLabel = new Label
            {
                Text = "Colorado Department of Education Transportation Reporting\n\n" +
                       "This form will generate the annual CDE-40 report required by September 15.\n" +
                       "The report includes:\n" +
                       "• Total miles driven\n" +
                       "• Number of pupils transported\n" +
                       "• Cost per student\n" +
                       "• Vehicle fleet information\n" +
                       "• Transportation funding analysis",
                Location = new Point(40, 120),
                Size = new Size(700, 200),
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };

            this.Controls.Add(infoLabel);

            // Generate report button
            generateReportButton = new SfButton
            {
                Text = "Generate CDE-40 Report",
                Size = new Size(200, 40),
                Location = new Point(40, 350),
                Style = { BackColor = ColorTranslator.FromHtml("#2196F3") }
            };
            generateReportButton.Click += GenerateReportButton_Click;

            // Close button
            closeButton = new SfButton
            {
                Text = "Close",
                Size = new Size(100, 40),
                Location = new Point(260, 350)
            };
            closeButton.Click += (s, e) => this.Close();

            this.Controls.Add(generateReportButton);
            this.Controls.Add(closeButton);
        }

        private void GenerateReportButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("CDE-40 report generation will be implemented in Phase 3.\n\n" +
                           "This will include:\n" +
                           "• Data extraction from database\n" +
                           "• Report formatting and validation\n" +
                           "• PDF export functionality\n" +
                           "• Email submission capabilities",
                           "CDE-40 Report Generation",
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Information);
        }
    }
}
