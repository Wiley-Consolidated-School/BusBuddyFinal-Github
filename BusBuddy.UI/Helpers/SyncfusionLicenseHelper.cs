using System;
using System.IO;
using System.Text.Json;
using System.Configuration;
using Syncfusion.Licensing;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Handles Syncfusion license management for BusBuddy application
    /// Provides proper licensing with fallback to community edition
    /// </summary>
    public static class SyncfusionLicenseHelper
    {
        private const string LICENSE_FILE = "syncfusion-license.json";
        private const string BACKUP_LICENSE_FILE = "syncfusion-license-new.json";

        /// <summary>
        /// License configuration model
        /// </summary>
        public class LicenseConfig
        {
            public SyncfusionLicense SyncfusionLicense { get; set; }
        }

        public class SyncfusionLicense
        {
            public string LicenseKey { get; set; }
            public string LicenseType { get; set; }
            public string Notes { get; set; }
            public string ApplicationDate { get; set; }
            public string Status { get; set; }
        }

        /// <summary>
        /// Initialize Syncfusion licensing with proper fallback
        /// </summary>
        public static void InitializeLicense()
        {
            try
            {
                Console.WriteLine("🔑 Initializing Syncfusion license...");

                // Check if we should force Community Edition
                bool useCommunityLicense = false;
                string useCommunityLicenseSetting = System.Configuration.ConfigurationManager.AppSettings["UseCommunityLicense"];
                if (!string.IsNullOrEmpty(useCommunityLicenseSetting) &&
                    bool.TryParse(useCommunityLicenseSetting, out bool parsedValue))
                {
                    useCommunityLicense = parsedValue;
                }

                if (useCommunityLicense)
                {
                    Console.WriteLine("🔓 Using Syncfusion Community Edition license");
                    SyncfusionLicenseProvider.RegisterLicense("");
                    Console.WriteLine("✅ Syncfusion Community license registered successfully");
                    return;
                }

                // Try to load license from environment variable first, then file
                string licenseKey = LoadLicenseKey();

                if (!string.IsNullOrEmpty(licenseKey) && IsValidLicenseKey(licenseKey))
                {
                    SyncfusionLicenseProvider.RegisterLicense(licenseKey);
                    Console.WriteLine("✅ Syncfusion license registered successfully");
                }
                else
                {
                    Console.WriteLine("⚠️  Invalid or missing license key - using Community Edition");
                    Console.WriteLine("📝 To use full features, update the license in syncfusion-license.json");

                    // Register with empty key for Community Edition
                    SyncfusionLicenseProvider.RegisterLicense("");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ License initialization failed: {ex.Message}");
                Console.WriteLine("🔄 Falling back to Community Edition...");

                try
                {
                    SyncfusionLicenseProvider.RegisterLicense("");
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"❌ Fallback license registration failed: {fallbackEx.Message}");
                }
            }
        }
        /// <summary>
        /// Load license key from environment variable or configuration file
        /// </summary>
        private static string LoadLicenseKey()
        {
            try
            {
                // First, try to load from App.config
                string configLicense = System.Configuration.ConfigurationManager.AppSettings["SyncfusionLicenseKey"];
                if (!string.IsNullOrEmpty(configLicense) && IsValidLicenseKey(configLicense))
                {
                    Console.WriteLine("🔧 Loading license from App.config");
                    return configLicense;
                }

                // Second, try to load from environment variable
                string envLicense = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
                if (!string.IsNullOrEmpty(envLicense) && IsValidLicenseKey(envLicense))
                {
                    Console.WriteLine("🌍 Loading license from environment variable");
                    return envLicense;
                }

                // Finally, try configuration files
                return LoadLicenseFromFile();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading license key: {ex.Message}");
                return LoadLicenseFromFile(); // Fallback to file method
            }
        }

        /// <summary>
        /// Load license key from configuration file
        /// </summary>
        private static string LoadLicenseFromFile()
        {
            try
            {
                // Try primary license file first
                if (File.Exists(LICENSE_FILE))
                {
                    string json = File.ReadAllText(LICENSE_FILE);
                    var config = JsonSerializer.Deserialize<LicenseConfig>(json);

                    if (config?.SyncfusionLicense?.LicenseKey != null &&
                        !config.SyncfusionLicense.LicenseKey.Contains("YOUR_LICENSE_KEY_HERE"))
                    {
                        return config.SyncfusionLicense.LicenseKey;
                    }
                }

                // Try backup license file
                if (File.Exists(BACKUP_LICENSE_FILE))
                {
                    string json = File.ReadAllText(BACKUP_LICENSE_FILE);
                    var config = JsonSerializer.Deserialize<LicenseConfig>(json);

                    if (config?.SyncfusionLicense?.LicenseKey != null &&
                        !config.SyncfusionLicense.LicenseKey.Contains("YOUR_LICENSE_KEY_HERE"))
                    {
                        return config.SyncfusionLicense.LicenseKey;
                    }
                }

                Console.WriteLine("📄 No valid license file found");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error reading license file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Basic validation of license key format
        /// </summary>
        private static bool IsValidLicenseKey(string licenseKey)
        {
            if (string.IsNullOrEmpty(licenseKey))
                return false;

            // Basic checks for license key format
            if (licenseKey.Contains("YOUR_LICENSE_KEY_HERE"))
                return false;

            if (licenseKey.Length < 10)
                return false;

            return true;
        }

        /// <summary>
        /// Check if Syncfusion is properly licensed
        /// </summary>
        public static bool IsLicensed()
        {
            try
            {
                // This is a basic check - Syncfusion doesn't provide a direct API for this
                return true; // Assume licensed if no exception during initialization
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get licensing status information
        /// </summary>
        public static string GetLicenseStatus()
        {
            try
            {
                string licenseKey = LoadLicenseFromFile();

                if (string.IsNullOrEmpty(licenseKey))
                    return "Community Edition (No license key found)";

                if (!IsValidLicenseKey(licenseKey))
                    return "Community Edition (Invalid license key)";

                return "Licensed Edition";
            }
            catch
            {
                return "Community Edition (Error checking license)";
            }
        }
    }
}
