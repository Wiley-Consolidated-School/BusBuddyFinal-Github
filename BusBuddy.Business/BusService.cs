using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BusBuddy.Data;
using BusBuddy.Models;
using Microsoft.EntityFrameworkCore;

namespace BusBuddy.Business
{
    /// <summary>
    /// Bus service that provides access to vehicle data using proper database terminology
    /// Maps between "Bus" concepts and the underlying "Vehicle" data structure
    /// </summary>
    public class BusService : IBusService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly BusBuddyContext _context;

        public BusService(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        }

        public BusService(BusBuddyContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public BusService() : this(new VehicleRepository()) { }

        /// <summary>
        /// Gets all buses from the database (fetches from Vehicles table)
        /// Uses Entity Framework if context is available, otherwise uses repository
        /// </summary>
        public async Task<List<Bus>> GetAllBusesAsync()
        {
            try
            {
                List<Vehicle> vehicles;

                if (_context != null)
                {
                    // Use Entity Framework approach as requested by user
                    vehicles = await _context.Vehicles.ToListAsync();
                    System.Diagnostics.Debug.WriteLine($"Retrieved {vehicles.Count} vehicles from database using EF");
                }
                else
                {
                    // Use repository approach
                    vehicles = await Task.FromResult(_vehicleRepository.GetAllVehicles());
                    System.Diagnostics.Debug.WriteLine($"Retrieved {vehicles.Count} vehicles from database using repository");
                }

                // Convert vehicles to buses
                var buses = vehicles.Select(v => new Bus
                {
                    Id = v.VehicleID,
                    BusNumber = v.VehicleNumber ?? v.BusNumber ?? "Unknown",
                    VehicleNumber = v.VehicleNumber,
                    Make = v.Make,
                    Model = v.Model,
                    Year = v.Year,
                    Capacity = v.Capacity,
                    SeatingCapacity = v.SeatingCapacity,
                    FuelType = v.FuelType,
                    Status = v.Status,
                    VINNumber = v.VINNumber,
                    LicenseNumber = v.LicenseNumber,
                    DateLastInspection = ParseDateLastInspection(v.DateLastInspection),
                    Notes = v.Notes
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"Converted {buses.Count} vehicles to buses");
                return buses;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAllBusesAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets all buses synchronously
        /// </summary>
        public List<Bus> GetAllBuses()
        {
            try
            {
                var vehicles = _vehicleRepository.GetAllVehicles();
                System.Diagnostics.Debug.WriteLine($"Retrieved {vehicles.Count} vehicles from database");

                var buses = vehicles.Select(v => new Bus
                {
                    Id = v.VehicleID,
                    BusNumber = v.VehicleNumber ?? v.BusNumber ?? "Unknown",
                    VehicleNumber = v.VehicleNumber,
                    Make = v.Make,
                    Model = v.Model,
                    Year = v.Year,
                    Capacity = v.Capacity,
                    SeatingCapacity = v.SeatingCapacity,
                    FuelType = v.FuelType,
                    Status = v.Status,
                    VINNumber = v.VINNumber,
                    LicenseNumber = v.LicenseNumber,
                    DateLastInspection = ParseDateLastInspection(v.DateLastInspection),
                    Notes = v.Notes
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"Converted {buses.Count} vehicles to buses");
                return buses;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAllBuses: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets a bus by its ID
        /// </summary>
        public async Task<Bus?> GetBusByIdAsync(int busId)
        {
            try
            {
                Vehicle vehicle;

                if (_context != null)
                {
                    vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleID == busId);
                }
                else
                {
                    vehicle = await Task.FromResult(_vehicleRepository.GetVehicleById(busId));
                }

                if (vehicle == null)
                {
                    System.Diagnostics.Debug.WriteLine($"No vehicle found with ID {busId}");
                    return null;
                }

                return new Bus
                {
                    Id = vehicle.VehicleID,
                    BusNumber = vehicle.VehicleNumber ?? vehicle.BusNumber ?? "Unknown",
                    VehicleNumber = vehicle.VehicleNumber,
                    Make = vehicle.Make,
                    Model = vehicle.Model,
                    Year = vehicle.Year,
                    Capacity = vehicle.Capacity,
                    SeatingCapacity = vehicle.SeatingCapacity,
                    FuelType = vehicle.FuelType,
                    Status = vehicle.Status,
                    VINNumber = vehicle.VINNumber,
                    LicenseNumber = vehicle.LicenseNumber,
                    DateLastInspection = ParseDateLastInspection(vehicle.DateLastInspection),
                    Notes = vehicle.Notes
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetBusByIdAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets active buses only
        /// </summary>
        public async Task<List<Bus>> GetActiveBusesAsync()
        {
            try
            {
                var allBuses = await GetAllBusesAsync();
                return allBuses.Where(b => b.Status?.Equals("Active", StringComparison.OrdinalIgnoreCase) == true).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetActiveBusesAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs diagnostics on data retrieval
        /// </summary>
        public string DiagnoseDataRetrieval()
        {
            try
            {
                if (_context != null)
                {
                    return $"Using Entity Framework Context - Database: {_context.Database.GetConnectionString()}";
                }
                else if (_vehicleRepository is VehicleRepository repo)
                {
                    return repo.DiagnoseDataRetrieval();
                }
                else
                {
                    return "Repository diagnostics not available for this implementation";
                }
            }
            catch (Exception ex)
            {
                return $"Error running diagnostics: {ex.Message}";
            }
        }

        /// <summary>
        /// Helper method to parse DateLastInspection string to DateTime?
        /// </summary>
        private static DateTime? ParseDateLastInspection(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            if (DateTime.TryParse(dateString, out DateTime result))
                return result;

            return null;
        }
    }
}
