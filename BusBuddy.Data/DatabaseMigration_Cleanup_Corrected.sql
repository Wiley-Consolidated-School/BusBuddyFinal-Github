-- BusBuddy Database Migration and Cleanup Script (Corrected)
-- This script migrates from deprecated "Vehicles" table to "Buses" table
-- and ensures all foreign keys point to the correct tables

USE BusBuddy;
GO

PRINT 'Starting BusBuddy Database Migration and Cleanup...';
PRINT '================================================';

-- Step 1: Drop all foreign key constraints that reference the old Vehicles table
PRINT 'Step 1: Dropping foreign key constraints that reference Vehicles table...';

-- Get all foreign keys that reference Vehicles table
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql = @sql + 'ALTER TABLE ' + QUOTENAME(tp.name) + ' DROP CONSTRAINT ' + QUOTENAME(fk.name) + ';' + CHAR(13) + CHAR(10)
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
WHERE tr.name = 'Vehicles';

IF LEN(@sql) > 0
BEGIN
    PRINT 'Dropping foreign key constraints...';
    EXEC sp_executesql @sql;
    PRINT 'All foreign key constraints to Vehicles table dropped.';
END
ELSE
BEGIN
    PRINT 'No foreign key constraints to Vehicles table found.';
END

-- Step 2: Migrate data from Vehicles table to Buses table (if not already done)
PRINT 'Step 2: Migrating data from Vehicles to Buses table...';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Vehicles')
BEGIN
    -- Check if there's any data in Vehicles that's not in Buses
    DECLARE @vehicleCount INT, @busCount INT;
    SELECT @vehicleCount = COUNT(*) FROM Vehicles;
    SELECT @busCount = COUNT(*) FROM Buses;

    PRINT CONCAT('  - Found ', @vehicleCount, ' records in Vehicles table');
    PRINT CONCAT('  - Found ', @busCount, ' records in Buses table');

    -- If Vehicles has data and Buses is empty, migrate it
    IF @vehicleCount > 0 AND @busCount = 0
    BEGIN
        -- Map Vehicles columns to Buses columns correctly
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
            Status
        FROM Vehicles;

        PRINT CONCAT('  - Migrated ', @@ROWCOUNT, ' records from Vehicles to Buses');
    END
    ELSE IF @vehicleCount > 0 AND @busCount > 0
    BEGIN
        PRINT '  - Both tables have data. Checking for duplicates...';

        -- Check if we need to migrate any additional records
        DECLARE @newRecords INT;
        SELECT @newRecords = COUNT(*)
        FROM Vehicles v
        WHERE NOT EXISTS (SELECT 1 FROM Buses b WHERE b.BusNumber = v.BusNumber);

        IF @newRecords > 0
        BEGIN
            INSERT INTO Buses (BusNumber, Year, Make, Model, Capacity, VIN, LicenseNumber, LastInspectionDate, Status)
            SELECT
                v.BusNumber,
                v.Year,
                v.Make,
                v.Model,
                v.SeatingCapacity,
                v.VINNumber,
                v.LicenseNumber,
                v.DateLastInspection,
                v.Status
            FROM Vehicles v
            WHERE NOT EXISTS (SELECT 1 FROM Buses b WHERE b.BusNumber = v.BusNumber);

            PRINT CONCAT('  - Migrated ', @@ROWCOUNT, ' additional records from Vehicles to Buses');
        END
        ELSE
        BEGIN
            PRINT '  - No additional records to migrate (all Vehicles data already in Buses)';
        END
    END
    ELSE
    BEGIN
        PRINT '  - Vehicles table is empty, no migration needed';
    END
END

-- Step 3: Update all foreign key columns to reference BusId instead of Vehicles.Id
PRINT 'Step 3: Updating foreign key column references...';

-- Create a mapping table for Vehicle Id to Bus Id if we have data in both tables
DECLARE @mappingTable TABLE (VehicleId INT, BusId INT);

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Vehicles') AND EXISTS (SELECT * FROM Buses)
BEGIN
    INSERT INTO @mappingTable (VehicleId, BusId)
    SELECT v.Id, b.BusId
    FROM Vehicles v
    INNER JOIN Buses b ON v.BusNumber = b.BusNumber;

    PRINT CONCAT('  - Created mapping for ', @@ROWCOUNT, ' vehicle-to-bus relationships');
END

-- Update Activities table
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Activities') AND name = 'VehicleID')
BEGIN
    -- Add BusID column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Activities') AND name = 'BusID')
    BEGIN
        ALTER TABLE Activities ADD BusID INT NULL;
        PRINT '  - Added BusID column to Activities table';
    END

    -- Update BusID based on mapping if we have vehicles data
    IF EXISTS (SELECT * FROM @mappingTable)
    BEGIN
        UPDATE a SET BusID = m.BusId
        FROM Activities a
        INNER JOIN @mappingTable m ON a.VehicleID = m.VehicleId;

        PRINT CONCAT('  - Updated ', @@ROWCOUNT, ' Activities records with BusID');
    END

    -- Drop VehicleID column
    ALTER TABLE Activities DROP COLUMN VehicleID;
    PRINT '  - Dropped VehicleID column from Activities';
END

-- Update ActivitySchedule table
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ActivitySchedule') AND name = 'VehicleID')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ActivitySchedule') AND name = 'BusID')
    BEGIN
        ALTER TABLE ActivitySchedule ADD BusID INT NULL;
        PRINT '  - Added BusID column to ActivitySchedule table';
    END

    IF EXISTS (SELECT * FROM @mappingTable)
    BEGIN
        UPDATE a SET BusID = m.BusId
        FROM ActivitySchedule a
        INNER JOIN @mappingTable m ON a.VehicleID = m.VehicleId;

        PRINT CONCAT('  - Updated ', @@ROWCOUNT, ' ActivitySchedule records with BusID');
    END

    ALTER TABLE ActivitySchedule DROP COLUMN VehicleID;
    PRINT '  - Dropped VehicleID column from ActivitySchedule';
END

-- Update Fuel table
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Fuel') AND name = 'VehicleID')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Fuel') AND name = 'BusID')
    BEGIN
        ALTER TABLE Fuel ADD BusID INT NULL;
        PRINT '  - Added BusID column to Fuel table';
    END

    IF EXISTS (SELECT * FROM @mappingTable)
    BEGIN
        UPDATE f SET BusID = m.BusId
        FROM Fuel f
        INNER JOIN @mappingTable m ON f.VehicleID = m.VehicleId;

        PRINT CONCAT('  - Updated ', @@ROWCOUNT, ' Fuel records with BusID');
    END

    ALTER TABLE Fuel DROP COLUMN VehicleID;
    PRINT '  - Dropped VehicleID column from Fuel';
END

-- Update Maintenance table
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Maintenance') AND name = 'VehicleID')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Maintenance') AND name = 'BusID')
    BEGIN
        ALTER TABLE Maintenance ADD BusID INT NULL;
        PRINT '  - Added BusID column to Maintenance table';
    END

    IF EXISTS (SELECT * FROM @mappingTable)
    BEGIN
        UPDATE m SET BusID = mt.BusId
        FROM Maintenance m
        INNER JOIN @mappingTable mt ON m.VehicleID = mt.VehicleId;

        PRINT CONCAT('  - Updated ', @@ROWCOUNT, ' Maintenance records with BusID');
    END

    ALTER TABLE Maintenance DROP COLUMN VehicleID;
    PRINT '  - Dropped VehicleID column from Maintenance';
END

-- Update Routes table
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Routes') AND name = 'AMVehicleID')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Routes') AND name = 'AMBusID')
    BEGIN
        ALTER TABLE Routes ADD AMBusID INT NULL;
        PRINT '  - Added AMBusID column to Routes table';
    END

    IF EXISTS (SELECT * FROM @mappingTable)
    BEGIN
        UPDATE r SET AMBusID = m.BusId
        FROM Routes r
        INNER JOIN @mappingTable m ON r.AMVehicleID = m.VehicleId;

        PRINT CONCAT('  - Updated ', @@ROWCOUNT, ' Routes AM records with AMBusID');
    END

    ALTER TABLE Routes DROP COLUMN AMVehicleID;
    PRINT '  - Dropped AMVehicleID column from Routes';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Routes') AND name = 'PMVehicleID')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Routes') AND name = 'PMBusID')
    BEGIN
        ALTER TABLE Routes ADD PMBusID INT NULL;
        PRINT '  - Added PMBusID column to Routes table';
    END

    IF EXISTS (SELECT * FROM @mappingTable)
    BEGIN
        UPDATE r SET PMBusID = m.BusId
        FROM Routes r
        INNER JOIN @mappingTable m ON r.PMVehicleID = m.VehicleId;

        PRINT CONCAT('  - Updated ', @@ROWCOUNT, ' Routes PM records with PMBusID');
    END

    ALTER TABLE Routes DROP COLUMN PMVehicleID;
    PRINT '  - Dropped PMVehicleID column from Routes';
END

-- Step 4: Create new foreign key constraints pointing to Buses table
PRINT 'Step 4: Creating new foreign key constraints to Buses table...';

-- Activities -> Buses
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Activities') AND name = 'BusID')
   AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Activities_Buses_BusID')
BEGIN
    ALTER TABLE Activities
    ADD CONSTRAINT FK_Activities_Buses_BusID
    FOREIGN KEY (BusID) REFERENCES Buses(BusId);
    PRINT '  - Created FK: Activities.BusID -> Buses.BusId';
END

-- ActivitySchedule -> Buses
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ActivitySchedule') AND name = 'BusID')
   AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ActivitySchedule_Buses_BusID')
BEGIN
    ALTER TABLE ActivitySchedule
    ADD CONSTRAINT FK_ActivitySchedule_Buses_BusID
    FOREIGN KEY (BusID) REFERENCES Buses(BusId);
    PRINT '  - Created FK: ActivitySchedule.BusID -> Buses.BusId';
END

-- Fuel -> Buses
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Fuel') AND name = 'BusID')
   AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Fuel_Buses_BusID')
BEGIN
    ALTER TABLE Fuel
    ADD CONSTRAINT FK_Fuel_Buses_BusID
    FOREIGN KEY (BusID) REFERENCES Buses(BusId);
    PRINT '  - Created FK: Fuel.BusID -> Buses.BusId';
END

-- Maintenance -> Buses
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Maintenance') AND name = 'BusID')
   AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Maintenance_Buses_BusID')
BEGIN
    ALTER TABLE Maintenance
    ADD CONSTRAINT FK_Maintenance_Buses_BusID
    FOREIGN KEY (BusID) REFERENCES Buses(BusId);
    PRINT '  - Created FK: Maintenance.BusID -> Buses.BusId';
END

-- Routes -> Buses (AM)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Routes') AND name = 'AMBusID')
   AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Routes_Buses_AMBusID')
BEGIN
    ALTER TABLE Routes
    ADD CONSTRAINT FK_Routes_Buses_AMBusID
    FOREIGN KEY (AMBusID) REFERENCES Buses(BusId);
    PRINT '  - Created FK: Routes.AMBusID -> Buses.BusId';
END

-- Routes -> Buses (PM)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Routes') AND name = 'PMBusID')
   AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Routes_Buses_PMBusID')
BEGIN
    ALTER TABLE Routes
    ADD CONSTRAINT FK_Routes_Buses_PMBusID
    FOREIGN KEY (PMBusID) REFERENCES Buses(BusId);
    PRINT '  - Created FK: Routes.PMBusID -> Buses.BusId';
END

-- Step 5: Drop the old Vehicles table
PRINT 'Step 5: Removing deprecated Vehicles table...';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Vehicles')
BEGIN
    DROP TABLE Vehicles;
    PRINT '  - Dropped Vehicles table successfully';
END
ELSE
BEGIN
    PRINT '  - Vehicles table not found (already removed)';
END

-- Step 6: Add some sample data to populate empty tables for testing
PRINT 'Step 6: Adding sample data for testing...';

-- Get a valid BusId for the sample data
DECLARE @sampleBusId INT;
SELECT TOP 1 @sampleBusId = BusId FROM Buses ORDER BY BusId;

IF @sampleBusId IS NOT NULL
BEGIN
    -- Add sample routes if none exist
    IF NOT EXISTS (SELECT * FROM Routes)
    BEGIN
        INSERT INTO Routes (RouteName, StartTime, EndTime, Description, AMDriverID, PMDriverID, AMBusID, PMBusID)
        VALUES
        ('Route 1 - Elementary', '07:00:00', '15:30:00', 'Main elementary school route', 1, 1, @sampleBusId, @sampleBusId),
        ('Route 2 - High School', '06:45:00', '16:00:00', 'High school route downtown', 1, 1, @sampleBusId, @sampleBusId);

        PRINT CONCAT('  - Added ', @@ROWCOUNT, ' sample routes');
    END

    -- Add sample fuel records if none exist
    IF NOT EXISTS (SELECT * FROM Fuel)
    BEGIN
        INSERT INTO Fuel (BusID, Date, Gallons, Cost, Odometer, Notes)
        VALUES
        (@sampleBusId, GETDATE(), 25.5, 89.25, 45000, 'Regular fuel fill-up'),
        (@sampleBusId, DATEADD(day, -7, GETDATE()), 24.8, 86.80, 44750, 'Weekly fuel fill-up');

        PRINT CONCAT('  - Added ', @@ROWCOUNT, ' sample fuel records');
    END

    -- Add sample maintenance records if none exist
    IF NOT EXISTS (SELECT * FROM Maintenance)
    BEGIN
        INSERT INTO Maintenance (BusID, Date, Type, Description, Cost, NextDueDate, Notes)
        VALUES
        (@sampleBusId, DATEADD(day, -30, GETDATE()), 'Oil Change', 'Regular 5000 mile oil change', 45.00, DATEADD(day, 90, GETDATE()), 'Used 5W-30 oil'),
        (@sampleBusId, DATEADD(day, -60, GETDATE()), 'Inspection', 'Annual safety inspection', 75.00, DATEADD(year, 1, GETDATE()), 'Passed all safety checks');

        PRINT CONCAT('  - Added ', @@ROWCOUNT, ' sample maintenance records');
    END
END
ELSE
BEGIN
    PRINT '  - No buses found, skipping sample data creation';
END

PRINT '';
PRINT 'Migration and cleanup completed successfully!';
PRINT '==========================================';
