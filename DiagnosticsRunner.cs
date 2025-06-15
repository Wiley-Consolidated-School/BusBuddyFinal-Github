using System;
using BusBuddy.Data;

namespace BusBuddy.DiagnosticsRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting BusBuddy Database Diagnostics...");
            Console.WriteLine();

            try
            {
                DatabaseDiagnostics.RunDiagnostics();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Diagnostics failed: {ex.Message}");
                Console.WriteLine($"Full error: {ex}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
