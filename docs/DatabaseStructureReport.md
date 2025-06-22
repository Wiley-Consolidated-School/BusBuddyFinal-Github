# BusBuddy Database Structure Report
Generated on: June 22, 2025 at 03:56 AM MDT

## Database Configuration

### Connection Strings
- **Production Database**: `Server=.\SQLEXPRESS01;Database=BusBuddy;Trusted_Connection=True;TrustServerCertificate=True;`
- **Test Database**: `Server=.\SQLEXPRESS01;Database=BusBuddy_Test;Trusted_Connection=True;TrustServerCertificate=True;`
- **Provider**: Microsoft.Data.SqlClient (SQL Server Express)

### Database Files
- **Schema Script**: `BusBuddy.Data\DatabaseScript.SqlServer.sql` ✅ EXISTS
- **Test Seed Data**: `BusBuddy.Data\TestSeedData.sql` ✅ EXISTS
- **Entity Framework Context**: `BusBuddy.Data\BusBuddyContext.cs` ✅ EXISTS

## Database Schema Overview

### Total Tables: 8
1. **Vehicles** (Parent table)
2. **Drivers** (Parent table) 
3. **Routes** (References Vehicles, Drivers)
4. **Activities** (References Vehicles, Drivers)
5. **ActivitySchedule** (References Vehicles, Drivers)
6. **Fuel** (References Vehicles)
7. **Maintenance** (References Vehicles)
8. **SchoolCalendar** (Independent)

---

## Table Structures

### 1. Vehicles Table
**Purpose**: Fleet management and vehicle tracking
**Primary Key**: `Id` (INT IDENTITY)

| Field Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| Id | INT IDENTITY(1,1) | PRIMARY KEY | Auto-incrementing vehicle ID |
| VehicleNumber | NVARCHAR(50) | NOT NULL | Fleet number (e.g., "001") |
| BusNumber | NVARCHAR(50) | NULL | Bus identification number |
| Make | NVARCHAR(50) | NULL | Vehicle manufacturer |
| Model | NVARCHAR(50) | NULL | Vehicle model |
| Year | INT | NULL | Manufacturing year |
| SeatingCapacity | INT | NULL | Passenger seating capacity |
| VINNumber | NVARCHAR(100) | NULL | Vehicle identification number |
| LicenseNumber | NVARCHAR(50) | NULL | License plate number |
| DateLastInspection | NVARCHAR(50) | NULL | Last inspection date |
| Notes | NVARCHAR(MAX) | NULL | Additional notes |
| FuelType | NVARCHAR(50) | NULL | Fuel type (Diesel, Gas, etc.) |
| Status | NVARCHAR(50) | NULL | Vehicle status (Active, Maintenance, etc.) |

**Referenced By**: Routes (AMVehicleID, PMVehicleID), Activities (AssignedVehicleID), ActivitySchedule (ScheduledVehicleID), Fuel (VehicleFueledID), Maintenance (VehicleID)

---

### 2. Drivers Table
**Purpose**: Driver management and certification tracking
**Primary Key**: `DriverID` (INT IDENTITY)

| Field Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| DriverID | INT IDENTITY(1,1) | PRIMARY KEY | Auto-incrementing driver ID |
| DriverName | NVARCHAR(100) | NULL | Full driver name (computed) |
| DriverPhone | NVARCHAR(20) | NULL | Driver phone number |
| DriverEmail | NVARCHAR(100) | NULL | Driver email address |
| Address | NVARCHAR(200) | NULL | Home address |
| City | NVARCHAR(50) | NULL | City |
| State | NVARCHAR(20) | NULL | State abbreviation |
| Zip | NVARCHAR(10) | NULL | ZIP code |
| DriversLicenseType | NVARCHAR(50) | NULL | License type (CDL, Passenger) |
| TrainingComplete | INT | NOT NULL DEFAULT 0 | Training status (0=No, 1=Yes) |
| Notes | NVARCHAR(MAX) | NULL | Additional notes |
| Status | NVARCHAR(50) | NULL | Driver status (Active, Inactive, etc.) |
| FirstName | NVARCHAR(100) | NULL | First name |
| LastName | NVARCHAR(100) | NULL | Last name |
| CDLExpirationDate | DATETIME | NULL | CDL expiration date |

**Referenced By**: Routes (AMDriverID, PMDriverID), Activities (DriverID), ActivitySchedule (ScheduledDriverID)

**Missing Field**: `RouteType` (needed for driver pay calculations - CDL, SmallBus, SPED)

---

### 3. Routes Table
**Purpose**: Daily route management and mileage tracking
**Primary Key**: `RouteID` (INT IDENTITY)

| Field Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| RouteID | INT IDENTITY(1,1) | PRIMARY KEY | Auto-incrementing route ID |
| Date | NVARCHAR(50) | NOT NULL | Route date |
| RouteName | NVARCHAR(100) | NULL | Route name/identifier |
| AMVehicleID | INT | NULL | Morning vehicle assignment |
| AMBeginMiles | FLOAT | NULL | Morning starting mileage |
| AMEndMiles | FLOAT | NULL | Morning ending mileage |
| AMRiders | INT | NULL | Morning passenger count |
| AMDriverID | INT | NULL | Morning driver assignment |
| PMVehicleID | INT | NULL | Afternoon vehicle assignment |
| PMBeginMiles | FLOAT | NULL | Afternoon starting mileage |
| PMEndMiles | FLOAT | NULL | Afternoon ending mileage |
| PMRiders | INT | NULL | Afternoon passenger count |
| PMDriverID | INT | NULL | Afternoon driver assignment |
| Notes | NVARCHAR(MAX) | NULL | Additional notes |

**Foreign Keys**:
- AMVehicleID → Vehicles(Id)
- AMDriverID → Drivers(DriverID)
- PMVehicleID → Vehicles(Id)
- PMDriverID → Drivers(DriverID)

**Missing Field**: `RouteType` (NVARCHAR(50)) - Required for driver pay calculations

---

### 4. Activities Table
**Purpose**: Special transportation requests and activities
**Primary Key**: `ActivityID` (INT IDENTITY)

| Field Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| ActivityID | INT IDENTITY(1,1) | PRIMARY KEY | Auto-incrementing activity ID |
| Date | NVARCHAR(50) | NULL | Activity date |
| ActivityType | NVARCHAR(100) | NULL | Type of activity |
| Destination | NVARCHAR(200) | NULL | Activity destination |
| LeaveTime | NVARCHAR(50) | NULL | Departure time |
| EventTime | NVARCHAR(50) | NULL | Event start time |
| ReturnTime | NVARCHAR(50) | NULL | Return time |
| RequestedBy | NVARCHAR(100) | NULL | Person requesting activity |
| AssignedVehicleID | INT | NULL | Assigned vehicle |
| DriverID | INT | NULL | Assigned driver |
| Notes | NVARCHAR(MAX) | NULL | Additional notes |

**Foreign Keys**:
- AssignedVehicleID → Vehicles(Id)
- DriverID → Drivers(DriverID)

---

### 5. ActivitySchedule Table
**Purpose**: Scheduled transportation activities
**Primary Key**: `ScheduleID` (INT IDENTITY)

| Field Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| ScheduleID | INT IDENTITY(1,1) | PRIMARY KEY | Auto-incrementing schedule ID |
| Date | NVARCHAR(50) | NULL | Scheduled date |
| TripType | NVARCHAR(100) | NULL | Type of trip |
| ScheduledVehicleID | INT | NULL | Scheduled vehicle |
| ScheduledDestination | NVARCHAR(200) | NULL | Destination |
| ScheduledLeaveTime | NVARCHAR(50) | NULL | Scheduled departure |
| ScheduledEventTime | NVARCHAR(50) | NULL | Scheduled event time |
| ScheduledReturnTime | NVARCHAR(50) | NULL | Scheduled return |
| ScheduledRiders | INT | NULL | Expected riders |
| ScheduledDriverID | INT | NULL | Scheduled driver |
| Notes | NVARCHAR(MAX) | NULL | Additional notes |

**Foreign Keys**:
- ScheduledVehicleID → Vehicles(Id)
- ScheduledDriverID → Drivers(DriverID)

---

### 6. Fuel Table
**Purpose**: Fuel consumption and cost tracking
**Primary Key**: `FuelID` (INT IDENTITY)

| Field Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| FuelID | INT IDENTITY(1,1) | PRIMARY KEY | Auto-incrementing fuel record ID |
| FuelDate | NVARCHAR(50) | NULL | Fuel purchase date |
| FuelLocation | NVARCHAR(200) | NULL | Fuel station location |
| VehicleFueledID | INT | NULL | Vehicle that was fueled |
| VehicleOdometerReading | FLOAT | NULL | Odometer reading at fuel |
| FuelType | NVARCHAR(50) | NULL | Type of fuel |
| FuelAmount | FLOAT | NULL | Amount of fuel (gallons) |
| FuelCost | FLOAT | NULL | Cost of fuel |
| Notes | NVARCHAR(MAX) | NULL | Additional notes |

**Foreign Keys**:
- VehicleFueledID → Vehicles(Id)

---

### 7. Maintenance Table
**Purpose**: Vehicle maintenance and repair tracking
**Primary Key**: `MaintenanceID` (INT IDENTITY)

| Field Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| MaintenanceID | INT IDENTITY(1,1) | PRIMARY KEY | Auto-incrementing maintenance ID |
| Date | NVARCHAR(50) | NULL | Maintenance date |
| VehicleID | INT | NULL | Vehicle serviced |
| OdometerReading | FLOAT | NULL | Odometer reading |
| MaintenanceCompleted | NVARCHAR(MAX) | NULL | Work performed |
| Vendor | NVARCHAR(200) | NULL | Service vendor |
| RepairCost | FLOAT | NULL | Cost of repairs |
| Notes | NVARCHAR(MAX) | NULL | Additional notes |

**Foreign Keys**:
- VehicleID → Vehicles(Id)

---

### 8. SchoolCalendar Table
**Purpose**: School calendar and route scheduling
**Primary Key**: `CalendarID` (INT IDENTITY)

| Field Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| CalendarID | INT IDENTITY(1,1) | PRIMARY KEY | Auto-incrementing calendar ID |
| Date | NVARCHAR(50) | NULL | Calendar date |
| EndDate | NVARCHAR(50) | NULL | End date (for ranges) |
| Category | NVARCHAR(100) | NULL | Event category |
| Description | NVARCHAR(MAX) | NULL | Event description |
| RouteNeeded | INT | NULL | Whether routes are needed |
| Notes | NVARCHAR(MAX) | NULL | Additional notes |

**No Foreign Keys** (Independent table)

---

## Database Indexes

### Performance Optimization Indexes
- **Routes**: idx_routes_date, idx_routes_driver, idx_routes_vehicle
- **Activities**: idx_activities_date, idx_activities_driver, idx_activities_vehicle
- **Fuel**: idx_fuel_date, idx_fuel_vehicle
- **Maintenance**: idx_maintenance_date, idx_maintenance_vehicle
- **SchoolCalendar**: idx_calendar_date, idx_calendar_enddate, idx_calendar_category
- **ActivitySchedule**: idx_activityschedule_date, idx_activityschedule_driver, idx_activityschedule_vehicle

---

## Sample Data Included

### Vehicles (3 records)
- Bus 1 (001) - Blue Bird Vision 2018, 72 seats, Active
- Bus 2 (002) - International CE 2019, 72 seats, Active  
- Bus 3 (003) - Blue Bird Vision 2020, 72 seats, Maintenance

### Drivers (3 records)
- John Smith - CDL, Training Complete
- Jane Doe - CDL, Training Complete
- Bob Johnson - Passenger License, Training Incomplete

---

## Required Updates for Driver Pay System

### 1. Routes Table Update
**Missing Field**: `RouteType` NVARCHAR(50)
**Required Values**: "CDL", "SmallBus", "SPED"
**Purpose**: Driver pay calculation based on route type

### 2. Pay Rate Configuration
**File Location**: `BusBuddy.UI/Resources/payrates.json`
**Structure**: 
```json
{
  "CDLTripRate": 33.00,
  "SmallBusTripRate": 15.00,
  "SPEDDayRate": 66.00
}
```

### 3. Additional Management Forms
**Required**: PayRatesForm for pay scheme configuration
**Existing**: 8 of 9 management forms implemented

---

## Database Health Status

✅ **Schema File**: Located and valid  
✅ **Connection Strings**: Configured for SQL Server Express  
✅ **Foreign Key Relationships**: Properly defined  
✅ **Indexes**: Performance optimized  
⚠️ **Missing Field**: Routes.RouteType (required for driver pay)  
⚠️ **Pay Configuration**: payrates.json not yet created  

**Recommendation**: Add RouteType field to Routes table before implementing driver pay calculations.
