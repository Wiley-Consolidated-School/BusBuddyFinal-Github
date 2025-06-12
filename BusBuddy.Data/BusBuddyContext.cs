using System;
using System.Data.Common;
using BusBuddy.Models;

namespace BusBuddy.Data
{
    // Simplified context class without EntityFrameworkCore dependencies
    public class BusBuddyContext : IDisposable
    {
        private readonly DbConnection _connection;

        public BusBuddyContext(DbConnection connection)
        {
            _connection = connection;
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
