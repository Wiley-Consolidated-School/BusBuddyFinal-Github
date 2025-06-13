-- Weekly Timecard Report Generator
-- Generates a formatted weekly timecard report matching school district requirements

-- Parameters for week selection
DECLARE @WeekStartDate DATE = '2025-06-09';  -- Monday of the week
DECLARE @WeekEndDate DATE = DATEADD(DAY, 6, @WeekStartDate);  -- Sunday of the week
DECLARE @DriverID INT = 1;  -- Change to specific driver ID

-- Get Driver Information
DECLARE @DriverName NVARCHAR(100);
DECLARE @EmployeeID NVARCHAR(50);

SELECT
    @DriverName = DriverName,
    @EmployeeID = CAST(DriverID AS NVARCHAR(50))  -- Use DriverID as Employee ID for now
FROM Drivers
WHERE DriverID = @DriverID;

-- Create the weekly timecard report
PRINT '================================================================';
PRINT '              SCHOOL DISTRICT TIMECARD REPORT';
PRINT '================================================================';
PRINT 'Employee: ' + ISNULL(@DriverName, 'Unknown Driver');
PRINT 'Employee ID: ' + ISNULL(@EmployeeID, 'N/A');
PRINT 'Week Ending: ' + CONVERT(VARCHAR(10), @WeekEndDate, 101);
PRINT 'Pay Period: ' + CONVERT(VARCHAR(10), @WeekStartDate, 101) + ' - ' + CONVERT(VARCHAR(10), @WeekEndDate, 101);
PRINT '================================================================';
PRINT '';

-- Daily breakdown
WITH WeeklyTimecard AS (
    SELECT
        tc.Date,
        DATENAME(WEEKDAY, tc.Date) as DayOfWeek,
        tc.DayType,
        tc.AMClockIn,
        tc.LunchClockOut,
        tc.LunchClockIn,
        tc.PMClockOut,
        tc.TotalTime,
        tc.Overtime,
        tc.PTOHours,
        tc.Notes,
        ROW_NUMBER() OVER (ORDER BY tc.Date) as DayNumber
    FROM TimeCard tc
    WHERE tc.DriverID = @DriverID
        AND tc.Date BETWEEN @WeekStartDate AND @WeekEndDate
),
WeekDays AS (
    -- Generate all 7 days of the week
    SELECT @WeekStartDate as WeekDate, 'Monday' as DayName, 1 as DayNum
    UNION SELECT DATEADD(DAY, 1, @WeekStartDate), 'Tuesday', 2
    UNION SELECT DATEADD(DAY, 2, @WeekStartDate), 'Wednesday', 3
    UNION SELECT DATEADD(DAY, 3, @WeekStartDate), 'Thursday', 4
    UNION SELECT DATEADD(DAY, 4, @WeekStartDate), 'Friday', 5
    UNION SELECT DATEADD(DAY, 5, @WeekStartDate), 'Saturday', 6
    UNION SELECT DATEADD(DAY, 6, @WeekStartDate), 'Sunday', 7
)
SELECT
    wd.DayName + ' ' + CONVERT(VARCHAR(10), wd.WeekDate, 101) as [Date],
    ISNULL(tc.DayType, 'No Entry') as [Day Type],
    CASE
        WHEN tc.AMClockIn IS NOT NULL THEN CONVERT(VARCHAR(8), tc.AMClockIn, 108)
        WHEN tc.PTOHours > 0 THEN 'PTO'
        ELSE '---'
    END as [Clock In],
    CASE
        WHEN tc.LunchClockOut IS NOT NULL THEN CONVERT(VARCHAR(8), tc.LunchClockOut, 108)
        WHEN tc.PTOHours > 0 THEN 'PTO'
        ELSE '---'
    END as [Lunch Out],
    CASE
        WHEN tc.LunchClockIn IS NOT NULL THEN CONVERT(VARCHAR(8), tc.LunchClockIn, 108)
        WHEN tc.PTOHours > 0 THEN 'PTO'
        ELSE '---'
    END as [Lunch In],
    CASE
        WHEN tc.PMClockOut IS NOT NULL THEN CONVERT(VARCHAR(8), tc.PMClockOut, 108)
        WHEN tc.PTOHours > 0 THEN 'PTO'
        ELSE '---'
    END as [Clock Out],
    ISNULL(CAST(tc.TotalTime AS VARCHAR(10)), '0.00') as [Total Hours],
    ISNULL(CAST(tc.Overtime AS VARCHAR(10)), '0.00') as [OT Hours],
    ISNULL(CAST(tc.PTOHours AS VARCHAR(10)), '0.00') as [PTO Hours],
    ISNULL(LEFT(tc.Notes, 50), '') as [Notes]
FROM WeekDays wd
LEFT JOIN WeeklyTimecard tc ON wd.WeekDate = tc.Date
ORDER BY wd.DayNum;

-- Weekly Summary
PRINT '';
PRINT '================================================================';
PRINT '                    WEEKLY SUMMARY';
PRINT '================================================================';

SELECT
    COUNT(CASE WHEN TotalTime > 0 OR PTOHours > 0 THEN 1 END) as [Days Worked],
    ISNULL(SUM(TotalTime), 0) as [Total Regular Hours],
    ISNULL(SUM(Overtime), 0) as [Total Overtime Hours],
    ISNULL(SUM(PTOHours), 0) as [Total PTO Hours],
    ISNULL(SUM(TotalTime), 0) + ISNULL(SUM(PTOHours), 0) as [Total Paid Hours]
FROM TimeCard
WHERE DriverID = @DriverID
    AND Date BETWEEN @WeekStartDate AND @WeekEndDate;

-- Detailed time analysis
PRINT '';
PRINT '================================================================';
PRINT '                  DETAILED TIME ANALYSIS';
PRINT '================================================================';

SELECT
    CONVERT(VARCHAR(10), Date, 101) as [Date],
    DATENAME(WEEKDAY, Date) as [Day],
    -- Calculate actual work segments
    CASE
        WHEN AMClockIn IS NOT NULL AND LunchClockOut IS NOT NULL THEN
            CAST(DATEDIFF(MINUTE, AMClockIn, LunchClockOut) / 60.0 AS DECIMAL(5,2))
        ELSE 0
    END as [Morning Hours],
    CASE
        WHEN LunchClockIn IS NOT NULL AND PMClockOut IS NOT NULL THEN
            CAST(DATEDIFF(MINUTE, LunchClockIn, PMClockOut) / 60.0 AS DECIMAL(5,2))
        ELSE 0
    END as [Afternoon Hours],
    CASE
        WHEN LunchClockOut IS NOT NULL AND LunchClockIn IS NOT NULL THEN
            CAST(DATEDIFF(MINUTE, LunchClockOut, LunchClockIn) / 60.0 AS DECIMAL(5,2))
        ELSE 0
    END as [Break Hours],
    TotalTime as [Total Hours],
    Overtime as [OT Hours]
FROM TimeCard
WHERE DriverID = @DriverID
    AND Date BETWEEN @WeekStartDate AND @WeekEndDate
    AND (TotalTime > 0 OR PTOHours > 0)
ORDER BY Date;

PRINT '';
PRINT '================================================================';
PRINT '                    CERTIFICATION';
PRINT '================================================================';
PRINT 'I certify that the information contained in this timecard is';
PRINT 'true and accurate to the best of my knowledge.';
PRINT '';
PRINT 'Employee Signature: ________________________ Date: __________';
PRINT '';
PRINT 'Supervisor Signature: ______________________ Date: __________';
PRINT '';
PRINT 'Report Generated: ' + CONVERT(VARCHAR(19), GETDATE(), 120);
PRINT '================================================================';
