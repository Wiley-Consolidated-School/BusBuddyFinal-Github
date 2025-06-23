using System;
using System.Reflection;
using System.IO;
using System.Configuration;
using System.Windows.Forms;
using Syncfusion.Licensing;
using System.Threading;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Helper class to ensure proper Syncfusion Community License registration.
    /// Based on official Syncfusion licensing documentation:
    /// https://help.syncfusion.com/windowsforms/licensing/how-to-register-in-an-application
    /// </summary>
    public static class CommunityEditionHelper
    {
        // Track if the license has been registered to avoid multiple attempts
        private static bool _licenseRegistered = false;
        private static readonly object _lockObject = new object();
        private static readonly int MAX_RETRIES = 3;

        // The actual Syncfusion license key for BusBuddy
        private const string DEFAULT_LICENSE_KEY = "Ngo9BigBOggjHTQxAR8/V1NNaF1cXGNCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXlccnVdQ2NdU0JxVkRWYUA=";

        /// <summary>
        /// Registers the Syncfusion Community License at application startup.
        /// Must be called before any Syncfusion controls are created and before any UI initialization.
        /// According to official documentation, place this in Main() before Application.EnableVisualStyles().
        /// </summary>
        public static void RegisterCommunityLicense()
        {
            if (_licenseRegistered)
                return;

            lock (_lockObject)
            {
                if (_licenseRegistered)
                    return;

                Console.WriteLine("🔑 CommunityEditionHelper: Registering Syncfusion Community License...");

                try
                {
                    // Register empty string according to Syncfusion docs for Community Edition
                    RegisterWithRetry();

                    // Set flag to avoid re-registration
                    _licenseRegistered = true;

                    Console.WriteLine("✅ CommunityEditionHelper: Community Edition license registered successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ CommunityEditionHelper: License registration error: {ex.Message}");
                    // Don't show a MessageBox here as UI might not be initialized yet
                    // Just log the error and allow the application to continue
                }
            }
        }

        /// <summary>
        /// Registers the license with retry mechanism
        /// </summary>
        private static void RegisterWithRetry()
        {
            int attempts = 0;
            bool success = false;
            Exception lastException = null;

            while (!success && attempts < MAX_RETRIES)
            {
                try
                {
                    attempts++;
                    Console.WriteLine($"🔑 Attempt {attempts} to register Community License...");

                    // Get the license key (will be empty string for Community Edition)
                    string licenseKey = GetLicenseKey();

                    // Register the license using the official documented method
                    SyncfusionLicenseProvider.RegisterLicense(licenseKey);

                    success = true;
                    Console.WriteLine($"✅ License registered successfully on attempt {attempts}");
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Console.WriteLine($"⚠️ Attempt {attempts} failed: {ex.Message}");

                    // Wait briefly before retry
                    Thread.Sleep(100);
                }
            }

            if (!success && lastException != null)
            {
                throw new Exception($"Failed to register license after {MAX_RETRIES} attempts", lastException);
            }
        }

        /// <summary>
        /// Gets the license key from configuration settings or environment variables.
        /// Returns empty string for Community Edition.
        /// </summary>
        public static string GetLicenseKey()
        {
            try
            {
                // First check if we're explicitly configured to use Community Edition
                string useCommunityLicenseSetting = ConfigurationManager.AppSettings["UseCommunityLicense"];
                if (!string.IsNullOrEmpty(useCommunityLicenseSetting) &&
                    bool.TryParse(useCommunityLicenseSetting, out bool useCommunity) &&
                    useCommunity)
                {
                    Console.WriteLine("🔓 Configured to use Syncfusion Community Edition");
                    return "";
                }

                // Next try environment variable (useful for CI/CD environments)
                string envKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
                if (!string.IsNullOrEmpty(envKey))
                {
                    Console.WriteLine("🔑 Using Syncfusion license key from environment variable");
                    return envKey;
                }

                // Then try app settings
                string configKey = ConfigurationManager.AppSettings["SyncfusionLicenseKey"];
                if (!string.IsNullOrEmpty(configKey))
                {
                    Console.WriteLine("🔑 Using Syncfusion license key from app settings");
                    return configKey;
                }

                // Finally, fall back to Community Edition (default license key)
                Console.WriteLine("🔓 No license key found - using default Community Edition license");
                return DEFAULT_LICENSE_KEY;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error getting license key: {ex.Message}");
                // Default to Community Edition on error
                return DEFAULT_LICENSE_KEY;
            }
        }
    }
}
