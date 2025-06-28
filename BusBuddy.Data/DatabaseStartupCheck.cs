using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusBuddy.Data;
using Microsoft.EntityFrameworkCore;

namespace BusBuddy
{
    /// <summary>
    /// Utility class to check and repair database connection on application startup
    /// </summary>
    public static class DatabaseStartupCheck
    {
        /// <summary>
        /// Checks database connectivity and attempts repair if needed
        /// </summary>
        /// <returns>True if database is accessible or was successfully repaired</returns>
        public static async Task<bool> CheckAndRepairDatabaseAsync()
        {
            Console.WriteLine("Checking database connectivity on startup...");

            try
            {
                // Try to get a database context
                using (var context = DatabaseConfiguration.CreateContext())
                {
                    // Try to access a database entity to verify connectivity
                    var vehicleCount = await context.Vehicles.CountAsync();
                    Console.WriteLine($"✅ Database connection successful! Found {vehicleCount} vehicles.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database connection failed: {ex.Message}");

                // Check if this is a database offline issue
                bool isDatabaseOffline = ex.Message.Contains("offline") ||
                                       ex.Message.Contains("database is not accessible") ||
                                       ex.Message.Contains("Cannot open database");

                if (isDatabaseOffline)
                {
                    var result = MessageBox.Show(
                        "The BusBuddy database appears to be offline or inaccessible. Would you like to attempt to repair it now?",
                        "Database Connection Error",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.Yes)
                    {
                        Console.WriteLine("Attempting to repair database...");
                        bool repaired = await DatabaseRepair.RepairDatabaseAsync();

                        if (repaired)
                        {
                            MessageBox.Show(
                                "Database repair completed successfully! The application will now continue.",
                                "Database Repair",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            );
                            return true;
                        }
                        else
                        {
                            MessageBox.Show(
                                "Database repair failed. Please check the SQL Server service or try running the fix-database.ps1 script manually.",
                                "Database Repair Failed",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("User declined database repair");
                        return false;
                    }
                }
                else
                {
                    // If it's not a database offline issue, just log and continue
                    Console.WriteLine("This appears to be a different type of database error, not an offline issue");
                    return false;
                }
            }
        }
    }
}
