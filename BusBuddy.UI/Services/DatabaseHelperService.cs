using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BusBuddy.Data;
using BusBuddy.Models;

namespace BusBuddy.UI.Services
{
    public class DatabaseHelperService : IDatabaseHelperService
    {
        private readonly BusBuddyContext _context;

        public DatabaseHelperService(BusBuddyContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await _context.Database.CanConnectAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database connection test failed: {ex.Message}");
                return false;
            }
        }

        public List<Route> GetAllRoutesWithDetails()
        {
            try
            {
                return _context.Routes.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting routes: {ex.Message}");
                return new List<Route>();
            }
        }
    }
}
