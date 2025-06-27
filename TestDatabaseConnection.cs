using System;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using BusBuddy.Data;

namespace BusBuddy
{
    /// <summary>
    /// Standalone database connection test utility
    /// </summary>
    public class DatabaseConnectionTester
    {
        public static void TestConnection()
        {
            Console.WriteLine("🔗 Testing database connection...");

            try
            {
                // Test 1: Configuration reading
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
                if (string.IsNullOrEmpty(connectionString))
                {
                    connectionString = "Server=.\\SQLEXPRESS01;Database=BusBuddy;Trusted_Connection=True;TrustServerCertificate=True;";
                    Console.WriteLine("⚠️ Using fallback connection string");
                }

                Console.WriteLine($"Connection string: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");

                // Test 2: DbContext creation
                var options = new DbContextOptionsBuilder<BusBuddyContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                using var context = new BusBuddyContext(options);

                // Test 3: Database accessibility
                Console.WriteLine("Testing database connection...");
                var canConnect = context.Database.CanConnect();
                Console.WriteLine($"Database can connect: {canConnect}");

                if (canConnect)
                {
                    // Test 4: Basic query
                    var vehicleCount = context.Vehicles.CountAsync().Result;
                    Console.WriteLine($"✅ Database connection successful! Vehicle count: {vehicleCount}");
                }
                else
                {
                    Console.WriteLine("❌ Database connection failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database connection error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
