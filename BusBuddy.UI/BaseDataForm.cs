using System;
using System.Windows.Forms;
using BusBuddy.Business;

namespace BusBuddy.UI
{
    public class BaseDataForm : Form
    {
        protected readonly ErrorProvider _errorProvider;
        protected readonly DatabaseHelperService _databaseService;

        public BaseDataForm()
        {
            _errorProvider = new ErrorProvider();
            _databaseService = new DatabaseHelperService();

            // Set common form properties
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new System.Drawing.Size(800, 600);
        }

        protected Label CreateLabel(string text, int x, int y)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = new System.Drawing.Point(x, y);
            label.AutoSize = true;
            this.Controls.Add(label);
            return label;
        }

        protected TextBox CreateTextBox(int x, int y, int width = 200)
        {
            TextBox textBox = new TextBox();
            textBox.Location = new System.Drawing.Point(x, y);
            textBox.Size = new System.Drawing.Size(width, 23);
            this.Controls.Add(textBox);
            return textBox;
        }

        protected DateTimePicker CreateDatePicker(int x, int y, int width = 200)
        {
            DateTimePicker picker = new DateTimePicker();
            picker.Location = new System.Drawing.Point(x, y);
            picker.Size = new System.Drawing.Size(width, 23);
            picker.Format = DateTimePickerFormat.Short;
            this.Controls.Add(picker);
            return picker;
        }

        protected ComboBox CreateComboBox(int x, int y, int width = 200)
        {
            ComboBox comboBox = new ComboBox();
            comboBox.Location = new System.Drawing.Point(x, y);
            comboBox.Size = new System.Drawing.Size(width, 23);
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(comboBox);
            return comboBox;
        }

        protected Button CreateButton(string text, int x, int y, EventHandler? clickHandler = null)
        {
            Button button = new Button();
            button.Text = text;
            button.Location = new System.Drawing.Point(x, y);
            button.Size = new System.Drawing.Size(100, 30);
            if (clickHandler != null)
            {
                button.Click += clickHandler;
            }
            this.Controls.Add(button);
            return button;
        }

        protected DataGridView CreateDataGridView(int x, int y, int width, int height)
        {
            DataGridView grid = new DataGridView();
            grid.Location = new System.Drawing.Point(x, y);
            grid.Size = new System.Drawing.Size(width, height);
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Controls.Add(grid);
            return grid;
        }

        protected CheckBox CreateCheckBox(string text, int x, int y)
        {
            CheckBox checkBox = new CheckBox();
            checkBox.Text = text;
            checkBox.Location = new System.Drawing.Point(x, y);
            checkBox.AutoSize = true;
            this.Controls.Add(checkBox);
            return checkBox;
        }

        protected GroupBox CreateGroupBox(string text, int x, int y, int width, int height)
        {
            GroupBox groupBox = new GroupBox();
            groupBox.Text = text;
            groupBox.Location = new System.Drawing.Point(x, y);
            groupBox.Size = new System.Drawing.Size(width, height);
            this.Controls.Add(groupBox);
            return groupBox;
        }

        protected void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected void ShowWarningMessage(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        protected bool ConfirmDelete()
        {
            return MessageBox.Show("Are you sure you want to delete this record?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        protected bool ValidateForm()
        {
            // This should be overridden by derived classes to provide specific validation
            return true;
        }
    }
}
