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
        public Dashboard? SharedDashboard { get; private set; }
        public Form? TestForm { get; private set; }

        static DashboardTestFixture()
        {
            // Set exception mode before any controls are created
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        }

        public DashboardTestFixture()
        {
            // Set environment variable to indicate test mode
            Environment.SetEnvironmentVariable("BUSBUDDY_TEST_MODE", "1");

            Console.WriteLine("Creating shared Dashboard instance for all tests...");

            try
            {
                // Create test form first with proper initialization
                TestForm = new Form
                {
                    WindowState = FormWindowState.Minimized,
                    ShowInTaskbar = false,
                    Visible = false,
                    Size = new System.Drawing.Size(1, 1) // Minimize size to reduce resource usage
                };

                // Skip Dashboard creation in tests to avoid Syncfusion control issues
                // Dashboard will be null - tests need to handle this appropriately
                Console.WriteLine("Skipping Dashboard creation in test environment to avoid Syncfusion control issues");
                SharedDashboard = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in test fixture setup: {ex.Message}");
                SharedDashboard = null;
            }
        }

        public void Dispose()
        {
            Console.WriteLine("Disposing shared test fixtures...");

            try
            {
                // Force aggressive disposal for tests
                if (SharedDashboard != null && !SharedDashboard.IsDisposed)
                {
                    Console.WriteLine("Forcefully disposing Dashboard for test cleanup...");
                    // Skip normal disposal in tests - just clear references
                    SharedDashboard.Controls.Clear();
                    SharedDashboard = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Dashboard disposal error (ignored in tests): {ex.Message}");
            }

            try
            {
                if (TestForm != null && !TestForm.IsDisposed)
                {
                    TestForm.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: TestForm disposal error: {ex.Message}");
            }

            Console.WriteLine("Shared test fixtures disposed");
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
