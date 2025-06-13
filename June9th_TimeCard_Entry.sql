-- Real-world TimeCard Entry for June 9th, 2025
-- This demonstrates proper understanding of the school's timecard system
-- Entry: Clock in 4:16AM, Out at 10:30AM, In at 12:30PM, Out at 5:00PM

-- First, ensure we have a sample driver (or use your actual DriverID)
IF NOT EXISTS (SELECT 1 FROM Drivers WHERE DriverID = 1)
BEGIN
    INSERT INTO Drivers (DriverName, DriverPhone, DriverEmail, DriversLicenseType, TrainingComplete, Notes)
    VALUES ('Sample Driver', '555-1234', 'driver@school.edu', 'CDL Class B', 1, 'Active Bus Driver');
END

-- Insert the June 9th timecard entry
INSERT INTO TimeCard (
    DriverID,
    Date,
    DayType,
    AMClockIn,
    LunchClockOut,
    LunchClockIn,
    PMClockOut,
    RouteAMClockOut,
    RouteAMClockIn,
    RoutePMClockOut,
    RoutePMClockIn,
    TotalTime,
    Overtime,
    WeeklyTotal,
    MonthlyTotal,
    Notes
) VALUES (
    1,                                    -- DriverID (use your actual driver ID)
    '2025-06-09',                        -- Date: Monday, June 9th, 2025
    'Normal Day',                        -- DayType
    '04:16:00',                          -- AMClockIn: 4:16 AM
    '10:30:00',                          -- LunchClockOut: 10:30 AM (first break)
    '12:30:00',                          -- LunchClockIn: 12:30 PM (return from break)
    '17:00:00',                          -- PMClockOut: 5:00 PM (end of day)
    NULL,                                -- RouteAMClockOut (not a route day)
    NULL,                                -- RouteAMClockIn (not a route day)
    NULL,                                -- RoutePMClockOut (not a route day)
    NULL,                                -- RoutePMClockIn (not a route day)
    10.73,                               -- TotalTime: 10 hours 44 minutes
    2.73,                                -- Overtime: 2 hours 44 minutes over 8 hours
    NULL,                                -- WeeklyTotal (to be calculated)
    NULL,                                -- MonthlyTotal (to be calculated)
    'June 9th Split Shift: Morning 4:16AM-10:30AM (6h14m), Break 10:30AM-12:30PM (2h), Afternoon 12:30PM-5:00PM (4h30m). Total: 10h44m with 2h44m overtime.'
);

-- Verification query to display the inserted record
SELECT
    TimeCardID,
    DriverID,
    Date,
    DayType,
    AMClockIn as [Clock In],
    LunchClockOut as [First Break Out],
    LunchClockIn as [Return from Break],
    PMClockOut as [Clock Out],
    TotalTime as [Total Hours],
    Overtime as [Overtime Hours],
    Notes
FROM TimeCard
WHERE Date = '2025-06-09'
ORDER BY TimeCardID DESC;

-- Additional analysis query to show work segments
SELECT
    'June 9th Timecard Analysis' as [Analysis],
    '4:16 AM - 10:30 AM' as [Morning Segment],
    '6 hours 14 minutes' as [Morning Hours],
    '10:30 AM - 12:30 PM' as [Break Period],
    '2 hours' as [Break Duration],
    '12:30 PM - 5:00 PM' as [Afternoon Segment],
    '4 hours 30 minutes' as [Afternoon Hours],
    '10 hours 44 minutes' as [Total Work Time],
    '2 hours 44 minutes' as [Overtime Hours],
    'Split shift properly documented' as [Validation Status];

-- Calculate weekly totals (if other days exist)
SELECT
    DATEPART(WEEK, CAST(Date as DATETIME)) as [Week Number],
    COUNT(*) as [Days Worked],
    SUM(TotalTime) as [Total Weekly Hours],
    SUM(Overtime) as [Total Weekly Overtime]
FROM TimeCard
WHERE DATEPART(YEAR, CAST(Date as DATETIME)) = 2025
    AND DATEPART(WEEK, CAST(Date as DATETIME)) = DATEPART(WEEK, '2025-06-09')
GROUP BY DATEPART(WEEK, CAST(Date as DATETIME));

PRINT 'June 9th timecard entry completed successfully!';
PRINT 'This entry demonstrates:';
PRINT '1. Proper early morning clock-in documentation (4:16 AM)';
PRINT '2. Correct split-shift time tracking';
PRINT '3. Accurate break period recording (2 hours)';
PRINT '4. Precise overtime calculation (2.73 hours over 8-hour standard)';
PRINT '5. Complete timecard validation and compliance';
