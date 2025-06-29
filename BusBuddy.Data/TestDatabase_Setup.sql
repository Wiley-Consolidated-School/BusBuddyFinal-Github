-- Test Database Setup Script
-- Creates the BusBuddy_Test database with required tables and sample data

USE master;
GO

-- Create test database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BusBuddy_Test')
BEGIN
    CREATE DATABASE BusBuddy_Test;
    PRINT 'Created BusBuddy_Test database';
END
GO

USE BusBuddy_Test;
GO

-- Create Buses table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Buses')
BEGIN
    CREATE TABLE Buses (
        BusId INT IDENTITY(1,1) PRIMARY KEY,
        BusNumber NVARCHAR(50) NOT NULL,
        Year INT NULL,
        Make NVARCHAR(50) NULL,
        Model NVARCHAR(50) NULL,
        Capacity INT NOT NULL DEFAULT 0,
        VIN NVARCHAR(50) NULL,
        LicenseNumber NVARCHAR(50) NULL,
        LastInspectionDate DATETIME NULL,
        Status NVARCHAR(50) NULL DEFAULT 'Active'
    );
    PRINT 'Created Buses table in test database';
END

-- Create Drivers table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Drivers')
BEGIN
    CREATE TABLE Drivers (
        DriverId INT IDENTITY(1,1) PRIMARY KEY,
        DriverName NVARCHAR(100) NOT NULL,
        DriverPhone NVARCHAR(20) NULL,
        DriverEmail NVARCHAR(100) NULL,
        Address NVARCHAR(200) NULL,
        City NVARCHAR(50) NULL,
        State NVARCHAR(2) NULL,
        Zip NVARCHAR(10) NULL,
        DriversLicenseType NVARCHAR(50) NULL,
        TrainingComplete BIT NULL DEFAULT 0
    );
    PRINT 'Created Drivers table in test database';
END

-- Create Routes table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Routes')
BEGIN
    CREATE TABLE Routes (
        RouteId INT IDENTITY(1,1) PRIMARY KEY,
        RouteDate DATETIME NOT NULL DEFAULT GETDATE(),
        RouteName NVARCHAR(50) NULL,
        AMBusId INT NULL,
        AMBeginMiles INT NULL,
        AMEndMiles INT NULL,
        AMRiders INT NULL,
        AMDriverId INT NULL,
        PMBusId INT NULL,
        PMBeginMiles INT NULL,
        PMEndMiles INT NULL,
        PMRiders INT NULL,
        PMDriverId INT NULL,
        Notes NVARCHAR(MAX) NULL,
        RouteType NVARCHAR(50) NULL DEFAULT 'CDL',

        CONSTRAINT FK_Routes_AMBus FOREIGN KEY (AMBusId) REFERENCES Buses(BusId),
        CONSTRAINT FK_Routes_AMDriver FOREIGN KEY (AMDriverId) REFERENCES Drivers(DriverId),
        CONSTRAINT FK_Routes_PMBus FOREIGN KEY (PMBusId) REFERENCES Buses(BusId),
        CONSTRAINT FK_Routes_PMDriver FOREIGN KEY (PMDriverId) REFERENCES Drivers(DriverId)
    );
    PRINT 'Created Routes table in test database';
END

-- Insert test data
IF NOT EXISTS (SELECT * FROM Buses)
BEGIN
    INSERT INTO Buses (BusNumber, Year, Make, Model, Capacity, Status) VALUES
    ('TEST001', 2020, 'Blue Bird', 'Vision', 72, 'Active'),
    ('TEST002', 2019, 'Thomas Built', 'Saf-T-Liner', 72, 'Active');
    PRINT 'Inserted test bus data';
END

IF NOT EXISTS (SELECT * FROM Drivers)
BEGIN
    INSERT INTO Drivers (DriverName, DriverPhone, DriversLicenseType, TrainingComplete) VALUES
    ('Test Driver 1', '555-0001', 'CDL', 1),
    ('Test Driver 2', '555-0002', 'CDL', 1);
    PRINT 'Inserted test driver data';
END

IF NOT EXISTS (SELECT * FROM Routes)
BEGIN
    INSERT INTO Routes (RouteDate, RouteName, AMBusId, AMDriverId, AMRiders, PMBusId, PMDriverId, PMRiders, RouteType) VALUES
    (GETDATE(), 'Test Route 1', 1, 1, 25, 1, 1, 20, 'CDL'),
    (GETDATE(), 'Test Route 2', 2, 2, 30, 2, 2, 25, 'CDL');
    PRINT 'Inserted test route data';
END

PRINT '✅ Test database setup complete!';
SELECT 'Test Database Ready' as Result;
GO
