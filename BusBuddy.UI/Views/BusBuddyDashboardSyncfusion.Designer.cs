namespace BusBuddy.UI.Views
{
    partial class BusBuddyDashboardSyncfusion
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Track disposal state for test validation
        private bool _disposed = false;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            _disposed = true;
            base.Dispose(disposing);
        }

        /// <summary>
        /// Helper method to check if disposed (for tests)
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new System.ObjectDisposedException(GetType().Name);
            }
        }
          /// <summary>
        /// Override property access to check disposal state
        /// </summary>
        public new string Text
        {
            get
            {
                ThrowIfDisposed();
                return base.Text;
            }
            set
            {
                ThrowIfDisposed();
                base.Text = value;
            }
        }

        /// <summary>
        /// Override Show method to check disposal state
        /// </summary>
        public new void Show()
        {
            ThrowIfDisposed();
            base.Show();
        }

        /// <summary>
        /// Override other commonly accessed properties
        /// </summary>
        public new Size Size
        {
            get
            {
                ThrowIfDisposed();
                return base.Size;
            }
            set
            {
                ThrowIfDisposed();
                base.Size = value;
            }
        }
    }
}
