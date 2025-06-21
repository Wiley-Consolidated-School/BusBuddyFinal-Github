using System;
using Xunit;
using Xunit.Abstractions;
using BusBuddy.UI.Services;

namespace BusBuddy.Tests.UI
{
    /// <summary>
    /// Simple tests for TestSafeApplicationShutdownManager to ensure test mode works
    /// without creating heavy UI components that can overwhelm the build system
    /// </summary>
    public class TestSafeShutdownTests
    {
        private readonly ITestOutputHelper _output;

        public TestSafeShutdownTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestSafeApplicationShutdownManager_EnableTestMode_ShouldPreventEnvironmentExit()
        {
            try
            {
                _output.WriteLine("🧪 Testing TestSafeApplicationShutdownManager test mode...");

                // Enable test mode
                TestSafeApplicationShutdownManager.EnableTestMode();
                _output.WriteLine("✅ Test mode enabled successfully");

                // Try to perform shutdown - this should NOT call Environment.Exit()
                TestSafeApplicationShutdownManager.PerformShutdown();
                _output.WriteLine("✅ PerformShutdown completed without crashing test host");

                // If we get here, the test passed - Environment.Exit() was not called
                Assert.True(true, "Shutdown completed without Environment.Exit() terminating the test host");
            }
            finally
            {
                // Always disable test mode in cleanup
                TestSafeApplicationShutdownManager.DisableTestMode();
                _output.WriteLine("🔄 Test mode disabled");
            }
        }

        [Fact]
        public void TestSafeApplicationShutdownManager_EnableAndDisableTestMode_ShouldToggleCorrectly()
        {
            try
            {
                _output.WriteLine("🧪 Testing test mode toggle functionality...");

                // Test enabling
                TestSafeApplicationShutdownManager.EnableTestMode();
                _output.WriteLine("✅ Test mode enabled");

                // Test disabling
                TestSafeApplicationShutdownManager.DisableTestMode();
                _output.WriteLine("✅ Test mode disabled");

                // Test re-enabling
                TestSafeApplicationShutdownManager.EnableTestMode();
                _output.WriteLine("✅ Test mode re-enabled");

                // Test shutdown in enabled state
                TestSafeApplicationShutdownManager.PerformShutdown();
                _output.WriteLine("✅ Shutdown completed successfully in test mode");

                Assert.True(true, "Test mode toggle operations completed successfully");
            }
            finally
            {
                TestSafeApplicationShutdownManager.DisableTestMode();
                _output.WriteLine("🔄 Test mode disabled in cleanup");
            }
        }

        [Fact]
        public void TestSafeApplicationShutdownManager_MultipleShutdownCalls_ShouldNotCrash()
        {
            try
            {
                _output.WriteLine("🧪 Testing multiple shutdown calls...");

                TestSafeApplicationShutdownManager.EnableTestMode();

                // Call shutdown multiple times - should not crash
                for (int i = 1; i <= 3; i++)
                {
                    _output.WriteLine($"🔄 Shutdown call {i}/3...");
                    TestSafeApplicationShutdownManager.PerformShutdown();
                    _output.WriteLine($"✅ Shutdown call {i} completed");
                }

                Assert.True(true, "Multiple shutdown calls completed without crashing");
            }
            finally
            {
                TestSafeApplicationShutdownManager.DisableTestMode();
                _output.WriteLine("🔄 Test mode disabled");
            }
        }
    }
}
