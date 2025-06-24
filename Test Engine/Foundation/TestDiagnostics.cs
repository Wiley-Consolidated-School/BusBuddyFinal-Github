using System;
using System.IO;

namespace BusBuddy.TestEngine.Foundation
{
    /// <summary>
    /// Simple test diagnostics - does it work? Yes or no.
    /// Logs basic information to help troubleshoot when tests fail.
    /// </summary>
    public static class TestDiagnostics
    {
        private static readonly object _logLock = new object();
        private static readonly string _logDirectory = Path.Combine(Environment.CurrentDirectory, "TestResults");
        private static readonly string _logFilePath = Path.Combine(_logDirectory, $"test-log-{DateTime.Now:yyyy-MM-dd}.log");

        /// <summary>
        /// Log a simple message - either it worked or it didn't
        /// </summary>
        /// <param name="testName">Name of the test or component</param>
        /// <param name="message">What happened</param>
        /// <param name="isSuccess">Did it work? True = yes, False = no</param>
        public static void Log(string testName, string message, bool isSuccess = true)
        {
            var status = isSuccess ? "✅ SUCCESS" : "❌ FAILED";
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{status}] {testName}: {message}";

            // Write to console
            Console.WriteLine(logEntry);

            // Write to file
            try
            {
                lock (_logLock)
                {
                    Directory.CreateDirectory(_logDirectory);
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }
            }
            catch
            {
                // If logging fails, don't fail the test - the test result is what matters
            }
        }

        /// <summary>
        /// Log that something worked
        /// </summary>
        public static void LogSuccess(string testName, string message) => Log(testName, message, true);

        /// <summary>
        /// Log that something didn't work
        /// </summary>
        public static void LogFailure(string testName, string message) => Log(testName, message, false);

        /// <summary>
        /// Log the start of a test - DIWYN style: simple test beginning marker
        /// </summary>
        public static void LogTestStart(string testName) => Log(testName, "Test Started", true);

        /// <summary>
        /// Log the end of a test - DIWYN style: simple test completion marker
        /// </summary>
        public static void LogTestEnd(string testName) => Log(testName, "Test Completed", true);

        /// <summary>
        /// Log levels for DIWYN - keep it simple: Debug, Warning, Error
        /// </summary>
        public enum LogLevel
        {
            Debug,
            Warning,
            Error,
            Info  // Add missing Info level
        }

        /// <summary>
        /// Log an operation with a specific level - DIWYN style compatibility method
        /// </summary>
        public static void LogOperation(string testName, string message, LogLevel level)
        {
            bool isSuccess = level != LogLevel.Error; // Errors are failures, Debug/Warning/Info are successes
            string levelText = level switch
            {
                LogLevel.Debug => "DEBUG",
                LogLevel.Warning => "WARNING",
                LogLevel.Error => "ERROR",
                LogLevel.Info => "INFO",
                _ => "INFO"
            };

            var formattedMessage = $"[{levelText}] {message}";
            Log(testName, formattedMessage, isSuccess);
        }

        /// <summary>
        /// Log an exception - DIWYN style compatibility method
        /// </summary>
        public static void LogException(string testName, Exception ex)
        {
            Log(testName, $"Exception: {ex.Message}", false);
        }

        /// <summary>
        /// Log form creation - DIWYN style compatibility method
        /// </summary>
        public static void LogFormCreation(string testName, object form)
        {
            string formType = form?.GetType().Name ?? "Unknown";
            Log(testName, $"Form created: {formType}", true);
        }

        /// <summary>
        /// Log form disposal - DIWYN style compatibility method
        /// </summary>
        public static void LogFormDisposal(string testName, object form, object? additionalInfo = null)
        {
            string formType = form?.GetType().Name ?? "Unknown";
            string message = $"Form disposed: {formType}";
            if (additionalInfo != null)
            {
                string infoText = additionalInfo switch
                {
                    bool success => success ? "successfully" : "with errors",
                    string text => text,
                    _ => additionalInfo.ToString() ?? ""
                };
                if (!string.IsNullOrEmpty(infoText))
                    message += $" - {infoText}";
            }
            Log(testName, message, true);
        }

        /// <summary>
        /// Log a general message during a test - DIWYN style: what's happening
        /// </summary>
        public static void LogMessage(string message) => Log("INFO", message, true);
    }
}
