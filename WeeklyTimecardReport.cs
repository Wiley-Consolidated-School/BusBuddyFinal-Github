using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.Business
{
    public class WeeklyTimecardReport
    {
        private readonly TimeCardRepository _timeCardRepository;

        public WeeklyTimecardReport()
        {
            _timeCardRepository = new TimeCardRepository();
        }

        public WeeklyTimecardReport(TimeCardRepository timeCardRepository)
        {
            _timeCardRepository = timeCardRepository;
        }

        /// <summary>
        /// Gets the start date (Monday) of the week containing the specified date
        /// </summary>
        public DateTime GetWeekStartDate(DateTime date)
        {
            var daysFromMonday = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
            if (daysFromMonday < 0) daysFromMonday += 7; // Handle Sunday
            return date.AddDays(-daysFromMonday).Date;
        }

        /// <summary>
        /// Gets the end date (Sunday) of the week containing the specified date
        /// </summary>
        public DateTime GetWeekEndDate(DateTime date)
        {
            return GetWeekStartDate(date).AddDays(6);
        }

        /// <summary>
        /// Gets all timecard entries for a specific week
        /// </summary>
        public List<TimeCard> GetWeeklyTimeCards(DateTime weekDate)
        {
            var startDate = GetWeekStartDate(weekDate);
            var endDate = GetWeekEndDate(weekDate);

            return _timeCardRepository.GetTimeCardsByDateRange(startDate, endDate)
                .OrderBy(tc => tc.Date)
                .ToList();
        }

        /// <summary>
        /// Generates a formatted weekly timecard report
        /// </summary>
        public string GenerateWeeklyReport(DateTime weekDate, string employeeName = "Employee")
        {
            var startDate = GetWeekStartDate(weekDate);
            var endDate = GetWeekEndDate(weekDate);
            var timeCards = GetWeeklyTimeCards(weekDate);

            var report = new StringBuilder();

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
                var timeCard = timeCards.FirstOrDefault(tc => tc.Date?.Date == currentDate.Date);

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

            // Add notes if any entries have them
            var entriesWithNotes = timeCards.Where(tc => !string.IsNullOrEmpty(tc.Notes)).ToList();
            if (entriesWithNotes.Any())
            {
                report.AppendLine();
                report.AppendLine("NOTES:");
                foreach (var entry in entriesWithNotes)
                {
                    report.AppendLine($"{entry.Date:MM/dd/yyyy}: {entry.Notes}");
                }
            }

            report.AppendLine();
            report.AppendLine("=================================================================");

            return report.ToString();
        }

        /// <summary>
        /// Generates a report for the week containing June 9th (your example data)
        /// </summary>
        public string GenerateJune9WeekReport(string employeeName = "Steve McKitrick")
        {
            return GenerateWeeklyReport(new DateTime(2025, 6, 9), employeeName);
        }

        /// <summary>
        /// Generates reports for multiple weeks
        /// </summary>
        public Dictionary<DateTime, string> GenerateMultipleWeekReports(List<DateTime> weekDates, string employeeName = "Employee")
        {
            var reports = new Dictionary<DateTime, string>();

            foreach (var weekDate in weekDates)
            {
                var weekStart = GetWeekStartDate(weekDate);
                reports[weekStart] = GenerateWeeklyReport(weekDate, employeeName);
            }

            return reports;
        }
    }
}
