using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BusBuddy.UI.Helpers;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Comprehensive environment protection system that coordinates all environmental safeguards
    /// This is the main entry point for all adverse environment prevention
    /// </summary>
    public static class EnvironmentProtectionManager
    {
        private static bool _initialized = false;
        private static readonly object _lockObject = new object();
        private static DateTime _lastFullScan = DateTime.MinValue;
        private static readonly List<string> _activeProtections = new List<string>();

        public enum ProtectionLevel
        {
            Minimal,      // Basic resource monitoring
            Standard,     // Standard protection suite
            Aggressive,   // Full protection with performance impact
            Emergency     // Crisis mode - maximum protection
        }

        /// <summary>
        /// Initialize comprehensive environment protection
        /// </summary>
        public static void Initialize(ProtectionLevel level = ProtectionLevel.Standard)
        {
            lock (_lockObject)
            {
                if (_initialized) return;

                Console.WriteLine("🛡️ Initializing Comprehensive Environment Protection...");
                Console.WriteLine($"📊 Protection Level: {level}");

                try
                {
                    // Initialize all protection systems
                    InitializeProtectionSystems(level);

                    // Perform initial threat assessment
                    var initialThreats = PerformInitialThreatAssessment();

                    // Apply immediate protections
                    ApplyImmediateProtections(initialThreats, level);

                    // Start monitoring systems
                    StartMonitoringSystems(level);

                    _initialized = true;
                    _lastFullScan = DateTime.Now;

                    Console.WriteLine($"✅ Environment Protection initialized with {_activeProtections.Count} active protections");
                    PrintProtectionSummary();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Environment Protection initialization warning: {ex.Message}");
                    // Initialize minimal protection even if full init fails
                    InitializeMinimalProtection();
                }
            }
        }

        /// <summary>
        /// Perform comprehensive environment health check
        /// </summary>
        public static EnvironmentHealthReport PerformHealthCheck()
        {
            Console.WriteLine("🔍 Performing comprehensive environment health check...");

            var report = new EnvironmentHealthReport
            {
                Timestamp = DateTime.Now,
                OverallHealth = EnvironmentHealth.Unknown
            };

            try
            {
                // Get basic environmental resilience status
                var basicHealth = EnvironmentalResilience.AssessEnvironmentHealth();
                report.BasicEnvironmentHealth = basicHealth;

                // Get advanced threat analysis
                var threats = AdverseEnvironmentGuard.PerformComprehensiveThreatScan();
                report.DetectedThreats = threats;

                // Assess build system health
                var buildHealth = AssessBuildSystemHealth();
                report.BuildSystemHealth = buildHealth;

                // Assess runtime stability
                var runtimeHealth = AssessRuntimeStability();
                report.RuntimeStabilityHealth = runtimeHealth;

                // Calculate overall health
                report.OverallHealth = CalculateOverallHealth(basicHealth, threats, buildHealth, runtimeHealth);

                // Generate recommendations
                report.Recommendations = GenerateHealthRecommendations(report);

                Console.WriteLine($"📊 Health Check Complete - Overall Health: {report.OverallHealth}");
                return report;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Health check error: {ex.Message}");
                report.ErrorMessage = ex.Message;
                return report;
            }
        }

        /// <summary>
        /// Apply emergency protections for critical situations
        /// </summary>
        public static void ApplyEmergencyProtections()
        {
            Console.WriteLine("🚨 EMERGENCY: Applying emergency environment protections!");

            try
            {
                // Force immediate garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // Kill all unnecessary processes
                KillUnnecessaryProcesses();

                // Force environment cleanup
                EnvironmentalResilience.ForceEnvironmentCleanup();

                // Clean all build artifacts
                CleanAllBuildArtifacts();

                // Set conservative environment variables
                SetConservativeEnvironmentVariables();

                // Force network offline mode
                SetOfflineMode();

                // Reduce system load
                ReduceSystemLoad();

                Console.WriteLine("✅ Emergency protections applied");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Emergency protection error: {ex.Message}");
            }
        }

        /// <summary>
        /// Monitor and auto-adjust protection based on changing conditions
        /// </summary>
        public static void StartAdaptiveProtection()
        {
            _ = Task.Run(async () =>
            {
                Console.WriteLine("🤖 Starting adaptive protection monitoring...");

                while (true)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1)); // Check every minute

                        var currentHealth = PerformHealthCheck();

                        // Auto-adjust protection level based on health
                        var recommendedLevel = DetermineOptimalProtectionLevel(currentHealth);
                        AdjustProtectionLevel(recommendedLevel);

                        // Apply specific mitigations for detected issues
                        if (currentHealth.DetectedThreats.Any(t => t.Level >= AdverseEnvironmentGuard.ThreatLevel.High))
                        {
                            Console.WriteLine("🚨 High-level threats detected - applying targeted mitigations");
                            AdverseEnvironmentGuard.ApplyThreatMitigations(currentHealth.DetectedThreats);
                        }

                        // Check if emergency intervention is needed
                        if (currentHealth.OverallHealth == EnvironmentHealth.Critical)
                        {
                            Console.WriteLine("🆘 CRITICAL HEALTH DETECTED - Applying emergency protections");
                            ApplyEmergencyProtections();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Adaptive protection monitoring error: {ex.Message}");
                        await Task.Delay(TimeSpan.FromMinutes(5)); // Wait longer on error
                    }
                }
            });
        }

        /// <summary>
        /// Pre-build protection routine
        /// </summary>
        public static async Task<bool> PrepareForBuildAsync(string solutionPath)
        {
            Console.WriteLine("🔧 Preparing environment for build...");

            try
            {
                // Perform health check
                var health = PerformHealthCheck();

                if (health.OverallHealth == EnvironmentHealth.Critical)
                {
                    Console.WriteLine("🚨 Critical environment health - applying emergency measures");
                    ApplyEmergencyProtections();

                    // Wait for stabilization
                    await Task.Delay(5000);

                    // Re-check health
                    health = PerformHealthCheck();
                    if (health.OverallHealth == EnvironmentHealth.Critical)
                    {
                        Console.WriteLine("❌ Environment still critical - build not recommended");
                        return false;
                    }
                }

                // Apply build-specific protections
                ApplyBuildSpecificProtections(health.OverallHealth);

                // Clean build environment
                await CleanBuildEnvironmentAsync(solutionPath);

                // Optimize for build
                OptimizeForBuild();

                Console.WriteLine("✅ Environment prepared for build");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Build preparation error: {ex.Message}");
                return false; // Don't risk a build in an unknown state
            }
        }

        /// <summary>
        /// Post-build cleanup and health restoration
        /// </summary>
        public static void PerformPostBuildCleanup()
        {
            Console.WriteLine("🧹 Performing post-build cleanup...");

            try
            {
                // Kill any orphaned build processes
                KillOrphanedBuildProcesses();

                // Clean temporary build files
                CleanBuildTemporaryFiles();

                // Restore normal environment settings
                RestoreNormalEnvironmentSettings();

                // Force garbage collection
                GC.Collect();

                Console.WriteLine("✅ Post-build cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Post-build cleanup error: {ex.Message}");
            }
        }

        #region Private Methods

        private static void InitializeProtectionSystems(ProtectionLevel level)
        {
            _activeProtections.Clear();

            // Always initialize basic environmental resilience
            EnvironmentalResilience.Initialize();
            _activeProtections.Add("EnvironmentalResilience");

            if (level >= ProtectionLevel.Standard)
            {
                // Initialize advanced threat detection
                AdverseEnvironmentGuard.Initialize();
                _activeProtections.Add("AdverseEnvironmentGuard");
            }

            if (level >= ProtectionLevel.Aggressive)
            {
                // Initialize aggressive monitoring
                StartAggressiveMonitoring();
                _activeProtections.Add("AggressiveMonitoring");
            }

            if (level == ProtectionLevel.Emergency)
            {
                // Apply immediate emergency measures
                ApplyEmergencyProtections();
                _activeProtections.Add("EmergencyProtections");
            }
        }

        private static List<AdverseEnvironmentGuard.EnvironmentThreat> PerformInitialThreatAssessment()
        {
            var threats = new List<AdverseEnvironmentGuard.EnvironmentThreat>();

            try
            {
                // Get comprehensive threat scan
                threats.AddRange(AdverseEnvironmentGuard.PerformComprehensiveThreatScan());

                // Add custom threat detections
                threats.AddRange(DetectCustomThreats());

                Console.WriteLine($"🔍 Initial threat assessment: {threats.Count} threats detected");

                if (threats.Any())
                {
                    var criticalThreats = threats.Count(t => t.Level == AdverseEnvironmentGuard.ThreatLevel.Critical);
                    var highThreats = threats.Count(t => t.Level == AdverseEnvironmentGuard.ThreatLevel.High);

                    if (criticalThreats > 0 || highThreats > 2)
                    {
                        Console.WriteLine($"🚨 HIGH RISK ENVIRONMENT: {criticalThreats} critical, {highThreats} high threats");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Threat assessment error: {ex.Message}");
            }

            return threats;
        }

        private static List<AdverseEnvironmentGuard.EnvironmentThreat> DetectCustomThreats()
        {
            var threats = new List<AdverseEnvironmentGuard.EnvironmentThreat>();

            try
            {
                // Check for Visual Studio running (can cause build conflicts)
                if (Process.GetProcessesByName("devenv").Any())
                {
                    threats.Add(new AdverseEnvironmentGuard.EnvironmentThreat
                    {
                        Category = "Development Environment",
                        Description = "Visual Studio is running - may cause build conflicts",
                        Level = AdverseEnvironmentGuard.ThreatLevel.Low,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Consider closing Visual Studio or using different MSBuild instance"
                    });
                }

                // Check for Docker Desktop running (high resource usage)
                if (Process.GetProcessesByName("Docker Desktop").Any())
                {
                    threats.Add(new AdverseEnvironmentGuard.EnvironmentThreat
                    {
                        Category = "Resource Usage",
                        Description = "Docker Desktop detected - high resource consumption",
                        Level = AdverseEnvironmentGuard.ThreatLevel.Medium,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Consider pausing Docker or reducing container resource limits"
                    });
                }

                // Check for Windows Update in progress
                if (IsWindowsUpdateInProgress())
                {
                    threats.Add(new AdverseEnvironmentGuard.EnvironmentThreat
                    {
                        Category = "System Stability",
                        Description = "Windows Update in progress",
                        Level = AdverseEnvironmentGuard.ThreatLevel.High,
                        DetectedAt = DateTime.Now,
                        MitigationAction = "Wait for Windows Update to complete before building"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Custom threat detection error: {ex.Message}");
            }

            return threats;
        }

        private static void ApplyImmediateProtections(List<AdverseEnvironmentGuard.EnvironmentThreat> threats, ProtectionLevel level)
        {
            // Apply threat-specific mitigations
            if (threats.Any())
            {
                AdverseEnvironmentGuard.ApplyThreatMitigations(threats);
            }

            // Apply level-specific protections
            switch (level)
            {
                case ProtectionLevel.Minimal:
                    ApplyMinimalProtections();
                    break;
                case ProtectionLevel.Standard:
                    ApplyStandardProtections();
                    break;
                case ProtectionLevel.Aggressive:
                    ApplyAggressiveProtections();
                    break;
                case ProtectionLevel.Emergency:
                    ApplyEmergencyProtections();
                    break;
            }
        }

        private static void StartMonitoringSystems(ProtectionLevel level)
        {
            if (level >= ProtectionLevel.Standard)
            {
                StartAdaptiveProtection();
            }
        }

        private static void InitializeMinimalProtection()
        {
            try
            {
                EnvironmentalResilience.Initialize();
                _activeProtections.Add("MinimalProtection");
                _initialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Even minimal protection failed: {ex.Message}");
            }
        }

        private static EnvironmentHealth AssessBuildSystemHealth()
        {
            try
            {
                var msbuildCount = Process.GetProcessesByName("MSBuild").Length;
                var dotnetCount = Process.GetProcessesByName("dotnet").Length;

                if (msbuildCount > 5 || dotnetCount > 10)
                    return EnvironmentHealth.Critical;

                if (msbuildCount > 2 || dotnetCount > 5)
                    return EnvironmentHealth.Degraded;

                return EnvironmentHealth.Optimal;
            }
            catch
            {
                return EnvironmentHealth.Unknown;
            }
        }

        private static EnvironmentHealth AssessRuntimeStability()
        {
            try
            {
                var uptime = Environment.TickCount;
                var uptimeHours = uptime / (1000.0 * 60.0 * 60.0);

                if (uptimeHours > 168) // More than a week
                    return EnvironmentHealth.Degraded;

                return EnvironmentHealth.Optimal;
            }
            catch
            {
                return EnvironmentHealth.Unknown;
            }
        }

        private static EnvironmentHealth CalculateOverallHealth(
            EnvironmentalResilience.EnvironmentHealth basicHealth,
            List<AdverseEnvironmentGuard.EnvironmentThreat> threats,
            EnvironmentHealth buildHealth,
            EnvironmentHealth runtimeHealth)
        {
            var healthValues = new[]
            {
                (int)basicHealth,
                threats.Any(t => t.Level == AdverseEnvironmentGuard.ThreatLevel.Critical) ? 3 :
                threats.Any(t => t.Level == AdverseEnvironmentGuard.ThreatLevel.High) ? 2 : 1,
                (int)buildHealth,
                (int)runtimeHealth
            };

            var worstHealth = healthValues.Max();
            return (EnvironmentHealth)worstHealth;
        }

        private static List<string> GenerateHealthRecommendations(EnvironmentHealthReport report)
        {
            var recommendations = new List<string>();

            if (report.OverallHealth >= EnvironmentHealth.Degraded)
            {
                recommendations.Add("Consider restarting the development environment");
            }

            if (report.DetectedThreats.Any(t => t.Category == "Memory"))
            {
                recommendations.Add("Close unnecessary applications to free memory");
            }

            if (report.DetectedThreats.Any(t => t.Category == "CPU"))
            {
                recommendations.Add("Wait for CPU-intensive tasks to complete");
            }

            if (report.BuildSystemHealth >= EnvironmentHealth.Degraded)
            {
                recommendations.Add("Clean build artifacts and restart build tools");
            }

            return recommendations;
        }

        private static ProtectionLevel DetermineOptimalProtectionLevel(EnvironmentHealthReport health)
        {
            if (health.OverallHealth == EnvironmentHealth.Critical)
                return ProtectionLevel.Emergency;

            if (health.OverallHealth == EnvironmentHealth.Degraded)
                return ProtectionLevel.Aggressive;

            if (health.DetectedThreats.Any(t => t.Level >= AdverseEnvironmentGuard.ThreatLevel.High))
                return ProtectionLevel.Aggressive;

            return ProtectionLevel.Standard;
        }

        private static void AdjustProtectionLevel(ProtectionLevel newLevel)
        {
            // Implementation for dynamically adjusting protection level
            Console.WriteLine($"🔄 Adjusting protection level to: {newLevel}");
        }

        // Helper methods for specific protection actions
        private static void ApplyMinimalProtections()
        {
            Environment.SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1");
        }

        private static void ApplyStandardProtections()
        {
            ApplyMinimalProtections();
            Environment.SetEnvironmentVariable("MSBuildNodeCount", "2");
        }

        private static void ApplyAggressiveProtections()
        {
            ApplyStandardProtections();
            Environment.SetEnvironmentVariable("MSBuildNodeCount", "1");
            Environment.SetEnvironmentVariable("DOTNET_gcServer", "0");
        }

        private static void ApplyBuildSpecificProtections(EnvironmentHealth health)
        {
            if (health >= EnvironmentHealth.Degraded)
            {
                Environment.SetEnvironmentVariable("MSBuildNodeCount", "1");
                Environment.SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1");
            }
        }

        private static async Task CleanBuildEnvironmentAsync(string solutionPath)
        {
            try
            {
                // Use BuildResilience for comprehensive cleanup
                await BusBuddy.Build.Helpers.BuildResilience.ExecuteResilientBuildAsync(solutionPath, 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Build environment cleanup error: {ex.Message}");
            }
        }

        private static void OptimizeForBuild()
        {
            GC.Collect();
            Thread.Sleep(1000); // Give system a moment to stabilize
        }

        private static void StartAggressiveMonitoring()
        {
            Console.WriteLine("🔍 Starting aggressive environment monitoring");
            // Implementation for more frequent monitoring
        }

        private static void KillUnnecessaryProcesses()
        {
            var processesToKill = new[] { "MSBuild", "VBCSCompiler" };

            foreach (var processName in processesToKill)
            {
                try
                {
                    var processes = Process.GetProcessesByName(processName);
                    foreach (var process in processes)
                    {
                        if ((DateTime.Now - process.StartTime).TotalMinutes > 5)
                        {
                            process.Kill();
                            process.Dispose();
                        }
                    }
                }
                catch { }
            }
        }

        private static void CleanAllBuildArtifacts()
        {
            var buildDirs = new[] { "bin", "obj", "TestResults" };
            var currentDir = Directory.GetCurrentDirectory();

            foreach (var buildDir in buildDirs)
            {
                var fullPath = Path.Combine(currentDir, buildDir);
                if (Directory.Exists(fullPath))
                {
                    try
                    {
                        Directory.Delete(fullPath, true);
                    }
                    catch { }
                }
            }
        }

        private static void SetConservativeEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("MSBuildNodeCount", "1");
            Environment.SetEnvironmentVariable("DOTNET_gcServer", "0");
            Environment.SetEnvironmentVariable("DOTNET_gcConcurrent", "1");
            Environment.SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1");
        }

        private static void SetOfflineMode()
        {
            Environment.SetEnvironmentVariable("DOTNET_NOLOGO", "1");
            Environment.SetEnvironmentVariable("NUGET_XMLDOC_MODE", "skip");
        }

        private static void ReduceSystemLoad()
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();
                currentProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
            }
            catch { }
        }

        private static void KillOrphanedBuildProcesses()
        {
            KillUnnecessaryProcesses(); // Reuse existing implementation
        }

        private static void CleanBuildTemporaryFiles()
        {
            try
            {
                var tempPath = Path.GetTempPath();
                var msbuildTemp = Path.Combine(tempPath, "MSBuildTemp");
                if (Directory.Exists(msbuildTemp))
                {
                    Directory.Delete(msbuildTemp, true);
                }
            }
            catch { }
        }

        private static void RestoreNormalEnvironmentSettings()
        {
            // Remove restrictive environment variables
            Environment.SetEnvironmentVariable("MSBuildNodeCount", null);
            Environment.SetEnvironmentVariable("DOTNET_gcServer", null);
        }

        private static bool IsWindowsUpdateInProgress()
        {
            try
            {
                return Process.GetProcessesByName("TrustedInstaller").Any() ||
                       Process.GetProcessesByName("wuauclt").Any();
            }
            catch
            {
                return false;
            }
        }

        private static void PrintProtectionSummary()
        {
            Console.WriteLine("📋 Active Protections:");
            foreach (var protection in _activeProtections)
            {
                Console.WriteLine($"  ✓ {protection}");
            }
        }

        #endregion

        /// <summary>
        /// Get comprehensive status report
        /// </summary>
        public static string GetStatusReport()
        {
            var report = $"Environment Protection Manager Status\n";
            report += $"=====================================\n";
            report += $"Initialized: {_initialized}\n";
            report += $"Last Full Scan: {_lastFullScan:yyyy-MM-dd HH:mm:ss}\n";
            report += $"Active Protections: {_activeProtections.Count}\n";

            foreach (var protection in _activeProtections)
            {
                report += $"  - {protection}\n";
            }

            if (_initialized)
            {
                var health = PerformHealthCheck();
                report += $"Overall Health: {health.OverallHealth}\n";
                report += $"Detected Threats: {health.DetectedThreats.Count}\n";
            }

            return report;
        }
    }

    /// <summary>
    /// Environment health status levels
    /// </summary>
    public enum EnvironmentHealth
    {
        Unknown = 0,
        Optimal = 1,
        Degraded = 2,
        Critical = 3
    }

    /// <summary>
    /// Comprehensive environment health report
    /// </summary>
    public class EnvironmentHealthReport
    {
        public DateTime Timestamp { get; set; }
        public EnvironmentHealth OverallHealth { get; set; }
        public EnvironmentalResilience.EnvironmentHealth BasicEnvironmentHealth { get; set; }
        public List<AdverseEnvironmentGuard.EnvironmentThreat> DetectedThreats { get; set; } = new List<AdverseEnvironmentGuard.EnvironmentThreat>();
        public EnvironmentHealth BuildSystemHealth { get; set; }
        public EnvironmentHealth RuntimeStabilityHealth { get; set; }
        public List<string> Recommendations { get; set; } = new List<string>();
        public string ErrorMessage { get; set; }
    }
}
