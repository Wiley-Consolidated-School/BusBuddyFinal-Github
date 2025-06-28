using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace BusBuddy.Build.Helpers
{
    /// <summary>
    /// Build resilience helper to prevent and recover from MSBuild failures
    /// Specifically targets pipe communication issues and build process conflicts
    /// </summary>
    public static class BuildResilience
    {
        /// <summary>
        /// Execute a dotnet build command with automatic retry and error recovery
        /// </summary>
        public static async Task<bool> ExecuteResilientBuildAsync(string solutionPath, int maxRetries = 3)
        {
            Console.WriteLine($"🔨 Starting resilient build of {solutionPath}");

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    Console.WriteLine($"📝 Build attempt {attempt}/{maxRetries}");

                    // Pre-build cleanup if this is a retry
                    if (attempt > 1)
                    {
                        await PerformPreBuildCleanupAsync(solutionPath);
                    }

                    // Execute the build
                    var success = await ExecuteSingleBuildAttemptAsync(solutionPath);

                    if (success)
                    {
                        Console.WriteLine($"✅ Build succeeded on attempt {attempt}");
                        return true;
                    }

                    Console.WriteLine($"❌ Build failed on attempt {attempt}");

                    // If not the last attempt, wait and prepare for retry
                    if (attempt < maxRetries)
                    {
                        Console.WriteLine($"⏳ Waiting before retry attempt {attempt + 1}...");
                        await Task.Delay(2000 * attempt); // Progressive backoff

                        // Force environment cleanup
                        BusBuddy.UI.Helpers.EnvironmentalResilience.ForceEnvironmentCleanup();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"💥 Build attempt {attempt} threw exception: {ex.Message}");

                    if (attempt == maxRetries)
                    {
                        Console.WriteLine("❌ All build attempts failed");
                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Perform pre-build cleanup to prevent MSBuild pipe issues
        /// </summary>
        private static async Task PerformPreBuildCleanupAsync(string solutionPath)
        {
            Console.WriteLine("🧹 Performing pre-build cleanup...");

            try
            {
                // Step 1: Kill any orphaned MSBuild processes
                await KillOrphanedBuildProcessesAsync();

                // Step 2: Clean solution
                await ExecuteCleanCommandAsync(solutionPath);

                // Step 3: Remove build artifacts
                CleanBuildArtifacts(solutionPath);

                // Step 4: Clear MSBuild temp files
                ClearMSBuildTempFiles();

                Console.WriteLine("✅ Pre-build cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Pre-build cleanup warning: {ex.Message}");
                // Continue anyway - cleanup errors shouldn't stop the build
            }
        }

        /// <summary>
        /// Execute a single build attempt with pipe break detection
        /// </summary>
        private static async Task<bool> ExecuteSingleBuildAttemptAsync(string solutionPath)
        {
            try
            {
                using var process = new Process();
                process.StartInfo.FileName = "dotnet";
                process.StartInfo.Arguments = $"build \"{solutionPath}\" --verbosity minimal --nologo";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                // Set environment variables to reduce pipe pressure
                process.StartInfo.Environment["MSBuildNodeCount"] = "1";
                process.StartInfo.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";

                var outputTask = Task.Run(() => process.StandardOutput.ReadToEnd());
                var errorTask = Task.Run(() => process.StandardError.ReadToEnd());

                process.Start();

                // Wait for completion with timeout
                var completed = process.WaitForExit(300000); // 5 minute timeout

                if (!completed)
                {
                    Console.WriteLine("⏰ Build timeout - killing process");
                    try { process.Kill(); } catch { }
                    return false;
                }

                var output = await outputTask;
                var error = await errorTask;

                // Check for pipe break indicators
                if (error.Contains("Pipe is broken") ||
                    error.Contains("NodeEndpointOutOfProcBase") ||
                    error.Contains("exited prematurely"))
                {
                    Console.WriteLine("🚰 MSBuild pipe break detected");
                    return false;
                }

                // Check exit code
                if (process.ExitCode == 0)
                {
                    return true;
                }

                Console.WriteLine($"Build failed with exit code: {process.ExitCode}");
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Error output: {error}");
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Build execution error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kill orphaned MSBuild and dotnet processes
        /// </summary>
        private static async Task KillOrphanedBuildProcessesAsync()
        {
            try
            {
                var processNames = new[] { "MSBuild", "dotnet", "VBCSCompiler" };

                foreach (var processName in processNames)
                {
                    var processes = Process.GetProcessesByName(processName);
                    foreach (var process in processes)
                    {
                        try
                        {
                            // Only kill if it's been running for more than 5 minutes
                            if (DateTime.Now - process.StartTime > TimeSpan.FromMinutes(5))
                            {
                                Console.WriteLine($"🔪 Killing orphaned process: {processName} (PID: {process.Id})");
                                process.Kill();
                                await Task.Delay(100); // Give it time to die
                            }
                        }
                        catch
                        {
                            // Ignore errors killing individual processes
                        }
                        finally
                        {
                            process?.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error killing orphaned processes: {ex.Message}");
            }
        }

        /// <summary>
        /// Execute dotnet clean command
        /// </summary>
        private static async Task ExecuteCleanCommandAsync(string solutionPath)
        {
            try
            {
                using var process = new Process();
                process.StartInfo.FileName = "dotnet";
                process.StartInfo.Arguments = $"clean \"{solutionPath}\" --nologo";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                await Task.Run(() => process.WaitForExit(30000)); // 30 second timeout

                if (!process.HasExited)
                {
                    try { process.Kill(); } catch { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Clean command warning: {ex.Message}");
            }
        }

        /// <summary>
        /// Clean build artifacts manually
        /// </summary>
        private static void CleanBuildArtifacts(string solutionPath)
        {
            try
            {
                var solutionDir = Path.GetDirectoryName(solutionPath);
                if (string.IsNullOrEmpty(solutionDir)) return;

                // Clean bin and obj directories
                var dirsToClean = new[] { "bin", "obj", "TestResults" };

                foreach (var dirName in dirsToClean)
                {
                    var dirs = Directory.GetDirectories(solutionDir, dirName, SearchOption.AllDirectories);
                    foreach (var dir in dirs)
                    {
                        try
                        {
                            Directory.Delete(dir, true);
                        }
                        catch
                        {
                            // Ignore individual cleanup failures
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Build artifacts cleanup warning: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear MSBuild temp files that can cause pipe issues
        /// </summary>
        private static void ClearMSBuildTempFiles()
        {
            try
            {
                var tempPath = Path.Combine(Path.GetTempPath(), "MSBuildTemp");
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }

                // Also clean any .failure.txt files
                var tempFiles = Directory.GetFiles(Path.GetTempPath(), "MSBuild_*.failure.txt");
                foreach (var file in tempFiles)
                {
                    try { File.Delete(file); } catch { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ MSBuild temp cleanup warning: {ex.Message}");
            }
        }
    }
}

