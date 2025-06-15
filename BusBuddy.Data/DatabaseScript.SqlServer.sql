-- BusBuddy Database Schema for SQL Server
--
-- Compatible with SQL Server Express
-- Maintained for test and production compatibility
--
-- Last updated: June 15, 2025
--
-- 13 tables, including Vehicles (with FuelType, Status columns), Fuel, etc.

-- Drop existing tables if they exist (in correct order to avoid foreign key conflicts)
-- First drop all foreign key constraints to avoid conflicts
DECLARE @sql NVARCHAR(MAX) = ''
SELECT @sql = @sql + 'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + ' DROP CONSTRAINT ' + QUOTENAME(name) + ';' + CHAR(13)
FROM sys.foreign_keys

IF @sql <> ''
BEGIN
    EXEC sp_executesql @sql
END

-- Drop dependent tables first (those with foreign keys)
IF OBJECT_ID('ActivitySchedule', 'U') IS NOT NULL DROP TABLE ActivitySchedule;
IF OBJECT_ID('Activities', 'U') IS NOT NULL DROP TABLE Activities;
IF OBJECT_ID('Routes', 'U') IS NOT NULL DROP TABLE Routes;
IF OBJECT_ID('Maintenance', 'U') IS NOT NULL DROP TABLE Maintenance;
IF OBJECT_ID('Fuel', 'U') IS NOT NULL DROP TABLE Fuel;

-- Drop independent tables
IF OBJECT_ID('SchoolCalendar', 'U') IS NOT NULL DROP TABLE SchoolCalendar;

-- Drop parent tables last
IF OBJECT_ID('Drivers', 'U') IS NOT NULL DROP TABLE Drivers;
IF OBJECT_ID('Vehicles', 'U') IS NOT NULL DROP TABLE Vehicles;

-- Vehicles Table
CREATE TABLE Vehicles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    VehicleNumber NVARCHAR(50) NOT NULL,
    BusNumber NVARCHAR(50),
    Make NVARCHAR(50),
    Model NVARCHAR(50),
    Year INT,
    SeatingCapacity INT,
    VINNumber NVARCHAR(100),
    LicenseNumber NVARCHAR(50),
    DateLastInspection NVARCHAR(50),
    Notes NVARCHAR(MAX),
    FuelType NVARCHAR(50), -- Added for schema compatibility
    Status NVARCHAR(50)    -- Added for schema compatibility
);

-- Drivers Table
CREATE TABLE Drivers (
    DriverID INT IDENTITY(1,1) PRIMARY KEY,
    DriverName NVARCHAR(100),
    DriverPhone NVARCHAR(20),
    DriverEmail NVARCHAR(100),
    Address NVARCHAR(200),
    City NVARCHAR(50),
    State NVARCHAR(20),
    Zip NVARCHAR(10),
    DriversLicenseType NVARCHAR(50),
    TrainingComplete INT NOT NULL DEFAULT 0, -- Use TrainingComplete (int) to match model
    Notes NVARCHAR(MAX),
    -- Additional columns for Entity Framework compatibility
    Status NVARCHAR(50),
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    CDLExpirationDate DATETIME
);

-- Routes Table
CREATE TABLE Routes (
    RouteID INT IDENTITY(1,1) PRIMARY KEY,
    Date NVARCHAR(50) NOT NULL,
    RouteName NVARCHAR(100),
    AMVehicleID INT,
    AMBeginMiles FLOAT,
    AMEndMiles FLOAT,
    AMRiders INT,
    AMDriverID INT,
    PMVehicleID INT,
    PMBeginMiles FLOAT,
    PMEndMiles FLOAT,
    PMRiders INT,
    PMDriverID INT,
    Notes NVARCHAR(MAX),
    FOREIGN KEY (AMVehicleID) REFERENCES Vehicles(Id),
    FOREIGN KEY (AMDriverID) REFERENCES Drivers(DriverID),
    FOREIGN KEY (PMVehicleID) REFERENCES Vehicles(Id),
    FOREIGN KEY (PMDriverID) REFERENCES Drivers(DriverID)
);

-- Activities Table
CREATE TABLE Activities (
    ActivityID INT IDENTITY(1,1) PRIMARY KEY,
    Date NVARCHAR(50),
    ActivityType NVARCHAR(100),
    Destination NVARCHAR(200),
    LeaveTime NVARCHAR(50),
    EventTime NVARCHAR(50),
    ReturnTime NVARCHAR(50),
    RequestedBy NVARCHAR(100),
    AssignedVehicleID INT,
    DriverID INT,
    Notes NVARCHAR(MAX),
    FOREIGN KEY (AssignedVehicleID) REFERENCES Vehicles(Id),
    FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID)
);

-- Fuel Table
CREATE TABLE Fuel (
    FuelID INT IDENTITY(1,1) PRIMARY KEY,
    FuelDate NVARCHAR(50),
    FuelLocation NVARCHAR(200),
    VehicleFueledID INT,
    VehicleOdometerReading FLOAT,
    FuelType NVARCHAR(50),
    FuelAmount FLOAT,
    FuelCost FLOAT,
    Notes NVARCHAR(MAX),
    FOREIGN KEY (VehicleFueledID) REFERENCES Vehicles(Id)
);

-- Maintenance Table
CREATE TABLE Maintenance (
    MaintenanceID INT IDENTITY(1,1) PRIMARY KEY,
    Date NVARCHAR(50),
    VehicleID INT,
    OdometerReading FLOAT,
    MaintenanceCompleted NVARCHAR(MAX),
    Vendor NVARCHAR(200),
    RepairCost FLOAT,
    Notes NVARCHAR(MAX),
    FOREIGN KEY (VehicleID) REFERENCES Vehicles(Id)
);

-- School Calendar Table
CREATE TABLE SchoolCalendar (
    CalendarID INT IDENTITY(1,1) PRIMARY KEY,
    Date NVARCHAR(50),
    EndDate NVARCHAR(50),
    Category NVARCHAR(100),
    Description NVARCHAR(MAX),
    RouteNeeded INT,
    Notes NVARCHAR(MAX)
);

-- Activity Schedule Table
CREATE TABLE ActivitySchedule (
    ScheduleID INT IDENTITY(1,1) PRIMARY KEY,
    Date NVARCHAR(50),
    TripType NVARCHAR(100),
    ScheduledVehicleID INT,
    ScheduledDestination NVARCHAR(200),
    ScheduledLeaveTime NVARCHAR(50),
    ScheduledEventTime NVARCHAR(50),
    ScheduledReturnTime NVARCHAR(50),
    ScheduledRiders INT,
    ScheduledDriverID INT,
    Notes NVARCHAR(MAX),
    FOREIGN KEY (ScheduledVehicleID) REFERENCES Vehicles(Id),
    FOREIGN KEY (ScheduledDriverID) REFERENCES Drivers(DriverID)
);

-- Create indexes for common queries
CREATE INDEX idx_routes_date ON Routes(Date);
CREATE INDEX idx_routes_driver ON Routes(AMDriverID, PMDriverID);
CREATE INDEX idx_routes_vehicle ON Routes(AMVehicleID, PMVehicleID);

CREATE INDEX idx_activities_date ON Activities(Date);
CREATE INDEX idx_activities_driver ON Activities(DriverID);
CREATE INDEX idx_activities_vehicle ON Activities(AssignedVehicleID);

CREATE INDEX idx_fuel_date ON Fuel(FuelDate);
CREATE INDEX idx_fuel_vehicle ON Fuel(VehicleFueledID);

CREATE INDEX idx_maintenance_date ON Maintenance(Date);
CREATE INDEX idx_maintenance_vehicle ON Maintenance(VehicleID);

CREATE INDEX idx_calendar_date ON SchoolCalendar(Date);
CREATE INDEX idx_calendar_enddate ON SchoolCalendar(EndDate);
CREATE INDEX idx_calendar_category ON SchoolCalendar(Category);

CREATE INDEX idx_activityschedule_date ON ActivitySchedule(Date);
CREATE INDEX idx_activityschedule_driver ON ActivitySchedule(ScheduledDriverID);
CREATE INDEX idx_activityschedule_vehicle ON ActivitySchedule(ScheduledVehicleID);

-- Insert some sample data for testing
INSERT INTO Vehicles (VehicleNumber, BusNumber, Make, Model, Year, SeatingCapacity, FuelType, Status) VALUES
('001', 'Bus 1', 'Blue Bird', 'Vision', 2018, 72, 'Diesel', 'Active'),
('002', 'Bus 2', 'International', 'CE', 2019, 72, 'Diesel', 'Active'),
('003', 'Bus 3', 'Blue Bird', 'Vision', 2020, 72, 'Diesel', 'Maintenance');

INSERT INTO Drivers (DriverName, DriverPhone, DriverEmail, DriversLicenseType, TrainingComplete) VALUES
('John Smith', '555-0101', 'john.smith@school.edu', 'CDL', 1),
('Jane Doe', '555-0102', 'jane.doe@school.edu', 'CDL', 1),
('Bob Johnson', '555-0103', 'bob.johnson@school.edu', 'Passenger', 0);
