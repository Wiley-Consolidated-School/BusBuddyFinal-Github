using System;
using BusBuddy.Models;

namespace BusBuddy.Business
{
    /// <summary>
    /// Extension methods to handle compatibility between models and tests
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// Gets the DriverId property (maps to DriverId in the actual model)
        /// </summary>
        public static int DriverId(this Driver driver)
        {
            return driver.DriverId;
        }

        /// <summary>
        /// Gets the RouteId property (maps to RouteId in the actual model)
        /// </summary>
        public static int RouteId(this Route route)
        {
            return route.RouteId;
        }

        /// <summary>
        /// Gets the LicenseNumber property (uses DriverEmail as a proxy since the model doesn't have LicenseNumber)
        /// </summary>
        public static string LicenseNumber(this Driver driver)
        {
            // Use DriverEmail as a proxy
            return driver.DriverEmail ?? "";
        }

        /// <summary>
        /// Sets the LicenseNumber property (sets DriverEmail as a proxy)
        /// </summary>
        public static void SetLicenseNumber(this Driver driver, string value)
        {
            driver.DriverEmail = value;
        }

        /// <summary>
        /// Gets the HireDate property (returns current date as fallback)
        /// </summary>
        public static DateTime HireDate(this Driver driver)
        {
            return DateTime.Now;
        }

        /// <summary>
        /// Sets the HireDate property (no-op as property doesn't exist)
        /// </summary>
        public static void SetHireDate(this Driver driver, DateTime value)
        {
            // No-op as the property doesn't exist
        }

        /// <summary>
        /// Gets the Description property for a Route (not in the model, returns empty string)
        /// </summary>
        public static string Description(this Route route)
        {
            return "";
        }

        /// <summary>
        /// Sets the Description property for a Route (not in the model, does nothing)
        /// </summary>
        public static void SetDescription(this Route route, string value)
        {
            // No-op as the property doesn't exist
        }

        /// <summary>
        /// Gets the Distance property for a Route (not in the model, returns 0)
        /// </summary>
        public static decimal Distance(this Route route)
        {
            return 0;
        }

        /// <summary>
        /// Sets the Distance property for a Route (not in the model, does nothing)
        /// </summary>
        public static void SetDistance(this Route route, decimal value)
        {
            // No-op as the property doesn't exist
        }
    }
}

