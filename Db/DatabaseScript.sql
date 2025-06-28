-- BusBuddy Database Script
-- Consolidated script for SQL Server Express
-- Date: 2025-06-28
-- Author: Steve McKitrick
-- Matches BusBuddy Tables.xlsx

-- Drop existing database (use with caution)
IF DB_ID('BusBuddy') IS NOT NULL
BEGIN
    ALTER DATABASE BusBuddy SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BusBuddy;
END
GO

-- Create database
CREATE DATABASE BusBuddy;
GO

USE BusBuddy;
GO

-- Vehicles Table
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

-- Drivers Table
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

-- Routes Table
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

-- Activities Table
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

-- Fuel Table
CREATE TABLE Fuel (
    FuelID INT PRIMARY KEY IDENTITY(1,1),
    FuelDate DATE NOT NULL,
    FuelLocation NVARCHAR(50), -- Key Pumps or Gas Station
    VehicleID INT,
    VehicleOdometerReading INT,
    FuelType NVARCHAR(50), -- Gasoline or Diesel
    FOREIGN KEY (VehicleID) REFERENCES Vehicles(VehicleID)
);

-- Maintenance Table
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

-- School Calendar Table
CREATE TABLE SchoolCalendar (
    CalendarID INT PRIMARY KEY IDENTITY(1,1),
    CalendarDate DATE NOT NULL,
    Category NVARCHAR(50), -- School Day, Holiday, Event
    Description NVARCHAR(200) -- Thanksgiving Break, etc.
);

-- Activity Schedule Table
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

-- Seed sample data
INSERT INTO Vehicles (BusNumber, Year, Make, Model, SeatingCapacity)
VALUES ('Bus 8', 2015, 'BlueBird', 'Vision', 40),
       ('Bus 9', 2019, 'International', 'CE', 30),
       ('Bus 10', 2021, 'BlueBird', 'Vision', 50);

INSERT INTO Drivers (DriverName, DriversLicenseType)
VALUES ('John Doe', 'CDL'),
       ('Jane Smith', 'Passenger'),
       ('Bob Johnson', 'CDL');

INSERT INTO Routes (RouteDate, RouteName, AMVehicleID, AMBeginMiles, AMEndMiles, AMRiders, AMDriverID, RouteType)
VALUES ('2025-06-28', 'Truck Plaza', 1, 1000, 1050, 20, 1, 'CDL'),
       ('2025-06-29', 'East Route', 2, 2000, 2050, 22, 2, 'CDL');

INSERT INTO Activities (ActivityDate, ActivityType, Destination, LeaveTime, EventTime, RequestedBy, AssignedVehicleID, DriverID)
VALUES ('2025-06-28', 'Field Trip', 'Museum', '08:00:00', '10:00:00', 'Ms. Smith', 1, 1),
       ('2025-06-29', 'Sports', 'Stadium', '09:00:00', '11:00:00', 'Coach Lee', 2, 2);

INSERT INTO Fuel (FuelDate, FuelLocation, VehicleID, VehicleOdometerReading, FuelType)
VALUES ('2025-06-28', 'Key Pumps', 1, 1100, 'Diesel'),
       ('2025-06-29', 'Gas Station', 2, 2100, 'Gasoline');

INSERT INTO Maintenance (MaintenanceDate, VehicleID, OdometerReading, MaintenanceCompleted, Vendor, RepairCost)
VALUES ('2025-06-28', 1, 1100, 'Oil Change', 'QuickLube', 75.00),
       ('2025-06-29', 2, 2100, 'Tire Rotation', 'TireShop', 50.00);

INSERT INTO SchoolCalendar (CalendarDate, Category, Description)
VALUES ('2025-08-15', 'School Day', 'First Day of School'),
       ('2025-11-27', 'Holiday', 'Thanksgiving Break');

INSERT INTO ActivitySchedule (ScheduledDate, TripType, ScheduledVehicleID, ScheduledDestination, ScheduledLeaveTime, ScheduledEventTime, ScheduledRiders, ScheduledDriverID)
VALUES ('2025-06-28', 'Field Trip', 1, 'Museum', '08:00:00', '10:00:00', 30, 1),
       ('2025-06-29', 'Sports', 2, 'Stadium', '09:00:00', '11:00:00', 25, 2);
