using System;
using System.Configuration;
using BusBuddy.Data;

class DatabaseInit
{
    static void Main()
    {
        try
        {
            Console.WriteLine("Initializing test database...");

            var testConnectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=BusBuddyDB_Test;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";
            var initializer = new SqlServerDatabaseInitializer(testConnectionString);
            initializer.Initialize();

            Console.WriteLine("Test database initialized successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
