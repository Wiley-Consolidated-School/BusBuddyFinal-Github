-- BusBuddy Complete Database Schema Script
-- This script creates all required tables for the BusBuddy application
-- Compatible with SQL Server Express and matches Entity Framework models
-- Date: June 28, 2025
-- Run this on the BusBuddy database to ensure all tables exist

USE BusBuddy;
GO

-- =======================================
-- Table: Buses (Primary Vehicle Table)
-- =======================================
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
    PRINT 'Created Buses table';
END
ELSE
BEGIN
    PRINT 'Buses table already exists';
END
GO

-- =======================================
-- Table: Drivers
-- =======================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Drivers')
BEGIN
    CREATE TABLE Drivers (
        DriverId INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        DriverPhone NVARCHAR(20) NULL,
        DriverEmail NVARCHAR(100) NULL,
        Address NVARCHAR(200) NULL,
        City NVARCHAR(50) NULL,
        State NVARCHAR(2) NULL,
        Zip NVARCHAR(10) NULL,
        DriversLicenseType NVARCHAR(50) NULL, -- CDL or Passenger
        TrainingComplete BIT NULL DEFAULT 0,
        DriverName AS Name -- Computed column for compatibility
    );
    PRINT 'Created Drivers table';
END
ELSE
BEGIN
    PRINT 'Drivers table already exists';
END
GO

-- =======================================
-- Table: Routes
-- =======================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Routes')
BEGIN
    CREATE TABLE Routes (
        RouteId INT IDENTITY(1,1) PRIMARY KEY,
        RouteDate DATETIME NOT NULL DEFAULT GETDATE(),
        RouteName NVARCHAR(50) NULL, -- Truck Plaza, East Route, West Route, SPED

        -- AM Route Data
        AMBusId INT NULL,
        AMBeginMiles INT NULL,
        AMEndMiles INT NULL,
        AMRiders INT NULL,
        AMDriverId INT NULL,

        -- PM Route Data
        PMBusId INT NULL,
        PMBeginMiles INT NULL,
        PMEndMiles INT NULL,
        PMRiders INT NULL,
        PMDriverId INT NULL,

        -- Additional Properties
        Notes NVARCHAR(MAX) NULL,
        RouteType NVARCHAR(50) NULL DEFAULT 'CDL', -- CDL, SmallBus, SPED

        -- Foreign Key Constraints
        CONSTRAINT FK_Routes_AMBus FOREIGN KEY (AMBusId) REFERENCES Buses(BusId),
        CONSTRAINT FK_Routes_AMDriver FOREIGN KEY (AMDriverId) REFERENCES Drivers(DriverId),
        CONSTRAINT FK_Routes_PMBus FOREIGN KEY (PMBusId) REFERENCES Buses(BusId),
        CONSTRAINT FK_Routes_PMDriver FOREIGN KEY (PMDriverId) REFERENCES Drivers(DriverId)
    );
    PRINT 'Created Routes table';
END
ELSE
BEGIN
    PRINT 'Routes table already exists';
END
GO

-- =======================================
-- Table: Activities
-- =======================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Activities')
BEGIN
    CREATE TABLE Activities (
        ActivityID INT IDENTITY(1,1) PRIMARY KEY,
        ActivityDate NVARCHAR(20) NULL, -- String date for compatibility
        ActivityType NVARCHAR(50) NULL, -- Sports Trip or Activity Trip
        Destination NVARCHAR(200) NULL,
        LeaveTime NVARCHAR(20) NULL, -- String time for compatibility
        EventTime NVARCHAR(20) NULL, -- String time for compatibility
        RequestedBy NVARCHAR(100) NULL,
        AssignedBusID INT NULL, -- Renamed from AssignedVehicleID to match model
        DriverId INT NULL,
        Notes NVARCHAR(MAX) NULL,

        CONSTRAINT FK_Activities_Bus FOREIGN KEY (AssignedBusID) REFERENCES Buses(BusId),
        CONSTRAINT FK_Activities_Driver FOREIGN KEY (DriverId) REFERENCES Drivers(DriverId)
    );
    PRINT 'Created Activities table';
END
ELSE
BEGIN
    PRINT 'Activities table already exists';
END
GO

-- =======================================
-- Table: ActivitySchedule
-- =======================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ActivitySchedule')
BEGIN
    CREATE TABLE ActivitySchedule (
        ScheduleID INT IDENTITY(1,1) PRIMARY KEY,
        ScheduledDate DATETIME NOT NULL,
        TripType NVARCHAR(50) NULL, -- Sports Trip or Activity Trip
        ScheduledVehicleID INT NULL,
        ScheduledDestination NVARCHAR(200) NULL,
        ScheduledLeaveTime NVARCHAR(20) NULL, -- String time for compatibility
        ScheduledEventTime NVARCHAR(20) NULL, -- String time for compatibility
        ScheduledRiders INT NULL,
        ScheduledDriverID INT NULL,
        Notes NVARCHAR(MAX) NULL,

        CONSTRAINT FK_ActivitySchedule_Bus FOREIGN KEY (ScheduledVehicleID) REFERENCES Buses(BusId),
        CONSTRAINT FK_ActivitySchedule_Driver FOREIGN KEY (ScheduledDriverID) REFERENCES Drivers(DriverId)
    );
    PRINT 'Created ActivitySchedule table';
END
ELSE
BEGIN
    PRINT 'ActivitySchedule table already exists';
END
GO

-- =======================================
-- Table: Fuel
-- =======================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Fuel')
BEGIN
    CREATE TABLE Fuel (
        FuelID INT IDENTITY(1,1) PRIMARY KEY,
        FuelDate NVARCHAR(20) NULL, -- String date for compatibility
        FuelLocation NVARCHAR(50) NULL, -- Key Pumps or Gas Station
        VehicleFueledID INT NULL, -- References Buses table
        VehicleOdometerReading DECIMAL(18,2) NULL,
        FuelType NVARCHAR(50) NULL, -- Gasoline or Diesel
        GallonsPurchased DECIMAL(18,2) NULL,
        PricePerGallon DECIMAL(18,2) NULL,
        TotalCost DECIMAL(18,2) NULL,
        Notes NVARCHAR(MAX) NULL,

        CONSTRAINT FK_Fuel_Bus FOREIGN KEY (VehicleFueledID) REFERENCES Buses(BusId)
    );
    PRINT 'Created Fuel table';
END
ELSE
BEGIN
    PRINT 'Fuel table already exists';
END
GO

-- =======================================
-- Table: Maintenance
-- =======================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Maintenance')
BEGIN
    CREATE TABLE Maintenance (
        MaintenanceID INT IDENTITY(1,1) PRIMARY KEY,
        Date NVARCHAR(20) NULL, -- String date for compatibility
        BusId INT NULL,
        OdometerReading DECIMAL(18,2) NULL,
        MaintenanceCompleted NVARCHAR(100) NULL,
        Vendor NVARCHAR(100) NULL,
        RepairCost DECIMAL(18,2) NULL,
        Notes NVARCHAR(MAX) NULL,

        CONSTRAINT FK_Maintenance_Bus FOREIGN KEY (BusId) REFERENCES Buses(BusId)
    );
    PRINT 'Created Maintenance table';
END
ELSE
BEGIN
    PRINT 'Maintenance table already exists';
END
GO

-- =======================================
-- Table: SchoolCalendars
-- =======================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SchoolCalendars')
BEGIN
    CREATE TABLE SchoolCalendars (
        CalendarID INT IDENTITY(1,1) PRIMARY KEY,
        CalendarDate DATETIME NOT NULL,
        Category NVARCHAR(50) NULL, -- School Day, Holiday, Event
        Description NVARCHAR(200) NULL, -- Thanksgiving Break, etc.
        IsSchoolDay BIT NULL DEFAULT 1
    );
    PRINT 'Created SchoolCalendars table';
END
ELSE
BEGIN
    PRINT 'SchoolCalendars table already exists';
END
GO

-- =======================================
-- Create Indexes for Performance
-- =======================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Routes_RouteDate')
BEGIN
    CREATE INDEX IX_Routes_RouteDate ON Routes (RouteDate);
    PRINT 'Created index IX_Routes_RouteDate';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Routes_AMBusId')
BEGIN
    CREATE INDEX IX_Routes_AMBusId ON Routes (AMBusId);
    PRINT 'Created index IX_Routes_AMBusId';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Routes_PMBusId')
BEGIN
    CREATE INDEX IX_Routes_PMBusId ON Routes (PMBusId);
    PRINT 'Created index IX_Routes_PMBusId';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Maintenance_BusId')
BEGIN
    CREATE INDEX IX_Maintenance_BusId ON Maintenance (BusId);
    PRINT 'Created index IX_Maintenance_BusId';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Fuel_VehicleFueledID')
BEGIN
    CREATE INDEX IX_Fuel_VehicleFueledID ON Fuel (VehicleFueledID);
    PRINT 'Created index IX_Fuel_VehicleFueledID';
END

-- =======================================
-- Insert Sample Data (if tables are empty)
-- =======================================

-- Sample Buses
IF NOT EXISTS (SELECT * FROM Buses)
BEGIN
    INSERT INTO Buses (BusNumber, Year, Make, Model, Capacity, Status) VALUES
    ('001', 2020, 'Blue Bird', 'Vision', 72, 'Active'),
    ('002', 2019, 'Thomas Built', 'Saf-T-Liner', 72, 'Active'),
    ('003', 2021, 'IC Bus', 'CE Series', 90, 'Active'),
    ('004', 2018, 'Blue Bird', 'All American', 72, 'Maintenance'),
    ('005', 2022, 'Thomas Built', 'Jouley', 35, 'Active');
    PRINT 'Inserted sample bus data';
END

-- Sample Drivers
IF NOT EXISTS (SELECT * FROM Drivers)
BEGIN
    INSERT INTO Drivers (Name, DriverPhone, DriversLicenseType, TrainingComplete) VALUES
    ('John Smith', '555-0101', 'CDL', 1),
    ('Mary Johnson', '555-0102', 'CDL', 1),
    ('Robert Brown', '555-0103', 'CDL', 1),
    ('Lisa Davis', '555-0104', 'Passenger', 1),
    ('Michael Wilson', '555-0105', 'CDL', 0);
    PRINT 'Inserted sample driver data';
END

-- Sample Routes
IF NOT EXISTS (SELECT * FROM Routes)
BEGIN
    INSERT INTO Routes (RouteDate, RouteName, AMBusId, AMDriverId, AMRiders, PMBusId, PMDriverId, PMRiders, RouteType) VALUES
    (GETDATE(), 'East Route', 1, 1, 35, 1, 1, 30, 'CDL'),
    (GETDATE(), 'West Route', 2, 2, 42, 2, 2, 38, 'CDL'),
    (GETDATE(), 'SPED Route', 5, 4, 12, 5, 4, 10, 'SmallBus');
    PRINT 'Inserted sample route data';
END

PRINT '✅ Database schema initialization complete!';
PRINT 'All required tables have been created or verified.';
GO
