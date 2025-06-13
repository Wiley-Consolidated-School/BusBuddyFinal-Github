using System;
using System.Data;
using System.Configuration;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace BusBuddy.Data
{    public abstract class BaseRepository
    {
        protected readonly string _connectionString;
        protected readonly string _providerName;

        protected BaseRepository()
        {
            var conn = ConfigurationManager.ConnectionStrings["DefaultConnection"];
            if (conn == null)
            {
                // Fallback for testing - try to use test database directly
                var currentDir = AppDomain.CurrentDomain.BaseDirectory;
                var testDbPath = Path.Combine(currentDir, "test_busbuddy.db");

                if (File.Exists(testDbPath))
                {
                    _connectionString = $"Data Source={testDbPath}";
                    _providerName = "Microsoft.Data.Sqlite";
                }
                else
                {
                    // Another fallback - use relative path
                    _connectionString = "Data Source=test_busbuddy.db";
                    _providerName = "Microsoft.Data.Sqlite";
                }
            }
            else
            {
                _connectionString = conn.ConnectionString;
                _providerName = conn.ProviderName;
            }
        }

        protected BaseRepository(string connectionString, string providerName)
        {
            _connectionString = connectionString;
            _providerName = providerName;
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
