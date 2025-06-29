-- BusBuddy Database Migration and Cleanup Script
-- This script migrates from deprecated "Vehicles" table to "Buses" table
-- and ensures all foreign keys point to the correct tables

USE BusBuddy;
GO

PRINT 'Starting BusBuddy Database Migration and Cleanup...';
PRINT '================================================';

-- Step 1: Drop all foreign key constraints that reference the old Vehicles table
PRINT 'Step 1: Dropping foreign key constraints that reference Vehicles table...';

-- Drop foreign keys from Activities table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK__Activitie__Vehic__412EB0B6')
BEGIN
    ALTER TABLE Activities DROP CONSTRAINT FK__Activitie__Vehic__412EB0B6;
    PRINT '  - Dropped FK from Activities.VehicleID to Vehicles.Id';
END

-- Drop foreign keys from ActivitySchedule table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK__ActivityS__Vehic__4CA06362')
BEGIN
    ALTER TABLE ActivitySchedule DROP CONSTRAINT FK__ActivityS__Vehic__4CA06362;
    PRINT '  - Dropped FK from ActivitySchedule.VehicleID to Vehicles.Id';
END

-- Drop foreign keys from Fuel table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK__Fuel__VehicleID__44FF419A')
BEGIN
    ALTER TABLE Fuel DROP CONSTRAINT FK__Fuel__VehicleID__44FF419A;
    PRINT '  - Dropped FK from Fuel.VehicleID to Vehicles.Id';
END

-- Drop foreign keys from Maintenance table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK__Maintenan__Vehic__47DBAE45')
BEGIN
    ALTER TABLE Maintenance DROP CONSTRAINT FK__Maintenan__Vehic__47DBAE45;
    PRINT '  - Dropped FK from Maintenance.VehicleID to Vehicles.Id';
END

-- Drop foreign keys from Routes table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK__Routes__AMVehicl__3B75D760')
BEGIN
    ALTER TABLE Routes DROP CONSTRAINT FK__Routes__AMVehicl__3B75D760;
    PRINT '  - Dropped FK from Routes.AMVehicleID to Vehicles.Id';
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK__Routes__PMVehicl__3C69FB99')
BEGIN
    ALTER TABLE Routes DROP CONSTRAINT FK__Routes__PMVehicl__3C69FB99;
    PRINT '  - Dropped FK from Routes.PMVehicleID to Vehicles.Id';
END

-- Step 2: Migrate any data from Vehicles table to Buses table (if not already done)
PRINT 'Step 2: Migrating data from Vehicles to Buses table...';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Vehicles')
BEGIN
    -- Check if there's any data in Vehicles that's not in Buses
    DECLARE @vehicleCount INT, @busCount INT;
    SELECT @vehicleCount = COUNT(*) FROM Vehicles;
    SELECT @busCount = COUNT(*) FROM Buses;

    PRINT CONCAT('  - Found ', @vehicleCount, ' records in Vehicles table');
    PRINT CONCAT('  - Found ', @busCount, ' records in Buses table');

    -- If Vehicles has data but Buses doesn't, migrate it
    IF @vehicleCount > 0 AND @busCount = 0
    BEGIN
        INSERT INTO Buses (BusNumber, Make, Model, Year, Capacity, Status, PurchaseDate, LastMaintenanceDate, Mileage, LicensePlate, Notes)
        SELECT BusNumber, Make, Model, Year, Capacity, Status, PurchaseDate, LastMaintenanceDate, Mileage, LicensePlate, Notes
        FROM Vehicles;

        PRINT CONCAT('  - Migrated ', @@ROWCOUNT, ' records from Vehicles to Buses');
    END
    ELSE
    BEGIN
        PRINT '  - No migration needed (Buses table already has data or Vehicles is empty)';
    END
END

-- Step 3: Update all VehicleID foreign key columns to reference BusID instead
PRINT 'Step 3: Updating foreign key column references...';

-- Update Activities table - rename VehicleID to BusID if needed
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Activities') AND name = 'VehicleID')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Activities') AND name = 'BusID')
BEGIN
    -- Add new BusID column
    ALTER TABLE Activities ADD BusID int NULL;

    -- Copy data if there are existing relationships
    UPDATE Activities SET BusID = VehicleID WHERE VehicleID IS NOT NULL;

    -- Drop old VehicleID column
    ALTER TABLE Activities DROP COLUMN VehicleID;

    PRINT '  - Updated Activities: VehicleID -> BusID';
END

-- Similar updates for other tables...
-- ActivitySchedule
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ActivitySchedule') AND name = 'VehicleID')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ActivitySchedule') AND name = 'BusID')
BEGIN
    ALTER TABLE ActivitySchedule ADD BusID int NULL;
    UPDATE ActivitySchedule SET BusID = VehicleID WHERE VehicleID IS NOT NULL;
    ALTER TABLE ActivitySchedule DROP COLUMN VehicleID;
    PRINT '  - Updated ActivitySchedule: VehicleID -> BusID';
END

-- Fuel
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Fuel') AND name = 'VehicleID')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Fuel') AND name = 'BusID')
BEGIN
    ALTER TABLE Fuel ADD BusID int NULL;
    UPDATE Fuel SET BusID = VehicleID WHERE VehicleID IS NOT NULL;
    ALTER TABLE Fuel DROP COLUMN VehicleID;
    PRINT '  - Updated Fuel: VehicleID -> BusID';
END

-- Maintenance
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Maintenance') AND name = 'VehicleID')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Maintenance') AND name = 'BusID')
BEGIN
    ALTER TABLE Maintenance ADD BusID int NULL;
    UPDATE Maintenance SET BusID = VehicleID WHERE VehicleID IS NOT NULL;
    ALTER TABLE Maintenance DROP COLUMN VehicleID;
    PRINT '  - Updated Maintenance: VehicleID -> BusID';
END

-- Routes table - handle both AM and PM vehicle assignments
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Routes') AND name = 'AMVehicleID')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Routes') AND name = 'AMBusID')
BEGIN
    ALTER TABLE Routes ADD AMBusID int NULL;
    UPDATE Routes SET AMBusID = AMVehicleID WHERE AMVehicleID IS NOT NULL;
    ALTER TABLE Routes DROP COLUMN AMVehicleID;
    PRINT '  - Updated Routes: AMVehicleID -> AMBusID';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Routes') AND name = 'PMVehicleID')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Routes') AND name = 'PMBusID')
BEGIN
    ALTER TABLE Routes ADD PMBusID int NULL;
    UPDATE Routes SET PMBusID = PMVehicleID WHERE PMVehicleID IS NOT NULL;
    ALTER TABLE Routes DROP COLUMN PMVehicleID;
    PRINT '  - Updated Routes: PMVehicleID -> PMBusID';
END

-- Step 4: Create new foreign key constraints pointing to Buses table
PRINT 'Step 4: Creating new foreign key constraints to Buses table...';

-- Activities -> Buses
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Activities_Buses_BusID')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Activities') AND name = 'BusID')
BEGIN
    ALTER TABLE Activities
    ADD CONSTRAINT FK_Activities_Buses_BusID
    FOREIGN KEY (BusID) REFERENCES Buses(BusID);
    PRINT '  - Created FK: Activities.BusID -> Buses.BusID';
END

-- ActivitySchedule -> Buses
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ActivitySchedule_Buses_BusID')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ActivitySchedule') AND name = 'BusID')
BEGIN
    ALTER TABLE ActivitySchedule
    ADD CONSTRAINT FK_ActivitySchedule_Buses_BusID
    FOREIGN KEY (BusID) REFERENCES Buses(BusID);
    PRINT '  - Created FK: ActivitySchedule.BusID -> Buses.BusID';
END

-- Fuel -> Buses
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Fuel_Buses_BusID')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Fuel') AND name = 'BusID')
BEGIN
    ALTER TABLE Fuel
    ADD CONSTRAINT FK_Fuel_Buses_BusID
    FOREIGN KEY (BusID) REFERENCES Buses(BusID);
    PRINT '  - Created FK: Fuel.BusID -> Buses.BusID';
END

-- Maintenance -> Buses
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Maintenance_Buses_BusID')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Maintenance') AND name = 'BusID')
BEGIN
    ALTER TABLE Maintenance
    ADD CONSTRAINT FK_Maintenance_Buses_BusID
    FOREIGN KEY (BusID) REFERENCES Buses(BusID);
    PRINT '  - Created FK: Maintenance.BusID -> Buses.BusID';
END

-- Routes -> Buses (AM)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Routes_Buses_AMBusID')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Routes') AND name = 'AMBusID')
BEGIN
    ALTER TABLE Routes
    ADD CONSTRAINT FK_Routes_Buses_AMBusID
    FOREIGN KEY (AMBusID) REFERENCES Buses(BusID);
    PRINT '  - Created FK: Routes.AMBusID -> Buses.BusID';
END

-- Routes -> Buses (PM)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Routes_Buses_PMBusID')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Routes') AND name = 'PMBusID')
BEGIN
    ALTER TABLE Routes
    ADD CONSTRAINT FK_Routes_Buses_PMBusID
    FOREIGN KEY (PMBusID) REFERENCES Buses(BusID);
    PRINT '  - Created FK: Routes.PMBusID -> Buses.BusID';
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

-- Add sample routes if none exist
IF NOT EXISTS (SELECT * FROM Routes)
BEGIN
    INSERT INTO Routes (RouteName, StartTime, EndTime, Description, AMDriverID, PMDriverID, AMBusID, PMBusID)
    VALUES
    ('Route 1 - Elementary', '07:00:00', '15:30:00', 'Main elementary school route', 1, 1, 1, 1),
    ('Route 2 - High School', '06:45:00', '16:00:00', 'High school route downtown', 1, 1, 1, 1);

    PRINT CONCAT('  - Added ', @@ROWCOUNT, ' sample routes');
END

-- Add sample fuel records if none exist
IF NOT EXISTS (SELECT * FROM Fuel)
BEGIN
    INSERT INTO Fuel (BusID, Date, Gallons, Cost, Odometer, Notes)
    VALUES
    (1, GETDATE(), 25.5, 89.25, 45000, 'Regular fuel fill-up'),
    (1, DATEADD(day, -7, GETDATE()), 24.8, 86.80, 44750, 'Weekly fuel fill-up');

    PRINT CONCAT('  - Added ', @@ROWCOUNT, ' sample fuel records');
END

-- Add sample maintenance records if none exist
IF NOT EXISTS (SELECT * FROM Maintenance)
BEGIN
    INSERT INTO Maintenance (BusID, Date, Type, Description, Cost, NextDueDate, Notes)
    VALUES
    (1, DATEADD(day, -30, GETDATE()), 'Oil Change', 'Regular 5000 mile oil change', 45.00, DATEADD(day, 90, GETDATE()), 'Used 5W-30 oil'),
    (1, DATEADD(day, -60, GETDATE()), 'Inspection', 'Annual safety inspection', 75.00, DATEADD(year, 1, GETDATE()), 'Passed all safety checks');

    PRINT CONCAT('  - Added ', @@ROWCOUNT, ' sample maintenance records');
END

PRINT '';
PRINT 'Migration and cleanup completed successfully!';
PRINT '==========================================';

-- Final verification
PRINT 'Final verification:';
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

PRINT '';
PRINT 'Foreign key constraints now pointing to Buses table:';
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
WHERE tr.name = 'Buses'
ORDER BY tp.name, fk.name;
