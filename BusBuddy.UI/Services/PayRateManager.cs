// Task 6.7: Create PayRateManager Helper
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Manages pay rate configuration operations for payrates.json
    /// </summary>
    public class PayRateManager
    {
        private readonly IErrorHandlerService _errorHandler;
        private readonly string _jsonPath;

        public PayRateManager(IErrorHandlerService errorHandler)
        {
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "payrates.json");
        }

        /// <summary>
        /// Loads pay rates from payrates.json file
        /// </summary>
        /// <returns>List of PayRate objects</returns>
        public List<PayRate> LoadPayRates()
        {
            try
            {
                if (File.Exists(_jsonPath))
                {
                    var json = File.ReadAllText(_jsonPath);
                    var rates = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json);
                    
                    return new List<PayRate>
                    {
                        new PayRate { RouteType = "CDL", Rate = rates.GetValueOrDefault("CDLTripRate", 33.00m) },
                        new PayRate { RouteType = "SmallBus", Rate = rates.GetValueOrDefault("SmallBusTripRate", 15.00m) },
                        new PayRate { RouteType = "SPED", Rate = rates.GetValueOrDefault("SPEDDayRate", 66.00m) }
                    };
                }
                
                // Return default rates if file doesn't exist
                return GetDefaultPayRates();
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Failed to load pay rates: {ex.Message}", "Pay Rate Error");
                return GetDefaultPayRates();
            }
        }

        /// <summary>
        /// Saves pay rates to payrates.json file
        /// </summary>
        /// <param name="payRates">List of PayRate objects to save</param>
        public void SavePayRates(List<PayRate> payRates)
        {
            try
            {
                if (payRates == null || payRates.Count != 3)
                {
                    throw new ArgumentException("Invalid pay rates provided. Must have exactly 3 rate types.");
                }

                // Validate all rates are positive
                if (payRates.Any(r => r.Rate <= 0))
                {
                    throw new ArgumentException("All pay rates must be positive values.");
                }

                // Validate required route types are present
                var requiredTypes = new[] { "CDL", "SmallBus", "SPED" };
                var providedTypes = payRates.Select(r => r.RouteType).ToArray();
                
                if (!requiredTypes.All(t => providedTypes.Contains(t)))
                {
                    throw new ArgumentException("Missing required route types. Must include CDL, SmallBus, and SPED.");
                }

                var rates = new Dictionary<string, decimal>
                {
                    { "CDLTripRate", payRates.First(r => r.RouteType == "CDL").Rate },
                    { "SmallBusTripRate", payRates.First(r => r.RouteType == "SmallBus").Rate },
                    { "SPEDDayRate", payRates.First(r => r.RouteType == "SPED").Rate }
                };

                var json = JsonSerializer.Serialize(rates, new JsonSerializerOptions { WriteIndented = true });
                
                // Ensure Resources directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(_jsonPath));
                
                File.WriteAllText(_jsonPath, json);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Failed to save pay rates: {ex.Message}", "Pay Rate Error");
                throw;
            }
        }

        /// <summary>
        /// Gets default pay rates
        /// </summary>
        /// <returns>List of default PayRate objects</returns>
        private List<PayRate> GetDefaultPayRates()
        {
            return new List<PayRate>
            {
                new PayRate { RouteType = "CDL", Rate = 33.00m },
                new PayRate { RouteType = "SmallBus", Rate = 15.00m },
                new PayRate { RouteType = "SPED", Rate = 66.00m }
            };
        }

        /// <summary>
        /// Gets pay rate for a specific route type
        /// </summary>
        /// <param name="routeType">Route type (CDL, SmallBus, SPED)</param>
        /// <returns>Pay rate for the route type</returns>
        public decimal GetPayRate(string routeType)
        {
            try
            {
                var payRates = LoadPayRates();
                var rate = payRates.FirstOrDefault(r => r.RouteType.Equals(routeType, StringComparison.OrdinalIgnoreCase));
                return rate?.Rate ?? 0m;
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError($"Failed to get pay rate for {routeType}: {ex.Message}", "Pay Rate Error");
                return 0m;
            }
        }
    }

    /// <summary>
    /// Represents a pay rate for a specific route type
    /// </summary>
    public class PayRate
    {
        public string RouteType { get; set; } = string.Empty;
        public decimal Rate { get; set; }
    }
}
