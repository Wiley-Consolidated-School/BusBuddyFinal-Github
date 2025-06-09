using System;
using System.IO;
using BusBuddy.Db;
using System.Configuration;
using System.Reflection;

namespace TestDatabaseInitializer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Testing DatabaseInitializer");
                
                // Setup the connection string
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var connectionStringsSection = config.GetSection("connectionStrings") as ConnectionStringsSection;
                
                if (connectionStringsSection != null)
                {
                    connectionStringsSection.ConnectionStrings.Clear();
                    
                    // Add SQLite connection string
                    connectionStringsSection.ConnectionStrings.Add(new ConnectionStringSettings(
                        "DefaultConnection",
                        "Data Source=test_busbuddy.db",
                        "Microsoft.Data.Sqlite"));
                    
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("connectionStrings");
                }
                
                // Initialize the database
                DatabaseInitializer.InitializeDatabase();
                
                Console.WriteLine("Database initialized successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine(ex.InnerException.StackTrace);
                }
            }
        }
    }
}
