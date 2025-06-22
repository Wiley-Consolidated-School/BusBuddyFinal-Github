namespace BusBuddy.UI.Views
{
    partial class BusBuddyDashboardSyncfusion
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                try
                {
                    LogInfo("Disposing BusBuddyDashboardSyncfusion...");

                    // Log current resources for debugging
                    LogCurrentResources();

                    // Cancel any background operations
                    if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                    {
                        _cancellationTokenSource.Cancel();
                    }

                    // 1. First dispose UI components and release event handlers
                    // Clear event handlers first to prevent callbacks during disposal
                    ClearEventHandlers();

                    // 2. Dispose Syncfusion controls with specific disposal needs
                    DisposeSyncfusionControls();
                    DisposeAnalyticsComponents();
                    DisposeDockingManager();

                    // 3. Dispose remaining panels and controls
                    DisposePanelControls();

                    // 4. Finally cleanup service connections and resources
                    CleanupRepositoryConnections();

                    // Dispose standard components
                    if (components != null)
                    {
                        components.Dispose();
                        components = null;
                    }

                    // Proper disposal instead of aggressive cleanup
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;

                    // Clear dictionary references
                    _navigationMethods?.Clear();
                    _navigationMethods = null;
                    _repositoryTypeMap?.Clear();
                    _repositoryTypeMap = null;

                    // Mark as disposed
                    _disposed = true;

                    LogInfo("BusBuddyDashboardSyncfusion disposed successfully");
                }
                catch (Exception ex)
                {
                    // Log but don't rethrow to ensure base.Dispose() is called
                    LogError("Error during dashboard disposal", ex);
                }
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            //
            // BusBuddyDashboardSyncfusion
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "BusBuddyDashboardSyncfusion";
            this.Text = "BusBuddy Dashboard";
            this.ResumeLayout(false);
        }

        #endregion
    }
}
