using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using System.Configuration;

namespace BusBuddy.Db
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }

    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;
        private readonly string _providerName;

        public DbConnectionFactory()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            _providerName = ConfigurationManager.ConnectionStrings["DefaultConnection"].ProviderName;
        }

        public IDbConnection CreateConnection()
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
