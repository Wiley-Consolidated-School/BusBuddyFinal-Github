using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using BusBuddy.Models;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// UI-specific data service for connection testing and simple operations
    /// Renamed from IDatabaseHelperService to avoid conflict with Business layer service
    /// </summary>
    public interface IUIDataService
    {
        Task<bool> TestConnectionAsync();
        List<Route> GetAllRoutesWithDetails();
        // Security-related methods
        string GetConnectionString();
        DataTable ExecuteQuery(string sql, Dictionary<string, object> parameters = null);
        // Add other database helper methods as needed
    }
}

