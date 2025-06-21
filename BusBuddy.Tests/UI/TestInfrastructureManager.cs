using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BusBuddy.Tests.UI
{
    /// <summary>
    /// Comprehensive test infrastructure manager for robust test execution
    /// Handles process cleanup, memory management, and test isolation
    /// </summary>
    public static class TestInfrastructureManager
    {
        private static readonly object _lock = new object();
        private static readonly List<WeakReference> _testFormReferences = new List<WeakReference>();
        private static readonly List<IDisposable> _testResources = new List<IDisposable>();
        private static System.Threading.Timer? _memoryCleanupTimer;
        private static volatile bool _cleanupInProgress = false;

        /// <summary>
        /// Initialize test infrastructure with automatic cleanup
        /// </summary>
        public static void Initialize()
        {
            lock (_lock)
            {
                if (_memoryCleanupTimer != null)
                    return;

                Console.WriteLine("🔧 Initializing test infrastructure...");                // Setup periodic memory cleanup
                _memoryCleanupTimer = new System.Threading.Timer(PeriodicCleanup, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

                // Setup application exit cleanup
                AppDomain.CurrentDomain.ProcessExit += (s, e) => Shutdown();

                Console.WriteLine("✅ Test infrastructure initialized");
            }
        }

        /// <summary>
        /// Register a form for automatic cleanup tracking
        /// </summary>
        public static void RegisterTestForm(Form form)
        {
            if (form == null) return;

            lock (_lock)
            {
                _testFormReferences.Add(new WeakReference(form));
                Console.WriteLine($"📝 Registered test form: {form.GetType().Name}");
            }
        }

        /// <summary>
        /// Register a disposable resource for cleanup
        /// </summary>
        public static void RegisterTestResource(IDisposable resource)
        {
            if (resource == null) return;

            lock (_lock)
            {
                _testResources.Add(resource);
                Console.WriteLine($"📝 Registered test resource: {resource.GetType().Name}");
            }
        }

        /// <summary>
        /// Force cleanup of all test resources and orphaned processes
        /// </summary>
        public static void ForceCleanup()
        {
            if (_cleanupInProgress) return;

            lock (_lock)
            {
                _cleanupInProgress = true;
                Console.WriteLine("🧹 Starting force cleanup...");

                try
                {
                    // Kill orphaned dotnet processes
                    KillOrphanedDotnetProcesses();

                    // Cleanup test forms
                    CleanupTestForms();

                    // Cleanup test resources
                    CleanupTestResources();

                    // Force garbage collection
                    ForceGarbageCollection();

                    Console.WriteLine("✅ Force cleanup completed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error during force cleanup: {ex.Message}");
                }
                finally
                {
                    _cleanupInProgress = false;
                }
            }
        }

        /// <summary>
        /// Cleanup before each test to ensure clean state
        /// </summary>
        public static void PreTestCleanup()
        {
            Console.WriteLine("🧹 Pre-test cleanup starting...");

            try
            {
                // Close any open forms from previous tests
                CloseOpenTestForms();

                // Process pending Windows messages
                ProcessPendingMessages();

                // Check for memory pressure
                CheckMemoryPressure();

                Console.WriteLine("✅ Pre-test cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Pre-test cleanup error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cleanup after each test
        /// </summary>
        public static void PostTestCleanup()
        {
            Console.WriteLine("🧹 Post-test cleanup starting...");

            try
            {
                // Close any forms opened during test
                CloseOpenTestForms();

                // Process pending messages
                ProcessPendingMessages();

                // Clean up weak references
                CleanupWeakReferences();

                Console.WriteLine("✅ Post-test cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Post-test cleanup error: {ex.Message}");
                // Don't throw - tests should complete even if cleanup fails
            }
        }

        /// <summary>
        /// Shutdown test infrastructure with enhanced Syncfusion control disposal
        /// </summary>
        public static void Shutdown()
        {
            lock (_lock)
            {
                Console.WriteLine("🔧 Shutting down test infrastructure...");

                try
                {
                    // Stop the cleanup timer first
                    _memoryCleanupTimer?.Dispose();
                    _memoryCleanupTimer = null;

                    // Enhanced cleanup for Syncfusion controls
                    Console.WriteLine("🔧 Starting enhanced Syncfusion cleanup...");

                    // Close all open forms with special Syncfusion handling
                    CloseAllFormsWithSyncfusionHandling();

                    // Process messages to allow disposal events to complete
                    ProcessPendingMessages();

                    // Force cleanup of remaining resources
                    ForceCleanup();

                    // Final garbage collection to clean up any remaining Syncfusion resources
                    Console.WriteLine("🗑️ Final garbage collection for Syncfusion cleanup...");
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    Console.WriteLine("✅ Test infrastructure shutdown completed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error during test infrastructure shutdown: {ex.Message}");
                    // Continue with shutdown even if there are errors
                }
            }
        }

        /// <summary>
        /// Close all forms with special handling for Syncfusion controls
        /// </summary>
        private static void CloseAllFormsWithSyncfusionHandling()
        {
            try
            {
                // Get all open forms
                var allForms = new List<Form>();
                foreach (Form form in Application.OpenForms)
                {
                    allForms.Add(form);
                }

                Console.WriteLine($"🔧 Closing {allForms.Count} open forms with Syncfusion handling...");

                // Close forms in reverse order (child forms first)
                for (int i = allForms.Count - 1; i >= 0; i--)
                {
                    var form = allForms[i];
                    if (form != null && !form.IsDisposed)
                    {
                        try
                        {
                            // Special Syncfusion disposal before closing
                            DisposeSyncfusionControlsSafely(form);

                            // Hide first to prevent visual glitches
                            if (form.Visible)
                                form.Hide();

                            // Close and dispose
                            form.Close();
                            form.Dispose();

                            Console.WriteLine($"✅ Closed form: {form.GetType().Name}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error closing form {form.GetType().Name}: {ex.Message}");
                        }
                    }
                }

                // Process any remaining messages
                ProcessPendingMessages();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in CloseAllFormsWithSyncfusionHandling: {ex.Message}");
            }
        }

        #region Private Methods

        private static void PeriodicCleanup(object? state)
        {
            if (_cleanupInProgress) return;

            try
            {
                CleanupWeakReferences();

                // Light memory cleanup every 30 seconds
                if (GC.GetTotalMemory(false) > 100 * 1024 * 1024) // 100MB threshold
                {
                    GC.Collect(0, GCCollectionMode.Optimized);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Periodic cleanup error: {ex.Message}");
            }
        }        private static void KillOrphanedDotnetProcesses()
        {
            try
            {
                // Skip process cleanup during active test runs to avoid killing test host
                var isTestRunning = System.Diagnostics.Process.GetCurrentProcess().ProcessName.Contains("testhost");
                if (isTestRunning)
                {
                    Console.WriteLine("⚠️ Skipping process cleanup during test execution");
                    return;
                }

                var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                var dotnetProcesses = Process.GetProcessesByName("dotnet")
                    .Where(p => p.Id != currentProcess.Id)
                    .Where(p => DateTime.Now - p.StartTime > TimeSpan.FromMinutes(10)) // Only very old processes
                    .ToList();

                if (dotnetProcesses.Count > 0)
                {
                    Console.WriteLine($"🔫 Found {dotnetProcesses.Count} old orphaned dotnet processes");

                    foreach (var process in dotnetProcesses)
                    {
                        try
                        {
                            Console.WriteLine($"🔫 Killing old orphaned process: {process.Id} (Age: {DateTime.Now - process.StartTime})");
                            process.Kill();
                            process.WaitForExit(1000);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Failed to kill process {process.Id}: {ex.Message}");
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error killing orphaned processes: {ex.Message}");
            }
        }

        private static void CleanupTestForms()
        {
            lock (_lock)
            {
                var formsToDispose = new List<Form>();

                foreach (var weakRef in _testFormReferences.ToList())
                {
                    if (weakRef.Target is Form form && !form.IsDisposed)
                    {
                        formsToDispose.Add(form);
                    }
                }

                foreach (var form in formsToDispose)
                {
                    try
                    {
                        if (form.InvokeRequired)
                        {
                            form.Invoke(new Action(() => SafeDisposeForm(form)));
                        }
                        else
                        {
                            SafeDisposeForm(form);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error disposing form {form.GetType().Name}: {ex.Message}");
                    }
                }

                _testFormReferences.Clear();
            }
        }

        private static void SafeDisposeForm(Form form)
        {
            try
            {
                if (form == null || form.IsDisposed)
                    return;

                // Close any child forms first
                var childForms = new List<Form>();
                foreach (Form child in Application.OpenForms)
                {
                    if (child != form && child.Owner == form)
                        childForms.Add(child);
                }

                foreach (var child in childForms)
                {
                    SafeDisposeForm(child);
                }

                // Special handling for Syncfusion controls to prevent disposal crashes
                DisposeSyncfusionControlsSafely(form);

                // Close and dispose
                if (!form.IsDisposed)
                {
                    if (form.Visible)
                        form.Hide();
                    form.Close();
                    form.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error in SafeDisposeForm: {ex.Message}");
            }
        }

        /// <summary>
        /// Safely dispose Syncfusion controls to prevent NullReferenceException during test shutdown
        /// </summary>
        private static void DisposeSyncfusionControlsSafely(Control parent)
        {
            try
            {
                var controlsToDispose = new List<Control>();
                CollectSyncfusionControls(parent, controlsToDispose);

                // Dispose Syncfusion controls in reverse order (children first)
                for (int i = controlsToDispose.Count - 1; i >= 0; i--)
                {
                    var control = controlsToDispose[i];
                    try
                    {
                        if (control != null && !control.IsDisposed)
                        {
                            // Special handling for different Syncfusion control types
                            var controlType = control.GetType().FullName;

                            if (controlType?.Contains("Syncfusion.WinForms.ListView.SfListView") == true)
                            {
                                // SfListView needs special disposal to prevent the UnWiredEvents crash
                                DisposeSfListViewSafely(control);
                            }
                            else if (controlType?.Contains("Syncfusion.WinForms.DataGrid.SfDataGrid") == true)
                            {
                                DisposeSfDataGridSafely(control);
                            }
                            else if (controlType?.Contains("Syncfusion") == true)
                            {
                                // Generic Syncfusion control disposal
                                control.Dispose();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error disposing Syncfusion control {control?.GetType().Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error in DisposeSyncfusionControlsSafely: {ex.Message}");
            }
        }

        private static void CollectSyncfusionControls(Control parent, List<Control> controlsToDispose)
        {
            try
            {
                foreach (Control control in parent.Controls)
                {
                    if (control.GetType().FullName?.Contains("Syncfusion") == true)
                    {
                        controlsToDispose.Add(control);
                    }

                    // Recursively check child controls
                    if (control.HasChildren)
                    {
                        CollectSyncfusionControls(control, controlsToDispose);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error collecting Syncfusion controls: {ex.Message}");
            }
        }

        private static void DisposeSfListViewSafely(Control listView)
        {
            try
            {
                // Clear any data source first to prevent events during disposal
                var dataSourceProperty = listView.GetType().GetProperty("DataSource");
                if (dataSourceProperty != null && dataSourceProperty.CanWrite)
                {
                    dataSourceProperty.SetValue(listView, null);
                }

                // Clear any items collection
                var itemsProperty = listView.GetType().GetProperty("Items");
                if (itemsProperty != null)
                {
                    var items = itemsProperty.GetValue(listView);
                    var clearMethod = items?.GetType().GetMethod("Clear");
                    clearMethod?.Invoke(items, null);
                }

                // Dispose the control
                listView.Dispose();
                Console.WriteLine("✅ SfListView disposed safely");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error disposing SfListView safely: {ex.Message}");
                // Try standard disposal as fallback
                try { listView.Dispose(); } catch { }
            }
        }

        private static void DisposeSfDataGridSafely(Control dataGrid)
        {
            try
            {
                // Clear any data source first
                var dataSourceProperty = dataGrid.GetType().GetProperty("DataSource");
                if (dataSourceProperty != null && dataSourceProperty.CanWrite)
                {
                    dataSourceProperty.SetValue(dataGrid, null);
                }

                // Dispose the control
                dataGrid.Dispose();
                Console.WriteLine("✅ SfDataGrid disposed safely");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error disposing SfDataGrid safely: {ex.Message}");
                // Try standard disposal as fallback
                try { dataGrid.Dispose(); } catch { }
            }
        }

        private static void CleanupTestResources()
        {
            lock (_lock)
            {
                foreach (var resource in _testResources.ToList())
                {
                    try
                    {
                        resource?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error disposing resource: {ex.Message}");
                    }
                }

                _testResources.Clear();
            }
        }        private static void CloseOpenTestForms()
        {
            try
            {
                var openForms = new List<Form>();
                foreach (Form form in Application.OpenForms)
                {
                    openForms.Add(form);
                }

                foreach (var form in openForms)
                {
                    if (form != null && !form.IsDisposed)
                    {
                        try
                        {
                            form.Close();
                            form.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error closing form {form.GetType().Name}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error closing open forms: {ex.Message}");
            }
        }

        private static void ProcessPendingMessages()
        {
            try
            {
                for (int i = 0; i < 10; i++) // Process up to 10 message pumps
                {
                    Application.DoEvents();
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error processing pending messages: {ex.Message}");
            }
        }

        private static void CleanupWeakReferences()
        {
            lock (_lock)
            {
                _testFormReferences.RemoveAll(wr => !wr.IsAlive);
            }
        }

        private static void CheckMemoryPressure()
        {
            var totalMemory = GC.GetTotalMemory(false);
            var memoryMB = totalMemory / 1024 / 1024;

            if (memoryMB > 200) // 200MB threshold
            {
                Console.WriteLine($"⚠️ High memory usage detected: {memoryMB}MB - forcing cleanup");
                ForceGarbageCollection();
            }
        }

        private static void ForceGarbageCollection()
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Console.WriteLine("🗑️ Forced garbage collection completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during garbage collection: {ex.Message}");
            }
        }

        #endregion
    }
}
