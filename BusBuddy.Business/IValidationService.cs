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
        bool IsValidBusNumber(string busNumber);
        bool IsValidDriverLicense(string licenseNumber, string licenseType);
        bool IsBusMaintenanceRequired(int busId);
        bool IsRouteFeasible(Route route);
        bool IsDriverEligibleForRoute(int DriverId, int RouteId);
        bool IsFuelLevelCritical(int busId);
        bool IsMaintenanceOverdue(Maintenance maintenance);
        bool IsValidActivitySchedule(ActivitySchedule schedule);
        bool IsBusAvailable(int busId, DateTime startTime, DateTime endTime);
        bool IsDriverAvailable(int DriverId, DateTime startTime, DateTime endTime);
        bool IsValidEmail(string email);
        bool IsValidPhoneNumber(string phoneNumber);
        bool IsStrongPassword(string password);
        bool IsValidZipCode(string zipCode);
        ValidationResult ValidateBus(Bus bus);
        ValidationResult ValidateDriver(Driver driver);
        ValidationResult ValidateRoute(Route route);
        ValidationResult ValidateMaintenance(Maintenance maintenance);
        ValidationResult ValidateFuelRecord(Fuel fuelRecord);
        ValidationResult ValidateBusAvailability(int busId, DateTime date, string assignmentType = "general", int? currentRouteId = null);
        ValidationResult ValidateDriverAvailability(int DriverId, DateTime date, string assignmentType = "general", int? currentRouteId = null);
    }
}

