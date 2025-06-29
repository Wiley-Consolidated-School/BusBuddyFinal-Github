-- =============================================
-- Link Activity Schedule to Calendar System
-- Consolidates school calendar tables and creates foreign key relationship
-- =============================================

USE BusBuddy;
GO

PRINT 'Starting calendar consolidation and activity schedule linking...'

-- Step 1: Backup existing data if needed
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SchoolCalendar_Backup' AND xtype='U')
BEGIN
    SELECT * INTO SchoolCalendar_Backup FROM SchoolCalendar;
    PRINT '✓ Backed up SchoolCalendar table'
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SchoolCalendars_Backup' AND xtype='U')
BEGIN
    SELECT * INTO SchoolCalendars_Backup FROM SchoolCalendars;
    PRINT '✓ Backed up SchoolCalendars table'
END

-- Step 2: Create a consolidated SchoolCalendar table (enhanced)
IF EXISTS (SELECT * FROM sysobjects WHERE name='SchoolCalendar_New' AND xtype='U')
    DROP TABLE SchoolCalendar_New;

CREATE TABLE SchoolCalendar_New (
    CalendarID int IDENTITY(1,1) PRIMARY KEY,
    CalendarDate date NOT NULL,
    DayType nvarchar(50) NULL,        -- From original SchoolCalendar
    Category nvarchar(50) NULL,       -- From SchoolCalendars
    Description nvarchar(200) NULL,   -- From SchoolCalendars
    IsSchoolDay bit NOT NULL DEFAULT 1, -- From SchoolCalendars
    Notes nvarchar(500) NULL,         -- Enhanced notes field
    CreatedDate datetime DEFAULT GETDATE(),
    ModifiedDate datetime DEFAULT GETDATE(),
    UNIQUE(CalendarDate) -- Ensure one entry per date
);

PRINT '✓ Created consolidated SchoolCalendar_New table'

-- Step 3: Migrate data from both tables
-- First from SchoolCalendar
INSERT INTO SchoolCalendar_New (CalendarDate, DayType, Notes, IsSchoolDay)
SELECT
    Date as CalendarDate,
    DayType,
    Notes,
    CASE
        WHEN DayType IN ('School Day', 'Regular Day', 'Full Day') THEN 1
        WHEN DayType IN ('Holiday', 'Weekend', 'No School', 'Break') THEN 0
        ELSE 1 -- Default to school day
    END as IsSchoolDay
FROM SchoolCalendar
WHERE Date IS NOT NULL;

PRINT '✓ Migrated data from SchoolCalendar'

-- Then from SchoolCalendars (update existing or insert new)
MERGE SchoolCalendar_New AS target
USING SchoolCalendars AS source
ON target.CalendarDate = source.CalendarDate
WHEN MATCHED THEN
    UPDATE SET
        Category = source.Category,
        Description = source.Description,
        IsSchoolDay = ISNULL(source.IsSchoolDay, target.IsSchoolDay),
        ModifiedDate = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (CalendarDate, Category, Description, IsSchoolDay)
    VALUES (source.CalendarDate, source.Category, source.Description, ISNULL(source.IsSchoolDay, 1));

PRINT '✓ Merged data from SchoolCalendars'

-- Step 4: Add CalendarID foreign key to ActivitySchedule
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'ActivitySchedule' AND COLUMN_NAME = 'CalendarID')
BEGIN
    ALTER TABLE ActivitySchedule
    ADD CalendarID int NULL;
    PRINT '✓ Added CalendarID column to ActivitySchedule'
END

-- Step 5: Update ActivitySchedule to link to calendar entries
UPDATE ActivitySchedule
SET CalendarID = (
    SELECT TOP 1 CalendarID
    FROM SchoolCalendar_New
    WHERE CalendarDate = ActivitySchedule.Date
)
WHERE CalendarID IS NULL;

PRINT '✓ Linked existing ActivitySchedule entries to calendar'

-- Step 6: Replace old tables with new consolidated table
BEGIN TRANSACTION;

-- Drop old tables
DROP TABLE SchoolCalendar;
DROP TABLE SchoolCalendars;

-- Rename new table
EXEC sp_rename 'SchoolCalendar_New', 'SchoolCalendar';

-- Add foreign key constraint
ALTER TABLE ActivitySchedule
ADD CONSTRAINT FK_ActivitySchedule_SchoolCalendar
FOREIGN KEY (CalendarID) REFERENCES SchoolCalendar(CalendarID);

COMMIT TRANSACTION;

PRINT '✓ Replaced old tables and added foreign key constraint'

-- Step 7: Create useful views for reporting
IF EXISTS (SELECT * FROM sysobjects WHERE name='vw_ActivityScheduleWithCalendar' AND xtype='V')
    DROP VIEW vw_ActivityScheduleWithCalendar;
GO

CREATE VIEW vw_ActivityScheduleWithCalendar AS
SELECT
    a.ScheduleID,
    a.Date,
    a.ActivityName,
    a.StartTime,
    a.EndTime,
    a.VehicleID,
    a.DriverID,
    a.Notes as ActivityNotes,
    c.DayType,
    c.Category,
    c.Description as CalendarDescription,
    c.IsSchoolDay,
    c.Notes as CalendarNotes
FROM ActivitySchedule a
LEFT JOIN SchoolCalendar c ON a.CalendarID = c.CalendarID;
GO

PRINT '✓ Created view for joined activity schedule and calendar data'

-- Step 8: Add indexes for performance
CREATE INDEX IX_SchoolCalendar_Date ON SchoolCalendar(CalendarDate);
CREATE INDEX IX_ActivitySchedule_CalendarID ON ActivitySchedule(CalendarID);
CREATE INDEX IX_ActivitySchedule_Date ON ActivitySchedule(Date);

PRINT '✓ Added performance indexes'

-- Step 9: Insert some sample data if calendar is empty
IF (SELECT COUNT(*) FROM SchoolCalendar) = 0
BEGIN
    PRINT 'Adding sample calendar data...'

    INSERT INTO SchoolCalendar (CalendarDate, DayType, Category, IsSchoolDay, Description)
    VALUES
    (GETDATE(), 'School Day', 'Regular', 1, 'Current school day'),
    (DATEADD(day, 1, GETDATE()), 'School Day', 'Regular', 1, 'Tomorrow'),
    (DATEADD(day, -1, GETDATE()), 'School Day', 'Regular', 1, 'Yesterday'),
    (DATEADD(day, 7, GETDATE()), 'Weekend', 'Non-School', 0, 'Next weekend'),
    ('2024-12-25', 'Holiday', 'Christmas', 0, 'Christmas Day'),
    ('2024-07-04', 'Holiday', 'Independence', 0, 'Independence Day');

    PRINT '✓ Added sample calendar data'
END

PRINT ''
PRINT '========================================='
PRINT 'Calendar consolidation completed successfully!'
PRINT '========================================='
PRINT ''
PRINT 'Summary of changes:'
PRINT '- Consolidated SchoolCalendar and SchoolCalendars into one table'
PRINT '- Added CalendarID foreign key to ActivitySchedule'
PRINT '- Created view vw_ActivityScheduleWithCalendar for easy reporting'
PRINT '- Added performance indexes'
PRINT '- Backup tables created: SchoolCalendar_Backup, SchoolCalendars_Backup'
PRINT ''

-- Display current table structure
PRINT 'Current SchoolCalendar structure:'
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SchoolCalendar'
ORDER BY ORDINAL_POSITION;

PRINT ''
PRINT 'Sample linked data:'
SELECT TOP 5
    ScheduleID,
    Date,
    ActivityName,
    CalendarDescription,
    IsSchoolDay
FROM vw_ActivityScheduleWithCalendar
ORDER BY Date DESC;
