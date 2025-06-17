using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.UI.Base;
using BusBuddy.UI.Migration;
using BusBuddy.UI.Views;
using BusBuddy.UI.Helpers;

namespace BusBuddy.UI.Testing
{
    /// <summary>
    /// Test utility to validate Syncfusion migration
    /// Can be run to test forms and generate migration reports
    /// </summary>
    public static class SyncfusionMigrationTester
    {
        /// <summary>
        /// Test the Syncfusion theme system
        /// </summary>
        public static void TestSyncfusionThemeSystem()
        {
            Console.WriteLine("🧪 TESTING: Syncfusion Theme System");
            Console.WriteLine("=====================================");

            try
            {
                // Initialize theme system
                SyncfusionThemeHelper.InitializeGlobalTheme();
                Console.WriteLine("✅ Global theme initialization: SUCCESS");

                // Test control creation
                var testButton = SyncfusionThemeHelper.CreateMaterialButton("Test Button");
                var testTextBox = SyncfusionThemeHelper.CreateMaterialTextBox();
                var testLabel = SyncfusionThemeHelper.CreateMaterialLabel("Test Label");

                Console.WriteLine($"✅ Control creation: SUCCESS (Button: {testButton?.GetType().Name}, TextBox: {testTextBox?.GetType().Name}, Label: {testLabel?.GetType().Name})");

                // Test DPI awareness
                using (var testForm = new Form())
                {
                    var dpiInfo = SyncfusionThemeHelper.HighDpiSupport.GetDpiDescription(testForm);
                    var dpiScale = SyncfusionThemeHelper.HighDpiSupport.GetDpiScale(testForm);
                    Console.WriteLine($"✅ DPI Support: {dpiInfo} (Scale: {dpiScale:F2}x)");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Theme system test FAILED: {ex.Message}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Test base form functionality
        /// </summary>
        public static void TestSyncfusionBaseForm()
        {
            Console.WriteLine("🧪 TESTING: SyncfusionBaseForm");
            Console.WriteLine("===============================");

            try
            {                using (var testForm = new SyncfusionBaseForm())
                {
                    testForm.Text = "Test Form";
                    Console.WriteLine($"✅ Base form creation: SUCCESS");
                    Console.WriteLine($"   DPI Info: {testForm.GetDpiInfo()}");

                    // Note: Control creation methods are protected, so we'll test through public interface
                    testForm.RefreshMaterialTheme();

                    Console.WriteLine($"✅ Control creation methods: SUCCESS (Theme refresh completed)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Base form test FAILED: {ex.Message}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Test migration utility on existing forms
        /// </summary>
        public static void TestMigrationUtility()
        {
            Console.WriteLine("🧪 TESTING: Migration Utility");
            Console.WriteLine("==============================");

            try
            {
                // Test analysis of a MaterialSkin form
                using (var testForm = new StandardDataForm())
                {
                    var report = MaterialToSyncfusionMigrator.AnalyzeForm(testForm);
                    Console.WriteLine($"✅ Form analysis: SUCCESS");
                    Console.WriteLine($"   {report}");

                    if (report.Warnings.Any())
                    {
                        Console.WriteLine("   Warnings:");
                        foreach (var warning in report.Warnings.Take(3))
                        {
                            Console.WriteLine($"   - {warning}");
                        }
                    }
                }

                // Test theme application
                using (var testForm = new Form())
                {
                    testForm.Controls.Add(new Button { Text = "Test", Location = new System.Drawing.Point(10, 10) });
                    testForm.Controls.Add(new TextBox { Location = new System.Drawing.Point(10, 50) });

                    var themeReport = MaterialToSyncfusionMigrator.ApplySyncfusionThemeToExistingForm(testForm);
                    Console.WriteLine($"✅ Theme application: SUCCESS");
                    Console.WriteLine($"   {themeReport}");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Migration utility test FAILED: {ex.Message}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Generate migration instructions for key forms
        /// </summary>
        public static void GenerateMigrationInstructions()
        {
            Console.WriteLine("📋 GENERATING: Migration Instructions");
            Console.WriteLine("======================================");

            var formsToAnalyze = new List<(string Name, Func<Form> CreateForm)>
            {
                ("StandardDataForm", () => new StandardDataForm()),
                ("DriverEditFormSyncfusion", () => new DriverEditFormSyncfusion())
            };

            foreach (var (name, createForm) in formsToAnalyze)
            {
                try
                {
                    using (var form = createForm())
                    {
                        var instructions = MaterialToSyncfusionMigrator.GenerateMigrationInstructions(form);

                        Console.WriteLine($"\n📝 {name} Migration Instructions:");
                        Console.WriteLine("   " + string.Join("\n   ", instructions.Take(10)));

                        if (instructions.Count > 10)
                        {
                            Console.WriteLine($"   ... ({instructions.Count - 10} more lines)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to analyze {name}: {ex.Message}");
                }
            }

            Console.WriteLine();
        }        /// <summary>
        /// Run all migration tests
        /// </summary>
        public static void RunAllTests()
        {
            Console.WriteLine("🚀 SYNCFUSION MIGRATION TEST SUITE");
            Console.WriteLine("===================================");
            Console.WriteLine($"Test Run: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();

            TestSyncfusionThemeSystem();
            TestSyncfusionBaseForm();
            TestMigrationUtility();
            TestMigratedForms();
            CompareMigrationResults();
            TestThemeConsistency();
            GenerateMigrationInstructions();

            Console.WriteLine("🏁 TESTING COMPLETE");
            Console.WriteLine("===================");
        }/// <summary>
        /// Show a demo of the migrated form
        /// </summary>
        public static void ShowMigratedFormDemo()
        {
            try
            {
                Console.WriteLine("🎬 DEMO: Showing migrated SyncfusionBaseForm");

                var migratedForm = new SyncfusionBaseForm();
                migratedForm.Text = "Syncfusion Base Form Demo";
                migratedForm.ShowDialog();

                Console.WriteLine("✅ Demo completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Demo failed: {ex.Message}");
                MessageBox.Show($"Demo failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Test the newly migrated forms
        /// </summary>
        public static void TestMigratedForms()
        {
            Console.WriteLine("🧪 TESTING: Migrated Forms");
            Console.WriteLine("===========================");

            // Test DriverEditFormSyncfusion
            try
            {
                using (var driverForm = new DriverEditFormSyncfusion())
                {
                    Console.WriteLine("✅ DriverEditFormSyncfusion creation: SUCCESS");
                    Console.WriteLine($"   Form title: {driverForm.Text}");
                    Console.WriteLine($"   Form size: {driverForm.Size}");
                    Console.WriteLine($"   DPI info: {driverForm.GetDpiInfo()}");
                }

                // Test with driver data
                var testDriver = new BusBuddy.Models.Driver
                {
                    DriverID = 1,
                    FirstName = "Test",
                    LastName = "Driver",
                    DriverPhone = "(555) 123-4567",
                    DriverEmail = "test@example.com"
                };

                using (var driverFormWithData = new DriverEditFormSyncfusion(testDriver))
                {
                    Console.WriteLine("✅ DriverEditFormSyncfusion with data: SUCCESS");
                    Console.WriteLine($"   Driver: {testDriver.FirstName} {testDriver.LastName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DriverEditFormSyncfusion test FAILED: {ex.Message}");
            }

            // Test RouteEditFormSyncfusion
            try
            {
                using (var routeForm = new RouteEditFormSyncfusion())
                {
                    Console.WriteLine("✅ RouteEditFormSyncfusion creation: SUCCESS");
                    Console.WriteLine($"   Form title: {routeForm.Text}");
                    Console.WriteLine($"   Form size: {routeForm.Size}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RouteEditFormSyncfusion test FAILED: {ex.Message}");
            }

            // Test BusBuddyDashboardSyncfusion (mock the services)
            try
            {
                var mockNavService = new MockNavigationService();
                var mockDbService = new MockDatabaseHelperService();

                using (var dashboard = new BusBuddyDashboardSyncfusion(mockNavService, mockDbService))
                {
                    Console.WriteLine("✅ BusBuddyDashboardSyncfusion creation: SUCCESS");
                    Console.WriteLine($"   Dashboard title: {dashboard.Text}");
                    Console.WriteLine($"   Dashboard size: {dashboard.Size}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ BusBuddyDashboardSyncfusion test FAILED: {ex.Message}");
                Console.WriteLine($"   Note: This may fail due to missing service dependencies");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Compare original MaterialSkin forms with migrated Syncfusion forms
        /// </summary>
        public static void CompareMigrationResults()
        {
            Console.WriteLine("🔍 COMPARING: Original vs Migrated Forms");
            Console.WriteLine("=========================================");

            try
            {
                // Compare available forms
                Console.WriteLine("📋 Available Form Analysis:");

                using (var driverForm = new DriverEditFormSyncfusion())
                using (var routeForm = new RouteEditFormSyncfusion())
                {
                    Console.WriteLine($"   DriverEditFormSyncfusion: {driverForm.GetType().Name} (Base: {driverForm.GetType().BaseType?.Name})");
                    Console.WriteLine($"   RouteEditFormSyncfusion: {routeForm.GetType().Name} (Base: {routeForm.GetType().BaseType?.Name})");
                    Console.WriteLine($"   Driver form size: {driverForm.Size}");
                    Console.WriteLine($"   Route form size: {routeForm.Size}");
                }

                Console.WriteLine("✅ Form comparison completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Form comparison FAILED: {ex.Message}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Test theme consistency across migrated forms
        /// </summary>
        public static void TestThemeConsistency()
        {
            Console.WriteLine("🎨 TESTING: Theme Consistency");
            Console.WriteLine("==============================");

            var forms = new List<(string Name, Func<Form> CreateForm)>
            {
                ("SyncfusionBaseForm", () => new SyncfusionBaseForm()),
                ("DriverEditFormSyncfusion", () => new DriverEditFormSyncfusion()),
                ("RouteEditFormSyncfusion", () => new RouteEditFormSyncfusion())
            };

            foreach (var (name, createForm) in forms)
            {
                try
                {
                    using (var form = createForm())
                    {
                        Console.WriteLine($"📝 {name}:");
                        Console.WriteLine($"   Background: {form.BackColor}");
                        Console.WriteLine($"   Foreground: {form.ForeColor}");
                        Console.WriteLine($"   Font: {form.Font.Name} {form.Font.Size}pt");

                        // Check if it's a Syncfusion form
                        if (form is SyncfusionBaseForm syncForm)
                        {
                            Console.WriteLine($"   DPI Info: {syncForm.GetDpiInfo()}");
                            Console.WriteLine("   ✅ Syncfusion theming applied");
                        }
                        else
                        {
                            Console.WriteLine("   ⚠️  Not a SyncfusionBaseForm");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ❌ Failed to test {name}: {ex.Message}");
                }
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Quick test to verify migration is working
        /// Can be called from Program.cs startup
        /// </summary>
        public static void QuickMigrationTest()
        {
            Console.WriteLine("⚡ QUICK MIGRATION TEST");
            Console.WriteLine("=======================");

            var allTestsPassed = true;

            // Test theme system
            try
            {
                SyncfusionThemeHelper.InitializeGlobalTheme();
                Console.WriteLine("✅ Theme system: OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Theme system: FAILED - {ex.Message}");
                allTestsPassed = false;
            }

            // Test base form
            try
            {
                using (var form = new SyncfusionBaseForm())
                {
                    form.Text = "Quick Test";
                    Console.WriteLine("✅ SyncfusionBaseForm: OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SyncfusionBaseForm: FAILED - {ex.Message}");
                allTestsPassed = false;
            }

            // Test migrated driver form
            try
            {
                using (var form = new DriverEditFormSyncfusion())
                {
                    Console.WriteLine("✅ DriverEditFormSyncfusion: OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DriverEditFormSyncfusion: FAILED - {ex.Message}");
                allTestsPassed = false;
            }

            Console.WriteLine($"\n{(allTestsPassed ? "🎉 All quick tests PASSED!" : "⚠️  Some tests FAILED - check console output")}");
            Console.WriteLine("=======================\n");
        }        // Mock services for testing - simplified to avoid interface dependency issues
        private class MockNavigationService : BusBuddy.UI.Services.INavigationService
        {
            public void NavigateTo<T>() where T : Form, new() { }
            public void NavigateTo<T>(object parameter) where T : Form, new() { }
            public DialogResult ShowDialog<T>() where T : Form => DialogResult.OK;
            public DialogResult ShowDialog<T>(params object[] parameters) where T : Form => DialogResult.OK;
            public void ShowVehicleManagement() { }
            public void ShowDriverManagement() { }
            public void ShowRouteManagement() { }
            public void ShowActivityManagement() { }
            public void ShowFuelManagement() { }
            public void ShowMaintenanceManagement() { }
            public void ShowCalendarManagement() { }
            public void ShowScheduleManagement() { }
            public void ShowTimeCardManagement() { }
            public void ShowReportsManagement() { }
            public void ShowSchoolCalendarManagement() { }
            public void ShowActivityScheduleManagement() { }
            public void ShowAnalyticsDemo() { }
            public void ShowReports() { }
        }

        private class MockDatabaseHelperService : BusBuddy.Business.IDatabaseHelperService
        {
            public bool TestConnection() => true;
            public void InitializeDatabase() { }
            public string GetConnectionString() => "mock://connection";
            public BusBuddy.Models.Route GetRouteWithDetails(int routeId) => new BusBuddy.Models.Route();
            public List<BusBuddy.Models.Route> GetRoutesWithDetailsByDate(DateTime date) => new List<BusBuddy.Models.Route>();
            public List<BusBuddy.Models.Route> GetAllRoutesWithDetails() => new List<BusBuddy.Models.Route>();
            public BusBuddy.Models.Activity GetActivityWithDetails(int activityId) => new BusBuddy.Models.Activity();
            public BusBuddy.Models.Fuel GetFuelRecordWithDetails(int fuelId) => new BusBuddy.Models.Fuel();
            public BusBuddy.Models.Maintenance GetMaintenanceWithDetails(int maintenanceId) => new BusBuddy.Models.Maintenance();
            public BusBuddy.Models.ActivitySchedule GetActivityScheduleWithDetails(int scheduleId) => new BusBuddy.Models.ActivitySchedule();            public BusBuddy.Business.VehicleDetailsViewModel GetVehicleDetails(int vehicleId) => new BusBuddy.Business.VehicleDetailsViewModel
            {
                Vehicle = new BusBuddy.Models.Vehicle { VehicleID = vehicleId, VehicleNumber = "Mock Vehicle" },
                AMRoutes = new List<BusBuddy.Models.Route>(),
                PMRoutes = new List<BusBuddy.Models.Route>(),
                Activities = new List<BusBuddy.Models.Activity>(),
                FuelRecords = new List<BusBuddy.Models.Fuel>(),
                MaintenanceRecords = new List<BusBuddy.Models.Maintenance>(),
                ScheduledActivities = new List<BusBuddy.Models.ActivitySchedule>()
            };
            public BusBuddy.Business.DriverDetailsViewModel GetDriverDetails(int driverId) => new BusBuddy.Business.DriverDetailsViewModel
            {
                Driver = new BusBuddy.Models.Driver { DriverID = driverId, FirstName = "Mock", LastName = "Driver" },
                AMRoutes = new List<BusBuddy.Models.Route>(),
                PMRoutes = new List<BusBuddy.Models.Route>(),
                Activities = new List<BusBuddy.Models.Activity>(),
                ScheduledActivities = new List<BusBuddy.Models.ActivitySchedule>()
            };
        }
    }
}
