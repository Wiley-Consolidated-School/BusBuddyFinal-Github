using System;
using System.Data;
using System.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace BusBuddy.Data
{
    public abstract class BaseRepository
    {
        protected readonly string _connectionString;
        protected readonly string _providerName;

        protected BaseRepository()
        {
            var conn = ConfigurationManager.ConnectionStrings["DefaultConnection"];
            if (conn == null)
                throw new InvalidOperationException("DefaultConnection is missing in configuration. Ensure App.config is present and copied to output directory.");
            _connectionString = conn.ConnectionString;
            _providerName = conn.ProviderName;
        }

        protected IDbConnection CreateConnection()
        {
            if (_providerName == "Microsoft.Data.Sqlite")
            {
                return new SqliteConnection(_connectionString);
            }
            else
            {
                return new SqlConnection(_connectionString);
            }
        }
    }
}
