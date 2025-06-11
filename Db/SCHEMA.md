# Database Schema

## Vehicles
- Id: INT (Primary Key)
- VehicleNumber: NVARCHAR(MAX) / TEXT
- BusNumber: NVARCHAR(MAX) / TEXT
- Make: NVARCHAR(MAX) / TEXT
- Model: NVARCHAR(MAX) / TEXT
- Year: INT / INTEGER
- SeatingCapacity: INT / INTEGER
- VINNumber: NVARCHAR(MAX) / TEXT
- LicenseNumber: NVARCHAR(MAX) / TEXT
- DateLastInspection: NVARCHAR(MAX) / TEXT
- FuelType: NVARCHAR(MAX) / TEXT
- Status: NVARCHAR(MAX) / TEXT
- Notes: NVARCHAR(MAX) / TEXT

## Drivers
- DriverID: INT (Primary Key, AUTOINCREMENT)
- DriverName: NVARCHAR(MAX) / TEXT
- DriverPhone: NVARCHAR(MAX) / TEXT
- DriverEmail: NVARCHAR(MAX) / TEXT
- Address: NVARCHAR(MAX) / TEXT
- City: NVARCHAR(MAX) / TEXT
- State: NVARCHAR(MAX) / TEXT
- Zip: NVARCHAR(MAX) / TEXT
- DriversLicenseType: NVARCHAR(MAX) / TEXT
- TrainingComplete: INT / INTEGER
- Notes: NVARCHAR(MAX) / TEXT

## Routes
- RouteID: INT (Primary Key, AUTOINCREMENT)
- Date: NVARCHAR(MAX) / TEXT
- RouteName: NVARCHAR(MAX) / TEXT
- AMVehicleID: INT (Foreign Key to Vehicles.Id)
- AMBeginMiles: DECIMAL(18,2) / REAL
- AMEndMiles: DECIMAL(18,2) / REAL
- AMRiders: INT / INTEGER
- AMDriverID: INT (Foreign Key to Drivers.DriverID)
- PMVehicleID: INT (Foreign Key to Vehicles.Id)
- PMBeginMiles: DECIMAL(18,2) / REAL
- PMEndMiles: DECIMAL(18,2) / REAL
- PMRiders: INT / INTEGER
- PMDriverID: INT (Foreign Key to Drivers.DriverID)
- Notes: NVARCHAR(MAX) / TEXT

## Relationships
- Routes.AMVehicleID, Routes.PMVehicleID → Vehicles.Id
- Routes.AMDriverID, Routes.PMDriverID → Drivers.DriverID

## Additional tables
- See `BusBuddy.Data/DatabaseScript.sql` and `DatabaseScript.SqlServer.sql` for full schema (includes Activities, Fuel, Maintenance, SchoolCalendar, ActivitySchedule, TimeCard, etc.)

---

**ER Diagram:**
- (To be added: `ERD.png`)

---

**Note:**
- SQLite uses `TEXT`, `INTEGER`, `REAL` types; SQL Server uses `NVARCHAR(MAX)`, `INT`, `DECIMAL(18,2)`.
- Foreign key constraints are enforced in both scripts.
