using System;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Simple logging helper to replace console logging throughout the application
    /// Provides structured logging with levels and categories
    /// </summary>
    public static class BusBuddyLogger
    {
        public enum LogLevel
        {
            Info,
            Warning,
            Error,
            Debug
        }

        private static readonly string LogFilePath = "logs\\BusBuddy.log";

        static BusBuddyLogger()
        {
            // Ensure logs directory exists
            try
            {
                var logDir = System.IO.Path.GetDirectoryName(LogFilePath);
                if (!string.IsNullOrEmpty(logDir) && !System.IO.Directory.Exists(logDir))
                {
                    System.IO.Directory.CreateDirectory(logDir);
                }
            }
            catch
            {
                // Ignore if we can't create log directory
            }
        }

        public static void Log(LogLevel level, string category, string message)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var levelIcon = level switch
            {
                LogLevel.Info => "ℹ️",
                LogLevel.Warning => "⚠️",
                LogLevel.Error => "❌",
                LogLevel.Debug => "🔍",
                _ => "📝"
            };

            var logMessage = $"[{timestamp}] {levelIcon} [{category}] {message}";

            // Write to console (for development)
            Console.WriteLine(logMessage);

            // Write to file (for production)
            try
            {
                using (var writer = new System.IO.StreamWriter(LogFilePath, true))
                {
                    writer.WriteLine(logMessage);
                }
            }
            catch
            {
                // Ignore file logging errors to prevent cascading failures
            }
        }

        public static void Info(string category, string message) => Log(LogLevel.Info, category, message);
        public static void Warning(string category, string message) => Log(LogLevel.Warning, category, message);
        public static void Error(string category, string message) => Log(LogLevel.Error, category, message);
        public static void Debug(string category, string message) => Log(LogLevel.Debug, category, message);

        // Convenience methods for common categories
        public static void Dashboard(string message) => Info("Dashboard", message);
        public static void Navigation(string message) => Info("Navigation", message);
        public static void Theme(string message) => Info("Theme", message);
        public static void DataRefresh(string message) => Info("DataRefresh", message);
    }
}

