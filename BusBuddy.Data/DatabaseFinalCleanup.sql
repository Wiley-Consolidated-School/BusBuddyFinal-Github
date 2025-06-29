-- BusBuddy Database Final Cleanup Script
-- Removes deprecated "Vehicles" table and adds proper sample data

USE BusBuddy;
GO

PRINT 'Starting BusBuddy Database Final Cleanup...';
PRINT '==========================================';

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

-- Get the first available bus and driver
DECLARE @firstBusId INT, @firstDriverId INT;
SELECT TOP 1 @firstBusId = BusId FROM Buses ORDER BY BusId;
SELECT TOP 1 @firstDriverId = DriverId FROM Drivers ORDER BY DriverId;

PRINT 'Using Bus ID: ' + ISNULL(CAST(@firstBusId AS VARCHAR(10)), 'NULL');
PRINT 'Using Driver ID: ' + ISNULL(CAST(@firstDriverId AS VARCHAR(10)), 'NULL');

IF @firstBusId IS NOT NULL AND @firstDriverId IS NOT NULL
BEGIN
    -- Add sample routes if none exist (using actual column names)
    IF NOT EXISTS (SELECT * FROM Routes)
    BEGIN
        INSERT INTO Routes (Date, RouteName, AMVehicleID, AMDriverID, PMVehicleID, PMDriverID, Notes)
        VALUES
        (GETDATE(), 'Route 1 - Elementary School', @firstBusId, @firstDriverId, @firstBusId, @firstDriverId, 'Main elementary school route'),
        (GETDATE(), 'Route 2 - High School', @firstBusId, @firstDriverId, @firstBusId, @firstDriverId, 'High school route downtown');

        PRINT 'Added ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' sample routes.';
    END

    -- Add sample fuel records if none exist
    IF NOT EXISTS (SELECT * FROM Fuel)
    BEGIN
        INSERT INTO Fuel (Date, VehicleID, Gallons, Cost, Odometer, Notes)
        VALUES
        (GETDATE(), @firstBusId, 25.5, 89.25, 45000, 'Regular fuel fill-up'),
        (DATEADD(day, -7, GETDATE()), @firstBusId, 24.8, 86.80, 44750, 'Weekly fuel fill-up'),
        (DATEADD(day, -14, GETDATE()), @firstBusId, 26.1, 91.35, 44500, 'Previous week fuel fill-up');

        PRINT 'Added ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' sample fuel records.';
    END

    -- Add sample maintenance records if none exist (using actual column names)
    IF NOT EXISTS (SELECT * FROM Maintenance)
    BEGIN
        INSERT INTO Maintenance (Date, VehicleID, MaintenanceType, Description, Cost, Odometer, Notes)
        VALUES
        (DATEADD(day, -30, GETDATE()), @firstBusId, 'Oil Change', 'Regular 5000 mile oil change', 45.00, 44200, 'Used 5W-30 oil, replaced filter'),
        (DATEADD(day, -60, GETDATE()), @firstBusId, 'Inspection', 'Annual safety inspection', 75.00, 43800, 'Passed all safety checks'),
        (DATEADD(day, -90, GETDATE()), @firstBusId, 'Brake Service', 'Brake pad replacement', 180.00, 43400, 'Replaced front brake pads');

        PRINT 'Added ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' sample maintenance records.';
    END
END
ELSE
BEGIN
    PRINT 'Cannot add sample data - missing Bus or Driver records.';
END

PRINT '';
PRINT 'Database cleanup completed successfully!';
PRINT '======================================';

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
UNION ALL
SELECT 'Activities', COUNT(*) FROM Activities
UNION ALL
SELECT 'ActivitySchedule', COUNT(*) FROM ActivitySchedule
ORDER BY TableName;

PRINT '';
PRINT 'Foreign key relationships still using VehicleID:';
SELECT
    fk.name AS ConstraintName,
    tp.name AS FromTable,
    cp.name AS FromColumn,
    tr.name AS ToTable,
    cr.name AS ToColumn
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
INNER JOIN sys.columns cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
WHERE cp.name LIKE '%Vehicle%'
ORDER BY tp.name;
