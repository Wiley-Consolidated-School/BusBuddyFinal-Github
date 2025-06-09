-- BusBuddy Enhanced Database Schema SQL Script

-- Vehicles Table (existing but updated with new columns)
CREATE TABLE Vehicles (
    Id INTEGER PRIMARY KEY,
    VehicleNumber TEXT NOT NULL,
    BusNumber TEXT,
    Make TEXT,
    Model TEXT,
    Year INTEGER,
    SeatingCapacity INTEGER,
    VINNumber TEXT,
    LicenseNumber TEXT,
    DateLastInspection TEXT,
    FuelType TEXT,
    Status TEXT,
    Notes TEXT
);

-- Drivers Table
CREATE TABLE Drivers (
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
CREATE TABLE Routes (
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
CREATE TABLE Activities (
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
CREATE TABLE Fuel (
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
CREATE TABLE Maintenance (
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
CREATE TABLE SchoolCalendar (
    CalendarID INTEGER PRIMARY KEY AUTOINCREMENT,
    Date TEXT,
    EndDate TEXT,
    Category TEXT,
    Description TEXT,
    RouteNeeded INTEGER,
    Notes TEXT
);

-- Activity Schedule Table
CREATE TABLE ActivitySchedule (
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

-- Time Card Table
CREATE TABLE TimeCard (
    TimeCardID INTEGER PRIMARY KEY AUTOINCREMENT,
    DriverID INTEGER,
    Date TEXT,
    DayType TEXT,
    AMClockIn TEXT,
    LunchClockOut TEXT,
    LunchClockIn TEXT,
    PMClockOut TEXT,
    RouteAMClockOut TEXT,
    RouteAMClockIn TEXT,
    RoutePMClockOut TEXT,
    RoutePMClockIn TEXT,
    TotalTime REAL,
    Overtime REAL,
    WeeklyTotal REAL,
    MonthlyTotal REAL,
    Notes TEXT,
    FOREIGN KEY (DriverID) REFERENCES Drivers(DriverID)
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

CREATE INDEX IF NOT EXISTS idx_timecard_date ON TimeCard(Date);
CREATE INDEX IF NOT EXISTS idx_timecard_daytype ON TimeCard(DayType);
CREATE INDEX IF NOT EXISTS idx_timecard_driver ON TimeCard(DriverID);
