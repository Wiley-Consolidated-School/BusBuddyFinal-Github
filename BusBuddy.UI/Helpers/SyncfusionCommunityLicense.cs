using System;
using System.Threading;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Helper class for registering Syncfusion Community License
    /// </summary>
    public static class SyncfusionCommunityLicense
    {
        private static readonly object _lockObj = new object();
        private static bool _registered = false;
        private static int _registrationAttempts = 0;
        private const int MAX_ATTEMPTS = 3;

        /// <summary>
        /// Ensures that Syncfusion license is registered before any Syncfusion component is initialized.
        /// This must be called before Application.Run and any UI component initialization.
        /// </summary>
        public static bool EnsureRegistered()
        {
            if (_registered)
            {
                return true;
            }

            lock (_lockObj)
            {
                if (_registered)
                {
                    return true;
                }

                if (_registrationAttempts >= MAX_ATTEMPTS)
                {
                    Console.WriteLine("⚠️ Maximum Syncfusion license registration attempts reached.");
                    return false;
                }

                _registrationAttempts++;

                try
                {
                    Console.WriteLine($"🔑 Registering Syncfusion Community License (attempt {_registrationAttempts})...");

                    // Register the empty string for Community License
                    // This must happen before any Syncfusion controls are created
                    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("");

                    // Give a short delay to ensure registration is processed
                    Thread.Sleep(100);

                    Console.WriteLine("✅ Syncfusion Community License registered successfully");
                    _registered = true;
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error registering Syncfusion license: {ex.Message}");
                    return false;
                }
            }
        }
    }
}
