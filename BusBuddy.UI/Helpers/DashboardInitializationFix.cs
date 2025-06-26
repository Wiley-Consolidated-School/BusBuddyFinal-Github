using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Helper class for managing Dashboard initialization and providing proper cancellation support.
    /// This addresses issues with asynchronous operations during Dashboard startup and shutdown.
    /// </summary>
    public static class DashboardInitializationFix
    {
        private static CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Creates a new CancellationTokenSource for Dashboard initialization.
        /// </summary>
        /// <returns>The CancellationTokenSource that can be used to cancel operations.</returns>
        public static CancellationTokenSource CreateCancellationTokenSource()
        {
            // Dispose any existing token source
            if (_cancellationTokenSource != null)
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                }
                _cancellationTokenSource.Dispose();
            }

            // Create a new token source
            _cancellationTokenSource = new CancellationTokenSource();
            return _cancellationTokenSource;
        }

        /// <summary>
        /// Cancels any pending initialization operations.
        /// </summary>
        public static void CancelInitialization()
        {
            try
            {
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                    Console.WriteLine("🛑 Dashboard initialization canceled");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error canceling initialization: {ex.Message}");
            }
        }

        /// <summary>
        /// Safely executes an asynchronous operation with cancellation support.
        /// </summary>
        /// <param name="action">The asynchronous action to execute.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task SafeExecuteAsync(Func<CancellationToken, Task> action)
        {
            if (_cancellationTokenSource == null)
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }

            try
            {
                await action(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("⚠️ Operation was canceled");
                // Operation canceled - this is expected behavior
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during dashboard initialization: {ex.Message}");
                // Log exception but don't rethrow to prevent application crashes
            }
        }

        /// <summary>
        /// Safely invokes an action on the UI thread.
        /// </summary>
        /// <param name="control">The control to invoke on. Can be null (operation will be skipped).</param>
        /// <param name="action">The action to execute on the UI thread.</param>
        public static void SafeInvokeOnUI(Control? control, Action action)
        {
            if (control == null || control.IsDisposed)
            {
                Console.WriteLine("⚠️ Cannot invoke on disposed control");
                return;
            }

            try
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(action);
                }
                else
                {
                    action();
                }
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("⚠️ Control was disposed during UI invocation");
                // Control was disposed during invocation - ignore
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"⚠️ Invalid operation during UI invocation: {ex.Message}");
                // Invalid operation - likely threading related
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during UI invocation: {ex.Message}");
                // Log exception but don't rethrow to prevent application crashes
            }
        }
    }
}
