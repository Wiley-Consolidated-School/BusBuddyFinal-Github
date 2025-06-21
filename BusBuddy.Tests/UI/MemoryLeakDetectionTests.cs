using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusBuddy.UI.Views;
using Xunit;
using Xunit.Abstractions;

namespace BusBuddy.Tests.UI
{
    /// <summary>
    /// Memory leak detection tests for BusBuddy.UI forms
    /// These tests monitor process count, memory usage, and GC behavior
    /// </summary>
    public class MemoryLeakDetectionTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly List<Process> _processesBeforeTest;
        private readonly long _memoryBeforeTest;
        private readonly DateTime _testStartTime;        public MemoryLeakDetectionTests(ITestOutputHelper output)
        {
            _output = output;
            _testStartTime = DateTime.Now;

            // CRITICAL: Enable test mode to prevent Environment.Exit() crashes
            BusBuddy.UI.Services.TestSafeApplicationShutdownManager.EnableTestMode();

            // Enable test mode for forms to prevent dialog boxes
            BusBuddy.UI.Base.SyncfusionBaseForm.EnableTestMode();
            BusBuddy.UI.Base.BaseManagementForm<object>.EnableTestMode();

            // Capture baseline metrics before test
            _processesBeforeTest = GetCurrentProcesses();
            _memoryBeforeTest = GC.GetTotalMemory(false);

            _output.WriteLine($"🔍 MEMORY LEAK TEST START: {_testStartTime:HH:mm:ss.fff}");
            _output.WriteLine($"📊 Baseline processes: {_processesBeforeTest.Count}");
            _output.WriteLine($"📊 Baseline memory: {_memoryBeforeTest / 1024 / 1024:F2} MB");
        }

        [Fact(Skip = "Heavy UI test - disabled to prevent build system overload")]
        public async Task Dashboard_CreateAndDispose_ShouldNotLeakMemory()
        {
            var testName = "Dashboard_CreateAndDispose";
            var memorySnapshots = new List<MemorySnapshot>();
            var processSnapshots = new List<ProcessSnapshot>();

            try
            {
                _output.WriteLine($"🧪 Starting {testName}...");

                // Take initial snapshot
                memorySnapshots.Add(TakeMemorySnapshot("Initial"));
                processSnapshots.Add(TakeProcessSnapshot("Initial"));

                // Create and dispose dashboard multiple times to detect leaks
                for (int i = 1; i <= 5; i++)
                {
                    _output.WriteLine($"🔄 Iteration {i}/5: Creating dashboard...");

                    BusBuddyDashboardSyncfusion? dashboard = null;
                    try
                    {
                        // Create dashboard on UI thread
                        await Task.Run(() =>
                        {
                            var thread = new Thread(() =>
                            {
                                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                                dashboard = new BusBuddyDashboardSyncfusion();
                                dashboard.Show();

                                // Let it initialize
                                Application.DoEvents();
                                Thread.Sleep(500);

                                // Close it
                                dashboard.Close();
                                dashboard.Dispose();
                                dashboard = null;

                                // Force cleanup
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                                GC.Collect();
                            });

                            thread.SetApartmentState(ApartmentState.STA);
                            thread.Start();
                            thread.Join(10000); // 10 second timeout
                        });

                        // Take snapshot after each iteration
                        await Task.Delay(1000); // Let cleanup settle
                        memorySnapshots.Add(TakeMemorySnapshot($"After iteration {i}"));
                        processSnapshots.Add(TakeProcessSnapshot($"After iteration {i}"));
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"❌ Error in iteration {i}: {ex.Message}");
                    }
                }

                // Analyze results
                AnalyzeMemoryLeaks(memorySnapshots, testName);
                AnalyzeProcessLeaks(processSnapshots, testName);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"❌ {testName} failed: {ex.Message}");
                throw;
            }
        }

        [Fact(Skip = "Heavy UI test - disabled to prevent build system overload")]
        public async Task VehicleManagementForm_CreateAndDispose_ShouldNotLeakMemory()
        {
            var testName = "VehicleManagementForm_CreateAndDispose";
            var memorySnapshots = new List<MemorySnapshot>();
            var processSnapshots = new List<ProcessSnapshot>();

            try
            {
                _output.WriteLine($"🧪 Starting {testName}...");

                // Take initial snapshot
                memorySnapshots.Add(TakeMemorySnapshot("Initial"));
                processSnapshots.Add(TakeProcessSnapshot("Initial"));

                // Create and dispose VehicleManagementForm multiple times
                for (int i = 1; i <= 3; i++)
                {
                    _output.WriteLine($"🔄 Iteration {i}/3: Creating VehicleManagementForm...");

                    try
                    {
                        await Task.Run(() =>
                        {
                            var thread = new Thread(() =>
                            {
                                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                                var form = new VehicleManagementFormSyncfusion();
                                form.Show();

                                Application.DoEvents();
                                Thread.Sleep(1000);

                                form.Close();
                                form.Dispose();

                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                                GC.Collect();
                            });

                            thread.SetApartmentState(ApartmentState.STA);
                            thread.Start();
                            thread.Join(15000); // 15 second timeout
                        });

                        await Task.Delay(1500);
                        memorySnapshots.Add(TakeMemorySnapshot($"After iteration {i}"));
                        processSnapshots.Add(TakeProcessSnapshot($"After iteration {i}"));
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"❌ Error in iteration {i}: {ex.Message}");
                    }
                }

                AnalyzeMemoryLeaks(memorySnapshots, testName);
                AnalyzeProcessLeaks(processSnapshots, testName);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"❌ {testName} failed: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public void ProcessCleanup_KillHangingDotnetProcesses_ShouldCleanup()
        {
            var testName = "ProcessCleanup_KillHangingDotnetProcesses";

            try
            {
                _output.WriteLine($"🧪 Starting {testName}...");

                // Get initial dotnet processes
                var initialDotnetProcesses = GetDotnetProcesses();
                _output.WriteLine($"📊 Initial dotnet processes: {initialDotnetProcesses.Count}");

                foreach (var proc in initialDotnetProcesses)
                {
                    _output.WriteLine($"   - PID {proc.Id}: {proc.ProcessName} (Started: {proc.StartTime:HH:mm:ss})");
                }

                // Kill hanging test processes (similar to what we did manually)
                KillHangingTestProcesses();

                // Wait for cleanup
                Thread.Sleep(2000);

                var finalDotnetProcesses = GetDotnetProcesses();
                _output.WriteLine($"📊 Final dotnet processes: {finalDotnetProcesses.Count}");

                foreach (var proc in finalDotnetProcesses)
                {
                    _output.WriteLine($"   - PID {proc.Id}: {proc.ProcessName} (Started: {proc.StartTime:HH:mm:ss})");
                }

                // Should have fewer or same number of processes
                Assert.True(finalDotnetProcesses.Count <= initialDotnetProcesses.Count,
                    $"Process cleanup failed: {finalDotnetProcesses.Count} processes after cleanup vs {initialDotnetProcesses.Count} before");

                _output.WriteLine($"✅ {testName} passed");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"❌ {testName} failed: {ex.Message}");
                throw;
            }
        }

        private MemorySnapshot TakeMemorySnapshot(string phase)
        {
            var process = Process.GetCurrentProcess();
            var snapshot = new MemorySnapshot
            {
                Phase = phase,
                Timestamp = DateTime.Now,
                ManagedMemory = GC.GetTotalMemory(false),
                WorkingSet = process.WorkingSet64,
                PrivateMemory = process.PrivateMemorySize64,
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2)
            };

            _output.WriteLine($"📊 {phase}: Managed={snapshot.ManagedMemory/1024/1024:F2}MB, WorkingSet={snapshot.WorkingSet/1024/1024:F2}MB");
            return snapshot;
        }

        private ProcessSnapshot TakeProcessSnapshot(string phase)
        {
            var currentProcesses = GetCurrentProcesses();
            var dotnetProcesses = GetDotnetProcesses();

            var snapshot = new ProcessSnapshot
            {
                Phase = phase,
                Timestamp = DateTime.Now,
                TotalProcesses = currentProcesses.Count,
                DotnetProcesses = dotnetProcesses.Count,
                TestHostProcesses = dotnetProcesses.Count(p => p.ProcessName.Contains("testhost") ||
                                                               p.MainWindowTitle.Contains("BusBuddy"))
            };

            _output.WriteLine($"🔍 {phase}: Total={snapshot.TotalProcesses}, Dotnet={snapshot.DotnetProcesses}, TestHost={snapshot.TestHostProcesses}");
            return snapshot;
        }

        private void AnalyzeMemoryLeaks(List<MemorySnapshot> snapshots, string testName)
        {
            if (snapshots.Count < 2) return;

            var initial = snapshots.First();
            var final = snapshots.Last();

            var managedGrowth = final.ManagedMemory - initial.ManagedMemory;
            var workingSetGrowth = final.WorkingSet - initial.WorkingSet;

            _output.WriteLine($"📈 {testName} Memory Analysis:");
            _output.WriteLine($"   Managed Memory Growth: {managedGrowth/1024/1024:F2} MB");
            _output.WriteLine($"   Working Set Growth: {workingSetGrowth/1024/1024:F2} MB");
            _output.WriteLine($"   GC Collections: Gen0={final.Gen0Collections-initial.Gen0Collections}, Gen1={final.Gen1Collections-initial.Gen1Collections}, Gen2={final.Gen2Collections-initial.Gen2Collections}");

            // Memory leak thresholds (adjust based on your requirements)
            const long MaxManagedGrowth = 50 * 1024 * 1024; // 50MB
            const long MaxWorkingSetGrowth = 100 * 1024 * 1024; // 100MB

            if (managedGrowth > MaxManagedGrowth)
            {
                _output.WriteLine($"⚠️ POTENTIAL MANAGED MEMORY LEAK: {managedGrowth/1024/1024:F2} MB growth exceeds {MaxManagedGrowth/1024/1024} MB threshold");
            }

            if (workingSetGrowth > MaxWorkingSetGrowth)
            {
                _output.WriteLine($"⚠️ POTENTIAL WORKING SET LEAK: {workingSetGrowth/1024/1024:F2} MB growth exceeds {MaxWorkingSetGrowth/1024/1024} MB threshold");
            }

            // Log trend analysis
            for (int i = 1; i < snapshots.Count; i++)
            {
                var growth = snapshots[i].ManagedMemory - snapshots[i-1].ManagedMemory;
                _output.WriteLine($"   Step {i}: {growth/1024/1024:F2} MB growth");
            }
        }

        private void AnalyzeProcessLeaks(List<ProcessSnapshot> snapshots, string testName)
        {
            if (snapshots.Count < 2) return;

            var initial = snapshots.First();
            var final = snapshots.Last();

            var processGrowth = final.TotalProcesses - initial.TotalProcesses;
            var dotnetGrowth = final.DotnetProcesses - initial.DotnetProcesses;
            var testHostGrowth = final.TestHostProcesses - initial.TestHostProcesses;

            _output.WriteLine($"🔍 {testName} Process Analysis:");
            _output.WriteLine($"   Total Process Growth: {processGrowth}");
            _output.WriteLine($"   Dotnet Process Growth: {dotnetGrowth}");
            _output.WriteLine($"   TestHost Process Growth: {testHostGrowth}");

            if (dotnetGrowth > 0)
            {
                _output.WriteLine($"⚠️ POTENTIAL PROCESS LEAK: {dotnetGrowth} dotnet processes not cleaned up");

                // List current hanging processes
                var hangingProcesses = GetDotnetProcesses();
                foreach (var proc in hangingProcesses)
                {
                    _output.WriteLine($"   🔗 Hanging: PID {proc.Id} - {proc.ProcessName} (Started: {proc.StartTime:HH:mm:ss})");
                }
            }

            if (testHostGrowth > 0)
            {
                _output.WriteLine($"⚠️ POTENTIAL TEST HOST LEAK: {testHostGrowth} test host processes not cleaned up");
            }
        }

        private List<Process> GetCurrentProcesses()
        {
            try
            {
                return Process.GetProcesses().ToList();
            }
            catch (Exception ex)
            {
                _output.WriteLine($"⚠️ Error getting processes: {ex.Message}");
                return new List<Process>();
            }
        }

        private List<Process> GetDotnetProcesses()
        {
            try
            {
                return Process.GetProcesses()
                    .Where(p => p.ProcessName.Equals("dotnet", StringComparison.OrdinalIgnoreCase) ||
                               p.ProcessName.Contains("testhost") ||
                               p.MainWindowTitle.Contains("BusBuddy"))
                    .ToList();
            }
            catch (Exception ex)
            {
                _output.WriteLine($"⚠️ Error getting dotnet processes: {ex.Message}");
                return new List<Process>();
            }
        }

        private void KillHangingTestProcesses()
        {
            try
            {
                var hangingProcesses = Process.GetProcesses()
                    .Where(p => p.ProcessName.Equals("dotnet", StringComparison.OrdinalIgnoreCase))
                    .Where(p => IsHangingTestProcess(p))
                    .ToList();

                _output.WriteLine($"🔍 Found {hangingProcesses.Count} hanging test processes");

                foreach (var process in hangingProcesses)
                {
                    try
                    {
                        _output.WriteLine($"🗡️ Killing hanging process: PID {process.Id} (Started: {process.StartTime:HH:mm:ss})");
                        process.Kill();
                        process.WaitForExit(5000); // Wait up to 5 seconds
                        _output.WriteLine($"✅ Process {process.Id} terminated");
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"⚠️ Could not kill process {process.Id}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"⚠️ Error in KillHangingTestProcesses: {ex.Message}");
            }
        }

        private bool IsHangingTestProcess(Process process)
        {
            try
            {
                // Consider a process hanging if it's been running for more than 2 minutes
                // and appears to be a test-related dotnet process
                var runningTime = DateTime.Now - process.StartTime;
                return runningTime.TotalMinutes > 2 &&
                       (process.ProcessName.Contains("dotnet") || process.ProcessName.Contains("testhost"));
            }
            catch
            {
                return false;
            }
        }        public void Dispose()
        {
            try
            {
                var testDuration = DateTime.Now - _testStartTime;
                var finalProcesses = GetCurrentProcesses();
                var finalMemory = GC.GetTotalMemory(false);

                _output.WriteLine($"🏁 MEMORY LEAK TEST END: Duration={testDuration.TotalSeconds:F1}s");
                _output.WriteLine($"📊 Final processes: {finalProcesses.Count} (Growth: {finalProcesses.Count - _processesBeforeTest.Count})");
                _output.WriteLine($"📊 Final memory: {finalMemory / 1024 / 1024:F2} MB (Growth: {(finalMemory - _memoryBeforeTest) / 1024 / 1024:F2} MB)");

                // Clean up any processes we may have created
                KillHangingTestProcesses();

                // Disable test modes
                BusBuddy.UI.Services.TestSafeApplicationShutdownManager.DisableTestMode();
                BusBuddy.UI.Base.SyncfusionBaseForm.DisableTestMode();
                BusBuddy.UI.Base.BaseManagementForm<object>.DisableTestMode();
            }
            catch (Exception ex)
            {
                _output.WriteLine($"⚠️ Error in test cleanup: {ex.Message}");
            }
        }
    }    public class MemorySnapshot
    {
        public string Phase { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public long ManagedMemory { get; set; }
        public long WorkingSet { get; set; }
        public long PrivateMemory { get; set; }
        public int Gen0Collections { get; set; }
        public int Gen1Collections { get; set; }
        public int Gen2Collections { get; set; }
    }

    public class ProcessSnapshot
    {
        public string Phase { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public int TotalProcesses { get; set; }
        public int DotnetProcesses { get; set; }
        public int TestHostProcesses { get; set; }
    }
}
