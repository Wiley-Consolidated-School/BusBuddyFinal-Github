using System;
using System.Diagnostics;
using System.Threading;

namespace BusBuddy
{
    /// <summary>
    /// Process monitor to detect what form is actually running
    /// </summary>
    internal static class ProcessMonitor
    {
        static void Main(string[] args)
        {
            Console.WriteLine("[MONITOR] Starting BusBuddy process monitor...");

            try
            {
                // Start the main BusBuddy application
                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "run --project BusBuddy.csproj",
                    WorkingDirectory = @"c:\Users\steve.mckitrick\Desktop\BusBuddy",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false
                };

                Console.WriteLine("[MONITOR] Starting BusBuddy application...");
                var process = Process.Start(startInfo);

                if (process != null)
                {
                    Console.WriteLine($"[MONITOR] Process started with PID: {process.Id}");

                    // Monitor for 10 seconds
                    for (int i = 0; i < 10; i++)
                    {
                        Thread.Sleep(1000);

                        if (process.HasExited)
                        {
                            Console.WriteLine($"[MONITOR] Process exited with code: {process.ExitCode}");
                            break;
                        }

                        Console.WriteLine($"[MONITOR] Process still running... {i + 1}/10 seconds");
                    }

                    // Try to get output
                    if (!process.HasExited)
                    {
                        Console.WriteLine("[MONITOR] Process is still running. Reading output...");

                        // Read any available output
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();

                        if (!string.IsNullOrEmpty(output))
                        {
                            Console.WriteLine("[MONITOR] STDOUT:");
                            Console.WriteLine(output);
                        }

                        if (!string.IsNullOrEmpty(error))
                        {
                            Console.WriteLine("[MONITOR] STDERR:");
                            Console.WriteLine(error);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("[MONITOR] Failed to start process");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MONITOR] Error: {ex.Message}");
            }

            Console.WriteLine("[MONITOR] Press any key to exit...");
            Console.ReadKey();
        }
    }
}
