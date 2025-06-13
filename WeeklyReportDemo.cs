using System;
using System.IO;
using System.Collections.Generic;
using BusBuddy.Models;

namespace BusBuddy
{
    class WeeklyReportDemo
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("=================================================================");
            System.Console.WriteLine("            BusBuddy Weekly Timecard Report Demo");
            System.Console.WriteLine("=================================================================");
            System.Console.WriteLine();

            try
            {
                // Create sample timecard data for June 9-13, 2025 week
                var sampleTimeCards = CreateSampleTimeCardData();

                // Generate and display the weekly report
                var report = GenerateWeeklyReportFromData(sampleTimeCards, "Steve McKitrick");

                System.Console.WriteLine(report);

                // Save the report to a file
                var fileName = $"Weekly_Timecard_Report_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var filePath = Path.Combine(Environment.CurrentDirectory, fileName);

                File.WriteAllText(filePath, report);
                System.Console.WriteLine($"\nReport saved to: {filePath}");

                System.Console.WriteLine("\n=================================================================");
                System.Console.WriteLine("This demonstrates how your weekly timecard report would look");
                System.Console.WriteLine("based on your June 9-11 entries plus weekend days.");
                System.Console.WriteLine("=================================================================");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error generating report: {ex.Message}");
            }

            System.Console.WriteLine("\nPress any key to exit...");
            System.Console.ReadKey();
        }

        static List<TimeCard> CreateSampleTimeCardData()
        {
            return new List<TimeCard>
            {
                // June 9th - Monday - Your split shift
                new TimeCard
                {
                    TimeCardID = 1,
                    Date = new DateTime(2025, 6, 9),
                    DayType = "Route Day",
                    AMClockIn = new TimeSpan(4, 16, 0),        // 4:16 AM
                    LunchClockOut = new TimeSpan(10, 30, 0),   // 10:30 AM
                    LunchClockIn = new TimeSpan(12, 30, 0),    // 12:30 PM
                    PMClockOut = new TimeSpan(17, 0, 0),       // 5:00 PM
                    TotalTime = 10.73m,                        // 10.73 hours
                    Overtime = 2.73m,                          // 2.73 overtime
                    PTOHours = 0,
                    Notes = "Split shift - Morning route 4:16-10:30, Afternoon route 12:30-17:00"
                },

                // June 10th - Tuesday - PTO Day
                new TimeCard
                {
                    TimeCardID = 2,
                    Date = new DateTime(2025, 6, 10),
                    DayType = "PTO Day",
                    AMClockIn = null,
                    LunchClockOut = null,
                    LunchClockIn = null,
                    PMClockOut = null,
                    PTOHours = 8.0m,
                    TotalTime = 8.0m,
                    Overtime = 0,
                    Notes = "PTO - Personal Time Off (8 hours)"
                },

                // June 11th - Wednesday - Another split shift
                new TimeCard
                {
                    TimeCardID = 3,
                    Date = new DateTime(2025, 6, 11),
                    DayType = "Route Day",
                    AMClockIn = new TimeSpan(5, 51, 0),        // 5:51 AM
                    LunchClockOut = new TimeSpan(12, 30, 0),   // 12:30 PM
                    LunchClockIn = new TimeSpan(13, 18, 0),    // 1:18 PM
                    PMClockOut = new TimeSpan(17, 7, 0),       // 5:07 PM
                    TotalTime = 10.47m,                        // 10.47 hours
                    Overtime = 2.47m,                          // 2.47 overtime
                    PTOHours = 0,
                    Notes = "Split shift - Morning: 5:51-12:30, Afternoon: 13:18-17:07"
                },

                // June 12th - Thursday - Regular day
                new TimeCard
                {
                    TimeCardID = 4,
                    Date = new DateTime(2025, 6, 12),
                    DayType = "Route Day",
                    AMClockIn = new TimeSpan(6, 30, 0),        // 6:30 AM
                    LunchClockOut = new TimeSpan(12, 0, 0),    // 12:00 PM
                    LunchClockIn = new TimeSpan(13, 0, 0),     // 1:00 PM
                    PMClockOut = new TimeSpan(16, 30, 0),      // 4:30 PM
                    TotalTime = 9.0m,                          // 9 hours
                    Overtime = 1.0m,                           // 1 hour overtime
                    PTOHours = 0,
                    Notes = "Regular route day"
                },

                // June 13th - Friday - Regular day
                new TimeCard
                {
                    TimeCardID = 5,
                    Date = new DateTime(2025, 6, 13),
                    DayType = "Route Day",
                    AMClockIn = new TimeSpan(6, 45, 0),        // 6:45 AM
                    LunchClockOut = new TimeSpan(12, 15, 0),   // 12:15 PM
                    LunchClockIn = new TimeSpan(13, 15, 0),    // 1:15 PM
                    PMClockOut = new TimeSpan(16, 45, 0),      // 4:45 PM
                    TotalTime = 8.5m,                          // 8.5 hours
                    Overtime = 0.5m,                           // 0.5 hour overtime
                    PTOHours = 0,
                    Notes = "Regular route day"
                }

                // Saturday and Sunday would have no entries (weekends)
            };
        }

        static string GenerateWeeklyReportFromData(List<TimeCard> timeCards, string employeeName)
        {
            var startDate = new DateTime(2025, 6, 9);  // Monday June 9th
            var endDate = new DateTime(2025, 6, 15);   // Sunday June 15th

            var report = new System.Text.StringBuilder();

            // Header
            report.AppendLine("=================================================================");
            report.AppendLine("                    WEEKLY TIMECARD REPORT");
            report.AppendLine("=================================================================");
            report.AppendLine($"Employee: {employeeName}");
            report.AppendLine($"Week of: {startDate:MM/dd/yyyy} - {endDate:MM/dd/yyyy}");
            report.AppendLine($"Report Generated: {DateTime.Now:MM/dd/yyyy hh:mm tt}");
            report.AppendLine("=================================================================");
            report.AppendLine();

            // Column headers
            report.AppendLine("Day        Date      Day Type    Clock In  Lunch Out Lunch In  Clock Out   Total   OT    PTO");
            report.AppendLine("----------- --------- ----------- --------- --------- --------- --------- ------- ----- -----");

            decimal weeklyTotal = 0;
            decimal weeklyOvertime = 0;
            decimal weeklyPTO = 0;

            // Process each day of the week
            for (int i = 0; i < 7; i++)
            {
                var currentDate = startDate.AddDays(i);
                var dayName = currentDate.DayOfWeek.ToString().Substring(0, 3);
                var timeCard = timeCards.Find(tc => tc.Date?.Date == currentDate.Date);

                if (timeCard != null)
                {
                    // Format times - handle nulls gracefully
                    var clockIn = timeCard.AMClockIn?.ToString(@"hh\:mm") ?? "-----";
                    var lunchOut = timeCard.LunchClockOut?.ToString(@"hh\:mm") ?? "-----";
                    var lunchIn = timeCard.LunchClockIn?.ToString(@"hh\:mm") ?? "-----";
                    var clockOut = timeCard.PMClockOut?.ToString(@"hh\:mm") ?? "-----";
                    var total = timeCard.TotalTime?.ToString("0.00") ?? "0.00";
                    var overtime = timeCard.Overtime?.ToString("0.00") ?? "0.00";
                    var pto = timeCard.PTOHours?.ToString("0.00") ?? "0.00";

                    var dayType = !string.IsNullOrEmpty(timeCard.DayType) ? timeCard.DayType : "Work Day";
                    if (dayType.Length > 11) dayType = dayType.Substring(0, 11);

                    report.AppendLine($"{dayName,-3} {currentDate:MM/dd/yyyy} {dayType,-11} {clockIn,9} {lunchOut,9} {lunchIn,9} {clockOut,9} {total,7} {overtime,5} {pto,5}");

                    weeklyTotal += timeCard.TotalTime ?? 0;
                    weeklyOvertime += timeCard.Overtime ?? 0;
                    weeklyPTO += timeCard.PTOHours ?? 0;
                }
                else
                {
                    // No timecard entry for this day
                    var dayType = (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                        ? "Weekend" : "No Entry";

                    report.AppendLine($"{dayName,-3} {currentDate:MM/dd/yyyy} {dayType,-11} {"-----",9} {"-----",9} {"-----",9} {"-----",9} {"0.00",7} {"0.00",5} {"0.00",5}");
                }
            }

            // Weekly totals
            report.AppendLine("----------- --------- ----------- --------- --------- --------- --------- ------- ----- -----");
            report.AppendLine($"{"WEEKLY TOTALS:",-55} {weeklyTotal,7:0.00} {weeklyOvertime,5:0.00} {weeklyPTO,5:0.00}");
            report.AppendLine();

            // Summary information
            report.AppendLine("SUMMARY:");
            report.AppendLine($"Regular Hours: {Math.Max(0, weeklyTotal - weeklyOvertime):0.00}");
            report.AppendLine($"Overtime Hours: {weeklyOvertime:0.00}");
            report.AppendLine($"PTO Hours: {weeklyPTO:0.00}");
            report.AppendLine($"Total Paid Hours: {weeklyTotal + weeklyPTO:0.00}");

            // Add notes
            report.AppendLine();
            report.AppendLine("NOTES:");
            foreach (var entry in timeCards)
            {
                if (!string.IsNullOrEmpty(entry.Notes))
                {
                    report.AppendLine($"{entry.Date:MM/dd/yyyy}: {entry.Notes}");
                }
            }

            report.AppendLine();
            report.AppendLine("=================================================================");

            return report.ToString();
        }
    }
}
