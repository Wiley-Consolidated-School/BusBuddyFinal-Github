using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BusBuddy.Data;

namespace BusBuddy
{
    class DebugConfig
    {
        /// <summary>
        /// Enable enhanced debugging features for multithreaded scenarios
        /// Based on Microsoft documentation: https://learn.microsoft.com/en-us/visualstudio/debugger/debug-multithreaded-applications-in-visual-studio
        /// </summary>
        public static void EnableEnhancedDebugging()
        {
            // Set thread names for easier debugging in Threads window
            Thread.CurrentThread.Name = "BusBuddy-Main-UI";
              // Add debug listeners for better tracing
            Trace.Listeners.Add(new ConsoleTraceListener());

            // Enable first-chance exception notifications
            AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
            {
                Debug.WriteLine($"🔍 [FIRST-CHANCE] {e.Exception.GetType().Name}: {e.Exception.Message}");
                Debug.WriteLine($"🔍 [THREAD] {Thread.CurrentThread.Name ?? Thread.CurrentThread.ManagedThreadId.ToString()}");
            };

            Debug.WriteLine("✅ Enhanced debugging enabled for BusBuddy");
        }

        /// <summary>
        /// Trace async operation for parallel debugging
        /// </summary>
        public static async Task<T> TraceAsync<T>(string operationName, Func<Task<T>> operation)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var taskId = Task.CurrentId ?? -1;

            Debug.WriteLine($"▶️ [ASYNC-START] {operationName} | Thread:{threadId} Task:{taskId}");

            try
            {
                var result = await operation();
                Debug.WriteLine($"✅ [ASYNC-END] {operationName} | Thread:{Thread.CurrentThread.ManagedThreadId} Task:{Task.CurrentId ?? -1}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ [ASYNC-ERROR] {operationName} | {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("=== Configuration Debug ===");

                var conn = ConfigurationManager.ConnectionStrings["DefaultConnection"];
                if (conn == null)
                {
                    Console.WriteLine("DefaultConnection is NULL");
                }
                else
                {
                    Console.WriteLine($"ConnectionString: {conn.ConnectionString}");
                    Console.WriteLine($"ProviderName: {conn.ProviderName}");
                }

                Console.WriteLine("\nAll connection strings:");
                foreach (ConnectionStringSettings connStr in ConfigurationManager.ConnectionStrings)
                {
                    Console.WriteLine($"  Name: {connStr.Name}");
                    Console.WriteLine($"  ConnectionString: {connStr.ConnectionString}");
                    Console.WriteLine($"  ProviderName: {connStr.ProviderName}");
                    Console.WriteLine();
                }

                Console.WriteLine("App Settings:");
                var allKeys = ConfigurationManager.AppSettings.AllKeys;
                if (allKeys != null)
                {
                    foreach (string? key in allKeys)
                    {
                        if (key != null)
                        {
                            Console.WriteLine($"  {key}: {ConfigurationManager.AppSettings[key] ?? "null"}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
