using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace BusBuddy.Data
{
    /// <summary>
    /// Enhanced diagnostics tool for BusBuddy database
    /// This class provides improved error handling and database recovery functionality
    /// </summary>
    public static class DatabaseDiagnosticsEnhanced
    {
        public static async Task<bool> RepairDatabaseAsync()
        {
            Console.WriteLine("=== Enhanced Database Repair Tool ===");

            try
            {
                // Step 1: Check if SQL Server is running
                Console.WriteLine("\n[Step 1] Checking SQL Server connectivity...");
                if (!await TestSqlServerConnectivityAsync())
                {
                    Console.WriteLine("❌ Failed to connect to SQL Server - cannot continue repair");
                    return false;
                }

                // Step 2: Check connection string
                Console.WriteLine("\n[Step 2] Checking connection string...");
                var connectionString = GetConnectionString();
                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("❌ Invalid connection string - cannot continue repair");
                    return false;
                }

                // Step 3: Check if database exists
                Console.WriteLine("\n[Step 3] Checking if database exists...");
                var dbExists = await CheckDatabaseExistsAsync();
                if (!dbExists)
                {
                    Console.WriteLine("Database does not exist - attempting to create it...");
                    if (!await CreateDatabaseAsync())
                    {
                        Console.WriteLine("❌ Failed to create database - cannot continue repair");
                        return false;
                    }
                }

                // Step 4: Check if database is offline
                Console.WriteLine("\n[Step 4] Checking database state...");
                var isOffline = await IsDatabaseOfflineAsync();
                if (isOffline)
                {
                    Console.WriteLine("Database is OFFLINE - attempting to bring online...");
                    if (!await BringDatabaseOnlineAsync())
                    {
                        Console.WriteLine("❌ Failed to bring database online - attempting recovery...");
                        if (!await RecoverDatabaseAsync())
                        {
                            Console.WriteLine("❌ Database recovery failed - manual intervention required");
                            return false;
                        }
                    }
                }

                // Step 5: Final connection test
                Console.WriteLine("\n[Step 5] Final connection test...");
                if (await TestDatabaseConnectionAsync())
                {
                    Console.WriteLine("✅ Database is now online and accessible");
                    return true;
                }
                else
                {
                    Console.WriteLine("❌ Database is still inaccessible after repair attempts");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during database repair: {ex.Message}");
                LogError("Database repair error", ex);
                return false;
            }
        }

        private static async Task<bool> TestSqlServerConnectivityAsync()
        {
            var masterConnectionString = "Server=.\\SQLEXPRESS01;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=10";

            try
            {
                using (var connection = new SqlConnection(masterConnectionString))
                {
                    await connection.OpenAsync();
                    Console.WriteLine("✅ Connected to SQL Server master database");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to connect to SQL Server: {ex.Message}");
                return false;
            }
        }

        private static string GetConnectionString()
        {
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("DefaultConnection not found, trying fallback...");
                    connectionString = "Server=.\\SQLEXPRESS01;Database=BusBuddy;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=30";
                }

                Console.WriteLine($"Using connection string: {connectionString}");
                return connectionString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting connection string: {ex.Message}");
                return "Server=.\\SQLEXPRESS01;Database=BusBuddy;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=30";
            }
        }

        private static async Task<bool> CheckDatabaseExistsAsync()
        {
            var masterConnectionString = "Server=.\\SQLEXPRESS01;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=10";

            try
            {
                using (var connection = new SqlConnection(masterConnectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SELECT COUNT(*) FROM sys.databases WHERE name = 'BusBuddy'", connection))
                    {
                        var result = await command.ExecuteScalarAsync();
                        return result != null && Convert.ToInt32(result) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if database exists: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> CreateDatabaseAsync()
        {
            var masterConnectionString = "Server=.\\SQLEXPRESS01;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=10";

            try
            {
                using (var connection = new SqlConnection(masterConnectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("CREATE DATABASE BusBuddy", connection))
                    {
                        await command.ExecuteNonQueryAsync();
                        Console.WriteLine("✅ Database created successfully");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating database: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> IsDatabaseOfflineAsync()
        {
            var masterConnectionString = "Server=.\\SQLEXPRESS01;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=10";

            try
            {
                using (var connection = new SqlConnection(masterConnectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SELECT state_desc FROM sys.databases WHERE name = 'BusBuddy'", connection))
                    {
                        var state = (string)await command.ExecuteScalarAsync();
                        Console.WriteLine($"Database state: {state}");
                        return state == "OFFLINE";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking database state: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> BringDatabaseOnlineAsync()
        {
            var masterConnectionString = "Server=.\\SQLEXPRESS01;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=10";

            try
            {
                using (var connection = new SqlConnection(masterConnectionString))
                {
                    await connection.OpenAsync();

                    // First, try to kill any active connections
                    using (var killCommand = new SqlCommand(@"
                        DECLARE @kill varchar(8000) = '';
                        SELECT @kill = @kill + 'kill ' + CONVERT(varchar(5), session_id) + ';'
                        FROM sys.dm_exec_sessions
                        WHERE database_id = DB_ID('BusBuddy');
                        EXEC(@kill);
                    ", connection))
                    {
                        await killCommand.ExecuteNonQueryAsync();
                    }

                    // Then bring the database online
                    using (var command = new SqlCommand("ALTER DATABASE BusBuddy SET ONLINE WITH ROLLBACK IMMEDIATE", connection))
                    {
                        await command.ExecuteNonQueryAsync();
                        Console.WriteLine("✅ Database brought online successfully");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error bringing database online: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> RecoverDatabaseAsync()
        {
            var masterConnectionString = "Server=.\\SQLEXPRESS01;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=10";

            try
            {
                using (var connection = new SqlConnection(masterConnectionString))
                {
                    await connection.OpenAsync();

                    // Try emergency mode
                    using (var command = new SqlCommand(@"
                        ALTER DATABASE BusBuddy SET EMERGENCY;
                        ALTER DATABASE BusBuddy SET SINGLE_USER;
                        DBCC CHECKDB (BusBuddy, REPAIR_ALLOW_DATA_LOSS);
                        ALTER DATABASE BusBuddy SET MULTI_USER;
                        ALTER DATABASE BusBuddy SET ONLINE;
                    ", connection))
                    {
                        await command.ExecuteNonQueryAsync();
                        Console.WriteLine("✅ Database recovered successfully");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recovering database: {ex.Message}");

                // Last resort - drop and recreate
                try
                {
                    Console.WriteLine("Attempting drastic recovery - drop and recreate database");
                    using (var connection = new SqlConnection(masterConnectionString))
                    {
                        await connection.OpenAsync();

                        using (var dropCommand = new SqlCommand("ALTER DATABASE BusBuddy SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE BusBuddy;", connection))
                        {
                            await dropCommand.ExecuteNonQueryAsync();
                        }

                        using (var createCommand = new SqlCommand("CREATE DATABASE BusBuddy", connection))
                        {
                            await createCommand.ExecuteNonQueryAsync();
                            Console.WriteLine("✅ Database recreated successfully");
                            return true;
                        }
                    }
                }
                catch (Exception recreateEx)
                {
                    Console.WriteLine($"Error recreating database: {recreateEx.Message}");
                    return false;
                }
            }
        }

        private static async Task<bool> TestDatabaseConnectionAsync()
        {
            var connectionString = GetConnectionString();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    Console.WriteLine("✅ Connected to BusBuddy database successfully");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to connect to BusBuddy database: {ex.Message}");
                return false;
            }
        }

        private static void LogError(string message, Exception ex)
        {
            try
            {
                Directory.CreateDirectory("logs");
                var logFileName = $"logs/database_repair_{DateTime.Now:yyyyMMdd}.log";
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n" +
                              $"Exception: {ex.Message}\n" +
                              $"Stack Trace: {ex.StackTrace}\n" +
                              $"Inner Exception: {ex.InnerException?.Message ?? "None"}\n" +
                              new string('-', 80) + "\n";

                File.AppendAllText(logFileName, logEntry);
            }
            catch
            {
                // If logging fails, just continue
            }
        }
    }
}
