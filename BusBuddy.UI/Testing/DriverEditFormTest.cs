using System;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.UI.Views;

namespace BusBuddy.UI.Testing
{
    /// <summary>
    /// Simple test class to verify the migrated DriverEditFormSyncfusion works properly
    /// </summary>
    public static class DriverEditFormTest
    {
        /// <summary>
        /// Test creating a new driver form
        /// </summary>
        public static void TestNewDriverForm()
        {
            try
            {
                using (var form = new DriverEditFormSyncfusion())
                {
                    Console.WriteLine("✅ Successfully created new DriverEditFormSyncfusion");
                    Console.WriteLine($"   Form Text: {form.Text}");
                    Console.WriteLine($"   Form Size: {form.Size}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating new DriverEditFormSyncfusion: {ex.Message}");
                Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Test creating a driver edit form with existing driver data
        /// </summary>
        public static void TestEditDriverForm()
        {
            try
            {
                var testDriver = new Driver
                {
                    DriverID = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    DriverPhone = "(555) 123-4567",
                    DriverEmail = "john.doe@example.com",
                    Address = "123 Main St",
                    City = "Anytown",
                    State = "CA",
                    Zip = "90210",
                    DriversLicenseType = "Class B",
                    Status = "Active",
                    IsTrainingComplete = true,
                    CDLExpirationDate = DateTime.Now.AddYears(2),
                    Notes = "Test driver for migration testing"
                };

                using (var form = new DriverEditFormSyncfusion(testDriver))
                {
                    Console.WriteLine("✅ Successfully created edit DriverEditFormSyncfusion");
                    Console.WriteLine($"   Form Text: {form.Text}");
                    Console.WriteLine($"   Driver: {testDriver.FirstName} {testDriver.LastName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating edit DriverEditFormSyncfusion: {ex.Message}");
                Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Run all tests for the migrated driver form
        /// </summary>
        public static void RunAllTests()
        {            Console.WriteLine("🧪 Testing DriverEditFormSyncfusion Migration");
            Console.WriteLine(new string('=', 50));

            TestNewDriverForm();
            Console.WriteLine();

            TestEditDriverForm();
            Console.WriteLine();

            Console.WriteLine("✅ Driver form migration tests completed");
        }
    }
}
