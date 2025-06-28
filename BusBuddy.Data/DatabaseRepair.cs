using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace BusBuddy.Data
{
    /// <summary>
    /// Database repair utility that provides an interface for the application to diagnose and fix
    /// database connectivity issues, especially the "database offline" state.
    /// </summary>
    public static class DatabaseRepair
    {
        private static readonly string RepairScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fix-database.ps1");

        /// <summary>
        /// Diagnoses database issues and attempts to fix them by calling the fix-database.ps1 script
        /// </summary>
        /// <returns>True if repair was successful, false otherwise</returns>
        public static async Task<bool> RepairDatabaseAsync()
        {
            Console.WriteLine("Starting database repair process...");

            try
            {
                // First, try using the enhanced diagnostic tool
                Console.WriteLine("Attempting in-process database repair...");
                bool repairSuccess = await DatabaseDiagnosticsEnhanced.RepairDatabaseAsync();

                if (repairSuccess)
                {
                    Console.WriteLine("✅ In-process database repair successful!");
                    return true;
                }

                // If in-process repair fails, try running the PowerShell script
                Console.WriteLine("In-process repair failed, running PowerShell repair script...");
                return await RunRepairScriptAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during database repair: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Runs the PowerShell repair script to fix database issues
        /// </summary>
        private static async Task<bool> RunRepairScriptAsync()
        {
            if (!File.Exists(RepairScriptPath))
            {
                Console.WriteLine($"❌ Repair script not found at: {RepairScriptPath}");
                return false;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{RepairScriptPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    Console.WriteLine("Starting PowerShell repair script...");
                    process.Start();

                    // Read output asynchronously
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    await Task.WhenAll(
                        Task.Run(() => process.WaitForExit()),
                        outputTask,
                        errorTask
                    );

                    string output = await outputTask;
                    string error = await errorTask;

                    // Log the output
                    Console.WriteLine("Repair script output:");
                    Console.WriteLine(output);

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("Repair script errors:");
                        Console.WriteLine(error);
                    }

                    // Check if repair was successful by looking for success message in output
                    bool success = output.Contains("SUCCESS: Database is now online and accessible");
                    Console.WriteLine($"Database repair script {(success ? "succeeded" : "failed")}");

                    return success;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running repair script: {ex.Message}");
                return false;
            }
        }
    }
}
