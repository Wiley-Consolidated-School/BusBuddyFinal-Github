using System;
using System.Windows.Forms;

namespace BusBuddy.Test
{
    public class TestForm : Form
    {
        public TestForm()
        {
            this.Text = "Test Form";
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label testLabel = new Label();
            testLabel.Text = "If you can see this, Windows Forms is working!";
            testLabel.AutoSize = true;
            testLabel.Location = new System.Drawing.Point(50, 50);
            this.Controls.Add(testLabel);
        }
    }

    internal static class TestProgram
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                MessageBox.Show("Starting test form...", "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);

                var testForm = new TestForm();
                Application.Run(testForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\nStack: {ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
