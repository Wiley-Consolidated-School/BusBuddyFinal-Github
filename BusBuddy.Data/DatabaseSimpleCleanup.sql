-- BusBuddy Database Simple Cleanup Script
-- Removes deprecated "Vehicles" table and fixes foreign key issues

USE BusBuddy;
GO

PRINT 'Starting BusBuddy Database Cleanup...';
PRINT '====================================';

-- Step 1: Drop all foreign key constraints that reference the old Vehicles table
PRINT 'Step 1: Dropping foreign key constraints that reference Vehicles table...';

DECLARE @DropConstraints NVARCHAR(MAX) = '';

-- Build dynamic SQL to drop all FK constraints pointing to Vehicles
SELECT @DropConstraints = @DropConstraints +
    'ALTER TABLE [' + SCHEMA_NAME(tp.schema_id) + '].[' + tp.name + '] DROP CONSTRAINT [' + fk.name + '];' + CHAR(13)
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
WHERE tr.name = 'Vehicles';

IF LEN(@DropConstraints) > 0
BEGIN
    EXEC sp_executesql @DropConstraints;
    PRINT 'Dropped all foreign key constraints to Vehicles table.';
END
ELSE
BEGIN
    PRINT 'No foreign key constraints to Vehicles table found.';
END

-- Step 2: Migrate data from Vehicles to Buses if Buses table is empty
PRINT 'Step 2: Checking for data migration needs...';

DECLARE @vehicleCount INT = 0, @busCount INT = 0;

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Vehicles')
    SELECT @vehicleCount = COUNT(*) FROM Vehicles;

SELECT @busCount = COUNT(*) FROM Buses;

PRINT 'Vehicle records: ' + CAST(@vehicleCount AS VARCHAR(10));
PRINT 'Bus records: ' + CAST(@busCount AS VARCHAR(10));

-- If Vehicles has data but Buses is empty, migrate
IF @vehicleCount > 0 AND @busCount = 0
BEGIN
    INSERT INTO Buses (BusNumber, Year, Make, Model, Capacity, VIN, LicenseNumber, LastInspectionDate, Status)
    SELECT
        BusNumber,
        Year,
        Make,
        Model,
        SeatingCapacity,    -- Maps to Capacity
        VINNumber,          -- Maps to VIN
        LicenseNumber,
        DateLastInspection, -- Maps to LastInspectionDate
        ISNULL(Status, 'Active') -- Default status if null
    FROM Vehicles;

    PRINT 'Migrated ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' records from Vehicles to Buses.';
END
ELSE
BEGIN
    PRINT 'Data migration not needed.';
END

-- Step 3: Drop the deprecated Vehicles table
PRINT 'Step 3: Removing deprecated Vehicles table...';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Vehicles')
BEGIN
    DROP TABLE Vehicles;
    PRINT 'Vehicles table dropped successfully.';
END
ELSE
BEGIN
    PRINT 'Vehicles table does not exist.';
END

-- Step 4: Add sample data if tables are mostly empty
PRINT 'Step 4: Adding sample data for testing...';

-- Get the first available bus
DECLARE @firstBusId INT;
SELECT TOP 1 @firstBusId = BusId FROM Buses ORDER BY BusId;

IF @firstBusId IS NOT NULL
BEGIN
    -- Add sample routes if none exist
    IF NOT EXISTS (SELECT * FROM Routes)
    BEGIN
        INSERT INTO Routes (RouteName, StartTime, EndTime, Description, AMDriverID, PMDriverID, AMVehicleID, PMVehicleID)
        VALUES
        ('Route 1 - Elementary', '07:00:00', '15:30:00', 'Main elementary school route', 1, 1, @firstBusId, @firstBusId),
        ('Route 2 - High School', '06:45:00', '16:00:00', 'High school route downtown', 1, 1, @firstBusId, @firstBusId);

        PRINT 'Added ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' sample routes.';
    END

    -- Add sample fuel records if none exist
    IF NOT EXISTS (SELECT * FROM Fuel)
    BEGIN
        INSERT INTO Fuel (VehicleID, Date, Gallons, Cost, Odometer, Notes)
        VALUES
        (@firstBusId, GETDATE(), 25.5, 89.25, 45000, 'Regular fuel fill-up'),
        (@firstBusId, DATEADD(day, -7, GETDATE()), 24.8, 86.80, 44750, 'Weekly fuel fill-up');

        PRINT 'Added ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' sample fuel records.';
    END

    -- Add sample maintenance records if none exist
    IF NOT EXISTS (SELECT * FROM Maintenance)
    BEGIN
        INSERT INTO Maintenance (VehicleID, Date, Type, Description, Cost, NextDueDate, Notes)
        VALUES
        (@firstBusId, DATEADD(day, -30, GETDATE()), 'Oil Change', 'Regular 5000 mile oil change', 45.00, DATEADD(day, 90, GETDATE()), 'Used 5W-30 oil'),
        (@firstBusId, DATEADD(day, -60, GETDATE()), 'Inspection', 'Annual safety inspection', 75.00, DATEADD(year, 1, GETDATE()), 'Passed all safety checks');

        PRINT 'Added ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' sample maintenance records.';
    END
END

PRINT '';
PRINT 'Database cleanup completed!';
PRINT '==========================';

-- Final status report
PRINT 'Final table record counts:';
SELECT 'Buses' AS TableName, COUNT(*) AS RecordCount FROM Buses
UNION ALL
SELECT 'Drivers', COUNT(*) FROM Drivers
UNION ALL
SELECT 'Routes', COUNT(*) FROM Routes
UNION ALL
SELECT 'Fuel', COUNT(*) FROM Fuel
UNION ALL
SELECT 'Maintenance', COUNT(*) FROM Maintenance
ORDER BY TableName;
