using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusBuddy.Models;

namespace BusBuddy.Business
{
    /// <summary>
    /// Interface for bus service operations
    /// Provides access to vehicle data using bus-specific terminology
    /// </summary>
    public interface IBusService
    {
        /// <summary>
        /// Gets all buses from the database asynchronously
        /// </summary>
        Task<List<Bus>> GetAllBusesAsync();

        /// <summary>
        /// Gets all buses from the database synchronously
        /// </summary>
        List<Bus> GetAllBuses();

        /// <summary>
        /// Gets a specific bus by ID
        /// </summary>
        Task<Bus?> GetBusByIdAsync(int busId);

        /// <summary>
        /// Gets only active buses
        /// </summary>
        Task<List<Bus>> GetActiveBusesAsync();

        /// <summary>
        /// Runs diagnostics on data retrieval
        /// </summary>
        string DiagnoseDataRetrieval();
    }
}
