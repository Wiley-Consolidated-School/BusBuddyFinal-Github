using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Implements the Disposable Pattern for managing child processes
    /// Ensures cleanup is idempotent (safe to call multiple times)
    ///
    /// DISPOSABLE PATTERN IMPLEMENTATION (June 27, 2025):
    /// ===================================================
    /// This class provides a standard disposable pattern for managing child processes
    /// in BusBuddy components like the dashboard or services.
    ///
    /// FEATURES:
    /// - Thread-safe disposal using disposeLock
    /// - Idempotent operation - safe to call multiple times
    /// - Comprehensive cleanup of all tracked child processes
    /// - Proper exception handling with logging
    /// - Follows standard .NET IDisposable patterns
    /// - Automatic process cleanup when processes exit naturally
    ///
    /// USAGE:
    /// - Use with 'using' statements for automatic disposal
    /// - Register child processes with RegisterChildProcess()
    /// - Call Dispose() explicitly when needed
    /// - Prevents resource leaks and orphaned processes
    /// </summary>
    public class ProcessManager : IDisposable
    {
        private readonly List<Process> _childProcesses = new List<Process>();
        private readonly object _processLock = new object();
        private bool _disposed = false;
        private readonly object _disposeLock = new object();

        /// <summary>
        /// Register a child process for tracking and automatic cleanup
        /// </summary>
        /// <param name="process">The process to register and track</param>
        public void RegisterChildProcess(Process process)
        {
            if (process == null) return;
            lock (_processLock)
            {
                // Configure process for proper event handling
                process.EnableRaisingEvents = true;
                // Remove from tracking when it exits naturally
                process.Exited += (sender, e) =>
                {
                    lock (_processLock)
                    {
                        _childProcesses.Remove(process);
                        Console.WriteLine($"🔄 Child process {process.Id} exited naturally and removed from tracking");
                    }
                };
                // Add to our tracking list
                _childProcesses.Add(process);
                Console.WriteLine($"📝 ProcessManager registered child process: {process.ProcessName} (ID: {process.Id})");
            }
        }

        /// <summary>
        /// Create a tracked process with proper lifecycle management
        /// </summary>
        /// <param name="startInfo">Process start information</param>
        /// <returns>A configured Process object ready to start</returns>
        public Process CreateTrackedProcess(ProcessStartInfo startInfo)
        {
            var process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };
            // Set up automatic removal when process exits
            process.Exited += (sender, e) =>
            {
                lock (_processLock)
                {
                    _childProcesses.Remove(process);
                    Console.WriteLine($"🔄 Tracked process {process.Id} exited naturally and removed from tracking");
                }
            };
            return process;
        }

        /// <summary>
        /// Start a child process with proper tracking and lifecycle management
        /// </summary>
        /// <param name="startInfo">Process start information</param>
        /// <returns>The started and tracked process</returns>
        public Process StartChildProcess(ProcessStartInfo startInfo)
        {
            var process = CreateTrackedProcess(startInfo);
            lock (_processLock)
            {
                _childProcesses.Add(process);
                process.Start();
                Console.WriteLine($"🚀 ProcessManager started and registered child process: {process.ProcessName} (ID: {process.Id})");
            }
            return process;
        }

        /// <summary>
        /// Gets the number of currently tracked child processes
        /// </summary>
        public int ChildProcessCount
        {
            get
            {
                lock (_processLock)
                {
                    return _childProcesses.Count;
                }
            }
        }

        /// <summary>
        /// Gets whether this ProcessManager has been disposed
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                lock (_disposeLock)
                {
                    return _disposed;
                }
            }
        }

        /// <summary>
        /// Implements the Disposable Pattern with idempotent cleanup
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected disposal method following standard .NET pattern
        /// </summary>
        /// <param name="disposing">True if disposing managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            lock (_disposeLock)
            {
                if (!_disposed && disposing)
                {
                    try
                    {
                        Console.WriteLine("🧹 Disposing ProcessManager resources...");
                        PerformCleanup();
                        _disposed = true;
                        Console.WriteLine("✅ ProcessManager disposal completed successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error during ProcessManager disposal: {ex.Message}");
                        _disposed = true; // Mark as disposed even if failed to prevent retry
                    }
                }
            }
        }

        /// <summary>
        /// Performs the actual cleanup of all tracked child processes
        /// </summary>
        private void PerformCleanup()
        {
            Console.WriteLine("🧹 ProcessManager cleaning up tracked child processes...");
            lock (_processLock)
            {
                // Create a copy to avoid modification issues during iteration
                var processesToCleanup = _childProcesses.ToList();
                foreach (var process in processesToCleanup)
                {
                    try
                    {
                        // Check if process object is valid and still running
                        if (process != null && !process.HasExited)
                        {
                            Console.WriteLine($"🛑 ProcessManager terminating tracked process: {process.ProcessName} (ID: {process.Id})");
                            // Attempt graceful shutdown first
                            try
                            {
                                if (!process.CloseMainWindow())
                                {
                                    // If graceful shutdown fails, force termination
                                    process.Kill(false); // Don't kill descendants to avoid killing ourselves
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                // Process might have exited between HasExited check and termination attempt
                                Console.WriteLine($"⚠️ Process {process.Id} exited during termination attempt");
                            }
                            // Wait for exit with timeout and proper exception handling
                            try
                            {
                                if (!process.WaitForExit(3000))
                                {
                                    Console.WriteLine($"⚠️ Process {process.Id} didn't respond to termination request within 3 seconds");
                                }
                                else
                                {
                                    Console.WriteLine($"✅ Process {process.Id} terminated successfully");
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                // Process already exited or was disposed
                                Console.WriteLine($"⚠️ Process {process.Id} was already disposed during wait");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"✅ Process {process?.Id ?? -1} already exited, skipping termination");
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // Ignore if the process is not associated or already exited
                        Console.WriteLine($"⚠️ Process {process?.Id ?? -1} is not associated or already exited");
                    }
                    catch (Exception ex)
                    {
                        // Log other unexpected errors but continue with cleanup
                        Console.WriteLine($"⚠️ Error cleaning up child process {process?.Id ?? -1}: {ex.Message}");
                    }
                    finally
                    {
                        // Always dispose the process object to free resources
                        try
                        {
                            process?.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error disposing process: {ex.Message}");
                        }
                    }
                }
                // Reset the list after cleanup
                _childProcesses.Clear();
                Console.WriteLine("✅ ProcessManager child process cleanup completed - process list cleared");
            }
        }

        /// <summary>
        /// Finalizer to ensure cleanup if Dispose is not called
        /// </summary>
        ~ProcessManager()
        {
            Dispose(false);
        }
    }
}

