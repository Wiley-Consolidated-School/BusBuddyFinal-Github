using System;
using BusBuddy.UI.Helpers;

namespace BusBuddy
{
    /// <summary>
    /// Simple test to verify Syncfusion license configuration
    /// </summary>
    class LicenseTest
    {
        static void TestLicense()
        {
            Console.WriteLine("🧪 Testing Syncfusion License Configuration...");

            try
            {
                // Test the license initialization
                SyncfusionLicenseHelper.InitializeLicense();
                Console.WriteLine("✅ License test completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ License test failed: {ex.Message}");
            }
        }
    }
}
