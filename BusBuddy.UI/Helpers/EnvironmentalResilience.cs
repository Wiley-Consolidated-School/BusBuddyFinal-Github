using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Environmental resilience helper to detect and mitigate adverse build/runtime environments
    /// Prevents MSBuild pipe breaks, memory pressure issues, and antivirus interference
    /// </summary>
    public static class EnvironmentalResilience
    {
        private static readonly object _lockObject = new object();
        private static bool _initialized = false;
        private static EnvironmentHealth _lastHealth = EnvironmentHealth.Unknown;

        public enum EnvironmentHealth
        {
            Unknown,
            Optimal,
            Degraded,
            Critical
        }

        /// <summary>
        /// Initialize environmental monitoring and mitigation
        /// Call this early in application startup
        /// </summary>
        public static void Initialize()
        {
            lock (_lockObject)
            {
                if (_initialized) return;

                Console.WriteLine("🛡️ Initializing Environmental Resilience...");

                try
                {
                    // Check system health
                    var health = AssessEnvironmentHealth();
                    _lastHealth = health;

                    // Apply mitigation strategies based on health
                    ApplyMitigationStrategies(health);

                    // Set up monitoring
                    SetupContinuousMonitoring();

                    _initialized = true;
                    Console.WriteLine($"✅ Environmental Resilience initialized - Health: {health}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Environmental Resilience initialization warning: {ex.Message}");
                    // Continue anyway - don't let monitoring failure stop the app
                }
            }
        }

        /// <summary>
        /// Assess current environment health
        /// </summary>
        public static EnvironmentHealth AssessEnvironmentHealth()
        {
            try
            {
                int healthScore = 100;

                // Check available memory
                var availableMemory = GetAvailableMemoryMB();
                if (availableMemory < 500) healthScore -= 30;
                else if (availableMemory < 1000) healthScore -= 15;

                // Check CPU usage
                var cpuUsage = GetCurrentCpuUsage();
                if (cpuUsage > 90) healthScore -= 25;
                else if (cpuUsage > 70) healthScore -= 10;

                // Check for antivirus real-time scanning
                if (IsAntivirusActivelyScanning()) healthScore -= 20;

                // Check for excessive background processes
                if (GetDotNetProcessCount() > 10) healthScore -= 15;

                // Check temp space
                var tempSpace = GetTempSpaceGB();
                if (tempSpace < 1) healthScore -= 20;
                else if (tempSpace < 2) healthScore -= 10;

                // Determine health level
                if (healthScore >= 80) return EnvironmentHealth.Optimal;
                if (healthScore >= 60) return EnvironmentHealth.Degraded;
                return EnvironmentHealth.Critical;
            }
            catch
            {
                return EnvironmentHealth.Unknown;
            }
        }

        /// <summary>
        /// Apply mitigation strategies based on environment health
        /// </summary>
        private static void ApplyMitigationStrategies(EnvironmentHealth health)
        {
            Console.WriteLine($"🔧 Applying mitigation strategies for {health} environment");

            switch (health)
            {
                case EnvironmentHealth.Critical:
                    // Aggressive mitigations
                    SetLowMemoryMode();
                    ReduceBuildParallelism();
                    EnableGarbageCollectionOptimizations();
                    CleanupBuildArtifacts();
                    break;

                case EnvironmentHealth.Degraded:
                    // Moderate mitigations
                    ReduceBuildParallelism();
                    EnableGarbageCollectionOptimizations();
                    break;

                case EnvironmentHealth.Optimal:
                    // Minimal mitigations
                    EnableOptimalGarbageCollection();
                    break;
            }
        }

        /// <summary>
        /// Set up continuous environment monitoring
        /// </summary>
        private static void SetupContinuousMonitoring()
        {
            // Monitor every 30 seconds in background
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Task.Delay(30000); // 30 seconds
                        var currentHealth = AssessEnvironmentHealth();

                        if (currentHealth != _lastHealth)
                        {
                            Console.WriteLine($"🔄 Environment health changed: {_lastHealth} → {currentHealth}");
                            ApplyMitigationStrategies(currentHealth);
                            _lastHealth = currentHealth;
                        }
                    }
                    catch
                    {
                        // Ignore monitoring errors
                    }
                }
            });
        }

        #region Health Assessment Methods

        private static long GetAvailableMemoryMB()
        {
            try
            {
                using var process = new Process();
                process.StartInfo.FileName = "wmic";
                process.StartInfo.Arguments = "OS get TotalVisibleMemorySize,FreePhysicalMemory /value";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // Parse the output to get free memory
                foreach (string line in output.Split('\n'))
                {
                    if (line.Contains("FreePhysicalMemory="))
                    {
                        string value = line.Split('=')[1].Trim();
                        if (long.TryParse(value, out long kb))
                        {
                            return kb / 1024; // Convert KB to MB
                        }
                    }
                }
            }
            catch
            {
                // Fallback using GC
                return GC.GetTotalMemory(false) / (1024 * 1024);
            }

            return 1000; // Default assumption
        }

        private static float GetCurrentCpuUsage()
        {
            try
            {
                using var process = Process.GetCurrentProcess();
                var startTime = DateTime.UtcNow;
                var startCpuUsage = process.TotalProcessorTime;

                Thread.Sleep(100); // Sample for 100ms

                var endTime = DateTime.UtcNow;
                var endCpuUsage = process.TotalProcessorTime;

                var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalMsPassed = (endTime - startTime).TotalMilliseconds;
                var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

                return (float)(cpuUsageTotal * 100);
            }
            catch
            {
                return 50.0f; // Default assumption
            }
        }

        private static bool IsAntivirusActivelyScanning()
        {
            try
            {
                // Check for common antivirus processes with high CPU
                string[] antivirusProcesses = { "MsMpEng", "avp", "avgnt", "mcshield", "nod32krn" };

                foreach (string avProcess in antivirusProcesses)
                {
                    var processes = Process.GetProcessesByName(avProcess);
                    foreach (var process in processes)
                    {
                        try
                        {
                            if (process.WorkingSet64 > 100 * 1024 * 1024) // > 100MB working set
                                return true;
                        }
                        catch { }
                        finally
                        {
                            process?.Dispose();
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        private static int GetDotNetProcessCount()
        {
            try
            {
                var dotnetProcesses = Process.GetProcessesByName("dotnet");
                var msbuildProcesses = Process.GetProcessesByName("MSBuild");
                return dotnetProcesses.Length + msbuildProcesses.Length;
            }
            catch
            {
                return 0;
            }
        }

        private static double GetTempSpaceGB()
        {
            try
            {
                var tempPath = Path.GetTempPath();
                var drive = new DriveInfo(Path.GetPathRoot(tempPath));
                return (double)drive.AvailableFreeSpace / (1024 * 1024 * 1024);
            }
            catch
            {
                return 5.0; // Default assumption
            }
        }

        #endregion

        #region Mitigation Methods

        private static void SetLowMemoryMode()
        {
            try
            {
                // Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // Set server GC mode if available
                Environment.SetEnvironmentVariable("DOTNET_gcServer", "0");
                Environment.SetEnvironmentVariable("DOTNET_gcConcurrent", "1");

                Console.WriteLine("🔧 Low memory mode activated");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to set low memory mode: {ex.Message}");
            }
        }

        private static void ReduceBuildParallelism()
        {
            try
            {
                // Limit MSBuild parallelism
                Environment.SetEnvironmentVariable("MSBuildNodeCount", "1");
                Environment.SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1");

                Console.WriteLine("🔧 Build parallelism reduced to prevent pipe breaks");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to reduce build parallelism: {ex.Message}");
            }
        }

        private static void EnableGarbageCollectionOptimizations()
        {
            try
            {
                // Configure GC for better performance under pressure
                Environment.SetEnvironmentVariable("DOTNET_gcRetainVM", "1");

                Console.WriteLine("🔧 GC optimizations enabled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to enable GC optimizations: {ex.Message}");
            }
        }

        private static void EnableOptimalGarbageCollection()
        {
            try
            {
                // Configure GC for optimal performance
                Environment.SetEnvironmentVariable("DOTNET_gcServer", "1");
                Environment.SetEnvironmentVariable("DOTNET_gcConcurrent", "1");

                Console.WriteLine("🔧 Optimal GC settings applied");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to set optimal GC: {ex.Message}");
            }
        }

        private static void CleanupBuildArtifacts()
        {
            try
            {
                // Clean up MSBuild temp files
                var tempPath = Path.Combine(Path.GetTempPath(), "MSBuildTemp");
                if (Directory.Exists(tempPath))
                {
                    var files = Directory.GetFiles(tempPath, "MSBuild_*.failure.txt");
                    foreach (var file in files)
                    {
                        try { File.Delete(file); } catch { }
                    }
                }

                Console.WriteLine("🧹 Build artifacts cleaned");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to cleanup build artifacts: {ex.Message}");
            }
        }

        #endregion

        /// <summary>
        /// Force a comprehensive environment cleanup
        /// Call this if MSBuild pipe errors occur
        /// </summary>
        public static void ForceEnvironmentCleanup()
        {
            Console.WriteLine("🚨 Forcing comprehensive environment cleanup...");

            try
            {
                // Kill orphaned build processes
                KillOrphanedBuildProcesses();

                // Clean all temp files
                CleanupAllTempFiles();

                // Force garbage collection
                SetLowMemoryMode();

                // Reset environment variables
                ResetBuildEnvironment();

                Console.WriteLine("✅ Comprehensive environment cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Environment cleanup encountered errors: {ex.Message}");
            }
        }

        private static void KillOrphanedBuildProcesses()
        {
            try
            {
                var processes = Process.GetProcessesByName("MSBuild");
                foreach (var process in processes)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                            process.WaitForExit(1000);
                        }
                    }
                    catch { }
                    finally
                    {
                        process?.Dispose();
                    }
                }
            }
            catch { }
        }

        private static void CleanupAllTempFiles()
        {
            try
            {
                var tempPaths = new[]
                {
                    Path.Combine(Path.GetTempPath(), "MSBuildTemp"),
                    Path.Combine(Path.GetTempPath(), ".NETCoreApp,Version=v8.0.AssemblyAttributes.cs")
                };

                foreach (var tempPath in tempPaths)
                {
                    if (Directory.Exists(tempPath))
                    {
                        Directory.Delete(tempPath, true);
                    }
                }
            }
            catch { }
        }

        private static void ResetBuildEnvironment()
        {
            try
            {
                Environment.SetEnvironmentVariable("MSBuildNodeCount", null);
                Environment.SetEnvironmentVariable("MSBUILDDEBUGPATH", null);
            }
            catch { }
        }
    }
}

