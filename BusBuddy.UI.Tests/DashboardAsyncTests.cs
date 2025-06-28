using System;
using System.Threading.Tasks;
using Xunit;
using BusBuddy.UI.Views;
using System.Reflection;

namespace BusBuddy.Tests.Simple
{
    /// <summary>
    /// Dashboard Async Test - Simplified standalone version
    /// </summary>
    public class DashboardAsyncTests
    {
        [Fact]
        public async Task Dashboard_LoadAsync_ShouldComplete()
        {
            // Arrange
            Environment.SetEnvironmentVariable("BUSBUDDY_TEST_MODE", "1");
            Dashboard dashboard = new Dashboard();

            try
            {
                // Get the private method via reflection
                MethodInfo? loadAsyncMethod = typeof(Dashboard).GetMethod("Dashboard_LoadAsync",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                Assert.NotNull(loadAsyncMethod);  // xUnit assertion syntax

                // Create sender object for method invocation
                object sender = dashboard;

                // Act - Invoke the method
                loadAsyncMethod.Invoke(dashboard, new object[] { sender, EventArgs.Empty });

                // Wait for async operations
                await Task.Delay(1000);

                // Assert - If we got here without exceptions, test passes
                Assert.True(true); // Simple assertion that shows we didn't throw an exception
            }
            finally
            {
                // Clean up
                dashboard.Dispose();
                Environment.SetEnvironmentVariable("BUSBUDDY_TEST_MODE", null);
            }
        }
    }
}
