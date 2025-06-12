using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace BusBuddy.Tests
{
    public class TestDatabaseHelper : IDisposable
    {
        private readonly SqliteConnection _connection;

        public TestDatabaseHelper()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Apply schema
            var schema = File.ReadAllText("Db/DatabaseScript.sql");
            using var command = _connection.CreateCommand();
            command.CommandText = schema;
            command.ExecuteNonQuery();
        }

        public SqliteConnection GetConnection() => _connection;

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
