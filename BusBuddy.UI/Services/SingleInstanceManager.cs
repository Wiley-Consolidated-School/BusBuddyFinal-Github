using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Professional single instance manager using mutex and named pipes
    /// Ensures only one instance of BusBuddy can run at a time
    /// Based on industry best practices for Windows applications
    /// </summary>
    public class SingleInstanceManager : IDisposable
    {
        private const string MUTEX_NAME = "Global\\BusBuddy_SingleInstance_Mutex_{0}";
        private const string PIPE_NAME = "BusBuddy_Communication_Pipe_{0}";
        private const int PIPE_TIMEOUT_MS = 5000;

        private readonly string _uniqueId;
        private readonly string _mutexName;
        private readonly string _pipeName;
        private Mutex _applicationMutex;
        private NamedPipeServerStream _pipeServer;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _disposed = false;
        private bool _ownsMutex = false;

        public event EventHandler<string[]> SecondInstanceDetected;

        /// <summary>
        /// Initialize single instance manager with unique application identifier
        /// </summary>
        /// <param name="uniqueId">Unique identifier for the application (typically GUID or app name)</param>
        public SingleInstanceManager(string uniqueId = "BusBuddy")
        {
            _uniqueId = uniqueId ?? throw new ArgumentNullException(nameof(uniqueId));
            _mutexName = string.Format(MUTEX_NAME, _uniqueId);
            _pipeName = string.Format(PIPE_NAME, _uniqueId);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Attempts to acquire single instance lock
        /// Returns true if this is the first instance, false if another instance is already running
        /// </summary>
        public bool TryAcquireLock()
        {
            try
            {
                Console.WriteLine($"🔒 Attempting to acquire single instance lock: {_mutexName}");

                // Create or open the named mutex
                _applicationMutex = new Mutex(true, _mutexName, out bool createdNew);
                _ownsMutex = createdNew;

                if (createdNew)
                {
                    Console.WriteLine("✅ Single instance lock acquired - this is the primary instance");
                    StartNamedPipeServer();
                    return true;
                }
                else
                {
                    Console.WriteLine("⚠️ Another instance is already running");
                    return false;
                }
            }
            catch (AbandonedMutexException ex)
            {
                // Previous instance crashed and left mutex abandoned - we can safely take over
                Console.WriteLine($"⚠️ Mutex was abandoned (previous instance crashed): {ex.Message}");
                Console.WriteLine("✅ Taking over abandoned mutex - this is now the primary instance");
                _ownsMutex = true;
                StartNamedPipeServer();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error acquiring single instance lock: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sends command line arguments to the existing instance
        /// </summary>
        /// <param name="args">Command line arguments to send</param>
        /// <returns>True if successfully communicated with existing instance</returns>
        public async Task<bool> SendArgsToExistingInstance(string[] args)
        {
            try
            {
                Console.WriteLine($"📤 Sending arguments to existing instance via pipe: {_pipeName}");

                using var pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out);

                // Try to connect to the existing instance
                await pipeClient.ConnectAsync(PIPE_TIMEOUT_MS);

                // Send the arguments as JSON
                var argsJson = System.Text.Json.JsonSerializer.Serialize(args);
                var buffer = Encoding.UTF8.GetBytes(argsJson);

                await pipeClient.WriteAsync(buffer, 0, buffer.Length);
                await pipeClient.FlushAsync();

                Console.WriteLine("✅ Successfully sent arguments to existing instance");
                return true;
            }
            catch (TimeoutException)
            {
                Console.WriteLine("⚠️ Timeout connecting to existing instance - it may be unresponsive");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error communicating with existing instance: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Brings the existing instance to the foreground
        /// </summary>
        public bool BringExistingInstanceToFront()
        {
            try
            {
                // Find the main window of the existing BusBuddy process
                var processes = Process.GetProcessesByName("BusBuddy");
                foreach (var process in processes)
                {
                    if (process.Id != Process.GetCurrentProcess().Id && process.MainWindowHandle != IntPtr.Zero)
                    {
                        Console.WriteLine($"🔍 Found existing instance with PID: {process.Id}");

                        // Restore window if minimized
                        if (IsIconic(process.MainWindowHandle))
                        {
                            ShowWindow(process.MainWindowHandle, SW_RESTORE);
                        }

                        // Bring to foreground
                        SetForegroundWindow(process.MainWindowHandle);

                        Console.WriteLine("✅ Brought existing instance to foreground");
                        return true;
                    }
                }

                Console.WriteLine("⚠️ Could not find main window of existing instance");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error bringing existing instance to front: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Forcefully terminates any orphaned BusBuddy processes
        /// Use with caution - only for cleanup scenarios
        /// </summary>
        public static void CleanupOrphanedProcesses()
        {
            try
            {
                Console.WriteLine("🧹 Cleaning up orphaned BusBuddy processes...");

                var currentProcessId = Process.GetCurrentProcess().Id;
                var processes = Process.GetProcessesByName("BusBuddy");

                foreach (var process in processes)
                {
                    if (process.Id != currentProcessId)
                    {
                        try
                        {
                            // CRITICAL FIX: Validate process before attempting operations
                            if (process == null || process.HasExited)
                            {
                                Console.WriteLine("⚠️ Process already exited or null, skipping");
                                continue;
                            }

                            Console.WriteLine($"🗑️ Terminating orphaned process PID: {process.Id}");
                            process.Kill();

                            // Wait with proper validation
                            if (!process.WaitForExit(3000)) // Wait up to 3 seconds
                            {
                                Console.WriteLine($"⚠️ Process {process.Id} did not exit within timeout");
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            Console.WriteLine($"⚠️ Process {process?.Id ?? -1} access error: {ex.Message}");
                            // This is expected if process already terminated
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Could not terminate process {process.Id}: {ex.Message}");
                        }
                    }
                }

                // Also cleanup any dotnet processes that might be running BusBuddy
                var dotnetProcesses = Process.GetProcessesByName("dotnet");
                foreach (var process in dotnetProcesses)
                {
                    try
                    {
                        // Check if this dotnet process is running BusBuddy
                        if (process.MainModule?.FileName?.Contains("BusBuddy") == true ||
                            process.ProcessName.Contains("BusBuddy"))
                        {
                            Console.WriteLine($"🗑️ Terminating BusBuddy dotnet process PID: {process.Id}");
                            process.Kill();
                            process.WaitForExit(3000);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Could not check/terminate dotnet process {process.Id}: {ex.Message}");
                    }
                }

                Console.WriteLine("✅ Cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Starts the named pipe server to listen for communication from secondary instances
        /// </summary>
        private void StartNamedPipeServer()
        {
            Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        _pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                        Console.WriteLine($"🔗 Named pipe server listening: {_pipeName}");

                        // Wait for client connection
                        await _pipeServer.WaitForConnectionAsync(_cancellationTokenSource.Token);

                        Console.WriteLine("📥 Received connection from secondary instance");

                        // Read the data
                        var buffer = new byte[4096];
                        int bytesRead = await _pipeServer.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);

                        if (bytesRead > 0)
                        {
                            var argsJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            var args = System.Text.Json.JsonSerializer.Deserialize<string[]>(argsJson);

                            Console.WriteLine($"📨 Received arguments from secondary instance: [{string.Join(", ", args)}]");

                            // Notify the main application
                            SecondInstanceDetected?.Invoke(this, args);
                        }

                        _pipeServer.Disconnect();
                        _pipeServer.Dispose();
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when cancellation is requested
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error in pipe server: {ex.Message}");
                        await Task.Delay(1000, _cancellationTokenSource.Token); // Wait before retrying
                    }
                }
            }, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Releases the single instance lock and cleans up resources
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                Console.WriteLine("🔓 Releasing single instance lock");

                _cancellationTokenSource?.Cancel();
                _pipeServer?.Dispose();
                if (_ownsMutex)
                {
                    _applicationMutex?.ReleaseMutex();
                }
                _applicationMutex?.Dispose();
                _cancellationTokenSource?.Dispose();

                Console.WriteLine("✅ Single instance lock released");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during disposal: {ex.Message}");
            }
            finally
            {
                _disposed = true;
            }
        }

        #region Win32 API Imports
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;
        #endregion
    }
}

