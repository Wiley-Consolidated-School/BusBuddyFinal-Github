using System;
using System.Windows.Forms;
using System.Drawing;

namespace BusBuddy.UI
{
    /// <summary>
    /// Helper class for creating UI components for route management forms
    /// </summary>
    public static class RouteManagementFormHelpers
    {
        /// <summary>
        /// Creates a label with the specified parameters
        /// </summary>
        public static Label CreateLabel(string text, int x, int y, Control parent)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = new System.Drawing.Point(x, y);
            label.AutoSize = true;
            parent.Controls.Add(label);
            return label;
        }

        /// <summary>
        /// Creates a text box with the specified parameters
        /// </summary>
        public static TextBox CreateTextBox(int x, int y, int width, Control parent)
        {
            TextBox textBox = new TextBox();
            textBox.Location = new System.Drawing.Point(x, y);
            textBox.Size = new System.Drawing.Size(width, 23);
            parent.Controls.Add(textBox);
            return textBox;
        }

        /// <summary>
        /// Creates a date time picker with the specified parameters
        /// </summary>
        public static DateTimePicker CreateDatePicker(int x, int y, int width, Control parent)
        {
            DateTimePicker picker = new DateTimePicker();
            picker.Location = new System.Drawing.Point(x, y);
            picker.Size = new System.Drawing.Size(width, 23);
            picker.Format = DateTimePickerFormat.Short;
            parent.Controls.Add(picker);
            return picker;
        }

        /// <summary>
        /// Creates a combo box with the specified parameters
        /// </summary>
        public static ComboBox CreateComboBox(int x, int y, int width, Control parent)
        {
            ComboBox comboBox = new ComboBox();
            comboBox.Location = new System.Drawing.Point(x, y);
            comboBox.Size = new System.Drawing.Size(width, 23);
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            parent.Controls.Add(comboBox);
            return comboBox;
        }

        /// <summary>
        /// Creates a button with the specified parameters
        /// </summary>
        public static Button CreateButton(string text, int x, int y, EventHandler clickHandler, Control parent)
        {
            Button button = new Button();
            button.Text = text;
            button.Location = new System.Drawing.Point(x, y);
            button.Size = new System.Drawing.Size(100, 30);
            if (clickHandler != null)
            {
                button.Click += clickHandler;
            }
            parent.Controls.Add(button);
            return button;
        }
    }
}
