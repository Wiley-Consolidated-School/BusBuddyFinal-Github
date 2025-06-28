using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows.Forms;

namespace BusBuddy.Debug
{
    /// <summary>
    /// Exception capture system for BusBuddy application
    /// Captures unhandled exceptions, dialog content, and stack traces
    /// </summary>
    public static class ExceptionCapture
    {
        private static string _logPath = "logs\\exception_capture.log";
        private static List<ExceptionInfo> _capturedExceptions = new List<ExceptionInfo>();

        public class ExceptionInfo
        {
            public DateTime Timestamp { get; set; }
            public string ExceptionType { get; set; }
            public string Message { get; set; }
            public string StackTrace { get; set; }
            public string SourceMethod { get; set; }
            public string SourceFile { get; set; }
            public int? SourceLine { get; set; }
            public string ThreadInfo { get; set; }
            public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initialize exception capture system
        /// Call this early in Program.cs Main method
        /// </summary>
        public static void Initialize()
        {
            try
            {
                // Set up global exception handlers
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

                Application.ThreadException += OnThreadException;
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

                Console.WriteLine("✅ ExceptionCapture: Global exception handlers registered");
                LogException("ExceptionCapture system initialized", "INFO");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ExceptionCapture initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle thread exceptions (UI thread)
        /// </summary>
        private static void OnThreadException(object eventSender, System.Threading.ThreadExceptionEventArgs e)
        {
            var exceptionInfo = CaptureExceptionDetails(e.Exception, "UI_THREAD");
            _capturedExceptions.Add(exceptionInfo);

            LogException($"UI THREAD EXCEPTION: {e.Exception.Message}", "ERROR");
            LogException($"Stack Trace: {e.Exception.StackTrace}", "ERROR");

            // Save detailed report
            SaveExceptionReport(exceptionInfo);

            // Show custom dialog instead of default
            ShowCustomExceptionDialog(exceptionInfo);
        }

        /// <summary>
        /// Handle unhandled domain exceptions
        /// </summary>
        private static void OnUnhandledException(object eventSender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                var exceptionInfo = CaptureExceptionDetails(ex, "APP_DOMAIN");
                _capturedExceptions.Add(exceptionInfo);

                LogException($"UNHANDLED DOMAIN EXCEPTION: {ex.Message}", "FATAL");
                LogException($"Stack Trace: {ex.StackTrace}", "FATAL");
                LogException($"Is Terminating: {e.IsTerminating}", "FATAL");

                // Save detailed report
                SaveExceptionReport(exceptionInfo);

                if (!e.IsTerminating)
                {
                    ShowCustomExceptionDialog(exceptionInfo);
                }
            }
        }

        /// <summary>
        /// Capture detailed exception information
        /// </summary>
        private static ExceptionInfo CaptureExceptionDetails(Exception ex, string context)
        {
            var info = new ExceptionInfo
            {
                Timestamp = DateTime.Now,
                ExceptionType = ex.GetType().FullName,
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                ThreadInfo = $"Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}, " +
                           $"Is Background: {System.Threading.Thread.CurrentThread.IsBackground}"
            };

            // Extract source method and file info from stack trace
            try
            {
                var stackTrace = new StackTrace(ex, true);
                var frame = stackTrace.GetFrame(0);
                if (frame != null)
                {
                    info.SourceMethod = frame.GetMethod()?.Name;
                    info.SourceFile = frame.GetFileName();
                    info.SourceLine = frame.GetFileLineNumber();
                }
            }
            catch { /* Ignore stack trace parsing errors */ }

            // Add context information
            info.Context["CaptureContext"] = context;
            info.Context["ProcessId"] = Process.GetCurrentProcess().Id;
            info.Context["WorkingSet"] = Environment.WorkingSet;
            info.Context["TotalMemory"] = GC.GetTotalMemory(false);

            return info;
        }

        /// <summary>
        /// Save exception report to JSON file
        /// </summary>
        private static void SaveExceptionReport(ExceptionInfo info)
        {
            try
            {
                var reportPath = $"logs\\exception_report_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var json = JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(reportPath, json);

                Console.WriteLine($"📄 Exception report saved: {reportPath}");
            }
            catch (Exception saveEx)
            {
                Console.WriteLine($"❌ Failed to save exception report: {saveEx.Message}");
            }
        }

        /// <summary>
        /// Show custom exception dialog with detailed information
        /// </summary>
        private static void ShowCustomExceptionDialog(ExceptionInfo info)
        {
            try
            {
                var message = $"BusBuddy Exception Captured\\n\\n" +
                             $"Type: {info.ExceptionType}\\n" +
                             $"Message: {info.Message}\\n" +
                             $"Time: {info.Timestamp:yyyy-MM-dd HH:mm:ss}\\n\\n" +
                             $"Source: {info.SourceMethod} in {Path.GetFileName(info.SourceFile)}:{info.SourceLine}\\n\\n" +
                             $"This exception has been logged for analysis.\\n" +
                             $"Check logs\\exception_*.log for details.";

                var result = MessageBox.Show(
                    message,
                    "BusBuddy - Exception Captured",
                    MessageBoxButtons.AbortRetryIgnore,
                    MessageBoxIcon.Error);

                LogException($"User action: {result}", "INFO");

                if (result == DialogResult.Abort)
                {
                    Environment.Exit(1);
                }
            }
            catch (Exception dialogEx)
            {
                Console.WriteLine($"❌ Failed to show exception dialog: {dialogEx.Message}");
            }
        }

        /// <summary>
        /// Log exception message to file
        /// </summary>
        private static void LogException(string message, string level)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] [{level}] {message}";

                Console.WriteLine(logEntry);

                Directory.CreateDirectory("logs");
                File.AppendAllText(_logPath, logEntry + Environment.NewLine);
            }
            catch { /* Ignore logging errors */ }
        }

        /// <summary>
        /// Get all captured exceptions for analysis
        /// </summary>
        public static List<ExceptionInfo> GetCapturedExceptions()
        {
            return new List<ExceptionInfo>(_capturedExceptions);
        }

        /// <summary>
        /// Clear captured exceptions
        /// </summary>
        public static void ClearCapturedExceptions()
        {
            _capturedExceptions.Clear();
            LogException("Captured exceptions cleared", "INFO");
        }
    }
}

