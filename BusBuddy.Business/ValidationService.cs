using System;
using System.Collections.Generic;
using System.Linq;
using BusBuddy.Data;
using BusBuddy.Models;

namespace BusBuddy.Business
{
    /// <summary>
    /// Centralized validation service for complex business rules and data integrity
    /// </summary>
    public class ValidationService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IMaintenanceRepository _maintenanceRepository;
        private readonly IFuelRepository _fuelRepository;

        public ValidationService(
            IVehicleRepository? vehicleRepository = null,
            IDriverRepository? driverRepository = null,
            IMaintenanceRepository? maintenanceRepository = null,
            IFuelRepository? fuelRepository = null)
        {
            _vehicleRepository = vehicleRepository ?? new VehicleRepository();
            _driverRepository = driverRepository ?? new DriverRepository();
            _maintenanceRepository = maintenanceRepository ?? new MaintenanceRepository();
            _fuelRepository = fuelRepository ?? new FuelRepository();
        }

        /// <summary>
        /// Validates that a vehicle exists and is available for operations
        /// </summary>
        public ValidationResult ValidateVehicleAvailability(int vehicleId, DateTime date, string operation = "")
        {
            var vehicle = _vehicleRepository.GetVehicleById(vehicleId);
            if (vehicle == null)
            {
                return ValidationResult.Failed($"Vehicle with ID {vehicleId} does not exist.");
            }

            if (vehicle.Status?.ToLower() == "out of service")
            {
                return ValidationResult.Failed($"Vehicle {vehicle.VehicleNumber} is currently out of service.");
            }

            // Check for maintenance conflicts
            var maintenanceRecords = _maintenanceRepository.GetMaintenanceByVehicle(vehicleId);
            var dateString = date.ToString("yyyy-MM-dd");
            var scheduledMaintenance = maintenanceRecords
                .Where(m => !string.IsNullOrEmpty(m.Date) && m.Date == dateString &&
                           m.Notes?.Contains("SCHEDULED") == true)
                .ToList();

            if (scheduledMaintenance.Any())
            {
                return ValidationResult.Failed($"Vehicle {vehicle.VehicleNumber} has scheduled maintenance on {date.ToShortDateString()}.");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// Validates that a driver exists and is available for operations
        /// </summary>
        public ValidationResult ValidateDriverAvailability(int driverId, DateTime date, string operation = "")
        {
            var driver = _driverRepository.GetDriverById(driverId);
            if (driver == null)
            {
                return ValidationResult.Failed($"Driver with ID {driverId} does not exist.");
            }

            if (driver.Status?.ToLower() == "inactive")
            {
                return ValidationResult.Failed($"Driver {driver.FirstName} {driver.LastName} is currently inactive.");
            }

            // Check for license expiration
            if (driver.CDLExpirationDate.HasValue && driver.CDLExpirationDate.Value < date)
            {
                return ValidationResult.Failed($"Driver {driver.FirstName} {driver.LastName}'s CDL expires on {driver.CDLExpirationDate.Value.ToShortDateString()}.");
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// Validates fuel record data integrity
        /// </summary>
        public ValidationResult ValidateFuelRecord(Fuel fuelRecord)
        {
            var validations = new List<ValidationResult>();

            // Check for null fuel record
            if (fuelRecord == null)
            {
                validations.Add(ValidationResult.Failed("Fuel record cannot be null."));
                return ValidationResult.Combine(validations);
            }

            // Validate required fields
            if (!fuelRecord.VehicleFueledID.HasValue)
            {
                validations.Add(ValidationResult.Failed("Vehicle ID is required for fuel record."));
            }

            if (string.IsNullOrWhiteSpace(fuelRecord.FuelDate))
            {
                validations.Add(ValidationResult.Failed("Fuel date is required for fuel record."));
            }

            if (string.IsNullOrWhiteSpace(fuelRecord.FuelLocation))
            {
                validations.Add(ValidationResult.Failed("Fuel location is required for fuel record."));
            }

            if (!fuelRecord.FuelAmount.HasValue || fuelRecord.FuelAmount <= 0)
            {
                validations.Add(ValidationResult.Failed("Fuel amount is required and must be greater than 0."));
            }

            // Validate vehicle exists (only if VehicleFueledID is provided)
            if (fuelRecord.VehicleFueledID.HasValue)
            {
                validations.Add(ValidateVehicleAvailability(fuelRecord.VehicleFueledID.Value,
                    fuelRecord.FuelDateAsDateTime ?? DateTime.Today, "fueling"));
            }

            // Validate odometer reading progression
            if (fuelRecord.VehicleFueledID.HasValue && fuelRecord.VehicleOdometerReading.HasValue)
            {
                var previousFuelRecords = _fuelRepository.GetFuelRecordsByVehicle(fuelRecord.VehicleFueledID.Value)
                    .Where(f => f.FuelDateAsDateTime < fuelRecord.FuelDateAsDateTime && f.VehicleOdometerReading.HasValue)
                    .OrderByDescending(f => f.FuelDateAsDateTime)
                    .Take(1);

                foreach (var previousRecord in previousFuelRecords)
                {
                    if (fuelRecord.VehicleOdometerReading < previousRecord.VehicleOdometerReading)
                    {
                        validations.Add(ValidationResult.Failed(
                            $"Odometer reading ({fuelRecord.VehicleOdometerReading}) cannot be less than previous reading ({previousRecord.VehicleOdometerReading}) from {previousRecord.FuelDateAsDateTime?.ToShortDateString() ?? previousRecord.FuelDate ?? "unknown date"}."));
                    }
                }
            }

            // Validate fuel amount and cost ranges
            if (fuelRecord.FuelAmount.HasValue && fuelRecord.FuelAmount > 200)
            {
                validations.Add(ValidationResult.Failed("Fuel amount cannot exceed 200 gallons."));
            }

            if (fuelRecord.FuelCost.HasValue && (fuelRecord.FuelCost <= 0 || fuelRecord.FuelCost > 1000))
            {
                validations.Add(ValidationResult.Failed("Fuel cost must be between $0 and $1000."));
            }

            return ValidationResult.Combine(validations);
        }

        /// <summary>
        /// Validates maintenance record data integrity
        /// </summary>
        public ValidationResult ValidateMaintenanceRecord(Maintenance maintenanceRecord)
        {
            var validations = new List<ValidationResult>();

            // Check for null maintenance record
            if (maintenanceRecord == null)
            {
                validations.Add(ValidationResult.Failed("Maintenance record cannot be null."));
                return ValidationResult.Combine(validations);
            }

            // Validate required fields
            if (!maintenanceRecord.VehicleID.HasValue)
            {
                validations.Add(ValidationResult.Failed("Vehicle ID is required for maintenance record."));
            }

            if (string.IsNullOrWhiteSpace(maintenanceRecord.Date))
            {
                validations.Add(ValidationResult.Failed("Date is required for maintenance record."));
            }

            if (string.IsNullOrWhiteSpace(maintenanceRecord.MaintenanceCompleted))
            {
                validations.Add(ValidationResult.Failed("Maintenance completed description is required."));
            }

            // Validate vehicle exists (only if VehicleID is provided)
            if (maintenanceRecord.VehicleID.HasValue)
            {
                validations.Add(ValidateVehicleAvailability(maintenanceRecord.VehicleID.Value,
                    maintenanceRecord.DateAsDateTime ?? DateTime.Today, "maintenance"));
            }

            // Validate cost
            if (maintenanceRecord.RepairCost.HasValue && maintenanceRecord.RepairCost < 0)
            {
                validations.Add(ValidationResult.Failed("Repair cost cannot be negative."));
            }

            // Validate odometer reading
            if (maintenanceRecord.OdometerReading.HasValue && maintenanceRecord.OdometerReading < 0)
            {
                validations.Add(ValidationResult.Failed("Odometer reading cannot be negative."));
            }

            return ValidationResult.Combine(validations);
        }

        /// <summary>
        /// Validates route assignment conflicts
        /// </summary>
        public ValidationResult ValidateRouteAssignment(Route route)
        {
            var validations = new List<ValidationResult>();

            // Validate AM assignments
            if (route.AMVehicleID.HasValue)
            {
                validations.Add(ValidateVehicleAvailability(route.AMVehicleID.Value, route.DateAsDateTime, "route assignment"));
            }

            if (route.AMDriverID.HasValue)
            {
                validations.Add(ValidateDriverAvailability(route.AMDriverID.Value, route.DateAsDateTime, "route assignment"));
            }

            // Validate PM assignments
            if (route.PMVehicleID.HasValue)
            {
                validations.Add(ValidateVehicleAvailability(route.PMVehicleID.Value, route.DateAsDateTime, "route assignment"));
            }

            if (route.PMDriverID.HasValue)
            {
                validations.Add(ValidateDriverAvailability(route.PMDriverID.Value, route.DateAsDateTime, "route assignment"));
            }

            // Validate mileage logic
            if (route.AMBeginMiles.HasValue && route.AMEndMiles.HasValue && route.AMEndMiles <= route.AMBeginMiles)
            {
                validations.Add(ValidationResult.Failed("AM ending miles must be greater than beginning miles."));
            }

            if (route.PMBeginMiles.HasValue && route.PMEndMiles.HasValue && route.PMEndMiles <= route.PMBeginMiles)
            {
                validations.Add(ValidationResult.Failed("PM ending miles must be greater than beginning miles."));
            }

            return ValidationResult.Combine(validations);
        }

        /// <summary>
        /// Validates a vehicle number for correct format and length
        /// </summary>
        public bool IsValidVehicleNumber(string? vehicleNumber)
        {
            // Accepts formats like BUS-123, BB-456, etc. (letters, dash, numbers, min 3 chars before dash)
            if (string.IsNullOrWhiteSpace(vehicleNumber))
                return false;
            // Example pattern: 2-6 uppercase letters, dash, 1-4 digits
            return System.Text.RegularExpressions.Regex.IsMatch(vehicleNumber, @"^[A-Z]{2,6}-\d{1,4}$");
        }
    }

    /// <summary>
    /// Represents the result of a validation operation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public List<string> Errors { get; private set; }

        private ValidationResult(bool isValid, IEnumerable<string>? errors = null)
        {
            IsValid = isValid;
            Errors = errors?.ToList() ?? new List<string>();
        }

        public static ValidationResult Success()
        {
            return new ValidationResult(true);
        }

        public static ValidationResult Failed(string error)
        {
            return new ValidationResult(false, new[] { error });
        }

        public static ValidationResult Failed(IEnumerable<string> errors)
        {
            return new ValidationResult(false, errors);
        }

        public static ValidationResult Combine(IEnumerable<ValidationResult> results)
        {
            var allResults = results.ToList();
            var allErrors = allResults.SelectMany(r => r.Errors).ToList();
            var isValid = allResults.All(r => r.IsValid);

            return new ValidationResult(isValid, allErrors);
        }

        public string GetErrorMessage()
        {
            return string.Join("\n", Errors);
        }
    }
}
