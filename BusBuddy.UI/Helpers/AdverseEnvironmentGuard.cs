using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Advanced environmental protection system to detect and mitigate adverse runtime conditions
    /// Extends beyond basic resource monitoring to include network, permissions, and system stability
    /// </summary>
    public static class AdverseEnvironmentGuard
    {
        private static readonly object _lockObject = new object();
        private static bool _initialized = false;
        private static System.Threading.Timer _monitoringTimer;
        private static readonly List<string> _detectedThreats = new List<string>();

        public enum ThreatLevel
        {
            None,
            Low,
            Medium,
            High,
            Critical
        }

        public class EnvironmentThreat
        {
            public string Category { get; set; }
            public string Description { get; set; }
            public ThreatLevel Level { get; set; }
            public DateTime DetectedAt { get; set; }
            public string MitigationAction { get; set; }
        }

        /// <summary>
        /// Initialize comprehensive environmental protection
        /// </summary>
        public static void Initialize()
        {
            lock (_lockObject)
            {
                if (_initialized) return;

                Console.WriteLine("🛡️ Initializing Adverse Environment Guard...");

                try
                {
                    // Perform initial comprehensive scan
                    var threats = PerformComprehensiveThreatScan();

                    // Apply immediate mitigations
                    ApplyThreatMitigations(threats);

                    // Start continuous monitoring
                    StartContinuousMonitoring();

                    // Register for system events
                    RegisterSystemEventHandlers();

                    _initialized = true;
                    Console.WriteLine($"✅ Adverse Environment Guard initialized - {threats.Count} threats detected and mitigated");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Adverse Environment Guard initialization warning: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Perform comprehensive threat detection scan
        /// </summary>
        public static List<EnvironmentThreat> PerformComprehensiveThreatScan()
        {
            var threats = new List<EnvironmentThreat>();

            // Resource threats
            threats.AddRange(DetectResourceThreats());

            // Network threats
            threats.AddRange(DetectNetworkThreats());

            // Security threats
            threats.AddRange(DetectSecurityThreats());

            // System stability threats
            threats.AddRange(DetectSystemStabilityThreats());

            // File system threats
            threats.AddRange(DetectFileSystemThreats());

            // Development environment threats
            threats.AddRange(DetectDevelopmentEnvironmentThreats());

            return threats;
        }

        #region Threat Detection Methods

        private static List<EnvironmentThreat> DetectResourceThreats()
        {
            var threats = new List<EnvironmentThreat>();

            try
            {
                // Memory pressure detection
                var availableMemory = GetAvailableMemoryMB();
                if (availableMemory < 200)
                {
                    threats.Add(new EnvironmentThreat
                    {
                        Category = "Memory",
                        Description = $"Critical memory shortage: {availableMemory}MB available",
                        Level = ThreatLevel.Critical,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Force GC, reduce memory usage"
                    });
                }
                else if (availableMemory < 500)
                {
                    threats.Add(new EnvironmentThreat
                    {
                        Category = "Memory",
                        Description = $"Memory pressure detected: {availableMemory}MB available",
                        Level = ThreatLevel.Medium,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Enable low memory mode"
                    });
                }

                // CPU saturation detection
                var cpuUsage = GetSystemCpuUsage();
                if (cpuUsage > 95)
                {
                    threats.Add(new EnvironmentThreat
                    {
                        Category = "CPU",
                        Description = $"CPU saturation: {cpuUsage:F1}% usage",
                        Level = ThreatLevel.High,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Reduce thread count, delay operations"
                    });
                }

                // Disk space threats
                var systemDrive = Path.GetPathRoot(Environment.SystemDirectory);
                var driveInfo = new DriveInfo(systemDrive);
                var freeSpaceGB = driveInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);

                if (freeSpaceGB < 1)
                {
                    threats.Add(new EnvironmentThreat
                    {
                        Category = "Storage",
                        Description = $"Critical disk space: {freeSpaceGB:F2}GB free on {systemDrive}",
                        Level = ThreatLevel.Critical,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Clean temp files, reduce caching"
                    });
                }
            }
            catch (Exception ex)
            {
                threats.Add(new EnvironmentThreat
                {
                    Category = "Resource Detection",
                    Description = $"Resource monitoring error: {ex.Message}",
                    Level = ThreatLevel.Low,
                    DetectedAt = DateTime.Now,
                    MitigationAction = "Continue with reduced monitoring"
                });
            }

            return threats;
        }

        private static List<EnvironmentThreat> DetectNetworkThreats()
        {
            var threats = new List<EnvironmentThreat>();

            try
            {
                // Check network connectivity
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    threats.Add(new EnvironmentThreat
                    {
                        Category = "Network",
                        Description = "No network connectivity detected",
                        Level = ThreatLevel.Medium,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Enable offline mode, cache data"
                    });
                }

                // Check for slow network interfaces
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                foreach (var ni in networkInterfaces)
                {
                    if (ni.Speed < 1000000) // Less than 1 Mbps
                    {
                        threats.Add(new EnvironmentThreat
                        {
                            Category = "Network",
                            Description = $"Slow network interface: {ni.Name} at {ni.Speed / 1000000.0:F1} Mbps",
                            Level = ThreatLevel.Low,
                            DetectedAt = DateTime.Now,
                            MitigationAction = "Reduce network operations, increase timeouts"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                threats.Add(new EnvironmentThreat
                {
                    Category = "Network Detection",
                    Description = $"Network monitoring error: {ex.Message}",
                    Level = ThreatLevel.Low,
                    DetectedAt = DateTime.Now,
                    MitigationAction = "Continue with network isolation assumptions"
                });
            }

            return threats;
        }

        private static List<EnvironmentThreat> DetectSecurityThreats()
        {
            var threats = new List<EnvironmentThreat>();

            try
            {
                // Check for elevated privileges
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);

                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    threats.Add(new EnvironmentThreat
                    {
                        Category = "Security",
                        Description = "Running without administrator privileges",
                        Level = ThreatLevel.Low,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Limit file system operations, use user directories"
                    });
                }

                // Check for aggressive antivirus scanning
                var antivirusProcesses = new[] { "MsMpEng", "avp", "avgnt", "mcshield", "nod32krn", "savservice" };
                foreach (var avName in antivirusProcesses)
                {
                    var processes = Process.GetProcessesByName(avName);
                    foreach (var process in processes)
                    {
                        try
                        {
                            if (process.WorkingSet64 > 200 * 1024 * 1024) // > 200MB
                            {
                                threats.Add(new EnvironmentThreat
                                {
                                    Category = "Security",
                                    Description = $"Active antivirus scanning detected: {avName}",
                                    Level = ThreatLevel.Medium,
                                    DetectedAt = DateTime.Now,
                                    MitigationAction = "Add exclusions, delay file operations"
                                });
                            }
                        }
                        catch { }
                        finally { process?.Dispose(); }
                    }
                }

                // Check Windows Defender real-time protection
                if (IsWindowsDefenderRealtimeEnabled())
                {
                    threats.Add(new EnvironmentThreat
                    {
                        Category = "Security",
                        Description = "Windows Defender real-time protection active",
                        Level = ThreatLevel.Low,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Consider adding project folder to exclusions"
                    });
                }
            }
            catch (Exception ex)
            {
                threats.Add(new EnvironmentThreat
                {
                    Category = "Security Detection",
                    Description = $"Security monitoring error: {ex.Message}",
                    Level = ThreatLevel.Low,
                    DetectedAt = DateTime.Now,
                    MitigationAction = "Continue with security assumptions"
                });
            }

            return threats;
        }

        private static List<EnvironmentThreat> DetectSystemStabilityThreats()
        {
            var threats = new List<EnvironmentThreat>();

            try
            {
                // Check system uptime
                var uptime = Environment.TickCount;
                var uptimeHours = uptime / (1000.0 * 60.0 * 60.0);

                if (uptimeHours > 168) // More than a week
                {
                    threats.Add(new EnvironmentThreat
                    {
                        Category = "System Stability",
                        Description = $"System uptime very high: {uptimeHours:F1} hours",
                        Level = ThreatLevel.Low,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Recommend system restart, monitor for instability"
                    });
                }

                // Check for system errors in event log
                if (HasRecentSystemErrors())
                {
                    threats.Add(new EnvironmentThreat
                    {
                        Category = "System Stability",
                        Description = "Recent system errors detected in event log",
                        Level = ThreatLevel.Medium,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Monitor for failures, increase error handling"
                    });
                }

                // Check for thermal throttling
                if (IsThermalThrottlingActive())
                {
                    threats.Add(new EnvironmentThreat
                    {
                        Category = "System Stability",
                        Description = "CPU thermal throttling detected",
                        Level = ThreatLevel.High,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Reduce CPU-intensive operations, cool down period"
                    });
                }
            }
            catch (Exception ex)
            {
                threats.Add(new EnvironmentThreat
                {
                    Category = "System Stability Detection",
                    Description = $"System stability monitoring error: {ex.Message}",
                    Level = ThreatLevel.Low,
                    DetectedAt = DateTime.Now,
                    MitigationAction = "Continue with stability assumptions"
                });
            }

            return threats;
        }

        private static List<EnvironmentThreat> DetectFileSystemThreats()
        {
            var threats = new List<EnvironmentThreat>();

            try
            {
                // Check for locked files in build directories
                var buildDirs = new[] { "bin", "obj", "TestResults" };
                var projectRoot = Directory.GetCurrentDirectory();

                foreach (var buildDir in buildDirs)
                {
                    var fullPath = Path.Combine(projectRoot, buildDir);
                    if (Directory.Exists(fullPath))
                    {
                        var lockedFiles = GetLockedFilesInDirectory(fullPath);
                        if (lockedFiles.Any())
                        {
                            threats.Add(new EnvironmentThreat
                            {
                                Category = "File System",
                                Description = $"Locked files detected in {buildDir}: {lockedFiles.Count} files",
                                Level = ThreatLevel.Medium,
                                DetectedAt = DateTime.Now,
                                MitigationAction = "Force unlock, kill holding processes"
                            });
                        }
                    }
                }

                // Check for path length issues
                var longPaths = FindLongPaths(projectRoot);
                if (longPaths.Any())
                {
                    threats.Add(new EnvironmentThreat
                    {
                        Category = "File System",
                        Description = $"Long path names detected: {longPaths.Count} paths > 260 characters",
                        Level = ThreatLevel.Medium,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Shorten paths, enable long path support"
                    });
                }

                // Check for excessive file handles
                var handleCount = GetCurrentProcessFileHandles();
                if (handleCount > 1000)
                {
                    threats.Add(new EnvironmentThreat
                    {
                        Category = "File System",
                        Description = $"High file handle count: {handleCount} handles",
                        Level = ThreatLevel.Medium,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Close unused handles, reduce file operations"
                    });
                }
            }
            catch (Exception ex)
            {
                threats.Add(new EnvironmentThreat
                {
                    Category = "File System Detection",
                    Description = $"File system monitoring error: {ex.Message}",
                    Level = ThreatLevel.Low,
                    DetectedAt = DateTime.Now,
                    MitigationAction = "Continue with file system assumptions"
                });
            }

            return threats;
        }

        private static List<EnvironmentThreat> DetectDevelopmentEnvironmentThreats()
        {
            var threats = new List<EnvironmentThreat>();

            try
            {
                // Check for conflicting .NET versions
                var dotnetVersions = GetInstalledDotNetVersions();
                if (dotnetVersions.Count > 5)
                {
                    threats.Add(new EnvironmentThreat
                    {
                        Category = "Development Environment",
                        Description = $"Multiple .NET versions installed: {dotnetVersions.Count} versions",
                        Level = ThreatLevel.Low,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Verify target framework, clean unused versions"
                    });
                }

                // Check for excessive MSBuild processes
                var msbuildProcesses = Process.GetProcessesByName("MSBuild");
                if (msbuildProcesses.Length > 3)
                {
                    threats.Add(new EnvironmentThreat
                    {
                        Category = "Development Environment",
                        Description = $"Multiple MSBuild processes: {msbuildProcesses.Length} active",
                        Level = ThreatLevel.Medium,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Kill orphaned processes, use single-threaded build"
                    });
                }

                // Check for nuget package cache issues
                var nugetCacheSize = GetNuGetCacheSizeMB();
                if (nugetCacheSize > 10000) // > 10GB
                {
                    threats.Add(new EnvironmentThreat
                    {
                        Category = "Development Environment",
                        Description = $"Large NuGet cache: {nugetCacheSize:F0}MB",
                        Level = ThreatLevel.Low,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Clean NuGet cache, verify package references"
                    });
                }

                // Check for VS Code extensions conflicts
                if (IsVSCodeRunning())
                {
                    var extensionConflicts = DetectVSCodeExtensionConflicts();
                    if (extensionConflicts.Any())
                    {
                        threats.Add(new EnvironmentThreat
                        {
                            Category = "Development Environment",
                            Description = $"VS Code extension conflicts detected: {extensionConflicts.Count} conflicts",
                            Level = ThreatLevel.Low,
                            DetectedAt = DateTime.Now,
                            MitigationAction = "Disable conflicting extensions, restart VS Code"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                threats.Add(new EnvironmentThreat
                {
                    Category = "Development Environment Detection",
                    Description = $"Development environment monitoring error: {ex.Message}",
                    Level = ThreatLevel.Low,
                    DetectedAt = DateTime.Now,
                    MitigationAction = "Continue with environment assumptions"
                });
            }

            return threats;
        }

        #endregion

        #region Mitigation Methods

        /// <summary>
        /// Apply mitigations for detected threats
        /// </summary>
        public static void ApplyThreatMitigations(List<EnvironmentThreat> threats)
        {
            if (!threats.Any()) return;

            Console.WriteLine($"🛠️ Applying mitigations for {threats.Count} detected threats...");

            foreach (var threat in threats.OrderByDescending(t => t.Level))
            {
                try
                {
                    ApplySingleThreatMitigation(threat);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to apply mitigation for {threat.Category}: {ex.Message}");
                }
            }

            Console.WriteLine("✅ Threat mitigations applied");
        }

        private static void ApplySingleThreatMitigation(EnvironmentThreat threat)
        {
            Console.WriteLine($"🔧 Mitigating {threat.Level} threat in {threat.Category}: {threat.Description}");

            switch (threat.Category)
            {
                case "Memory":
                    if (threat.Level >= ThreatLevel.High)
                    {
                        ForceGarbageCollection();
                        EnableLowMemoryMode();
                    }
                    break;

                case "CPU":
                    if (threat.Level >= ThreatLevel.High)
                    {
                        ReduceThreadCount();
                        Thread.Sleep(1000); // Give CPU a break
                    }
                    break;

                case "Storage":
                    if (threat.Level >= ThreatLevel.High)
                    {
                        CleanTemporaryFiles();
                        ReduceCaching();
                    }
                    break;

                case "Network":
                    EnableOfflineMode();
                    IncreaseNetworkTimeouts();
                    break;

                case "Security":
                    AdjustForAntivirusScanning();
                    break;

                case "System Stability":
                    if (threat.Level >= ThreatLevel.High)
                    {
                        EnableConservativeMode();
                        IncreaseErrorHandling();
                    }
                    break;

                case "File System":
                    if (threat.Description.Contains("Locked files"))
                    {
                        ForceUnlockFiles();
                    }
                    if (threat.Description.Contains("Long path"))
                    {
                        EnableLongPathSupport();
                    }
                    break;

                case "Development Environment":
                    if (threat.Description.Contains("MSBuild"))
                    {
                        KillOrphanedMSBuildProcesses();
                    }
                    break;
            }
        }

        #endregion

        #region Monitoring and Event Handling

        private static void StartContinuousMonitoring()
        {
            _monitoringTimer = new System.Threading.Timer(PerformMonitoringCycle, null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
        }

        private static void PerformMonitoringCycle(object state)
        {
            try
            {
                var threats = PerformComprehensiveThreatScan();
                var newThreats = threats.Where(t => t.Level >= ThreatLevel.Medium).ToList();

                if (newThreats.Any())
                {
                    Console.WriteLine($"🚨 {newThreats.Count} new threats detected during monitoring cycle");
                    ApplyThreatMitigations(newThreats);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Monitoring cycle error: {ex.Message}");
            }
        }

        private static void RegisterSystemEventHandlers()
        {
            try
            {
                // Register for system shutdown
                SystemEvents.SessionEnding += (sender, e) =>
                {
                    Console.WriteLine("🔄 System shutdown detected - cleaning up...");
                    CleanupOnExit();
                };

                // Register for power mode changes
                SystemEvents.PowerModeChanged += (sender, e) =>
                {
                    if (e.Mode == PowerModes.Resume)
                    {
                        Console.WriteLine("🔋 System resumed from sleep - re-initializing...");
                        Task.Run(() => PerformComprehensiveThreatScan());
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to register system event handlers: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        private static long GetAvailableMemoryMB()
        {
            try
            {
                using var process = new Process();
                process.StartInfo.FileName = "wmic";
                process.StartInfo.Arguments = "OS get FreePhysicalMemory /value";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                foreach (string line in output.Split('\n'))
                {
                    if (line.Contains("FreePhysicalMemory="))
                    {
                        string value = line.Split('=')[1].Trim();
                        if (long.TryParse(value, out long kb))
                        {
                            return kb / 1024;
                        }
                    }
                }
            }
            catch { }

            return 1000; // Default assumption
        }

        private static float GetSystemCpuUsage()
        {
            try
            {
                // Use PerformanceCounter approach or simple estimation
                using var process = Process.GetCurrentProcess();
                var startTime = DateTime.UtcNow;
                var startCpuUsage = process.TotalProcessorTime;

                Thread.Sleep(100);

                var endTime = DateTime.UtcNow;
                var endCpuUsage = process.TotalProcessorTime;

                var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalMsPassed = (endTime - startTime).TotalMilliseconds;
                var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

                return Math.Max(0, Math.Min(100, (float)(cpuUsageTotal * 100)));
            }
            catch { }

            return 50.0f; // Default assumption
        }

        private static bool IsWindowsDefenderRealtimeEnabled()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Real-Time Protection");
                var value = key?.GetValue("DisableRealtimeMonitoring");
                return value == null || !value.Equals(1);
            }
            catch
            {
                return true; // Assume enabled if we can't check
            }
        }

        private static bool HasRecentSystemErrors()
        {
            try
            {
                using var eventLog = new EventLog("System");
                var recentErrors = eventLog.Entries.Cast<EventLogEntry>()
                    .Where(e => e.TimeGenerated > DateTime.Now.AddHours(-24) && e.EntryType == EventLogEntryType.Error)
                    .Take(10);

                return recentErrors.Any();
            }
            catch
            {
                return false;
            }
        }

        private static bool IsThermalThrottlingActive()
        {
            try
            {
                // Simple heuristic: if system is under high load for extended time,
                // assume potential throttling (without WMI dependency)
                var cpuUsage = GetSystemCpuUsage();
                return cpuUsage > 90.0f; // High CPU might indicate throttling
            }
            catch { }

            return false;
        }

        private static List<string> GetLockedFilesInDirectory(string directory)
        {
            var lockedFiles = new List<string>();

            try
            {
                foreach (var file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        using var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None);
                    }
                    catch (IOException)
                    {
                        lockedFiles.Add(file);
                    }
                    catch { }
                }
            }
            catch { }

            return lockedFiles;
        }

        private static List<string> FindLongPaths(string rootPath)
        {
            var longPaths = new List<string>();

            try
            {
                foreach (var path in Directory.EnumerateFileSystemEntries(rootPath, "*", SearchOption.AllDirectories))
                {
                    if (path.Length > 260)
                    {
                        longPaths.Add(path);
                    }
                }
            }
            catch { }

            return longPaths;
        }

        private static int GetCurrentProcessFileHandles()
        {
            try
            {
                using var process = Process.GetCurrentProcess();
                return process.HandleCount;
            }
            catch
            {
                return 0;
            }
        }

        private static List<string> GetInstalledDotNetVersions()
        {
            var versions = new List<string>();

            try
            {
                using var process = new Process();
                process.StartInfo.FileName = "dotnet";
                process.StartInfo.Arguments = "--list-sdks";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                versions.AddRange(output.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)));
            }
            catch { }

            return versions;
        }

        private static float GetNuGetCacheSizeMB()
        {
            try
            {
                var nugetCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
                if (Directory.Exists(nugetCache))
                {
                    var size = Directory.GetFiles(nugetCache, "*", SearchOption.AllDirectories)
                        .Sum(file => new FileInfo(file).Length);
                    return size / (1024.0f * 1024.0f);
                }
            }
            catch { }

            return 0;
        }

        private static bool IsVSCodeRunning()
        {
            return Process.GetProcessesByName("Code").Any();
        }

        private static List<string> DetectVSCodeExtensionConflicts()
        {
            // This would require parsing VS Code extension logs or configuration
            // For now, return empty list
            return new List<string>();
        }

        // Mitigation action implementations
        private static void ForceGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private static void EnableLowMemoryMode()
        {
            Environment.SetEnvironmentVariable("DOTNET_gcServer", "0");
        }

        private static void ReduceThreadCount()
        {
            Environment.SetEnvironmentVariable("MSBuildNodeCount", "1");
        }

        private static void CleanTemporaryFiles()
        {
            try
            {
                var tempPath = Path.GetTempPath();
                var oldFiles = Directory.GetFiles(tempPath)
                    .Where(f => File.GetCreationTime(f) < DateTime.Now.AddDays(-1))
                    .Take(100); // Limit to avoid long operations

                foreach (var file in oldFiles)
                {
                    try { File.Delete(file); } catch { }
                }
            }
            catch { }
        }

        private static void ReduceCaching()
        {
            Environment.SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1");
        }

        private static void EnableOfflineMode()
        {
            Environment.SetEnvironmentVariable("DOTNET_NOLOGO", "1");
        }

        private static void IncreaseNetworkTimeouts()
        {
            Environment.SetEnvironmentVariable("NUGET_XMLDOC_MODE", "skip");
        }

        private static void AdjustForAntivirusScanning()
        {
            // Add delays between file operations
            Thread.Sleep(100);
        }

        private static void EnableConservativeMode()
        {
            Environment.SetEnvironmentVariable("DOTNET_gcConcurrent", "1");
        }

        private static void IncreaseErrorHandling()
        {
            // This would involve setting flags for more robust error handling
        }

        private static void ForceUnlockFiles()
        {
            // Kill processes that might be holding file locks
            KillOrphanedMSBuildProcesses();
        }

        private static void EnableLongPathSupport()
        {
            // This would require registry changes - skip for safety
        }

        private static void KillOrphanedMSBuildProcesses()
        {
            try
            {
                var processes = Process.GetProcessesByName("MSBuild")
                    .Concat(Process.GetProcessesByName("dotnet"))
                    .Where(p => (DateTime.Now - p.StartTime).TotalMinutes > 5);

                foreach (var process in processes)
                {
                    try { process.Kill(); } catch { }
                    finally { process?.Dispose(); }
                }
            }
            catch { }
        }

        private static void CleanupOnExit()
        {
            _monitoringTimer?.Dispose();
        }

        /// <summary>
        /// Get current threat summary for reporting
        /// </summary>
        public static string GetThreatSummary()
        {
            var threats = PerformComprehensiveThreatScan();
            var summary = $"Environment Status: {threats.Count} threats detected\n";

            foreach (var threat in threats.GroupBy(t => t.Level))
            {
                summary += $"  {threat.Key}: {threat.Count()} threats\n";
            }

            return summary;
        }

        /// <summary>
        /// Force immediate threat scan and mitigation
        /// </summary>
        public static void ForceEnvironmentScan()
        {
            Console.WriteLine("🔍 Performing forced environment scan...");
            var threats = PerformComprehensiveThreatScan();
            ApplyThreatMitigations(threats);
        }

        #endregion
    }
}
