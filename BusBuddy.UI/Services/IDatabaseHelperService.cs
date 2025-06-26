using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using BusBuddy.Models;

namespace BusBuddy.UI.Services
{
    public interface IDatabaseHelperService
    {
        Task<bool> TestConnectionAsync();
        List<Route> GetAllRoutesWithDetails();
        // Security-related methods
        string GetConnectionString();
        DataTable ExecuteQuery(string sql, Dictionary<string, object> parameters = null);
        // Add other database helper methods as needed
    }
}
