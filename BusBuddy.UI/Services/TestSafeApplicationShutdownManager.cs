using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.UI.Helpers;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Test-safe shutdown manager for BusBuddy.UI processes
    /// Prevents test host crashes by avoiding Environment.Exit() during testing
    /// </summary>
    public static class TestSafeApplicationShutdownManager
    {
        private static readonly object _shutdownLock = new object();
        private static bool _shutdownInProgress = false;
        private static readonly List<WeakReference> _trackedForms = new List<WeakReference>();
        private static bool _isTestEnvironment = false;

        /// <summary>
        /// Enable test mode to prevent Environment.Exit() calls
        /// </summary>
        public static void EnableTestMode()
        {
            _isTestEnvironment = true;
            Console.WriteLine("🧪 ApplicationShutdownManager: Test mode enabled - Environment.Exit() disabled");
        }

        /// <summary>
        /// Disable test mode to allow normal shutdown behavior
        /// </summary>
        public static void DisableTestMode()
        {
            _isTestEnvironment = false;
            Console.WriteLine("🔄 ApplicationShutdownManager: Test mode disabled - Normal shutdown enabled");
        }

        /// <summary>
        /// Manually suppress any active dialogs (for use during critical test operations)
        /// </summary>
        public static void SuppressActiveDialogs()
        {
            if (_isTestEnvironment)
            {
                // Simple dialog suppression - just log that we would suppress dialogs
                Console.WriteLine("🧪 ApplicationShutdownManager: Dialog suppression requested in test mode");
            }
        }

        /// <summary>
        /// Register a form for shutdown tracking
        /// </summary>
        public static void RegisterForm(Form form)
        {
            if (form == null) return;

            lock (_shutdownLock)
            {
                _trackedForms.Add(new WeakReference(form));
                Console.WriteLine($"📝 Registered form for shutdown: {form.GetType().Name}");
            }
        }

        /// <summary>
        /// Perform test-safe application shutdown
        /// </summary>
        public static void PerformShutdown()
        {
            lock (_shutdownLock)
            {
                if (_shutdownInProgress)
                {
                    Console.WriteLine("⚠️ Shutdown already in progress, skipping duplicate call");
                    return;
                }

                _shutdownInProgress = true;
            }

            try
            {
                Console.WriteLine($"🔥 ApplicationShutdownManager: Starting shutdown (Test mode: {_isTestEnvironment})...");

                // Step 1: Close all tracked forms
                CloseTrackedForms();

                // Step 2: Close all remaining application forms
                CloseAllApplicationForms();

                // Step 3: Force garbage collection
                ForceGarbageCollection();

                // Step 4: Only kill orphaned processes in production mode
                if (!_isTestEnvironment)
                {
                    KillOrphanedBusBuddyProcesses();
                }
                else
                {
                    Console.WriteLine("🧪 Test mode: Skipping orphaned process cleanup");
                }

                // Step 5: Exit application safely
                ExitApplicationSafely();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during shutdown: {ex.Message}");

                if (!_isTestEnvironment)
                {
                    // Only emergency exit in production mode
                    try
                    {
                        Console.WriteLine("🔥 Emergency application termination...");
                        Environment.Exit(1);
                    }
                    catch (Exception finalEx)
                    {
                        Console.WriteLine($"❌ Emergency termination failed: {finalEx.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("🧪 Test mode: Skipping emergency termination");
                }
            }
            finally
            {
                lock (_shutdownLock)
                {
                    _shutdownInProgress = false;
                }
            }
        }

        private static void CloseTrackedForms()
        {
            try
            {
                Console.WriteLine("🧽 Closing tracked forms...");

                var formsToClose = new List<Form>();

                lock (_shutdownLock)
                {
                    foreach (var weakRef in _trackedForms)
                    {
                        if (weakRef.Target is Form form && !form.IsDisposed)
                        {
                            formsToClose.Add(form);
                        }
                    }
                }

                foreach (var form in formsToClose)
                {
                    try
                    {
                        Console.WriteLine($"🧽 Closing tracked form: {form.GetType().Name}");
                        form.Close();
                        form.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error closing form {form.GetType().Name}: {ex.Message}");
                    }
                }

                Console.WriteLine("✅ Tracked forms closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error closing tracked forms: {ex.Message}");
            }
        }

        private static void CloseAllApplicationForms()
        {
            try
            {
                Console.WriteLine("🧽 Closing all remaining application forms...");

                var openForms = Application.OpenForms.Cast<Form>().ToList();
                Console.WriteLine($"🔍 Found {openForms.Count} open forms to close");

                foreach (var form in openForms)
                {
                    try
                    {
                        if (!form.IsDisposed)
                        {
                            form.Close();
                            form.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error closing form {form.GetType().Name}: {ex.Message}");
                    }
                }

                Console.WriteLine("✅ All application forms closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error closing application forms: {ex.Message}");
            }
        }

        private static void ForceGarbageCollection()
        {
            try
            {
                Console.WriteLine("🧽 Forcing garbage collection...");
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Console.WriteLine("✅ Garbage collection completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during garbage collection: {ex.Message}");
            }
        }

        private static void KillOrphanedBusBuddyProcesses()
        {
            try
            {
                Console.WriteLine("🔍 Checking for orphaned BusBuddy.UI processes...");

                var currentProcessId = Process.GetCurrentProcess().Id;
                var orphanedProcesses = Process.GetProcesses()
                    .Where(p => p.ProcessName.Contains("BusBuddy", StringComparison.OrdinalIgnoreCase))
                    .Where(p => p.Id != currentProcessId)
                    .ToList();

                if (orphanedProcesses.Count == 0)
                {
                    Console.WriteLine("✅ No orphaned BusBuddy.UI processes found");
                    return;
                }

                Console.WriteLine($"🗡️ Found {orphanedProcesses.Count} orphaned BusBuddy processes");

                foreach (var process in orphanedProcesses)
                {
                    try
                    {
                        // CRITICAL FIX: Validate process before attempting operations
                        if (process == null || process.HasExited)
                        {
                            Console.WriteLine("⚠️ Process already exited or null, skipping");
                            continue;
                        }

                        Console.WriteLine($"🗡️ Killing orphaned process: {process.ProcessName} (PID: {process.Id})");
                        process.Kill();

                        // Wait with proper validation
                        if (!process.WaitForExit(3000))
                        {
                            Console.WriteLine($"⚠️ Process {process.Id} did not exit within timeout");
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine($"⚠️ Process {process?.Id ?? -1} access error: {ex.Message}");
                        // This is expected if process already terminated
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Could not kill process {process?.Id ?? -1}: {ex.Message}");
                    }
                    finally
                    {
                        try
                        {
                            process?.Dispose();
                        }
                        catch
                        {
                            // Ignore disposal errors
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error checking for orphaned processes: {ex.Message}");
            }
        }

        private static void ExitApplicationSafely()
        {
            try
            {
                if (_isTestEnvironment)
                {
                    Console.WriteLine("🧪 Test mode: Skipping Application.Exit() and Environment.Exit()");
                    Console.WriteLine("✅ Test-safe shutdown completed");
                    return;
                }

                Console.WriteLine("🔥 Calling Application.Exit() to terminate all UI threads...");
                Application.Exit();

                // Wait a moment for Application.Exit() to work
                System.Threading.Thread.Sleep(2000);

                // If we're still running, use Environment.Exit() as backup
                Console.WriteLine("🔥 Calling Environment.Exit(0) as backup termination...");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during application exit: {ex.Message}");

                if (!_isTestEnvironment)
                {
                    try
                    {
                        Environment.Exit(1);
                    }
                    catch (Exception finalEx)
                    {
                        Console.WriteLine($"❌ Final exit attempt failed: {finalEx.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Get current shutdown status for diagnostics
        /// </summary>
        public static (bool InProgress, bool TestMode, int TrackedForms) GetStatus()
        {
            lock (_shutdownLock)
            {
                var aliveFormCount = _trackedForms.Count(wr => wr.Target != null && !((Form)wr.Target).IsDisposed);
                return (_shutdownInProgress, _isTestEnvironment, aliveFormCount);
            }
        }
    }
}

