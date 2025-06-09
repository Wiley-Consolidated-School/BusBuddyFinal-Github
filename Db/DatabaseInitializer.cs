using System;
using System.IO;
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace BusBuddy.Db
{
    public static class DatabaseInitializer
    {
        public static void InitializeDatabase()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var providerName = ConfigurationManager.ConnectionStrings["DefaultConnection"].ProviderName;
            
            if (providerName == "Microsoft.Data.Sqlite")
            {
                InitializeSqliteDatabase(connectionString);
            }
            else
            {
                InitializeSqlServerDatabase(connectionString);
            }
        }
        
        private static void InitializeSqliteDatabase(string connectionString)
        {
            var dbPath = connectionString.Replace("Data Source=", "").Trim();
            bool createTables = !File.Exists(dbPath);
            
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                
                if (createTables)
                {
                    ExecuteSqliteScript(connection);
                }
                
                // Validate schema after initialization
                ValidateSqliteSchema(connection);
            }
        }
        
        private static void InitializeSqlServerDatabase(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                // Check if tables exist
                var tableCount = CheckSqlServerTables(connection);
                
                if (tableCount == 0)
                {
                    ExecuteSqlServerScript(connection);
                }
                
                // Validate schema after initialization
                ValidateSqlServerSchema(connection);
            }
        }
        
        private static void ExecuteSqliteScript(SqliteConnection connection)
        {
            string script = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BusBuddy.Data", "DatabaseScript.sql"));
            
            foreach (var command in script.Split(';'))
            {
                if (!string.IsNullOrWhiteSpace(command))
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = command;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }        private static void ExecuteSqlServerScript(SqlConnection connection)
        {
            string sqlServerScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BusBuddy.Data", "DatabaseScript.SqlServer.sql");
            
            // If the SQL Server specific script exists, use it
            if (File.Exists(sqlServerScriptPath))
            {
                string script = File.ReadAllText(sqlServerScriptPath);
                
                // Execute the SQL Server script
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = script;
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error executing SQL Server script: {ex.Message}", ex);
                    }
                }
            }
            else
            {
                // Fall back to the SQLite script with conversions
                string script = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BusBuddy.Data", "DatabaseScript.sql"));
                
                // Replace SQLite-specific syntax with SQL Server syntax
                script = script.Replace("INTEGER PRIMARY KEY AUTOINCREMENT", "INT IDENTITY(1,1) PRIMARY KEY");
                script = script.Replace("REAL", "DECIMAL(18,2)");
                script = script.Replace("TEXT", "NVARCHAR(MAX)");
                script = script.Replace("IF NOT EXISTS", "");
                
                // Handle CREATE TABLE IF NOT EXISTS - SQL Server doesn't support this directly
                script = script.Replace("CREATE TABLE IF NOT EXISTS", "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'TEMP_TABLE_NAME') CREATE TABLE");
                
                // Process each table creation separately
                string[] tableNames = { "Vehicles", "Drivers", "Routes", "Activities", "Fuel", "Maintenance", "SchoolCalendar", "ActivitySchedule", "TimeCard" };
                
                foreach (var tableName in tableNames)
                {
                    // Extract the CREATE TABLE statement for this table
                    var startMarker = $"CREATE TABLE IF NOT EXISTS {tableName}";
                    var tableStatement = GetStatementFromScript(script, startMarker);
                    
                    if (!string.IsNullOrEmpty(tableStatement))
                    {
                        // Fix the SQL Server syntax for table creation
                        var modifiedStatement = $"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'{tableName}') CREATE TABLE {tableName} " + 
                                              tableStatement.Substring(tableStatement.IndexOf('('));
                        
                        // Remove SQLite-style FOREIGN KEY constraints - we'll add them after table creation
                        var cleanedStatement = RemoveForeignKeyConstraints(modifiedStatement);
                        
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = cleanedStatement;
                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"Error creating table {tableName}: {cleanedStatement}", ex);
                            }
                        }
                    }
                }
                
                // Add foreign key constraints after all tables are created
                AddForeignKeyConstraints(connection, script);
                
                // Create indexes
                CreateSqlServerIndexes(connection, script);
            }
        }
        
        private static int CheckSqlServerTables(SqlConnection connection)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
                return (int)cmd.ExecuteScalar();
            }
        }
        
        private static void ValidateSqliteSchema(SqliteConnection connection)
        {
            // Validate tables exist
            var requiredTables = new List<string> {
                "Vehicles", "Drivers", "Routes", "Activities", "Fuel", 
                "Maintenance", "SchoolCalendar", "ActivitySchedule", "TimeCard"
            };
            
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
                using (var reader = cmd.ExecuteReader())
                {
                    var existingTables = new List<string>();
                    while (reader.Read())
                    {
                        existingTables.Add(reader.GetString(0));
                    }
                    
                    // Check for missing tables
                    var missingTables = requiredTables.Except(existingTables).ToList();
                    if (missingTables.Any())
                    {
                        throw new Exception($"Missing tables in SQLite database: {string.Join(", ", missingTables)}");
                    }
                }
            }
            
            // Validate indexes exist
            ValidateSqliteIndexes(connection);
        }
        
        private static void ValidateSqliteIndexes(SqliteConnection connection)
        {
            var requiredIndexes = new List<string> {
                "idx_routes_date", "idx_routes_driver", "idx_routes_vehicle",
                "idx_activities_date", "idx_activities_driver", "idx_activities_vehicle",
                "idx_fuel_date", "idx_fuel_vehicle",
                "idx_maintenance_date", "idx_maintenance_vehicle",
                "idx_calendar_date", "idx_calendar_enddate", "idx_calendar_category",
                "idx_activityschedule_date", "idx_activityschedule_driver", "idx_activityschedule_vehicle",
                "idx_timecard_date", "idx_timecard_daytype", "idx_timecard_driver"
            };
            
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='index' AND name NOT LIKE 'sqlite_%'";
                using (var reader = cmd.ExecuteReader())
                {
                    var existingIndexes = new List<string>();
                    while (reader.Read())
                    {
                        existingIndexes.Add(reader.GetString(0));
                    }
                    
                    // Check for missing indexes
                    var missingIndexes = requiredIndexes.Except(existingIndexes).ToList();
                    if (missingIndexes.Any())
                    {
                        throw new Exception($"Missing indexes in SQLite database: {string.Join(", ", missingIndexes)}");
                    }
                }
            }
        }
        
        private static void ValidateSqlServerSchema(SqlConnection connection)
        {
            // Validate tables exist
            var requiredTables = new List<string> {
                "Vehicles", "Drivers", "Routes", "Activities", "Fuel", 
                "Maintenance", "SchoolCalendar", "ActivitySchedule", "TimeCard"
            };
            
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
                using (var reader = cmd.ExecuteReader())
                {
                    var existingTables = new List<string>();
                    while (reader.Read())
                    {
                        existingTables.Add(reader.GetString(0));
                    }
                    
                    // Check for missing tables
                    var missingTables = requiredTables.Except(existingTables, StringComparer.OrdinalIgnoreCase).ToList();
                    if (missingTables.Any())
                    {
                        throw new Exception($"Missing tables in SQL Server database: {string.Join(", ", missingTables)}");
                    }
                }
            }
            
            // Validate indexes exist
            ValidateSqlServerIndexes(connection);
        }
          private static void ValidateSqlServerIndexes(SqlConnection connection)
        {
            var requiredIndexes = new List<string> {
                "idx_routes_date", "idx_routes_driver", "idx_routes_vehicle",
                "idx_activities_date", "idx_activities_driver", "idx_activities_vehicle",
                "idx_fuel_date", "idx_fuel_vehicle",
                "idx_maintenance_date", "idx_maintenance_vehicle",
                "idx_calendar_date", "idx_calendar_enddate", "idx_calendar_category",
                "idx_activityschedule_date", "idx_activityschedule_driver", "idx_activityschedule_vehicle",
                "idx_timecard_date", "idx_timecard_daytype", "idx_timecard_driver"
            };
            
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT name FROM sys.indexes WHERE is_primary_key = 0 AND is_unique_constraint = 0";
                using (var reader = cmd.ExecuteReader())
                {
                    var existingIndexes = new List<string>();
                    while (reader.Read())
                    {
                        existingIndexes.Add(reader.GetString(0));
                    }
                    
                    // Check for missing indexes
                    var missingIndexes = requiredIndexes.Except(existingIndexes, StringComparer.OrdinalIgnoreCase).ToList();
                    if (missingIndexes.Any())
                    {
                        throw new Exception($"Missing indexes in SQL Server database: {string.Join(", ", missingIndexes)}");
                    }
                }
            }
        }
        
        // Helper method to extract a SQL statement from the script
        private static string GetStatementFromScript(string script, string startMarker)
        {
            var startIndex = script.IndexOf(startMarker);
            if (startIndex == -1) return string.Empty;
            
            var endIndex = script.IndexOf(");", startIndex);
            if (endIndex == -1) return string.Empty;
            
            return script.Substring(startIndex, endIndex - startIndex + 2);
        }
        
        // Helper method to remove SQLite-style FOREIGN KEY constraints
        private static string RemoveForeignKeyConstraints(string statement)
        {
            var lines = statement.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new List<string>();
            
            foreach (var line in lines)
            {
                if (!line.Trim().StartsWith("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(line);
                }
            }
            
            return string.Join(Environment.NewLine, result);
        }
        
        // Helper method to add foreign key constraints
        private static void AddForeignKeyConstraints(SqlConnection connection, string script)
        {
            var tableNames = new[] { "Routes", "Activities", "Fuel", "Maintenance", "ActivitySchedule", "TimeCard" };
            
            foreach (var tableName in tableNames)
            {
                var tableStatement = GetStatementFromScript(script, $"CREATE TABLE IF NOT EXISTS {tableName}");
                if (string.IsNullOrEmpty(tableStatement)) continue;
                
                var lines = tableStatement.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract foreign key details
                        var openParenIndex = trimmedLine.IndexOf('(');
                        var closeParenIndex = trimmedLine.IndexOf(')');
                        if (openParenIndex != -1 && closeParenIndex != -1)
                        {
                            var columnName = trimmedLine.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);
                            
                            var referencesIndex = trimmedLine.IndexOf("REFERENCES", StringComparison.OrdinalIgnoreCase);
                            if (referencesIndex != -1)
                            {
                                var refTableOpenParenIndex = trimmedLine.IndexOf('(', referencesIndex);
                                var refTableCloseParenIndex = trimmedLine.IndexOf(')', refTableOpenParenIndex);
                                
                                if (refTableOpenParenIndex != -1 && refTableCloseParenIndex != -1)
                                {
                                    var refTableName = trimmedLine.Substring(referencesIndex + 10, refTableOpenParenIndex - referencesIndex - 10).Trim();
                                    var refColumnName = trimmedLine.Substring(refTableOpenParenIndex + 1, refTableCloseParenIndex - refTableOpenParenIndex - 1);
                                    
                                    // Create the ALTER TABLE statement for SQL Server
                                    var alterStatement = $@"
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_{tableName}_{columnName}_{refTableName}') AND parent_object_id = OBJECT_ID(N'{tableName}'))
BEGIN
    ALTER TABLE {tableName} 
    ADD CONSTRAINT FK_{tableName}_{columnName}_{refTableName} 
    FOREIGN KEY ({columnName}) REFERENCES {refTableName}({refColumnName});
END";
                                    
                                    using (var cmd = connection.CreateCommand())
                                    {
                                        cmd.CommandText = alterStatement;
                                        try
                                        {
                                            cmd.ExecuteNonQuery();
                                        }
                                        catch (Exception ex)
                                        {
                                            throw new Exception($"Error adding foreign key constraint: {alterStatement}", ex);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        // Helper method to create indexes
        private static void CreateSqlServerIndexes(SqlConnection connection, string script)
        {
            var indexLines = script.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => line.Trim().StartsWith("CREATE INDEX", StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            foreach (var indexLine in indexLines)
            {
                // Extract index name
                var indexNameStart = indexLine.IndexOf("idx_", StringComparison.OrdinalIgnoreCase);
                if (indexNameStart == -1) continue;
                
                var indexNameEnd = indexLine.IndexOf(" ON ", indexNameStart, StringComparison.OrdinalIgnoreCase);
                if (indexNameEnd == -1) continue;
                
                var indexName = indexLine.Substring(indexNameStart, indexNameEnd - indexNameStart).Trim();
                
                // Modified index statement for SQL Server
                var sqlServerIndexStatement = $@"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'{indexName}' AND object_id = OBJECT_ID(N'{indexLine.Substring(indexNameEnd + 4).Trim()}'))
BEGIN
    {indexLine.Replace("CREATE INDEX IF NOT EXISTS", "CREATE INDEX")}
END";
                
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sqlServerIndexStatement;
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error creating index: {sqlServerIndexStatement}", ex);
                    }
                }
            }
        }
    }
}
