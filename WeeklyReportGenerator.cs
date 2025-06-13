using System;
using System.IO;
using BusBuddy.Business;

namespace BusBuddy
{
    class WeeklyReportGenerator
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("=================================================================");
            System.Console.WriteLine("            BusBuddy Weekly Timecard Report Generator");
            System.Console.WriteLine("=================================================================");
            System.Console.WriteLine();

            try
            {
                var reportGenerator = new WeeklyTimecardReport();

                // Generate report for the week of June 9th (your example data)
                System.Console.WriteLine("Generating weekly report for the week of June 9th, 2025...");
                System.Console.WriteLine();

                var june9Report = reportGenerator.GenerateJune9WeekReport("Steve McKitrick");

                // Display the report
                System.Console.WriteLine(june9Report);

                // Save the report to a file
                var fileName = $"Weekly_Timecard_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var filePath = Path.Combine(Environment.CurrentDirectory, fileName);

                File.WriteAllText(filePath, june9Report);
                System.Console.WriteLine($"Report saved to: {filePath}");
                System.Console.WriteLine();

                // Show additional options
                System.Console.WriteLine("=================================================================");
                System.Console.WriteLine("Additional Report Options:");
                System.Console.WriteLine("1. Run 'dotnet run June9Report' to generate June 9th week report");
                System.Console.WriteLine("2. Run 'dotnet run CurrentWeek' to generate current week report");
                System.Console.WriteLine("3. Run 'dotnet run CustomDate YYYY-MM-DD' for a specific date");
                System.Console.WriteLine("=================================================================");

                // Handle command line arguments
                if (args.Length > 0)
                {
                    HandleCommandLineArgs(args, reportGenerator);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error generating report: {ex.Message}");
                System.Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            System.Console.WriteLine();
            System.Console.WriteLine("Press any key to exit...");
            System.Console.ReadKey();
        }

        static void HandleCommandLineArgs(string[] args, WeeklyTimecardReport reportGenerator)
        {
            var command = args[0].ToLower();

            switch (command)
            {
                case "june9report":
                    var june9Report = reportGenerator.GenerateJune9WeekReport("Steve McKitrick");
                    System.Console.WriteLine("\n" + june9Report);
                    break;

                case "currentweek":
                    var currentWeekReport = reportGenerator.GenerateWeeklyReport(DateTime.Today, "Employee");
                    System.Console.WriteLine("\n" + currentWeekReport);
                    break;

                case "customdate":
                    if (args.Length > 1 && DateTime.TryParse(args[1], out DateTime customDate))
                    {
                        var customReport = reportGenerator.GenerateWeeklyReport(customDate, "Employee");
                        System.Console.WriteLine($"\nReport for week containing {customDate:MM/dd/yyyy}:");
                        System.Console.WriteLine(customReport);
                    }
                    else
                    {
                        System.Console.WriteLine("\nInvalid date format. Use YYYY-MM-DD format.");
                    }
                    break;

                default:
                    System.Console.WriteLine($"\nUnknown command: {command}");
                    break;
            }
        }
    }
}
