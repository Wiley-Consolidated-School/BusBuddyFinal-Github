using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace BusBuddy.Data
{
    /// <summary>
    /// Simple database checker that verifies BusBuddy database is online and accessible,
    /// brings it online if offline, and creates it from script if missing.
    /// </summary>
    public static class SimpleDatabaseCheck
    {
        private static readonly string DatabaseScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Db", "DatabaseScript.sql");

        /// <summary>
        /// Checks if the BusBuddy database is online and accessible.
        /// If offline, attempts to bring it online.
        /// If missing, runs DatabaseScript.sql to create it.
        /// </summary>
        /// <returns>True if database is accessible, false otherwise</returns>
        public static async Task<bool> CheckAndFixDatabaseAsync()
        {
            try
            {
                // Get connection string from configuration
                string connectionString = DatabaseConfiguration.GetConnectionString();
                string databaseName = DatabaseConfiguration.GetDatabaseName();

                Console.WriteLine($"Checking database: {databaseName}");

                // First, check if database exists
                if (!await DatabaseExistsAsync(connectionString, databaseName))
                {
                    Console.WriteLine("Database does not exist. Creating from script...");
                    return await CreateDatabaseFromScriptAsync(connectionString);
                }

                // Check if database is online and accessible
                if (await IsDatabaseOnlineAsync(connectionString))
                {
                    Console.WriteLine("✅ Database is online and accessible");
                    return true;
                }

                // Database exists but is offline - try to bring it online
                Console.WriteLine("Database is offline. Attempting to bring online...");
                return await BringDatabaseOnlineAsync(connectionString, databaseName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking database: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if the database exists
        /// </summary>
        private static async Task<bool> DatabaseExistsAsync(string connectionString, string databaseName)
        {
            try
            {
                // Connect to master database to check if target database exists
                var masterConnectionString = GetMasterConnectionString(connectionString);
                
                using (var connection = new SqlConnection(masterConnectionString))
                {
                    await connection.OpenAsync();
                    
                    using (var command = new SqlCommand($"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'", connection))
                    {
                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if database exists: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if the database is online and accessible
        /// </summary>
        private static async Task<bool> IsDatabaseOnlineAsync(string connectionString)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    using (var command = new SqlCommand("SELECT 1", connection))
                    {
                        await command.ExecuteScalarAsync();
                        return true;
                    }
                }
            }
            catch (SqlException ex)
            {
                // Check for specific offline database errors
                if (ex.Number == 942 || ex.Number == 4064 || ex.Number == 911)
                {
                    Console.WriteLine($"Database is offline (SQL Error {ex.Number}): {ex.Message}");
                    return false;
                }
                
                Console.WriteLine($"Database connection error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error checking database status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to bring the database online
        /// </summary>
        private static async Task<bool> BringDatabaseOnlineAsync(string connectionString, string databaseName)
        {
            try
            {
                var masterConnectionString = GetMasterConnectionString(connectionString);
                
                using (var connection = new SqlConnection(masterConnectionString))
                {
                    await connection.OpenAsync();
                    
                    // Set database to online
                    using (var command = new SqlCommand($"ALTER DATABASE [{databaseName}] SET ONLINE", connection))
                    {
                        await command.ExecuteNonQueryAsync();
                        Console.WriteLine("✅ Database brought online successfully");
                    }
                    
                    // Verify it's now accessible
                    return await IsDatabaseOnlineAsync(connectionString);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error bringing database online: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Creates the database from DatabaseScript.sql
        /// </summary>
        private static async Task<bool> CreateDatabaseFromScriptAsync(string connectionString)
        {
            try
            {
                if (!File.Exists(DatabaseScriptPath))
                {
                    Console.WriteLine($"❌ Database script not found at: {DatabaseScriptPath}");
                    return false;
                }

                string scriptContent = await File.ReadAllTextAsync(DatabaseScriptPath);
                var masterConnectionString = GetMasterConnectionString(connectionString);

                using (var connection = new SqlConnection(masterConnectionString))
                {
                    await connection.OpenAsync();
                    
                    // Split script by GO statements and execute each batch
                    string[] batches = scriptContent.Split(new string[] { "\nGO", "\r\nGO" }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (string batch in batches)
                    {
                        string trimmedBatch = batch.Trim();
                        if (!string.IsNullOrEmpty(trimmedBatch))
                        {
                            using (var command = new SqlCommand(trimmedBatch, connection))
                            {
                                command.CommandTimeout = 120; // 2 minutes timeout
                                await command.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }

                Console.WriteLine("✅ Database created successfully from script");
                
                // Verify the new database is accessible
                return await IsDatabaseOnlineAsync(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating database from script: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Converts a database connection string to connect to master database
        /// </summary>
        private static string GetMasterConnectionString(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            };
            return builder.ConnectionString;
        }
    }
}

