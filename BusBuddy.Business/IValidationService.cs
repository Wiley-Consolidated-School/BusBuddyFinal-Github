using System;
using System.Collections.Generic;
using BusBuddy.Models;

namespace BusBuddy.Business
{
    /// <summary>
    /// Interface for validation service that handles complex business rules and data integrity
    /// </summary>
    public interface IValidationService
    {
        bool IsValidVehicleNumber(string vehicleNumber);
        bool IsValidDriverLicense(string licenseNumber, string licenseType);
        bool IsVehicleMaintenanceRequired(int vehicleId);
        bool IsRouteFeasible(Route route);
        bool IsDriverEligibleForRoute(int driverId, int routeId);
        bool IsFuelLevelCritical(int vehicleId);
        bool IsMaintenanceOverdue(Maintenance maintenance);
        bool IsValidActivitySchedule(ActivitySchedule schedule);
        bool IsVehicleAvailable(int vehicleId, DateTime startTime, DateTime endTime);
        bool IsDriverAvailable(int driverId, DateTime startTime, DateTime endTime);
        bool IsValidEmail(string email);
        bool IsValidPhoneNumber(string phoneNumber);
        bool IsStrongPassword(string password);
        bool IsValidZipCode(string zipCode);
        List<string> ValidateVehicle(Vehicle vehicle);
        List<string> ValidateDriver(Driver driver);
        List<string> ValidateRoute(Route route);
        List<string> ValidateMaintenance(Maintenance maintenance);
        List<string> ValidateFuelRecord(Fuel fuelRecord);
    }
}
