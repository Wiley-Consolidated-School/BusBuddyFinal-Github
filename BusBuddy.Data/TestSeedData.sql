-- Insert dummy data for all main tables to support UI view tests
INSERT INTO Vehicles (VehicleNumber, BusNumber, Make, Model, Year, SeatingCapacity, VINNumber, LicenseNumber, DateLastInspection, Notes, FuelType, Status)
VALUES ('VN-001', 'B-001', 'Ford', 'Transit', 2020, 40, 'VIN123', 'LIC123', '2025-01-01', 'Test vehicle', 'Diesel', 'Active'),
       ('VN-002', 'B-002', 'Chevy', 'Express', 2019, 30, 'VIN124', 'LIC124', '2025-01-02', 'Test vehicle 2', 'Gasoline', 'Active'),
       ('VN-003', 'B-003', 'Bluebird', 'Vision', 2021, 50, 'VIN125', 'LIC125', '2025-01-03', 'Test vehicle 3', 'Diesel', 'Maintenance');

INSERT INTO Drivers (DriverName, DriverPhone, DriverEmail, Address, City, State, Zip, DriversLicenseType, TrainingComplete, Notes)
VALUES ('John Doe', '555-1234', 'john@example.com', '123 Main St', 'Testville', 'TS', '12345', 'A', 1, 'Test driver'),
       ('Jane Smith', '555-5678', 'jane@example.com', '456 Oak Ave', 'Testville', 'TS', '12345', 'B', 1, 'Test driver 2');

INSERT INTO Routes (Date, RouteName, AMVehicleID, AMBeginMiles, AMEndMiles, AMRiders, AMDriverID, PMVehicleID, PMBeginMiles, PMEndMiles, PMRiders, PMDriverID, Notes)
VALUES ('2025-06-01', 'Route 1', 1, 1000, 1050, 20, 1, 2, 1050, 1100, 18, 2, 'Test route'),
       ('2025-06-02', 'Route 2', 2, 2000, 2050, 22, 2, 3, 2050, 2100, 19, 1, 'Test route 2');

INSERT INTO Activities (Date, ActivityType, Destination, LeaveTime, EventTime, ReturnTime, RequestedBy, AssignedVehicleID, DriverID, Notes)
VALUES ('2025-06-01', 'Field Trip', 'Museum', '08:00', '10:00', '12:00', 'Ms. Smith', 1, 1, 'Test activity'),
       ('2025-06-02', 'Sports', 'Stadium', '09:00', '11:00', '13:00', 'Coach Lee', 2, 2, 'Test activity 2');

INSERT INTO Fuel (FuelDate, FuelLocation, VehicleFueledID, VehicleOdometerReading, FuelType, FuelAmount, FuelCost, Notes)
VALUES ('2025-06-01', 'Main Depot', 1, 1100, 'Diesel', 50, 150.00, 'Test fuel'),
       ('2025-06-02', 'Key Pumps', 2, 2100, 'Gasoline', 60, 180.00, 'Test fuel 2');

INSERT INTO Maintenance (Date, VehicleID, OdometerReading, MaintenanceCompleted, Vendor, RepairCost, Notes)
VALUES ('2025-06-01', 1, 1100, 'Oil Change', 'QuickLube', 75.00, 'Test maintenance'),
       ('2025-06-02', 2, 2100, 'Tire Rotation', 'TireShop', 50.00, 'Test maintenance 2');

INSERT INTO SchoolCalendar (Date, EndDate, Category, Description, RouteNeeded, Notes)
VALUES ('2025-06-01', '2025-06-01', 'School Day', 'Test day', 1, 'Test note'),
       ('2025-06-02', '2025-06-02', 'Holiday', 'Test holiday', 0, 'Test note 2');

INSERT INTO ActivitySchedule (Date, TripType, ScheduledVehicleID, ScheduledDestination, ScheduledLeaveTime, ScheduledEventTime, ScheduledReturnTime, ScheduledRiders, ScheduledDriverID, Notes)
VALUES ('2025-06-01', 'Field Trip', 1, 'Museum', '08:00', '10:00', '12:00', 30, 1, 'Test schedule'),
       ('2025-06-02', 'Sports', 2, 'Stadium', '09:00', '11:00', '13:00', 25, 2, 'Test schedule 2');
