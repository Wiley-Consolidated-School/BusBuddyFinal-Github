using System;
using BusBuddy.Data;
using System.Configuration;

namespace BusBuddy.Diagnostics
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("BusBuddy Database Connection Audit");
            Console.WriteLine("===================================");
            Console.WriteLine();

            // Display environment information
            var environment = ConfigurationManager.AppSettings["Environment"] ?? "Not Set";
            var provider = ConfigurationManager.AppSettings["DatabaseProvider"] ?? "Not Set";

            Console.WriteLine($"Environment: {environment}");
            Console.WriteLine($"Database Provider: {provider}");
            Console.WriteLine();

            // Run full diagnostics
            DatabaseDiagnostics.RunDiagnostics();

            Console.WriteLine();
            Console.WriteLine("Audit complete. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
