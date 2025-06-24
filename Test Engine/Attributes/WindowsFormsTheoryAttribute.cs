using System;
using Xunit;
using Xunit.Sdk;
using BusBuddy.TestEngine.Foundation;

namespace BusBuddy.TestEngine.Attributes
{
    /// <summary>
    /// Theory attribute for parameterized Windows Forms tests with timeout support
    /// Used for testing UI components with different parameters
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class WindowsFormsTheoryAttribute : TheoryAttribute
    {
        /// <summary>
        /// Creates a new parameterized Windows Forms test with the specified timeout
        /// </summary>
        /// <param name="timeoutMilliseconds">Timeout in milliseconds before the test is forcibly terminated</param>
        public WindowsFormsTheoryAttribute(int timeoutMilliseconds = 30000)
        {
            Timeout = timeoutMilliseconds;

            // Initialize Windows Forms for testing
            TestInitializer.InitializeTestEnvironment();

            // Set environment variable for test timeout
            Environment.SetEnvironmentVariable("TEST_TIMEOUT_MS", timeoutMilliseconds.ToString());
        }

        /// <summary>
        /// Gets the test timeout from environment variable or default value
        /// </summary>
        /// <returns>Timeout in milliseconds</returns>
        public static int GetTestTimeout() =>
            int.TryParse(Environment.GetEnvironmentVariable("TEST_TIMEOUT_MS"), out var timeout)
                ? timeout
                : 45000;
    }
}
