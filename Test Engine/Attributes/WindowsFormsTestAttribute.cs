using System;
using Xunit;
using BusBuddy.TestEngine.Foundation;

namespace BusBuddy.TestEngine.Attributes
{
    /// <summary>
    /// Simple Windows Forms test attribute - does it work? Yes or no.
    /// Initializes the environment and runs the test.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class WindowsFormsTestAttribute : FactAttribute
    {
        /// <summary>
        /// Creates a Windows Forms test - simple and straightforward
        /// </summary>
        public WindowsFormsTestAttribute()
        {
            // Initialize Windows Forms for testing - that's it
            TestInitializer.InitializeTestEnvironment();
        }
    }
}
