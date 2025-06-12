using Xunit;
using System;
using System.Collections.Generic;
using BusBuddy.Models;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using System.IO;
using BusBuddy.Data;
using Dapper;
using System.Linq;

namespace BusBuddy.Tests
{
    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    public class DatabaseFixture : IDisposable
    {
        private SqliteConnection _connection;

        public SqliteConnection Connection => _connection;

        public DatabaseFixture()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Initialize database and seed data
            var initializer = new DatabaseInitializer();
            initializer.Initialize(_connection);
        }

        public SqliteConnection GetConnection()
        {
            return _connection;
        }

        public bool TableExists(string tableName)
        {
            var sql = "SELECT name FROM sqlite_master WHERE type='table' AND name=@TableName";
            var result = _connection.QuerySingleOrDefault<string>(sql, new { TableName = tableName });
            return result != null;
        }

        public bool IndexExists(string indexName)
        {
            var sql = "SELECT name FROM sqlite_master WHERE type='index' AND name=@IndexName";
            var result = _connection.QuerySingleOrDefault<string>(sql, new { IndexName = indexName });
            return result != null;
        }

        public BusBuddyContext? CreateContext()
        {
            // If BusBuddyContext exists and works with EF Core
            try
            {
                // This is a fallback - return null if EF Core isn't available
                return null;
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
