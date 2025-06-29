-- BusBuddy Database Schema Fix Script
-- This script fixes table mismatches and ensures all required tables exist
-- Date: June 28, 2025

USE BusBuddy;
GO

-- Fix Drivers table - add Name column as alias to DriverName if needed
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Drivers') AND name = 'Name')
BEGIN
    ALTER TABLE Drivers ADD Name AS DriverName;
    PRINT 'Added Name computed column to Drivers table';
END

-- Ensure Buses table has correct structure
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Buses') AND name = 'BusId')
AND EXISTS (SELECT * FROM sys.tables WHERE name = 'Buses')
BEGIN
    -- Table exists but might have wrong column names
    PRINT 'Buses table structure needs review';
END

-- Check if we need to create Buses table (might exist as Vehicles)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Buses')
AND EXISTS (SELECT * FROM sys.tables WHERE name = 'Vehicles')
BEGIN
    -- Create a view that maps Vehicles to Buses for compatibility
    IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'Buses')
    BEGIN
        EXEC('CREATE VIEW Buses AS
              SELECT VehicleID as BusId,
                     BusNumber,
                     Year,
                     Make,
                     Model,
                     SeatingCapacity as Capacity,
                     VIN,
                     LicenseNumber,
                     DateLastInspection as LastInspectionDate,
                     ''Active'' as Status
              FROM Vehicles');
        PRINT 'Created Buses view mapping to Vehicles table';
    END
END

-- Ensure Routes table uses correct foreign key columns
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Routes')
BEGIN
    -- Check if Routes table has the right column names
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Routes') AND name = 'RouteId')
    AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Routes') AND name = 'RouteID')
    BEGIN
        PRINT 'Routes table has RouteID instead of RouteId - this is acceptable';
    END
END

-- Fix Activities table column names if needed
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Activities')
BEGIN
    -- Check if AssignedVehicleID should be AssignedBusID
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Activities') AND name = 'AssignedVehicleID')
    AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Activities') AND name = 'AssignedBusID')
    BEGIN
        -- Add computed column for compatibility
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Activities') AND name = 'AssignedBusID')
        BEGIN
            ALTER TABLE Activities ADD AssignedBusID AS AssignedVehicleID;
            PRINT 'Added AssignedBusID computed column to Activities table';
        END
    END
END

-- Fix Fuel table column names if needed
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Fuel')
BEGIN
    -- Check if VehicleID should be VehicleFueledID
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Fuel') AND name = 'VehicleID')
    AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Fuel') AND name = 'VehicleFueledID')
    BEGIN
        -- Add computed column for compatibility
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Fuel') AND name = 'VehicleFueledID')
        BEGIN
            ALTER TABLE Fuel ADD VehicleFueledID AS VehicleID;
            PRINT 'Added VehicleFueledID computed column to Fuel table';
        END
    END
END

-- Ensure we have a working database structure
PRINT '=== Current Database Tables ===';

SELECT
    TABLE_NAME as TableName,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) as ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

PRINT '✅ Database schema fixes applied!';
GO
