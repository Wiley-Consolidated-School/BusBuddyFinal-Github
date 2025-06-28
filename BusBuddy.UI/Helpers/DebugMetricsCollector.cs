using System;
using System.Diagnostics;
using System.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using BusBuddy.UI.Helpers;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Advanced debugging and performance monitoring helper for BusBuddy fine-tuning
    /// Provides comprehensive metrics collection and analysis capabilities
    /// </summary>
    public static class DebugMetricsCollector
    {
        private static readonly System.Threading.Timer _metricsTimer;
        private static readonly List<PerformanceSnapshot> _snapshots = new();
        private static readonly object _lockObject = new();

        // Configuration properties
        private static bool EnabledMetrics => bool.Parse(ConfigurationManager.AppSettings["EnablePerformanceMetrics"] ?? "false");
        private static bool EnabledMemoryTracking => bool.Parse(ConfigurationManager.AppSettings["EnableMemoryTracking"] ?? "false");
        private static string DebugLevel => ConfigurationManager.AppSettings["DebugLevel"] ?? "Standard";

        static DebugMetricsCollector()
        {
            if (EnabledMetrics)
            {
                // Collect metrics every 5 seconds during runtime
                _metricsTimer = new System.Threading.Timer(CollectSnapshot, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            }
        }

        /// <summary>
        /// Performance snapshot for trend analysis
        /// </summary>
        public class PerformanceSnapshot
        {
            public DateTime Timestamp { get; set; }
            public long MemoryUsageMB { get; set; }
            public double CpuUsagePercent { get; set; }
            public int ThreadCount { get; set; }
            public int HandleCount { get; set; }
            public long WorkingSetMB { get; set; }
            public string ActiveModule { get; set; } = string.Empty;
        }

        /// <summary>
        /// Start comprehensive performance monitoring
        /// </summary>
        public static void StartMonitoring(string module = "Application")
        {
            if (!EnabledMetrics) return;

            BusBuddyLogger.Debug("Metrics", $"Starting performance monitoring for {module}");
            CollectSnapshot(module);
        }

        /// <summary>
        /// Collect detailed performance snapshot
        /// </summary>
        private static void CollectSnapshot(object state)
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var snapshot = new PerformanceSnapshot
                {
                    Timestamp = DateTime.Now,
                    MemoryUsageMB = GC.GetTotalMemory(false) / 1024 / 1024,
                    ThreadCount = process.Threads.Count,
                    HandleCount = process.HandleCount,
                    WorkingSetMB = process.WorkingSet64 / 1024 / 1024,
                    ActiveModule = state?.ToString() ?? "Runtime",
                    CpuUsagePercent = GetCpuUsage()
                };

                lock (_lockObject)
                {
                    _snapshots.Add(snapshot);

                    // Keep only last 100 snapshots
                    if (_snapshots.Count > 100)
                    {
                        _snapshots.RemoveAt(0);
                    }
                }

                // Log significant changes
                if (DebugLevel == "Verbose")
                {
                    LogSnapshot(snapshot);
                }

                // Check for performance alerts
                CheckPerformanceAlerts(snapshot);
            }
            catch (Exception ex)
            {
                BusBuddyLogger.Error("Metrics", $"Error collecting snapshot: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current CPU usage percentage
        /// </summary>
        private static double GetCpuUsage()
        {
            try
            {
                using var process = Process.GetCurrentProcess();
                var startTime = DateTime.UtcNow;
                var startCpuUsage = process.TotalProcessorTime;

                Thread.Sleep(100); // Small delay for accurate measurement

                var endTime = DateTime.UtcNow;
                var endCpuUsage = process.TotalProcessorTime;

                var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalMsPassed = (endTime - startTime).TotalMilliseconds;
                var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

                return Math.Round(cpuUsageTotal * 100, 2);
            }
            catch
            {
                return 0; // Return 0 if unable to measure
            }
        }

        /// <summary>
        /// Log detailed snapshot information
        /// </summary>
        private static void LogSnapshot(PerformanceSnapshot snapshot)
        {
            var message = $"[{snapshot.ActiveModule}] Memory: {snapshot.MemoryUsageMB}MB, " +
                         $"CPU: {snapshot.CpuUsagePercent:F1}%, " +
                         $"Threads: {snapshot.ThreadCount}, " +
                         $"Handles: {snapshot.HandleCount}";

            Console.WriteLine($"📊 {message}");
            BusBuddyLogger.Debug("Metrics", message);
        }

        /// <summary>
        /// Check for performance alerts and warnings
        /// </summary>
        private static void CheckPerformanceAlerts(PerformanceSnapshot snapshot)
        {
            var memoryThresholdMB = int.Parse(ConfigurationManager.AppSettings["MemoryThresholdMB"] ?? "50");

            // Memory usage alert
            if (snapshot.MemoryUsageMB > memoryThresholdMB)
            {
                BusBuddyLogger.Warning("Performance",
                    $"High memory usage detected: {snapshot.MemoryUsageMB}MB > {memoryThresholdMB}MB threshold");
            }

            // High CPU usage alert
            if (snapshot.CpuUsagePercent > 50)
            {
                BusBuddyLogger.Warning("Performance",
                    $"High CPU usage detected: {snapshot.CpuUsagePercent:F1}% > 50% threshold");
            }

            // Excessive thread count
            if (snapshot.ThreadCount > 20)
            {
                BusBuddyLogger.Warning("Performance",
                    $"High thread count detected: {snapshot.ThreadCount} > 20 threads");
            }
        }

        /// <summary>
        /// Generate comprehensive performance report
        /// </summary>
        public static string GeneratePerformanceReport()
        {
            if (!EnabledMetrics) return "Performance metrics collection is disabled.";

            lock (_lockObject)
            {
                if (_snapshots.Count == 0) return "No performance data collected yet.";

                var latest = _snapshots[^1];
                var oldest = _snapshots[0];
                var avgMemory = _snapshots.Average(s => s.MemoryUsageMB);
                var maxMemory = _snapshots.Max(s => s.MemoryUsageMB);
                var avgCpu = _snapshots.Average(s => s.CpuUsagePercent);
                var maxCpu = _snapshots.Max(s => s.CpuUsagePercent);

                return $"📊 BusBuddy Performance Report\n" +
                       $"   📅 Collection Period: {oldest.Timestamp:HH:mm:ss} - {latest.Timestamp:HH:mm:ss}\n" +
                       $"   🧠 Memory Usage: Current {latest.MemoryUsageMB}MB | Avg {avgMemory:F1}MB | Peak {maxMemory}MB\n" +
                       $"   ⚡ CPU Usage: Current {latest.CpuUsagePercent:F1}% | Avg {avgCpu:F1}% | Peak {maxCpu:F1}%\n" +
                       $"   🧵 Threads: {latest.ThreadCount} | Handles: {latest.HandleCount}\n" +
                       $"   💾 Working Set: {latest.WorkingSetMB}MB\n" +
                       $"   📈 Snapshots Collected: {_snapshots.Count}";
            }
        }

        /// <summary>
        /// Track specific operation performance
        /// </summary>
        public static IDisposable TrackOperation(string operationName)
        {
            return new OperationTracker(operationName);
        }

        /// <summary>
        /// Operation performance tracker
        /// </summary>
        private class OperationTracker : IDisposable
        {
            private readonly string _operationName;
            private readonly Stopwatch _stopwatch;
            private readonly long _startMemory;

            public OperationTracker(string operationName)
            {
                _operationName = operationName;
                _stopwatch = Stopwatch.StartNew();
                _startMemory = GC.GetTotalMemory(false);

                if (DebugLevel == "Verbose")
                {
                    BusBuddyLogger.Debug("Operation", $"Starting: {operationName}");
                }
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                var endMemory = GC.GetTotalMemory(false);
                var memoryDelta = (endMemory - _startMemory) / 1024 / 1024; // MB

                var performanceThreshold = int.Parse(ConfigurationManager.AppSettings["PerformanceThresholdMs"] ?? "100");
                var isSlowOperation = _stopwatch.ElapsedMilliseconds > performanceThreshold;

                var logLevel = isSlowOperation ? "Warning" : "Debug";
                var message = $"Operation '{_operationName}' completed in {_stopwatch.ElapsedMilliseconds}ms, " +
                             $"Memory delta: {memoryDelta:+0;-0;0}MB";

                if (isSlowOperation)
                {
                    BusBuddyLogger.Warning("Performance", message);
                }
                else if (DebugLevel == "Detailed" || DebugLevel == "Verbose")
                {
                    BusBuddyLogger.Debug("Performance", message);
                }

                Console.WriteLine($"⏱️ {message}");
            }
        }

        /// <summary>
        /// Cleanup resources on application shutdown
        /// </summary>
        public static void Shutdown()
        {
            _metricsTimer?.Dispose();

            if (EnabledMetrics && _snapshots.Count > 0)
            {
                Console.WriteLine("\n" + GeneratePerformanceReport());
                BusBuddyLogger.Info("Metrics", "Performance monitoring stopped - " + GeneratePerformanceReport());
            }
        }
    }
}
