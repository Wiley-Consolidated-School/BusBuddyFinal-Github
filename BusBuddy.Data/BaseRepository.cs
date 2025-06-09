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
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            _providerName = ConfigurationManager.ConnectionStrings["DefaultConnection"].ProviderName;
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
