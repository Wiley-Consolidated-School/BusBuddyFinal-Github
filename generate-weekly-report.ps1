# Weekly Timecard Report Generator for BusBuddy
# This script generates weekly timecard reports based on your PDF template format

param(
    [string]$WeekOf = "",
    [string]$EmployeeName = "Steve McKitrick",
    [string]$OutputFile = ""
)

Write-Host "=================================================================" -ForegroundColor Green
Write-Host "          BusBuddy Weekly Timecard Report Generator" -ForegroundColor Green
Write-Host "=================================================================" -ForegroundColor Green
Write-Host ""

# If no week specified, use current week
if ($WeekOf -eq "") {
    $WeekOf = Get-Date -Format "MM/dd/yyyy"
    Write-Host "No week specified, using current week containing: $WeekOf" -ForegroundColor Yellow
}

# Parse the date
try {
    $targetDate = [DateTime]::Parse($WeekOf)
    Write-Host "Generating report for week containing: $($targetDate.ToString('MM/dd/yyyy'))" -ForegroundColor Cyan
} catch {
    Write-Host "Error: Invalid date format '$WeekOf'. Please use MM/dd/yyyy format." -ForegroundColor Red
    exit 1
}

# Calculate Monday (start of week)
$daysFromMonday = [int]$targetDate.DayOfWeek - [int][DayOfWeek]::Monday
if ($daysFromMonday -lt 0) { $daysFromMonday += 7 }
$monday = $targetDate.AddDays(-$daysFromMonday)
$sunday = $monday.AddDays(6)

Write-Host "Week: $($monday.ToString('MM/dd/yyyy')) - $($sunday.ToString('MM/dd/yyyy'))" -ForegroundColor Cyan
Write-Host ""

# Generate the report structure
$report = @"
=================================================================
                    WEEKLY TIMECARD REPORT
=================================================================
Employee: $EmployeeName
Week of: $($monday.ToString('MM/dd/yyyy')) - $($sunday.ToString('MM/dd/yyyy'))
Report Generated: $(Get-Date -Format 'MM/dd/yyyy hh:mm tt')
=================================================================

Day        Date      Day Type    Clock In  Lunch Out Lunch In  Clock Out   Total   OT    PTO
----------- --------- ----------- --------- --------- --------- --------- ------- ----- -----
"@

# Add each day of the week
$weeklyTotal = 0.0
$weeklyOvertime = 0.0
$weeklyPTO = 0.0

for ($i = 0; $i -lt 7; $i++) {
    $currentDate = $monday.AddDays($i)
    $dayName = $currentDate.ToString("ddd")
    $dateStr = $currentDate.ToString("MM/dd/yyyy")

    # Weekend handling
    if ($currentDate.DayOfWeek -eq [DayOfWeek]::Saturday -or $currentDate.DayOfWeek -eq [DayOfWeek]::Sunday) {
        $report += "`n$($dayName) $dateStr Weekend         -----     -----     -----     -----    0.00  0.00  0.00"
    } else {
        # Weekday - placeholder for actual timecard data
        $report += "`n$($dayName) $dateStr No Entry        -----     -----     -----     -----    0.00  0.00  0.00"
    }
}

# Add totals
$report += @"

----------- --------- ----------- --------- --------- --------- --------- ------- ----- -----
WEEKLY TOTALS:                                                        $($weeklyTotal.ToString("0.00"))  $($weeklyOvertime.ToString("0.00"))  $($weeklyPTO.ToString("0.00"))

SUMMARY:
Regular Hours: $((40.0).ToString("0.00"))
Overtime Hours: $($weeklyOvertime.ToString("0.00"))
PTO Hours: $($weeklyPTO.ToString("0.00"))
Total Paid Hours: $((40.0 + $weeklyPTO).ToString("0.00"))

NOTES:
(Add your timecard entries here - this is a template)

To populate with real data:
1. Enter your actual clock in/out times in the database
2. Run the full BusBuddy application
3. Use the Weekly Report feature

=================================================================
"@

# Output the report
Write-Host $report

# Save to file if specified
if ($OutputFile -eq "") {
    $OutputFile = "Weekly_Timecard_$($monday.ToString('yyyy-MM-dd')).txt"
}

try {
    $report | Out-File -FilePath $OutputFile -Encoding UTF8
    Write-Host ""
    Write-Host "Report saved to: $OutputFile" -ForegroundColor Green
} catch {
    Write-Host "Error saving file: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=================================================================" -ForegroundColor Green
Write-Host "Usage Examples:" -ForegroundColor Yellow
Write-Host "  .\generate-weekly-report.ps1                          # Current week"
Write-Host "  .\generate-weekly-report.ps1 -WeekOf '06/09/2025'     # Specific week"
Write-Host "  .\generate-weekly-report.ps1 -EmployeeName 'John Doe' # Different employee"
Write-Host "=================================================================" -ForegroundColor Green
