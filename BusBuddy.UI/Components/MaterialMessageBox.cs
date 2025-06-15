using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Theme;

namespace BusBuddy.UI.Components
{
    /// <summary>
    /// Material Design dialog helper for consistent user feedback
    /// Replaces standard MessageBox with Material Design dialogs
    /// </summary>
    public static class MaterialMessageBox
    {
        /// <summary>
        /// Show a Material Design message dialog
        /// </summary>
        public static DialogResult Show(IWin32Window owner, string message, string title = "Information",
            MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            using (var dialog = new MaterialMessageDialog(message, title, buttons, icon))
            {
                return dialog.ShowDialog(owner);
            }
        }

        /// <summary>
        /// Show a Material Design confirmation dialog
        /// </summary>
        public static DialogResult ShowConfirmation(IWin32Window owner, string message, string title = "Confirm")
        {
            return Show(owner, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        /// <summary>
        /// Show a Material Design error dialog
        /// </summary>
        public static DialogResult ShowError(IWin32Window owner, string message, string title = "Error")
        {
            return Show(owner, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Show a Material Design warning dialog
        /// </summary>
        public static DialogResult ShowWarning(IWin32Window owner, string message, string title = "Warning")
        {
            return Show(owner, message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Show a Material Design success dialog
        /// </summary>
        public static DialogResult ShowSuccess(IWin32Window owner, string message, string title = "Success")
        {
            return Show(owner, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    /// <summary>
    /// Material Design message dialog implementation
    /// </summary>
    internal class MaterialMessageDialog : StandardMaterialForm
    {
        private readonly string _message;
        private readonly string _title;
        private readonly MessageBoxButtons _buttons;
        private readonly MessageBoxIcon _icon;

        private MaterialCard _contentCard;
        private MaterialLabel _messageLabel;
        private Panel _buttonPanel;
        private DialogResult _result = DialogResult.Cancel;

        public MaterialMessageDialog(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            _message = message;
            _title = title;
            _buttons = buttons;
            _icon = icon;

            InitializeDialog();
        }

        private void InitializeDialog()
        {
            this.SuspendLayout();

            // Configure dialog properties
            this.Text = _title;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Set size based on content
            var scaleFactor = DpiScaleHelper.GetControlScaleFactor(this);
            this.Size = new Size(
                DpiScaleHelper.ScaleSize(400, scaleFactor),
                DpiScaleHelper.ScaleSize(200, scaleFactor)
            );

            CreateContent();
            CreateButtons();

            this.ResumeLayout(false);
        }

        private void CreateContent()
        {
            _contentCard = new MaterialCard
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                Margin = new Padding(8),
                BackColor = MaterialDesignThemeManager.DarkTheme.Surface
            };

            var contentLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // Icon
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Message
            contentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Add icon if specified
            if (_icon != MessageBoxIcon.None)
            {
                var iconLabel = CreateIconLabel();
                contentLayout.Controls.Add(iconLabel, 0, 0);
            }

            // Message label
            _messageLabel = new MaterialLabel
            {
                Text = _message,
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(_icon != MessageBoxIcon.None ? 16 : 0, 0, 0, 0),
                Font = new System.Drawing.Font("Roboto", 10, System.Drawing.FontStyle.Regular)
            };

            contentLayout.Controls.Add(_messageLabel, _icon != MessageBoxIcon.None ? 1 : 0, 0);
            if (_icon == MessageBoxIcon.None)
            {
                contentLayout.SetColumnSpan(_messageLabel, 2);
            }

            _contentCard.Controls.Add(contentLayout);
            this.Controls.Add(_contentCard);
        }

        private MaterialLabel CreateIconLabel()
        {
            var iconText = _icon switch
            {
                MessageBoxIcon.Information => "ℹ️",
                MessageBoxIcon.Warning => "⚠️",
                MessageBoxIcon.Error => "❌",
                MessageBoxIcon.Question => "❓",
                _ => ""
            };

            var iconColor = _icon switch
            {
                MessageBoxIcon.Information => Color.DodgerBlue,
                MessageBoxIcon.Warning => Color.Orange,
                MessageBoxIcon.Error => Color.Red,
                MessageBoxIcon.Question => Color.Green,
                _ => MaterialDesignThemeManager.DarkTheme.OnSurface
            };

            return new MaterialLabel
            {
                Text = iconText,
                ForeColor = iconColor,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 16, 0),
                Font = new System.Drawing.Font("Roboto", 24, System.Drawing.FontStyle.Regular)
            };
        }

        private void CreateButtons()
        {
            _buttonPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Bottom,
                BackColor = Color.Transparent,
                Padding = new Padding(24, 12, 24, 12)
            };

            var buttonLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true
            };

            switch (_buttons)
            {
                case MessageBoxButtons.OK:
                    AddButton("OK", DialogResult.OK, true);
                    break;

                case MessageBoxButtons.OKCancel:
                    AddButton("Cancel", DialogResult.Cancel, false);
                    AddButton("OK", DialogResult.OK, true);
                    break;

                case MessageBoxButtons.YesNo:
                    AddButton("No", DialogResult.No, false);
                    AddButton("Yes", DialogResult.Yes, true);
                    break;

                case MessageBoxButtons.YesNoCancel:
                    AddButton("Cancel", DialogResult.Cancel, false);
                    AddButton("No", DialogResult.No, false);
                    AddButton("Yes", DialogResult.Yes, true);
                    break;

                case MessageBoxButtons.RetryCancel:
                    AddButton("Cancel", DialogResult.Cancel, false);
                    AddButton("Retry", DialogResult.Retry, true);
                    break;

                case MessageBoxButtons.AbortRetryIgnore:
                    AddButton("Ignore", DialogResult.Ignore, false);
                    AddButton("Retry", DialogResult.Retry, false);
                    AddButton("Abort", DialogResult.Abort, true);
                    break;
            }

            _buttonPanel.Controls.Add(buttonLayout);
            this.Controls.Add(_buttonPanel);

            void AddButton(string text, DialogResult result, bool isDefault)
            {
                var button = new MaterialButton
                {
                    Text = text,
                    Size = new Size(100, 36),
                    UseAccentColor = isDefault,
                    Type = isDefault ? MaterialButton.MaterialButtonType.Contained : MaterialButton.MaterialButtonType.Outlined,
                    AutoSize = false,
                    Margin = new Padding(8, 0, 0, 0)
                };

                button.Click += (s, e) =>
                {
                    _result = result;
                    this.DialogResult = result;
                    this.Close();
                };

                if (isDefault)
                {
                    this.AcceptButton = button;
                }

                buttonLayout.Controls.Add(button);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Handle Escape key
            if (e.KeyCode == Keys.Escape)
            {
                _result = DialogResult.Cancel;
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        public new DialogResult ShowDialog(IWin32Window owner)
        {
            base.ShowDialog(owner);
            return _result;
        }
    }

    /// <summary>
    /// Material Design loading dialog for long-running operations
    /// </summary>
    public class MaterialLoadingDialog : StandardMaterialForm
    {
        private MaterialCard _contentCard;
        private MaterialLabel _messageLabel;
        private MaterialProgressBar _progressBar;
        private string _message;

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                if (_messageLabel != null)
                    _messageLabel.Text = value;
            }
        }

        public MaterialLoadingDialog(string message = "Loading...")
        {
            _message = message;
            InitializeDialog();
        }

        private void InitializeDialog()
        {
            this.SuspendLayout();

            // Configure dialog properties
            this.Text = "Please Wait";
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ControlBox = false;

            // Set size
            var scaleFactor = DpiScaleHelper.GetControlScaleFactor(this);
            this.Size = new Size(
                DpiScaleHelper.ScaleSize(300, scaleFactor),
                DpiScaleHelper.ScaleSize(120, scaleFactor)
            );

            CreateContent();

            this.ResumeLayout(false);
        }

        private void CreateContent()
        {
            _contentCard = new MaterialCard
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                Margin = new Padding(8),
                BackColor = MaterialDesignThemeManager.DarkTheme.Surface
            };

            var contentLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };

            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            contentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70F)); // Message
            contentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F)); // Progress bar

            // Message label
            _messageLabel = new MaterialLabel
            {
                Text = _message,
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Roboto", 10, System.Drawing.FontStyle.Regular)
            };

            // Progress bar
            _progressBar = new MaterialProgressBar
            {
                Dock = DockStyle.Fill,
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30
            };

            contentLayout.Controls.Add(_messageLabel, 0, 0);
            contentLayout.Controls.Add(_progressBar, 0, 1);

            _contentCard.Controls.Add(contentLayout);
            this.Controls.Add(_contentCard);
        }

        /// <summary>
        /// Update the progress message
        /// </summary>
        public void UpdateMessage(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(UpdateMessage), message);
                return;
            }

            Message = message;
        }

        /// <summary>
        /// Close the loading dialog
        /// </summary>
        public void CloseDialog()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(CloseDialog));
                return;
            }

            this.Close();
        }
    }
}
