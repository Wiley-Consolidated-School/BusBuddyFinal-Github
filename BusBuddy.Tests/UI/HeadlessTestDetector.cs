using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BusBuddy.Tests.UI
{
    /// <summary>
    /// Detects if tests are running in a headless environment where UI tests should be skipped
    /// Based on Syncfusion testing best practices
    /// </summary>
    public static class HeadlessTestDetector
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        /// <summary>
        /// Determines if the current environment supports UI testing
        /// </summary>
        public static bool IsHeadlessEnvironment()
        {
            try
            {
                // Check if we're in a CI environment
                if (IsContinuousIntegrationEnvironment())
                    return true;

                // Check if display is available
                if (!IsDisplayAvailable())
                    return true;

                // Check if running in interactive mode
                if (!Environment.UserInteractive)
                    return true;

                return false;
            }
            catch
            {
                // If we can't determine, assume headless for safety
                return true;
            }
        }

        /// <summary>
        /// Checks if running in a CI/CD environment
        /// </summary>
        private static bool IsContinuousIntegrationEnvironment()
        {
            var ciIndicators = new[]
            {
                "CI", "CONTINUOUS_INTEGRATION", "BUILD_ID", "BUILD_NUMBER",
                "JENKINS_URL", "GITHUB_ACTIONS", "AZURE_DEVOPS", "TEAMCITY_VERSION"
            };

            foreach (var indicator in ciIndicators)
            {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(indicator)))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a display is available for UI testing
        /// </summary>
        private static bool IsDisplayAvailable()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // On Windows, check if desktop window is available
                    return GetDesktopWindow() != IntPtr.Zero;
                }
                else
                {
                    // On non-Windows, check DISPLAY environment variable
                    return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY"));
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a skip reason for UI tests in headless environments
        /// </summary>
        public static string GetSkipReason()
        {
            if (IsContinuousIntegrationEnvironment())
                return "UI tests are skipped in CI/CD environments";

            if (!IsDisplayAvailable())
                return "UI tests require a display/desktop environment";

            if (!Environment.UserInteractive)
                return "UI tests require interactive user session";

            return "UI tests are skipped in headless environment";
        }
    }
}
