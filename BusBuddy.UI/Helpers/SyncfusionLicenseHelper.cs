using System;
using System.IO;
using System.Text.Json;
using Syncfusion.Licensing;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Helper class to manage Syncfusion licensing configuration
    /// </summary>
    public static class SyncfusionLicenseHelper
    {
        private static bool _licenseRegistered = false;
        private static string? _currentLicenseKey = null;

        /// <summary>
        /// Configuration class for Syncfusion license
        /// </summary>
        public class LicenseConfig
        {
            public SyncfusionLicenseInfo? SyncfusionLicense { get; set; }
        }

        public class SyncfusionLicenseInfo
        {
            public string? LicenseKey { get; set; }
            public string? LicenseType { get; set; }
            public string? Notes { get; set; }
            public string? ApplicationDate { get; set; }
            public string? Status { get; set; }
        }

        /// <summary>
        /// Registers the Syncfusion license key with enhanced error handling
        /// </summary>
        /// <param name="licenseKey">The license key to register</param>
        /// <returns>True if registration was successful, false otherwise</returns>
        public static bool RegisterLicense(string licenseKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(licenseKey))
                {
                    Console.WriteLine("⚠️  No Syncfusion license key provided");
                    return false;
                }

                // Avoid re-registering the same license
                if (_licenseRegistered && _currentLicenseKey == licenseKey)
                {
                    Console.WriteLine("✅ Syncfusion license already registered");
                    return true;
                }

                SyncfusionLicenseProvider.RegisterLicense(licenseKey);
                _licenseRegistered = true;
                _currentLicenseKey = licenseKey;

                Console.WriteLine("✅ Syncfusion license registered successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Syncfusion licensing notice: {ex.Message}");
                LogLicensingInformation();
                return false;
            }
        }

        /// <summary>
        /// Registers license from configuration file, with fallback to default
        /// </summary>
        /// <returns>True if registration was successful, false otherwise</returns>
        public static bool RegisterFromConfiguration()
        {
            try
            {
                // Try to read from configuration file first
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "syncfusion-license.json");
                if (File.Exists(configPath))
                {
                    var jsonContent = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<LicenseConfig>(jsonContent);

                    if (config?.SyncfusionLicense?.LicenseKey != null)
                    {
                        Console.WriteLine($"📄 Loading Syncfusion license from configuration ({config.SyncfusionLicense.Status})");
                        return RegisterLicense(config.SyncfusionLicense.LicenseKey);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Could not read license configuration: {ex.Message}");
            }

            // Fallback to default community license
            return RegisterDefaultCommunityLicense();
        }

        /// <summary>
        /// Registers the default community license key
        /// </summary>
        /// <returns>True if registration was successful, false otherwise</returns>
        public static bool RegisterDefaultCommunityLicense()
        {
            // This is a placeholder community license key
            // Replace with your actual community license when approved
            var defaultKey = "Ngo9BigBOggjHTQxAR8/V1NNaF1cXGNCeUx3Q3xbf1x1ZFdMZFVbQHdPIiBoS35Rc0VlWHlfeXVSRGRfWEBwVEBU";
            Console.WriteLine("🔑 Using default community license key (pending approval)");
            return RegisterLicense(defaultKey);
        }

        /// <summary>
        /// Logs helpful licensing information to the console
        /// </summary>
        private static void LogLicensingInformation()
        {
            Console.WriteLine("📋 Syncfusion Licensing Information:");
            Console.WriteLine("   • Application will continue with Community Edition limitations");
            Console.WriteLine("   • You may see watermarks or trial limitations");
            Console.WriteLine("   • To resolve: Apply for community license at:");
            Console.WriteLine("     https://www.syncfusion.com/products/communitylicense");
            Console.WriteLine("   • For commercial use, purchase a license at:");
            Console.WriteLine("     https://www.syncfusion.com/sales/products");
        }

        /// <summary>
        /// Gets the current license registration status
        /// </summary>
        /// <returns>True if a license has been registered</returns>
        public static bool IsLicenseRegistered()
        {
            return _licenseRegistered;
        }

        /// <summary>
        /// Displays licensing status information
        /// </summary>
        public static void DisplayLicenseStatus()
        {
            if (_licenseRegistered)
            {
                Console.WriteLine("✅ Syncfusion License Status: Registered");
            }
            else
            {
                Console.WriteLine("⚠️  Syncfusion License Status: Not Registered (Community Edition limitations apply)");
                LogLicensingInformation();
            }
        }
    }
}
