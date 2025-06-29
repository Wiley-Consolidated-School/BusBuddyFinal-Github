using System;
using System.Collections.Generic;
using System.Linq;
using BusBuddy.Data;
using BusBuddy.Models;

namespace BusBuddy.Business
{
    public class ValidationService : IValidationService
    {
        private readonly BusRepository _busRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IRouteRepository _routeRepository;
        private readonly IMaintenanceRepository _maintenanceRepository;
        private readonly IFuelRepository _fuelRepository;

        public ValidationService(
            BusRepository busRepository,
            IDriverRepository driverRepository,
            IRouteRepository routeRepository,
            IMaintenanceRepository maintenanceRepository,
            IFuelRepository fuelRepository)
        {
            _busRepository = busRepository;
            _driverRepository = driverRepository;
            _routeRepository = routeRepository;
            _maintenanceRepository = maintenanceRepository;
            _fuelRepository = fuelRepository;
        }

        /// <summary>
        /// Validates complete vehicle assignment for a route
        /// </summary>
        public ValidationResult ValidateVehicleAssignment(int busId, DateTime date, string assignmentType = "route")
        {
            try
            {
                var validations = new List<ValidationResult>();
                var bus = _busRepository.GetBusById(busId);
                if (bus == null)
                {
                    return ValidationResult.Failed($"Vehicle with ID {busId} not found.");
                }

                // Validate vehicle status (removing Status check as it doesn't exist in Bus model)
                // Note: Status property was removed from Bus model, validation logic updated accordingly

                // Validate vehicle availability
                validations.Add(ValidateBusAvailability(busId, date, assignmentType));
                return ValidationResult.Combine(validations);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error validating vehicle assignment for vehicle {busId}: {ex.Message}");
                return ValidationResult.Failed($"Validation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates driver assignment including license and availability
        /// </summary>
        public ValidationResult ValidateDriverAssignment(int DriverId, DateTime date, string assignmentType = "route")
        {
            try
            {
                var validations = new List<ValidationResult>();
                var driver = _driverRepository.GetDriverById(DriverId);
                if (driver == null)
                {
                    return ValidationResult.Failed($"Driver with ID {DriverId} not found.");
                }

                // Validate driver qualifications
                if (string.IsNullOrWhiteSpace(driver.DriversLicenseType))
                {
                    validations.Add(ValidationResult.Failed($"Driver {driver.DriverName} does not have a valid license type."));
                }

                // Check if training is complete
                if (!driver.TrainingComplete)
                {
                    validations.Add(ValidationResult.Failed($"Driver {driver.DriverName} has not completed required training."));
                }

                // Validate driver availability
                validations.Add(ValidateDriverAvailability(DriverId, date, assignmentType));
                return ValidationResult.Combine(validations);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error validating driver assignment for driver {DriverId}: {ex.Message}");
                return ValidationResult.Failed($"Validation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Core vehicle availability validation
        /// </summary>
        public ValidationResult ValidateBusAvailability(int busId, DateTime date, string assignmentType = "general", int? currentRouteId = null)
        {
            try
            {
                // Check if bus exists
                var bus = _busRepository.GetBusById(busId);
                if (bus == null)
                {
                    return ValidationResult.Failed($"Bus with ID {busId} does not exist.");
                }

                // Check if vehicle is out of service (removing Status check as it doesn't exist in Bus model)
                // Note: Status property was removed from Bus model, validation logic updated accordingly

                // Get conflicting route assignments for the date
                var routes = _routeRepository.GetRoutesByDate(date);
                var conflictingRoutes = routes.Where(r =>
                    (r.AMBusId == busId || r.PMBusId == busId) &&
                    (currentRouteId == null || r.RouteId != currentRouteId)).ToList();
                if (conflictingRoutes.Any() && assignmentType != "route")
                {
                    var routeNames = string.Join(", ", conflictingRoutes.Select(r => r.RouteName));
                    return ValidationResult.Failed($"Vehicle is already assigned to route(s): {routeNames} on {date:yyyy-MM-dd}");
                }

                // Check for maintenance conflicts
                var maintenanceRecords = _maintenanceRepository.GetMaintenanceByBus(busId);
                var scheduledMaintenance = maintenanceRecords?.Where(m =>
                    m.DateAsDateTime?.Date == date.Date && m.Notes?.Contains("SCHEDULED") == true).ToList();
                if (scheduledMaintenance?.Any() == true)
                {
                    return ValidationResult.Failed($"Vehicle has scheduled maintenance on {date:yyyy-MM-dd}");
                }
                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                return ValidationResult.Failed($"Error validating vehicle availability: {ex.Message}");
            }
        }

        /// <summary>
        /// Core driver availability validation
        /// </summary>
        public ValidationResult ValidateDriverAvailability(int DriverId, DateTime date, string assignmentType = "general", int? currentRouteId = null)
        {
            try
            {
                // Check if driver exists
                var driver = _driverRepository.GetDriverById(DriverId);
                if (driver == null)
                {
                    return ValidationResult.Failed($"Driver with ID {DriverId} does not exist.");
                }

                // Check if driver is inactive (removing Status check as it doesn't exist in Driver model)
                // Note: Status property was removed from Driver model, validation logic updated accordingly

                // Get conflicting route assignments for the date
                var routes = _routeRepository.GetRoutesByDate(date);
                var conflictingRoutes = routes.Where(r =>
                    (r.AMDriverId == DriverId || r.PMDriverId == DriverId) &&
                    (currentRouteId == null || r.RouteId != currentRouteId)).ToList();
                if (conflictingRoutes.Any() && assignmentType != "route")
                {
                    var routeNames = string.Join(", ", conflictingRoutes.Select(r => r.RouteName));
                    return ValidationResult.Failed($"Driver is already assigned to route(s): {routeNames} on {date:yyyy-MM-dd}");
                }
                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                return ValidationResult.Failed($"Error validating driver availability: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates fuel record entry
        /// </summary>
        public ValidationResult ValidateFuelRecord(Fuel fuelRecord)
        {
            var errors = new List<string>();
            if (fuelRecord == null)
            {
                errors.Add("Fuel record cannot be null");
                return ValidationResult.Failed(errors);
            }
            if (!fuelRecord.VehicleFueledID.HasValue || fuelRecord.VehicleFueledID <= 0)
                errors.Add("Valid vehicle ID is required");
            if (!fuelRecord.FuelAmount.HasValue || fuelRecord.FuelAmount <= 0)
                errors.Add("Fuel gallons must be greater than zero - negative values are not allowed");
            if (!fuelRecord.FuelCost.HasValue || fuelRecord.FuelCost <= 0)
                errors.Add("Fuel cost must be greater than zero");
            if (fuelRecord.FuelDateAsDateTime > DateTime.Now)
                errors.Add("Fuel date cannot be in the future");
            return errors.Any() ? ValidationResult.Failed(errors) : ValidationResult.Success();
        }

        /// <summary>
        /// Validates maintenance record entry
        /// </summary>
        public ValidationResult ValidateMaintenanceRecord(Maintenance maintenanceRecord)
        {
            var validations = new List<ValidationResult>();
            // Check for null record first
            if (maintenanceRecord == null)
            {
                return ValidationResult.Failed("Maintenance record cannot be null.");
            }
            // Validate required fields
            if (!maintenanceRecord.BusId.HasValue)
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
            // Validate vehicle exists (only if BusId is provided)
            // Note: Skip availability check for scheduled maintenance as it would create a circular validation
            if (maintenanceRecord.BusId.HasValue &&
                !(maintenanceRecord.Notes?.Contains("SCHEDULED") == true))
            {
                validations.Add(ValidateBusAvailability(maintenanceRecord.BusId.Value,
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
            if (route.AMBusId.HasValue)
            {
                validations.Add(ValidateBusAvailability(route.AMBusId.Value, route.DateAsDateTime, "route assignment", route.RouteId));
            }
            if (route.AMDriverId.HasValue)
            {
                validations.Add(ValidateDriverAvailability(route.AMDriverId.Value, route.DateAsDateTime, "route assignment", route.RouteId));
            }
            // Validate PM assignments
            if (route.PMBusId.HasValue)
            {
                validations.Add(ValidateBusAvailability(route.PMBusId.Value, route.DateAsDateTime, "route assignment", route.RouteId));
            }
            if (route.PMDriverId.HasValue)
            {
                validations.Add(ValidateDriverAvailability(route.PMDriverId.Value, route.DateAsDateTime, "route assignment", route.RouteId));
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

        #region Interface Implementation Methods
        public bool IsValidBusNumber(string busNumber)
        {
            if (string.IsNullOrWhiteSpace(busNumber))
                return false;
            // Bus number should be 1-4 characters, alphanumeric
            return busNumber.Length <= 4 && busNumber.All(c => char.IsLetterOrDigit(c));
        }

        public bool IsBusMaintenanceRequired(int busId)
        {
            var bus = _busRepository.GetBusById(busId);
            if (bus == null) return false;
            // Check if maintenance is overdue
            var lastMaintenance = _maintenanceRepository.GetMaintenanceByBus(busId)
                .OrderByDescending(m => m.DateAsDateTime)
                .FirstOrDefault();
            if (lastMaintenance?.DateAsDateTime == null)
                return true; // No maintenance history
            return (DateTime.Now - lastMaintenance.DateAsDateTime.Value).Days > 90;
        }

        public bool IsFuelLevelCritical(int busId)
        {
            var latestFuel = _fuelRepository.GetFuelRecordsByBus(busId)
                .OrderByDescending(f => f.FuelDateAsDateTime)
                .FirstOrDefault();
            if (latestFuel?.FuelAmount == null)
                return true; // No fuel data is critical
            // Assume critical if less than 25% of typical fuel capacity
            return latestFuel.FuelAmount < 20; // Less than 20 gallons
        }

        public bool IsBusAvailable(int busId, DateTime startTime, DateTime endTime)
        {
            // Check if bus exists
            var bus = _busRepository.GetBusById(busId);
            if (bus == null) return false;
            // Check maintenance schedule conflicts
            var maintenanceConflicts = _maintenanceRepository.GetMaintenanceByBus(busId)
                .Where(m => m.DateAsDateTime >= startTime.Date && m.DateAsDateTime <= endTime.Date);
            if (maintenanceConflicts.Any())
                return false;
            // Check route assignment conflicts
            var routeConflicts = _routeRepository.GetAllRoutes()
                .Where(r => (r.AMBusId == busId || r.PMBusId == busId) &&
                           r.DateAsDateTime >= startTime.Date && r.DateAsDateTime <= endTime.Date);
            return !routeConflicts.Any();
        }

        public bool IsValidDriverLicense(string licenseNumber, string licenseType)
        {
            if (string.IsNullOrWhiteSpace(licenseNumber) || string.IsNullOrWhiteSpace(licenseType))
                return false;
            // Basic validation for driver license format and type
            return licenseNumber.Length >= 5 && (licenseType == "CDL" || licenseType == "Regular");
        }

        public bool IsVehicleMaintenanceRequired(int BusId)
        {
            try
            {
                var maintenanceRecords = _maintenanceRepository.GetMaintenanceByBus(BusId);
                var lastMaintenance = maintenanceRecords?.OrderByDescending(m => m.DateAsDateTime).FirstOrDefault();
                if (lastMaintenance == null) return true; // No maintenance history
                // Maintenance required if last service was over 90 days ago
                return (DateTime.Now - (lastMaintenance.DateAsDateTime ?? DateTime.MinValue)).TotalDays > 90;
            }
            catch
            {
                return false; // Default to false if error occurs
            }
        }

        public bool IsRouteFeasible(Route route)
        {
            if (route == null) return false;
            // Basic feasibility checks - Only check RouteName since the Description and Distance properties don't exist
            return !string.IsNullOrWhiteSpace(route.RouteName);
        }

        public bool IsDriverEligibleForRoute(int DriverId, int RouteId)
        {
            try
            {
                var driver = _driverRepository.GetDriverById(DriverId);
                return driver != null && !string.IsNullOrWhiteSpace(driver.DriversLicenseType);
            }
            catch
            {
                return false;
            }
        }

        public bool IsMaintenanceOverdue(Maintenance maintenance)
        {
            if (maintenance == null) return false;
            // Overdue if scheduled date is in the past
            return maintenance.DateAsDateTime < DateTime.Now.Date;
        }

        public bool IsValidActivitySchedule(ActivitySchedule schedule)
        {
            if (schedule == null) return false;
            return schedule.ScheduledVehicleID.HasValue &&
                   schedule.ScheduledDriverID.HasValue &&
                   schedule.ScheduledLeaveTime.HasValue && schedule.ScheduledReturnTime.HasValue &&
                   schedule.ScheduledLeaveTime < schedule.ScheduledReturnTime;
        }

        public bool IsVehicleAvailable(int BusId, DateTime startTime, DateTime endTime)
        {
            try
            {
                // 1. Check if vehicle exists and is not out of service
                var bus = _busRepository.GetBusById(BusId);
                if (bus == null)
                {
                    return false;
                }
                // Note: Status property was removed from Bus model, assuming available
                // 2. Check for maintenance conflicts during the time period
                var maintenanceRecords = _maintenanceRepository.GetMaintenanceByBus(BusId);
                if (maintenanceRecords != null)
                {
                    var conflictingMaintenance = maintenanceRecords
                        .Where(m => m.DateAsDateTime.HasValue &&
                                   m.DateAsDateTime.Value.Date >= startTime.Date &&
                                   m.DateAsDateTime.Value.Date <= endTime.Date)
                        .Any();
                    if (conflictingMaintenance)
                    {
                        return false;
                    }
                }
                // 3. Check for route assignments during the time period
                // Check each day in the range for route assignments
                for (var day = startTime.Date; day <= endTime.Date; day = day.AddDays(1))
                {
                    var routes = _routeRepository.GetRoutesByDate(day);
                    if (routes != null)
                    {
                        var conflictingRoutes = routes
                            .Where(r => r.AMBusId == BusId || r.PMBusId == BusId)
                            .Any();
                        if (conflictingRoutes)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsDriverAvailable(int DriverId, DateTime startTime, DateTime endTime)
        {
            try
            {
                var driver = _driverRepository.GetDriverById(DriverId);
                return driver != null && !string.IsNullOrWhiteSpace(driver.DriversLicenseType);
            }
            catch
            {
                return false;
            }
        }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return false;
            // Remove all non-digits
            var digits = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"\D", "");
            // Valid if 10 or 11 digits (with or without country code)
            return digits.Length == 10 || digits.Length == 11;
        }

        public bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            // Strong password: at least 8 characters, contains uppercase, lowercase, digit
            return password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit);
        }

        public bool IsValidZipCode(string zipCode)
        {
            if (string.IsNullOrWhiteSpace(zipCode)) return false;
            // Valid US zip code formats: 12345 or 12345-6789
            return System.Text.RegularExpressions.Regex.IsMatch(zipCode, @"^\d{5}(-\d{4})?$");
        }

        public ValidationResult ValidateBus(Bus bus)
        {
            var errors = new List<string>();
            if (bus == null)
            {
                errors.Add("Bus cannot be null");
                return ValidationResult.Failed(errors);
            }
            if (string.IsNullOrWhiteSpace(bus.BusNumber))
                errors.Add("Bus number is required");
            else if (!IsValidBusNumber(bus.BusNumber))
                errors.Add("Bus number format is invalid");
            if (string.IsNullOrWhiteSpace(bus.Make))
                errors.Add("Bus make is required");
            if (string.IsNullOrWhiteSpace(bus.Model))
                errors.Add("Bus model is required");
            if (bus.Year < 1900 || bus.Year > DateTime.Now.Year + 1)
                errors.Add("Bus year must be valid");
            if (bus.Capacity <= 0)
                errors.Add("Bus capacity must be greater than zero");
            return errors.Any() ? ValidationResult.Failed(errors) : ValidationResult.Success();
        }

        public ValidationResult ValidateDriver(Driver driver)
        {
            var errors = new List<string>();
            if (driver == null)
            {
                errors.Add("Driver cannot be null");
                return ValidationResult.Failed(errors);
            }
            if (string.IsNullOrWhiteSpace(driver.DriverName))
                errors.Add("Driver name is required");
            if (string.IsNullOrWhiteSpace(driver.DriversLicenseType))
                errors.Add("Driver license type is required");
            if (!string.IsNullOrWhiteSpace(driver.DriverEmail) && !IsValidEmail(driver.DriverEmail))
                errors.Add("Driver email is invalid");
            if (!string.IsNullOrWhiteSpace(driver.DriverPhone) && !IsValidPhoneNumber(driver.DriverPhone))
                errors.Add("Driver phone number is invalid");
            return errors.Any() ? ValidationResult.Failed(errors) : ValidationResult.Success();
        }

        public ValidationResult ValidateRoute(Route route)
        {
            var errors = new List<string>();
            if (route == null)
            {
                errors.Add("Route cannot be null");
                return ValidationResult.Failed(errors);
            }
            if (string.IsNullOrWhiteSpace(route.RouteName))
                errors.Add("Route name is required");
            if (!IsRouteFeasible(route))
                errors.Add("Route configuration is not feasible");
            return errors.Any() ? ValidationResult.Failed(errors) : ValidationResult.Success();
        }

        public ValidationResult ValidateMaintenance(Maintenance maintenance)
        {
            var errors = new List<string>();
            if (maintenance == null)
            {
                errors.Add("Maintenance record cannot be null");
                return ValidationResult.Failed(errors);
            }
            if (!maintenance.BusId.HasValue || maintenance.BusId <= 0)
                errors.Add("Valid vehicle ID is required");
            if (string.IsNullOrWhiteSpace(maintenance.MaintenanceCompleted))
                errors.Add("Maintenance type is required");
            if (maintenance.RepairCost.HasValue && maintenance.RepairCost < 0)
                errors.Add("Maintenance cost cannot be negative");
            if (IsMaintenanceOverdue(maintenance))
                errors.Add("Maintenance is overdue");
            return errors.Any() ? ValidationResult.Failed(errors) : ValidationResult.Success();
        }
        #endregion
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

