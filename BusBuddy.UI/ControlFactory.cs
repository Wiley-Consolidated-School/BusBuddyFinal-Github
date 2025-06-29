using System.Drawing;
using System.Windows.Forms;

namespace BusBuddy.UI
{
    public static class ControlFactory
    {
        public static Button CreateButton(string text, int x, int y, int width, int height)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height)
            };
        }

        public static Button CreatePrimaryButton(string text, int x, int y, int width, int height)
        {
            var btn = CreateButton(text, x, y, width, height);
            btn.BackColor = Color.FromArgb(33, 150, 243); // Material Blue
            btn.ForeColor = Color.White;
            return btn;
        }

        public static Button CreateSecondaryButton(string text, int x, int y, int width, int height)
        {
            var btn = CreateButton(text, x, y, width, height);
            btn.BackColor = Color.FromArgb(224, 224, 224); // Light Gray
            btn.ForeColor = Color.Black;
            return btn;
        }

        public static Label CreateLabel(string text, int x, int y, int width, int height)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                AutoSize = false
            };
        }

        public static TextBox CreateTextBox(string text, int x, int y, int width, int height)
        {
            return new TextBox
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height)
            };
        }

        public static ComboBox CreateComboBox(string[] items, int x, int y, int width, int height, string placeholder = "")
        {
            var combo = new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            if (items != null)
                combo.Items.AddRange(items);
            // Windows Forms ComboBox does not support placeholder natively
            return combo;
        }

        public static ComboBox CreateStatusComboBox(string[] statuses, int x, int y, int width, int height)
        {
            return CreateComboBox(statuses, x, y, width, height, "Select Status");
        }

        public static DateTimePicker CreateDateTimePicker(int x, int y, int width, int height)
        {
            return new DateTimePicker
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                Format = DateTimePickerFormat.Short
            };
        }

        public static TextBox CreateSearchBox(string placeholder, int x, int y, int width, int height)
        {
            var box = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, height)
            };
            // Windows Forms TextBox does not support placeholder natively
            return box;
        }
    }
}
