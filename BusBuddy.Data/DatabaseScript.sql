-- BusBuddy Database Schema
--
-- Maintained for test and production compatibility
--
-- Last updated: June 12, 2025
--
-- 13 tables, including Vehicles (with FuelType, Status columns), Fuel, etc.
--
-- To update test data, see TestSeedData.sql and BusBuddy.Tests/TestBase.cs

-- NOTE: This script is for SQLite only. For SQL Server, use DatabaseScript.SqlServer.sql.
--
-- SQLite supports 'CREATE TABLE IF NOT EXISTS', but SQL Server does not.
-- If you see errors about 'IF NOT EXISTS', you are using the wrong script for SQL Server.
--
-- To initialize the test database, use this file with SQLite3.

-- BusBuddy Enhanced Database Schema SQL Script (SQLite3 compatible)

-- Vehicles Table
CREATE TABLE IF NOT EXISTS Vehicles (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    VehicleNumber TEXT NOT NULL,
    BusNumber TEXT,
    Make TEXT,
    Model TEXT,
    Year INTEGER,
    SeatingCapacity INTEGER,
    VINNumber TEXT,
    LicenseNumber TEXT,
    DateLastInspection TEXT,
    Notes TEXT,
    FuelType TEXT, -- Added for schema compatibility
    Status TEXT    -- Added for schema compatibility
);

-- Drivers Table
CREATE TABLE IF NOT EXISTS Drivers (
    DriverID INTEGER PRIMARY KEY AUTOINCREMENT,
    DriverName TEXT,
    DriverPhone TEXT,
    DriverEmail TEXT,
    Address TEXT,
    City TEXT,
    State TEXT,
    Zip TEXT,
    DriversLicenseType TEXT,
    TrainingComplete INTEGER,
    Notes TEXT
);

-- Routes Table
CREATE TABLE IF NOT EXISTS Routes (
    RouteID INTEGER PRIMARY KEY AUTOINCREMENT,
    Date TEXT NOT NULL,
    RouteName TEXT,
    AMVehicleID INTEGER,
    AMBeginMiles REAL,
    AMEndMiles REAL,
    AMRiders INTEGER,
    AMDriverID INTEGER,
    PMVehicleID INTEGER,
    PMBeginMiles REAL,
    PMEndMiles REAL,
    PMRiders INTEGER,
    PMDriverID INTEGER,
    Notes TEXT,
    FOREIGN KEY (AMVehicleID) REFERENCES Vehicles(Id),
    FOREIGN KEY (AMDriverID) REFERENCES Drivers(DriverID),
    FOREIGN KEY (PMVehicleID) REFERENCES Vehicles(Id),
    FOREIGN KEY (PMDriverID) REFERENCES Drivers(DriverID)
);

-- Activities Table
CREATE TABLE IF NOT EXISTS Activities (
    ActivityID INTEGER PRIMARY KEY AUTOINCREMENT,
    Date TEXT,
    ActivityType TEXT,
    Destination TEXT,
    LeaveTime TEXT,
    EventTime TEXT,
    ReturnTime TEXT,
    RequestedBy TEXT,
    AssignedVehicleID INTEGER,
    DriverID INTEGER,
    Notes TEXT,
    FOREIGN KEY (AssignedVehicleID) REFERENCES Vehicles(Id),
    FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID)
);

-- Fuel Table
CREATE TABLE IF NOT EXISTS Fuel (
    FuelID INTEGER PRIMARY KEY AUTOINCREMENT,
    FuelDate TEXT,
    FuelLocation TEXT,
    VehicleFueledID INTEGER,
    VehicleOdometerReading REAL,
    FuelType TEXT,
    FuelAmount REAL,
    FuelCost REAL,
    Notes TEXT,
    FOREIGN KEY (VehicleFueledID) REFERENCES Vehicles(Id)
);

-- Maintenance Table
CREATE TABLE IF NOT EXISTS Maintenance (
    MaintenanceID INTEGER PRIMARY KEY AUTOINCREMENT,
    Date TEXT,
    VehicleID INTEGER,
    OdometerReading REAL,
    MaintenanceCompleted TEXT,
    Vendor TEXT,
    RepairCost REAL,
    Notes TEXT,
    FOREIGN KEY (VehicleID) REFERENCES Vehicles(Id)
);

-- School Calendar Table
CREATE TABLE IF NOT EXISTS SchoolCalendar (
    CalendarID INTEGER PRIMARY KEY AUTOINCREMENT,
    Date TEXT,
    EndDate TEXT,
    Category TEXT,
    Description TEXT,
    RouteNeeded INTEGER,
    Notes TEXT
);

-- Activity Schedule Table
CREATE TABLE IF NOT EXISTS ActivitySchedule (
    ScheduleID INTEGER PRIMARY KEY AUTOINCREMENT,
    Date TEXT,
    TripType TEXT,
    ScheduledVehicleID INTEGER,
    ScheduledDestination TEXT,
    ScheduledLeaveTime TEXT,
    ScheduledEventTime TEXT,
    ScheduledReturnTime TEXT,
    ScheduledRiders INTEGER,
    ScheduledDriverID INTEGER,
    Notes TEXT,
    FOREIGN KEY (ScheduledVehicleID) REFERENCES Vehicles(Id),
    FOREIGN KEY (ScheduledDriverID) REFERENCES Drivers(DriverID)
);



-- Create indexes for common queries
CREATE INDEX IF NOT EXISTS idx_routes_date ON Routes(Date);
CREATE INDEX IF NOT EXISTS idx_routes_driver ON Routes(AMDriverID, PMDriverID);
CREATE INDEX IF NOT EXISTS idx_routes_vehicle ON Routes(AMVehicleID, PMVehicleID);

CREATE INDEX IF NOT EXISTS idx_activities_date ON Activities(Date);
CREATE INDEX IF NOT EXISTS idx_activities_driver ON Activities(DriverID);
CREATE INDEX IF NOT EXISTS idx_activities_vehicle ON Activities(AssignedVehicleID);

CREATE INDEX IF NOT EXISTS idx_fuel_date ON Fuel(FuelDate);
CREATE INDEX IF NOT EXISTS idx_fuel_vehicle ON Fuel(VehicleFueledID);

CREATE INDEX IF NOT EXISTS idx_maintenance_date ON Maintenance(Date);
CREATE INDEX IF NOT EXISTS idx_maintenance_vehicle ON Maintenance(VehicleID);

CREATE INDEX IF NOT EXISTS idx_calendar_date ON SchoolCalendar(Date);
CREATE INDEX IF NOT EXISTS idx_calendar_enddate ON SchoolCalendar(EndDate);
CREATE INDEX IF NOT EXISTS idx_calendar_category ON SchoolCalendar(Category);

CREATE INDEX IF NOT EXISTS idx_activityschedule_date ON ActivitySchedule(Date);
CREATE INDEX IF NOT EXISTS idx_activityschedule_driver ON ActivitySchedule(ScheduledDriverID);
CREATE INDEX IF NOT EXISTS idx_activityschedule_vehicle ON ActivitySchedule(ScheduledVehicleID);
