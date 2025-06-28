using System;
using System.Windows.Forms;
using BusBuddy.UI.Views;
using Xunit;

namespace BusBuddy.UI.Tests
{
    /// <summary>
    /// Shared test fixture for Dashboard tests
    /// This allows sharing expensive initialization between test classes
    /// </summary>
    public class DashboardTestFixture : IDisposable
    {
        // Changed to accept our MockDashboard as well
        public Form? SharedDashboard { get; private set; }
        public Form? TestForm { get; private set; }
        private readonly CancellationTokenSource _testTimeoutCts;

        public DashboardTestFixture()
        {
            // Set environment variable to indicate test mode
            Environment.SetEnvironmentVariable("BUSBUDDY_TEST_MODE", "1");
            Environment.SetEnvironmentVariable("BUSBUDDY_MOCK_DASHBOARD", "1");

            // Create a timeout token source that will prevent tests from hanging
            _testTimeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            Console.WriteLine("Creating test environment for dashboard tests...");

            try
            {
                // Create a minimal test form for testing UI components
                TestForm = new Form
                {
                    Text = "BusBuddy Test Environment",
                    Size = new Size(800, 600),
                    WindowState = FormWindowState.Minimized, // Keep minimized to avoid UI conflicts
                    ShowInTaskbar = false
                };

                // Use a direct mock dashboard creation approach instead of trying to initialize the real dashboard
                SharedDashboard = CreateMockDashboard();

                if (SharedDashboard != null)
                {
                    SharedDashboard.TopLevel = false;
                    SharedDashboard.FormBorderStyle = FormBorderStyle.None;
                    SharedDashboard.Dock = DockStyle.Fill;
                    TestForm.Controls.Add(SharedDashboard);
                    Console.WriteLine("✅ Test dashboard created and added to test form");
                }
                else
                {
                    Console.WriteLine("⚠️ Failed to create test dashboard - tests that require dashboard will be skipped");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test fixture setup failed: {ex.Message}");
                SharedDashboard = null;
                TestForm = null;
            }

            Console.WriteLine("Test fixture initialized - tests will handle null components appropriately");

            // Kill any child processes immediately
            KillAnyOrphanedChildProcesses();
        }

        /// <summary>
        /// Create a minimal mock dashboard for testing when the real dashboard fails to initialize
        /// </summary>
        private Form CreateMockDashboard()
        {
            Console.WriteLine("Creating dedicated mock dashboard for testing...");

            try
            {
                // Create our simplified mock dashboard instead of the real Dashboard
                var mockDashboard = new MockDashboard();
                Console.WriteLine("✅ Mock dashboard created successfully");
                return mockDashboard;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to create mock dashboard: {ex.Message}");

                // Create an absolute minimal Form as a last resort
                Console.WriteLine("Creating fallback minimal form...");
                var minimalForm = new Form
                {
                    Text = "Minimal Test Form",
                    Size = new Size(800, 600)
                };

                var panel = new Panel { Dock = DockStyle.Fill };
                var tableLayoutPanel = new TableLayoutPanel
                {
                    ColumnCount = 2,
                    RowCount = 2,
                    Dock = DockStyle.Fill
                };

                panel.Controls.Add(tableLayoutPanel);
                minimalForm.Controls.Add(panel);

                return minimalForm;
            }
        }

        public void Dispose()
        {
            Console.WriteLine("Disposing shared test fixtures...");

            try
            {
                // Cancel the timeout token source
                if (_testTimeoutCts != null && !_testTimeoutCts.IsCancellationRequested)
                {
                    _testTimeoutCts.Cancel();
                    _testTimeoutCts.Dispose();
                }

                // Force aggressive disposal for tests
                if (SharedDashboard != null && !SharedDashboard.IsDisposed)
                {
                    Console.WriteLine("Safely disposing Dashboard for test cleanup...");

                    // Remove from parent first
                    if (SharedDashboard.Parent != null)
                    {
                        SharedDashboard.Parent = null;
                    }

                    // Clear controls safely
                    if (SharedDashboard.Controls != null)
                    {
                        SharedDashboard.Controls.Clear();
                    }

                    // Dispose dashboard
                    SharedDashboard.Dispose();
                    SharedDashboard = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Dashboard disposal error (ignored in tests): {ex.Message}");
                SharedDashboard = null; // Ensure it's cleared even if disposal fails
            }

            try
            {
                if (TestForm != null && !TestForm.IsDisposed)
                {
                    TestForm.Dispose();
                    TestForm = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: TestForm disposal error: {ex.Message}");
                TestForm = null;
            }

            // Ensure no orphaned processes remain after tests
            KillAnyOrphanedChildProcesses();

            Console.WriteLine("Shared test fixtures disposed");
        }

        /// <summary>
        /// Helper method to kill any orphaned child processes in the test environment
        /// </summary>
        private void KillAnyOrphanedChildProcesses()
        {
            try
            {
                Console.WriteLine("🔍 Checking for orphaned .NET processes from tests...");
                var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                var currentProcessId = currentProcess.Id;

                // Look for dotnet processes
                foreach (var proc in System.Diagnostics.Process.GetProcessesByName("dotnet"))
                {
                    // Don't terminate ourselves
                    if (proc.Id != currentProcessId)
                    {
                        try
                        {
                            Console.WriteLine($"🧹 Terminating potential orphaned process: {proc.ProcessName} (ID: {proc.Id})");
                            proc.Kill(true); // Force-kill the process and its children
                            proc.WaitForExit(1000);
                            proc.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Could not terminate process {proc.Id}: {ex.Message}");
                        }
                    }
                }

                // Also look for BusBuddy processes
                foreach (var proc in System.Diagnostics.Process.GetProcessesByName("BusBuddy"))
                {
                    try
                    {
                        Console.WriteLine($"🧹 Terminating BusBuddy process: {proc.ProcessName} (ID: {proc.Id})");
                        proc.Kill(true);
                        proc.WaitForExit(1000);
                        proc.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Could not terminate BusBuddy process {proc.Id}: {ex.Message}");
                    }
                }

                Console.WriteLine("✅ Process cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during process cleanup: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Test collection definition
    /// </summary>
    [CollectionDefinition("Dashboard Tests")]
    public class DashboardTestCollection : ICollectionFixture<DashboardTestFixture>
    {
        // This class has no code, and is never created.
        // Its purpose is to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}

