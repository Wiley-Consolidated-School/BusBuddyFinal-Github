-- BusBuddy Database Script (No Drop/Create, SQL Server Compatible)
-- This version omits DROP DATABASE and CREATE DATABASE statements for safe schema migration.
-- Uses SQL Server syntax for conditional table creation.
-- Date: 2025-06-28
-- Author: Steve McKitrick

USE BusBuddy;
GO

-- Vehicles Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Vehicles')
BEGIN
    CREATE TABLE Vehicles (
        VehicleID INT PRIMARY KEY IDENTITY(1,1),
        BusNumber NVARCHAR(50) NOT NULL,
        Year INT,
        Make NVARCHAR(50),
        Model NVARCHAR(50),
        SeatingCapacity INT,
        VIN NVARCHAR(50),
        LicenseNumber NVARCHAR(50),
        DateLastInspection DATE
    );
END
GO

-- Drivers Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Drivers')
BEGIN
    CREATE TABLE Drivers (
        DriverID INT PRIMARY KEY IDENTITY(1,1),
        DriverName NVARCHAR(100) NOT NULL,
        DriverPhone NVARCHAR(20),
        DriverEmail NVARCHAR(100),
        Address NVARCHAR(200),
        City NVARCHAR(50),
        State NVARCHAR(2),
        Zip NVARCHAR(10),
        DriversLicenseType NVARCHAR(50), -- CDL or Passenger
        TrainingComplete BIT
    );
END
GO

-- Routes Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Routes')
BEGIN
    CREATE TABLE Routes (
        RouteID INT PRIMARY KEY IDENTITY(1,1),
        RouteDate DATE NOT NULL,
        RouteName NVARCHAR(50) NOT NULL, -- Truck Plaza, East Route, West Route, SPED
        AMVehicleID INT,
        AMBeginMiles INT,
        AMEndMiles INT,
        AMRiders INT,
        AMDriverID INT,
        PMVehicleID INT,
        PMBeginMiles INT,
        PMEndMiles INT,
        PMRiders INT,
        PMDriverID INT,
        Notes NVARCHAR(500), -- Added for additional route information
        RouteType NVARCHAR(50), -- Added for driver pay calculations: CDL, SmallBus, SPED
        FOREIGN KEY (AMVehicleID) REFERENCES Vehicles(VehicleID),
        FOREIGN KEY (AMDriverID) REFERENCES Drivers(DriverID),
        FOREIGN KEY (PMVehicleID) REFERENCES Vehicles(VehicleID),
        FOREIGN KEY (PMDriverID) REFERENCES Drivers(DriverID)
    );
END
GO

-- Activities Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Activities')
BEGIN
    CREATE TABLE Activities (
        ActivityID INT PRIMARY KEY IDENTITY(1,1),
        ActivityDate DATE NOT NULL,
        ActivityType NVARCHAR(50), -- Sports Trip or Activity Trip
        Destination NVARCHAR(200),
        LeaveTime TIME,
        EventTime TIME,
        RequestedBy NVARCHAR(100),
        AssignedVehicleID INT,
        DriverID INT,
        FOREIGN KEY (AssignedVehicleID) REFERENCES Vehicles(VehicleID),
        FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID)
    );
END
GO

-- Fuel Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Fuel')
BEGIN
    CREATE TABLE Fuel (
        FuelID INT PRIMARY KEY IDENTITY(1,1),
        FuelDate DATE NOT NULL,
        FuelLocation NVARCHAR(50), -- Key Pumps or Gas Station
        VehicleID INT,
        VehicleOdometerReading INT,
        FuelType NVARCHAR(50), -- Gasoline or Diesel
        FOREIGN KEY (VehicleID) REFERENCES Vehicles(VehicleID)
    );
END
GO

-- Maintenance Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Maintenance')
BEGIN
    CREATE TABLE Maintenance (
        MaintenanceID INT PRIMARY KEY IDENTITY(1,1),
        MaintenanceDate DATE NOT NULL,
        VehicleID INT,
        OdometerReading INT,
        MaintenanceCompleted NVARCHAR(200), -- Tires, Windshield, etc.
        Vendor NVARCHAR(100),
        RepairCost DECIMAL(10,2),
        FOREIGN KEY (VehicleID) REFERENCES Vehicles(VehicleID)
    );
END
GO

-- School Calendar Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SchoolCalendar')
BEGIN
    CREATE TABLE SchoolCalendar (
        CalendarID INT PRIMARY KEY IDENTITY(1,1),
        CalendarDate DATE NOT NULL,
        Category NVARCHAR(50), -- School Day, Holiday, Event
        Description NVARCHAR(200) -- Thanksgiving Break, etc.
    );
END
GO

-- Activity Schedule Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ActivitySchedule')
BEGIN
    CREATE TABLE ActivitySchedule (
        ScheduleID INT PRIMARY KEY IDENTITY(1,1),
        ScheduledDate DATE NOT NULL,
        TripType NVARCHAR(50), -- Sports Trip or Activity Trip
        ScheduledVehicleID INT,
        ScheduledDestination NVARCHAR(200),
        ScheduledLeaveTime TIME,
        ScheduledEventTime TIME,
        ScheduledRiders INT,
        ScheduledDriverID INT,
        FOREIGN KEY (ScheduledVehicleID) REFERENCES Vehicles(VehicleID),
        FOREIGN KEY (ScheduledDriverID) REFERENCES Drivers(DriverID)
    );
END
GO

-- (Seed data removed for cleanup)
