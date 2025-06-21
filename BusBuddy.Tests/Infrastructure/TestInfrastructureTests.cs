using System;
using Xunit;
using BusBuddy.Tests.UI;

namespace BusBuddy.Tests.Infrastructure
{
    /// <summary>
    /// Tests for the test infrastructure manager to ensure robust test execution
    /// </summary>
    public class TestInfrastructureTests : UITestBase
    {
        [Fact]
        public void TestInfrastructure_Initialize_Success()
        {
            // Arrange & Act
            TestInfrastructureManager.Initialize();

            // Assert - Should not throw
            Assert.True(true, "Test infrastructure initialized successfully");
        }

        [Fact]
        public void TestInfrastructure_PrePostCleanup_Success()
        {
            // Arrange & Act
            TestInfrastructureManager.PreTestCleanup();
            TestInfrastructureManager.PostTestCleanup();

            // Assert - Should not throw
            Assert.True(true, "Pre and post cleanup completed successfully");
        }

        [Fact]
        public void TestInfrastructure_ForceCleanup_Success()
        {
            // Arrange & Act
            TestInfrastructureManager.ForceCleanup();

            // Assert - Should not throw
            Assert.True(true, "Force cleanup completed successfully");
        }

        [Fact]
        public void Dashboard_CreateAndDispose_NoMemoryLeaks()
        {
            // Arrange
            var memoryBefore = GC.GetTotalMemory(true);

            // Act
            using (var dashboard = CreateDashboardSafely())
            {
                // Verify dashboard is created
                Assert.NotNull(dashboard);
                Assert.False(dashboard.IsDisposed);
            }

            // Force cleanup and memory collection
            TestInfrastructureManager.ForceCleanup();
            var memoryAfter = GC.GetTotalMemory(true);

            // Assert - Memory should not increase significantly
            var memoryIncrease = memoryAfter - memoryBefore;
            var memoryIncreaseMB = memoryIncrease / 1024 / 1024;

            Console.WriteLine($"Memory before: {memoryBefore / 1024 / 1024}MB");
            Console.WriteLine($"Memory after: {memoryAfter / 1024 / 1024}MB");
            Console.WriteLine($"Memory increase: {memoryIncreaseMB}MB");

            Assert.True(memoryIncreaseMB < 50, $"Memory increase {memoryIncreaseMB}MB is too high");
        }
    }
}
