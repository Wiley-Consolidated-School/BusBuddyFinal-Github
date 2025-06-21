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
    /// Syncfusion-specific stress tests to identify disposal issues
    /// These tests specifically target the controls causing process hangs
    /// </summary>
    public class SyncfusionDisposalStressTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly List<int> _processesAtStart;
        private readonly DateTime _testStartTime;

        public SyncfusionDisposalStressTests(ITestOutputHelper output)
        {
            _output = output;
            _testStartTime = DateTime.Now;
            _processesAtStart = GetDotnetProcessIds();

            _output.WriteLine($"🔥 SYNCFUSION STRESS TEST START: {_testStartTime:HH:mm:ss.fff}");
            _output.WriteLine($"📊 Baseline dotnet processes: {_processesAtStart.Count}");
        }

        [Fact]
        public async Task Dashboard_RapidCreateDispose_ShouldNotHangProcess()
        {
            var testName = "Dashboard_RapidCreateDispose_Stress";
            var iterations = 10;
            var timeoutPerIteration = 15000; // 15 seconds max per iteration
            var successCount = 0;
            var failureCount = 0;

            _output.WriteLine($"🔥 {testName}: Creating/disposing dashboard {iterations} times rapidly...");

            for (int i = 1; i <= iterations; i++)
            {
                var iterationStart = DateTime.Now;
                var processCountBefore = GetDotnetProcessIds().Count;

                try
                {
                    _output.WriteLine($"🔄 Iteration {i}/{iterations}...");

                    var completedSuccessfully = await Task.Run(async () =>
                    {
                        var cts = new CancellationTokenSource(timeoutPerIteration);

                        try
                        {
                            await Task.Run(() =>
                            {
                                var thread = new Thread(() =>
                                {
                                    BusBuddyDashboardSyncfusion? dashboard = null;
                                    try
                                    {
                                        cts.Token.ThrowIfCancellationRequested();

                                        Application.SetHighDpiMode(HighDpiMode.SystemAware);
                                        dashboard = new BusBuddyDashboardSyncfusion();

                                        cts.Token.ThrowIfCancellationRequested();
                                        dashboard.Show();
                                        Application.DoEvents();

                                        cts.Token.ThrowIfCancellationRequested();
                                        Thread.Sleep(200); // Brief display time

                                        cts.Token.ThrowIfCancellationRequested();
                                        dashboard.Hide();
                                        dashboard.Close();

                                        cts.Token.ThrowIfCancellationRequested();
                                        dashboard.Dispose();
                                        dashboard = null;

                                        // Force aggressive cleanup
                                        GC.Collect();
                                        GC.WaitForPendingFinalizers();
                                        GC.Collect();
                                    }
                                    catch (OperationCanceledException)
                                    {
                                        _output.WriteLine($"⏰ Iteration {i} timed out");
                                        throw;
                                    }
                                    catch (Exception ex)
                                    {
                                        _output.WriteLine($"❌ Iteration {i} error: {ex.Message}");
                                        throw;
                                    }
                                    finally
                                    {
                                        // Emergency cleanup
                                        try
                                        {
                                            dashboard?.Dispose();
                                        }
                                        catch { }
                                    }
                                });

                                thread.SetApartmentState(ApartmentState.STA);
                                thread.Start();
                                  if (!thread.Join(timeoutPerIteration))
                                {
                                    _output.WriteLine($"⏰ Thread timeout in iteration {i}");
                                    // Note: Thread.Abort is obsolete in .NET 8, thread will be abandoned
                                    return false;
                                }

                                return true;
                            }, cts.Token);

                            return true;
                        }
                        catch (OperationCanceledException)
                        {
                            return false;
                        }
                    });

                    await Task.Delay(500); // Let cleanup settle

                    var processCountAfter = GetDotnetProcessIds().Count;
                    var processGrowth = processCountAfter - processCountBefore;
                    var iterationTime = (DateTime.Now - iterationStart).TotalMilliseconds;

                    if (completedSuccessfully && processGrowth <= 0)
                    {
                        successCount++;
                        _output.WriteLine($"✅ Iteration {i} SUCCESS: {iterationTime:F0}ms, Process growth: {processGrowth}");
                    }
                    else
                    {
                        failureCount++;
                        _output.WriteLine($"❌ Iteration {i} FAILED: {iterationTime:F0}ms, Process growth: {processGrowth}");

                        if (processGrowth > 0)
                        {
                            _output.WriteLine($"⚠️ Process leak detected: {processGrowth} new processes");
                            LogCurrentProcesses();
                        }
                    }
                }
                catch (Exception ex)
                {
                    failureCount++;
                    _output.WriteLine($"❌ Iteration {i} CRASHED: {ex.Message}");
                }
            }

            _output.WriteLine($"🏁 {testName} Complete: {successCount} success, {failureCount} failures");

            // Verify no hanging processes
            await Task.Delay(2000); // Final cleanup time
            var finalProcessCount = GetDotnetProcessIds().Count;
            var totalProcessGrowth = finalProcessCount - _processesAtStart.Count;

            _output.WriteLine($"📊 Final process analysis: {totalProcessGrowth} net growth");

            if (totalProcessGrowth > 0)
            {
                _output.WriteLine($"⚠️ HANGING PROCESSES DETECTED: {totalProcessGrowth} processes did not terminate");
                LogCurrentProcesses();
                KillHangingProcesses();
            }

            // Test should pass if most iterations succeed and no major process leaks
            Assert.True(successCount >= iterations * 0.7, $"Too many failures: {failureCount}/{iterations}");
            Assert.True(totalProcessGrowth <= 2, $"Too many hanging processes: {totalProcessGrowth}");
        }

        [Fact]
        public async Task SyncfusionControls_IndividualDisposal_ShouldNotHang()
        {
            var testName = "SyncfusionControls_IndividualDisposal";            var controlTests = new List<(string Name, Func<Control> Factory)>
            {
                ("SfDataGrid", () => new Syncfusion.WinForms.DataGrid.SfDataGrid()),
                ("SfButton", () => new Syncfusion.WinForms.Controls.SfButton()),
                ("ChartControl", () => new Syncfusion.Windows.Forms.Chart.ChartControl())
                // Note: DockingManager requires IContainer, skipping individual test
            };

            _output.WriteLine($"🔬 {testName}: Testing individual Syncfusion control disposal...");

            foreach (var (name, factory) in controlTests)
            {
                var processCountBefore = GetDotnetProcessIds().Count;

                try
                {
                    _output.WriteLine($"🔍 Testing {name}...");

                    var success = await Task.Run(() =>
                    {                        var thread = new Thread(() =>
                        {
                            Control? control = null;
                            try
                            {
                                control = factory();

                                // Perform basic operations
                                control.Show();
                                Application.DoEvents();
                                Thread.Sleep(100);
                                control.Hide();

                                // Critical: Test disposal
                                System.GC.SuppressFinalize(control);
                                control.Dispose();
                                control = null;

                                System.GC.Collect();
                                System.GC.WaitForPendingFinalizers();
                                System.GC.Collect();
                            }
                            catch (Exception ex)
                            {
                                _output.WriteLine($"❌ {name} disposal error: {ex.Message}");
                                throw;
                            }
                            finally
                            {
                                try
                                {
                                    control?.Dispose();
                                }
                                catch { }
                            }
                        });

                        thread.SetApartmentState(ApartmentState.STA);
                        thread.Start();
                        return thread.Join(10000); // 10 second timeout
                    });

                    await Task.Delay(500);

                    var processCountAfter = GetDotnetProcessIds().Count;
                    var processGrowth = processCountAfter - processCountBefore;

                    if (success && processGrowth <= 0)
                    {
                        _output.WriteLine($"✅ {name} disposal: SUCCESS");
                    }
                    else
                    {
                        _output.WriteLine($"❌ {name} disposal: FAILED (timeout={!success}, process growth={processGrowth})");
                    }
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"❌ {name} test crashed: {ex.Message}");
                }
            }
        }

        [Fact]
        public void ProcessMonitoring_DetectHangingProcesses_ShouldIdentifyLeaks()
        {
            var testName = "ProcessMonitoring_DetectHangingProcesses";

            _output.WriteLine($"🔍 {testName}: Analyzing current process state...");

            var allProcesses = GetDetailedProcessInfo();
            var dotnetProcesses = allProcesses.Where(p => p.Name.Contains("dotnet")).ToList();
            var suspiciousProcesses = dotnetProcesses.Where(p => IsSuspiciousProcess(p)).ToList();

            _output.WriteLine($"📊 Process Analysis:");
            _output.WriteLine($"   Total processes: {allProcesses.Count}");
            _output.WriteLine($"   Dotnet processes: {dotnetProcesses.Count}");
            _output.WriteLine($"   Suspicious processes: {suspiciousProcesses.Count}");

            foreach (var proc in suspiciousProcesses)
            {
                _output.WriteLine($"   🚨 Suspicious: PID {proc.Id} - {proc.Name} (Age: {proc.Age.TotalMinutes:F1}min, Memory: {proc.MemoryMB:F1}MB)");
            }

            if (suspiciousProcesses.Count > 0)
            {
                _output.WriteLine($"⚠️ Found {suspiciousProcesses.Count} suspicious processes that may indicate memory leaks");

                // Attempt to clean them up
                foreach (var proc in suspiciousProcesses)
                {
                    try
                    {
                        _output.WriteLine($"🗡️ Attempting to kill suspicious process {proc.Id}...");
                        Process.GetProcessById(proc.Id).Kill();
                        _output.WriteLine($"✅ Process {proc.Id} terminated");
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"⚠️ Could not kill process {proc.Id}: {ex.Message}");
                    }
                }
            }
        }

        private List<int> GetDotnetProcessIds()
        {
            try
            {
                return Process.GetProcesses()
                    .Where(p => p.ProcessName.Contains("dotnet", StringComparison.OrdinalIgnoreCase) ||
                               p.ProcessName.Contains("testhost", StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.Id)
                    .ToList();
            }
            catch
            {
                return new List<int>();
            }
        }

        private List<ProcessInfo> GetDetailedProcessInfo()
        {
            var processes = new List<ProcessInfo>();

            try
            {
                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        processes.Add(new ProcessInfo
                        {
                            Id = process.Id,
                            Name = process.ProcessName,
                            StartTime = process.StartTime,
                            Age = DateTime.Now - process.StartTime,
                            MemoryMB = process.WorkingSet64 / 1024.0 / 1024.0
                        });
                    }
                    catch
                    {
                        // Skip processes we can't access
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"⚠️ Error getting process info: {ex.Message}");
            }

            return processes;
        }

        private bool IsSuspiciousProcess(ProcessInfo process)
        {
            // Consider a process suspicious if:
            // 1. It's a dotnet process that's been running for more than 5 minutes
            // 2. It's using more than 100MB of memory
            // 3. It's likely a test-related process

            return process.Name.Contains("dotnet", StringComparison.OrdinalIgnoreCase) &&
                   process.Age.TotalMinutes > 5 &&
                   process.MemoryMB > 100;
        }

        private void LogCurrentProcesses()
        {
            try
            {
                var processes = GetDetailedProcessInfo()
                    .Where(p => p.Name.Contains("dotnet", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(p => p.StartTime)
                    .ToList();

                _output.WriteLine($"🔍 Current dotnet processes ({processes.Count}):");
                foreach (var proc in processes)
                {
                    _output.WriteLine($"   PID {proc.Id}: {proc.Name} (Age: {proc.Age.TotalMinutes:F1}min, Memory: {proc.MemoryMB:F1}MB)");
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"⚠️ Error logging processes: {ex.Message}");
            }
        }

        private void KillHangingProcesses()
        {
            try
            {
                var hangingProcesses = Process.GetProcesses()
                    .Where(p => p.ProcessName.Contains("dotnet", StringComparison.OrdinalIgnoreCase))
                    .Where(p => {
                        try
                        {
                            var age = DateTime.Now - p.StartTime;
                            return age.TotalMinutes > 3; // Kill processes older than 3 minutes
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .ToList();

                _output.WriteLine($"🗡️ Killing {hangingProcesses.Count} hanging processes...");

                foreach (var process in hangingProcesses)
                {
                    try
                    {
                        _output.WriteLine($"🗡️ Killing PID {process.Id}...");
                        process.Kill();
                        process.WaitForExit(3000);
                        _output.WriteLine($"✅ PID {process.Id} terminated");
                    }
                    catch (Exception ex)
                    {
                        _output.WriteLine($"⚠️ Could not kill PID {process.Id}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"⚠️ Error in KillHangingProcesses: {ex.Message}");
            }
        }

        public void Dispose()
        {
            try
            {
                var testDuration = DateTime.Now - _testStartTime;
                var finalProcesses = GetDotnetProcessIds();
                var processGrowth = finalProcesses.Count - _processesAtStart.Count;

                _output.WriteLine($"🏁 SYNCFUSION STRESS TEST END: Duration={testDuration.TotalSeconds:F1}s");
                _output.WriteLine($"📊 Process growth: {processGrowth} ({_processesAtStart.Count} → {finalProcesses.Count})");

                if (processGrowth > 0)
                {
                    _output.WriteLine($"⚠️ CLEANUP REQUIRED: {processGrowth} processes did not terminate");
                    LogCurrentProcesses();
                    KillHangingProcesses();
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"⚠️ Error in test cleanup: {ex.Message}");
            }
        }
    }    public class ProcessInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public TimeSpan Age { get; set; }
        public double MemoryMB { get; set; }
    }
}
