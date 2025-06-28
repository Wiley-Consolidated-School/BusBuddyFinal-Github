using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Comprehensive shutdown manager for BusBuddy.UI processes
    /// Ensures all .NET instances are properly terminated when the application closes
    /// </summary>
    public static class ApplicationShutdownManager
    {
        private static readonly object _shutdownLock = new object();
        private static bool _shutdownInProgress = false;
        private static readonly List<WeakReference> _trackedForms = new List<WeakReference>();

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
        /// Perform comprehensive application shutdown to terminate all BusBuddy.UI processes
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
                Console.WriteLine("🔥 ApplicationShutdownManager: Starting comprehensive shutdown...");

                // Step 1: Close all tracked forms
                CloseTrackedForms();

                // Step 2: Close all remaining application forms
                CloseAllApplicationForms();

                // Step 3: Force garbage collection
                ForceGarbageCollection();

                // Step 4: Kill any orphaned BusBuddy.UI processes
                KillOrphanedBusBuddyProcesses();

                // Step 5: Exit the application
                ExitApplication();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during comprehensive shutdown: {ex.Message}");

                // Emergency termination
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
        }

        /// <summary>
        /// Close all tracked forms
        /// </summary>
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
                    _trackedForms.Clear();
                }

                foreach (var form in formsToClose)
                {
                    try
                    {
                        if (!form.IsDisposed)
                        {
                            Console.WriteLine($"🧽 Closing tracked form: {form.GetType().Name}");
                            form.Close();
                            form.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error closing tracked form {form.GetType().Name}: {ex.Message}");
                    }
                }

                Console.WriteLine("✅ Tracked forms closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error in CloseTrackedForms: {ex.Message}");
            }
        }

        /// <summary>
        /// Close all remaining application forms
        /// </summary>
        private static void CloseAllApplicationForms()
        {
            try
            {
                Console.WriteLine("🧽 Closing all remaining application forms...");

                var openForms = new List<Form>();
                foreach (Form form in Application.OpenForms)
                {
                    openForms.Add(form);
                }

                Console.WriteLine($"🧽 Found {openForms.Count} open forms to close");

                foreach (var form in openForms)
                {
                    try
                    {
                        if (form != null && !form.IsDisposed)
                        {
                            Console.WriteLine($"🧽 Closing form: {form.GetType().Name}");

                            // Hide first to prevent visual artifacts
                            if (form.Visible)
                            {
                                form.Hide();
                            }

                            form.Close();
                            form.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error closing form {form?.GetType().Name}: {ex.Message}");
                    }
                }

                Console.WriteLine("✅ All application forms closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error in CloseAllApplicationForms: {ex.Message}");
            }
        }

        /// <summary>
        /// Force garbage collection to cleanup resources
        /// </summary>
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
                Console.WriteLine($"⚠️ Error during garbage collection: {ex.Message}");
            }
        }

        /// <summary>
        /// Kill any orphaned BusBuddy.UI processes that might be lingering
        /// FIXED: Added proper process validation to prevent InvalidOperationException
        /// </summary>
        private static void KillOrphanedBusBuddyProcesses()
        {
            try
            {
                Console.WriteLine("🔥 Checking for orphaned BusBuddy.UI processes...");

                var currentProcessId = Process.GetCurrentProcess().Id;
                var busBuddyProcesses = Process.GetProcessesByName("BusBuddy.UI")
                    .Where(p => p.Id != currentProcessId) // Don't kill the current process
                    .ToList();

                if (busBuddyProcesses.Any())
                {
                    Console.WriteLine($"🔥 Found {busBuddyProcesses.Count} orphaned BusBuddy.UI processes");

                    foreach (var process in busBuddyProcesses)
                    {
                        try
                        {
                            // CRITICAL FIX: Validate process before attempting operations
                            if (process == null || process.HasExited)
                            {
                                Console.WriteLine("⚠️ Process already exited or null, skipping");
                                continue;
                            }

                            Console.WriteLine($"🔥 Killing orphaned process: PID {process.Id}");
                            process.Kill();

                            // Wait with proper timeout and validation
                            if (!process.WaitForExit(2000)) // Wait up to 2 seconds
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
                            Console.WriteLine($"⚠️ Error killing process {process?.Id ?? -1}: {ex.Message}");
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
                else
                {
                    Console.WriteLine("✅ No orphaned BusBuddy.UI processes found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error checking for orphaned processes: {ex.Message}");
            }
        }

        /// <summary>
        /// Exit the application with proper cleanup
        /// </summary>
        private static void ExitApplication()
        {
            try
            {
                Console.WriteLine("🔥 Calling Application.Exit() to terminate all UI threads...");
                Application.Exit();

                // Give Application.Exit() a moment to work
                System.Threading.Thread.Sleep(1000);

                // If we're still running, use Environment.Exit() as backup
                Console.WriteLine("🔥 Calling Environment.Exit(0) as backup termination...");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during application exit: {ex.Message}");

                // Final emergency termination
                try
                {
                    Console.WriteLine("🔥 Final emergency termination...");
                    Environment.Exit(1);
                }
                catch (Exception finalEx)
                {
                    Console.WriteLine($"❌ Final emergency termination failed: {finalEx.Message}");
                }
            }
        }

        /// <summary>
        /// Check if shutdown is in progress
        /// </summary>
        public static bool IsShutdownInProgress
        {
            get
            {
                lock (_shutdownLock)
                {
                    return _shutdownInProgress;
                }
            }
        }
    }
}

