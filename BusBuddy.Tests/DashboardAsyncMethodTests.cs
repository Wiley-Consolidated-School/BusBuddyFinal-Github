using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusBuddy.UI.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BusBuddy.Tests.UI
{
    /// <summary>
    /// Tests specifically for the Dashboard's async loading functionality
    /// Uses the standard MSTest framework that is already set up in the project
    /// </summary>
    [TestClass]
    public class DashboardAsyncMethodTests
    {
        private Dashboard _dashboard;

        [TestInitialize]
        public void Setup()
        {
            // Set test mode environment variable to ensure we don't create real UI components
            Environment.SetEnvironmentVariable("BUSBUDDY_TEST_MODE", "1");

            // Create dashboard instance for testing
            _dashboard = new Dashboard();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up after tests
            _dashboard?.Dispose();
            Environment.SetEnvironmentVariable("BUSBUDDY_TEST_MODE", null);
        }

        [TestMethod]
        public async Task Dashboard_LoadAsync_ShouldCompleteWithoutExceptions()
        {
            // Arrange
            // Get the private method via reflection
            MethodInfo loadAsyncMethod = typeof(Dashboard).GetMethod("Dashboard_LoadAsync",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(loadAsyncMethod, "Dashboard_LoadAsync method should exist");

            // Act - Invoke the method
            Exception caughtException = null;
            try
            {
                // We're invoking the method directly with null sender and empty EventArgs
                loadAsyncMethod.Invoke(_dashboard, new object[] { null, EventArgs.Empty });

                // Since it's async void, we need to allow some time for it to complete
                await Task.Delay(1000); // Give it a second to run
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert no exception was thrown
            Assert.IsNull(caughtException, $"Exception thrown: {caughtException?.Message ?? "None"}");
        }

        [TestMethod]
        public async Task Dashboard_LoadAsync_ShouldInitializeRequiredComponents()
        {
            // Arrange
            MethodInfo loadAsyncMethod = typeof(Dashboard).GetMethod("Dashboard_LoadAsync",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(loadAsyncMethod, "Dashboard_LoadAsync method should exist");

            // Act
            loadAsyncMethod.Invoke(_dashboard, new object[] { null, EventArgs.Empty });

            // Allow time for async operations to complete
            await Task.Delay(1000);

            // Assert - Check if critical components were initialized
            // Access private fields via reflection
            var contentPanelField = typeof(Dashboard).GetField("_contentPanel",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var panel = contentPanelField.GetValue(_dashboard) as Panel;

            Assert.IsNotNull(panel, "Content panel should be initialized");
            Assert.IsTrue(panel.Visible, "Content panel should be visible");
        }

        [TestMethod]
        public async Task InitializeDashboardAsync_ShouldComplete()
        {
            // Arrange
            MethodInfo initMethod = typeof(Dashboard).GetMethod("InitializeDashboardAsync",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(initMethod, "InitializeDashboardAsync method should exist");

            // Act
            Task task = (Task)initMethod.Invoke(_dashboard, new object[] { });

            // Assert
            try
            {
                await task; // This will throw if the task fails
                Assert.IsTrue(true, "Task completed successfully");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Task failed with exception: {ex.Message}");
            }

            // Additional verification
            var vehiclesGridField = typeof(Dashboard).GetField("_vehiclesGrid",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var grid = vehiclesGridField.GetValue(_dashboard);
            Assert.IsNotNull(grid, "Vehicles grid should be initialized");
        }
    }
}

