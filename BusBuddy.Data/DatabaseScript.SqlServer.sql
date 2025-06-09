-- BusBuddy Enhanced Database Schema SQL Script for SQL Server

-- Vehicles Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Vehicles')
CREATE TABLE Vehicles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    VehicleNumber NVARCHAR(MAX) NOT NULL,
    BusNumber NVARCHAR(MAX),
    Make NVARCHAR(MAX),
    Model NVARCHAR(MAX),
    Year INT,
    SeatingCapacity INT,
    VINNumber NVARCHAR(MAX),
    LicenseNumber NVARCHAR(MAX),
    DateLastInspection NVARCHAR(MAX),
    FuelType NVARCHAR(MAX),
    Status NVARCHAR(MAX),
    Notes NVARCHAR(MAX)
);

-- Drivers Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Drivers')
CREATE TABLE Drivers (
    DriverID INT IDENTITY(1,1) PRIMARY KEY,
    DriverName NVARCHAR(MAX),
    DriverPhone NVARCHAR(MAX),
    DriverEmail NVARCHAR(MAX),
    Address NVARCHAR(MAX),
    City NVARCHAR(MAX),
    State NVARCHAR(MAX),
    Zip NVARCHAR(MAX),
    DriversLicenseType NVARCHAR(MAX),
    TrainingComplete INT,
    Notes NVARCHAR(MAX)
);

-- Routes Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Routes')
CREATE TABLE Routes (
    RouteID INT IDENTITY(1,1) PRIMARY KEY,
    Date NVARCHAR(MAX) NOT NULL,
    RouteName NVARCHAR(MAX),
    AMVehicleID INT,
    AMBeginMiles DECIMAL(18,2),
    AMEndMiles DECIMAL(18,2),
    AMRiders INT,
    AMDriverID INT,
    PMVehicleID INT,
    PMBeginMiles DECIMAL(18,2),
    PMEndMiles DECIMAL(18,2),
    PMRiders INT,
    PMDriverID INT,
    Notes NVARCHAR(MAX)
);

-- Add foreign keys for Routes
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_Routes_AMVehicleID_Vehicles') AND parent_object_id = OBJECT_ID(N'Routes'))
ALTER TABLE Routes ADD CONSTRAINT FK_Routes_AMVehicleID_Vehicles FOREIGN KEY (AMVehicleID) REFERENCES Vehicles(Id);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_Routes_AMDriverID_Drivers') AND parent_object_id = OBJECT_ID(N'Routes'))
ALTER TABLE Routes ADD CONSTRAINT FK_Routes_AMDriverID_Drivers FOREIGN KEY (AMDriverID) REFERENCES Drivers(DriverID);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_Routes_PMVehicleID_Vehicles') AND parent_object_id = OBJECT_ID(N'Routes'))
ALTER TABLE Routes ADD CONSTRAINT FK_Routes_PMVehicleID_Vehicles FOREIGN KEY (PMVehicleID) REFERENCES Vehicles(Id);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_Routes_PMDriverID_Drivers') AND parent_object_id = OBJECT_ID(N'Routes'))
ALTER TABLE Routes ADD CONSTRAINT FK_Routes_PMDriverID_Drivers FOREIGN KEY (PMDriverID) REFERENCES Drivers(DriverID);

-- Activities Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Activities')
CREATE TABLE Activities (
    ActivityID INT IDENTITY(1,1) PRIMARY KEY,
    Date NVARCHAR(MAX),
    ActivityType NVARCHAR(MAX),
    Destination NVARCHAR(MAX),
    LeaveTime NVARCHAR(MAX),
    EventTime NVARCHAR(MAX),
    ReturnTime NVARCHAR(MAX),
    RequestedBy NVARCHAR(MAX),
    AssignedVehicleID INT,
    DriverID INT,
    Notes NVARCHAR(MAX)
);

-- Add foreign keys for Activities
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_Activities_AssignedVehicleID_Vehicles') AND parent_object_id = OBJECT_ID(N'Activities'))
ALTER TABLE Activities ADD CONSTRAINT FK_Activities_AssignedVehicleID_Vehicles FOREIGN KEY (AssignedVehicleID) REFERENCES Vehicles(Id);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_Activities_DriverID_Drivers') AND parent_object_id = OBJECT_ID(N'Activities'))
ALTER TABLE Activities ADD CONSTRAINT FK_Activities_DriverID_Drivers FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID);

-- Fuel Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Fuel')
CREATE TABLE Fuel (
    FuelID INT IDENTITY(1,1) PRIMARY KEY,
    FuelDate NVARCHAR(MAX),
    FuelLocation NVARCHAR(MAX),
    VehicleFueledID INT,
    VehicleOdometerReading DECIMAL(18,2),
    FuelType NVARCHAR(MAX),
    FuelAmount DECIMAL(18,2),
    FuelCost DECIMAL(18,2),
    Notes NVARCHAR(MAX)
);

-- Add foreign key for Fuel
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_Fuel_VehicleFueledID_Vehicles') AND parent_object_id = OBJECT_ID(N'Fuel'))
ALTER TABLE Fuel ADD CONSTRAINT FK_Fuel_VehicleFueledID_Vehicles FOREIGN KEY (VehicleFueledID) REFERENCES Vehicles(Id);

-- Maintenance Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Maintenance')
CREATE TABLE Maintenance (
    MaintenanceID INT IDENTITY(1,1) PRIMARY KEY,
    Date NVARCHAR(MAX),
    VehicleID INT,
    OdometerReading DECIMAL(18,2),
    MaintenanceCompleted NVARCHAR(MAX),
    Vendor NVARCHAR(MAX),
    RepairCost DECIMAL(18,2),
    Notes NVARCHAR(MAX)
);

-- Add foreign key for Maintenance
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_Maintenance_VehicleID_Vehicles') AND parent_object_id = OBJECT_ID(N'Maintenance'))
ALTER TABLE Maintenance ADD CONSTRAINT FK_Maintenance_VehicleID_Vehicles FOREIGN KEY (VehicleID) REFERENCES Vehicles(Id);

-- School Calendar Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'SchoolCalendar')
CREATE TABLE SchoolCalendar (
    CalendarID INT IDENTITY(1,1) PRIMARY KEY,
    Date NVARCHAR(MAX),
    EndDate NVARCHAR(MAX),
    Category NVARCHAR(MAX),
    Description NVARCHAR(MAX),
    RouteNeeded INT,
    Notes NVARCHAR(MAX)
);

-- Activity Schedule Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'ActivitySchedule')
CREATE TABLE ActivitySchedule (
    ScheduleID INT IDENTITY(1,1) PRIMARY KEY,
    Date NVARCHAR(MAX),
    TripType NVARCHAR(MAX),
    ScheduledVehicleID INT,
    ScheduledDestination NVARCHAR(MAX),
    ScheduledLeaveTime NVARCHAR(MAX),
    ScheduledEventTime NVARCHAR(MAX),
    ScheduledReturnTime NVARCHAR(MAX),
    ScheduledRiders INT,
    ScheduledDriverID INT,
    Notes NVARCHAR(MAX)
);

-- Add foreign keys for ActivitySchedule
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_ActivitySchedule_ScheduledVehicleID_Vehicles') AND parent_object_id = OBJECT_ID(N'ActivitySchedule'))
ALTER TABLE ActivitySchedule ADD CONSTRAINT FK_ActivitySchedule_ScheduledVehicleID_Vehicles FOREIGN KEY (ScheduledVehicleID) REFERENCES Vehicles(Id);

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_ActivitySchedule_ScheduledDriverID_Drivers') AND parent_object_id = OBJECT_ID(N'ActivitySchedule'))
ALTER TABLE ActivitySchedule ADD CONSTRAINT FK_ActivitySchedule_ScheduledDriverID_Drivers FOREIGN KEY (ScheduledDriverID) REFERENCES Drivers(DriverID);

-- Time Card Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'TimeCard')
CREATE TABLE TimeCard (
    TimeCardID INT IDENTITY(1,1) PRIMARY KEY,
    DriverID INT,
    Date NVARCHAR(MAX),
    DayType NVARCHAR(MAX),
    AMClockIn NVARCHAR(MAX),
    LunchClockOut NVARCHAR(MAX),
    LunchClockIn NVARCHAR(MAX),
    PMClockOut NVARCHAR(MAX),
    RouteAMClockOut NVARCHAR(MAX),
    RouteAMClockIn NVARCHAR(MAX),
    RoutePMClockOut NVARCHAR(MAX),
    RoutePMClockIn NVARCHAR(MAX),
    TotalTime DECIMAL(18,2),
    Overtime DECIMAL(18,2),
    WeeklyTotal DECIMAL(18,2),
    MonthlyTotal DECIMAL(18,2),
    Notes NVARCHAR(MAX)
);

-- Add foreign key for TimeCard
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'FK_TimeCard_DriverID_Drivers') AND parent_object_id = OBJECT_ID(N'TimeCard'))
ALTER TABLE TimeCard ADD CONSTRAINT FK_TimeCard_DriverID_Drivers FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID);

-- Create indexes for common queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_routes_date' AND object_id = OBJECT_ID(N'Routes'))
CREATE INDEX idx_routes_date ON Routes(Date);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_routes_driver' AND object_id = OBJECT_ID(N'Routes'))
CREATE INDEX idx_routes_driver ON Routes(AMDriverID, PMDriverID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_routes_vehicle' AND object_id = OBJECT_ID(N'Routes'))
CREATE INDEX idx_routes_vehicle ON Routes(AMVehicleID, PMVehicleID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_activities_date' AND object_id = OBJECT_ID(N'Activities'))
CREATE INDEX idx_activities_date ON Activities(Date);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_activities_driver' AND object_id = OBJECT_ID(N'Activities'))
CREATE INDEX idx_activities_driver ON Activities(DriverID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_activities_vehicle' AND object_id = OBJECT_ID(N'Activities'))
CREATE INDEX idx_activities_vehicle ON Activities(AssignedVehicleID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_fuel_date' AND object_id = OBJECT_ID(N'Fuel'))
CREATE INDEX idx_fuel_date ON Fuel(FuelDate);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_fuel_vehicle' AND object_id = OBJECT_ID(N'Fuel'))
CREATE INDEX idx_fuel_vehicle ON Fuel(VehicleFueledID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_maintenance_date' AND object_id = OBJECT_ID(N'Maintenance'))
CREATE INDEX idx_maintenance_date ON Maintenance(Date);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_maintenance_vehicle' AND object_id = OBJECT_ID(N'Maintenance'))
CREATE INDEX idx_maintenance_vehicle ON Maintenance(VehicleID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_calendar_date' AND object_id = OBJECT_ID(N'SchoolCalendar'))
CREATE INDEX idx_calendar_date ON SchoolCalendar(Date);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_calendar_enddate' AND object_id = OBJECT_ID(N'SchoolCalendar'))
CREATE INDEX idx_calendar_enddate ON SchoolCalendar(EndDate);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_calendar_category' AND object_id = OBJECT_ID(N'SchoolCalendar'))
CREATE INDEX idx_calendar_category ON SchoolCalendar(Category);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_activityschedule_date' AND object_id = OBJECT_ID(N'ActivitySchedule'))
CREATE INDEX idx_activityschedule_date ON ActivitySchedule(Date);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_activityschedule_driver' AND object_id = OBJECT_ID(N'ActivitySchedule'))
CREATE INDEX idx_activityschedule_driver ON ActivitySchedule(ScheduledDriverID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_activityschedule_vehicle' AND object_id = OBJECT_ID(N'ActivitySchedule'))
CREATE INDEX idx_activityschedule_vehicle ON ActivitySchedule(ScheduledVehicleID);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_timecard_date' AND object_id = OBJECT_ID(N'TimeCard'))
CREATE INDEX idx_timecard_date ON TimeCard(Date);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_timecard_daytype' AND object_id = OBJECT_ID(N'TimeCard'))
CREATE INDEX idx_timecard_daytype ON TimeCard(DayType);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'idx_timecard_driver' AND object_id = OBJECT_ID(N'TimeCard'))
CREATE INDEX idx_timecard_driver ON TimeCard(DriverID);
