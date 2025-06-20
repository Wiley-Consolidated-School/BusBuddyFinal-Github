using System.Collections.Generic;
using System.Threading.Tasks;
using BusBuddy.Models;

namespace BusBuddy.UI.Services
{
    public interface IDatabaseHelperService
    {
        Task<bool> TestConnectionAsync();
        List<Route> GetAllRoutesWithDetails();
        // Add other database helper methods as needed
    }
}
