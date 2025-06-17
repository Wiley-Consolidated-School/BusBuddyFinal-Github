using System;
using System.Windows.Forms;

namespace BusBuddy.Tests.UI
{
    /// <summary>
    /// Ensures Windows Forms is properly initialized for UI tests
    /// </summary>
    public static class WindowsFormsTestInitializer
    {
        private static readonly object _lock = new object();
        private static bool _initialized = false;

        /// <summary>
        /// Initializes Windows Forms for testing. Safe to call multiple times.
        /// </summary>
        public static void Initialize()
        {
            lock (_lock)
            {
                if (_initialized)
                    return;

                try
                {
                    Application.SetHighDpiMode(HighDpiMode.SystemAware);
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    _initialized = true;
                }
                catch (InvalidOperationException)
                {
                    // SetCompatibleTextRenderingDefault was already called
                    // This is okay, just mark as initialized
                    _initialized = true;
                }
            }
        }
    }
}
