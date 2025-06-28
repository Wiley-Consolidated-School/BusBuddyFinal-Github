using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BusBuddy.Data;
using BusBuddy.Models;

namespace BusBuddy.Business
{
    /// <summary>
    /// Bus service that provides access to bus data using BusRepository
    /// </summary>
    public class BusService : IBusService
    {
        private readonly BusRepository _busRepository;

        public BusService(BusRepository busRepository)
        {
            _busRepository = busRepository ?? throw new ArgumentNullException(nameof(busRepository));
        }

        public BusService() : this(new BusRepository()) { }

        /// <summary>
        /// Gets all buses from the database
        /// </summary>
        public async Task<List<Bus>> GetAllBusesAsync()
        {
            try
            {
                var buses = _busRepository.GetAllBuses().ToList();
                System.Diagnostics.Debug.WriteLine($"Retrieved {buses.Count} buses from database");
                return await Task.FromResult(buses);
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
                var buses = _busRepository.GetAllBuses().ToList();
                System.Diagnostics.Debug.WriteLine($"Retrieved {buses.Count} buses from database");
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
                var bus = _busRepository.GetBusById(busId);
                if (bus == null)
                {
                    System.Diagnostics.Debug.WriteLine($"No bus found with ID {busId}");
                }
                return await Task.FromResult(bus);
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
                // Since Bus model doesn't have Status property, return all buses
                var allBuses = await GetAllBusesAsync();
                return allBuses;
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
                _busRepository.DiagnoseDataRetrieval();
                return "Bus repository diagnostics completed successfully";
            }
            catch (Exception ex)
            {
                return $"Error running diagnostics: {ex.Message}";
            }
        }
    }
}

