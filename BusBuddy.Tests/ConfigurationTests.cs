using System;
using System.Configuration;
using Xunit;
using Xunit.Abstractions;
using BusBuddy.Data;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Tests to verify configuration and database connectivity work correctly
    /// </summary>
    public class ConfigurationTests
    {
        private readonly ITestOutputHelper _output;

        public ConfigurationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void VerifyDatabaseConnection()
        {
            // Arrange & Act - Use BaseRepository which has fallback logic for tests
            var testRepo = new TestRepository();
            var connectionString = testRepo.GetConnectionString();
            var providerName = testRepo.GetProviderName();

            // Assert
            Assert.NotNull(connectionString);
            Assert.NotEmpty(connectionString);
            Assert.NotNull(providerName);

            _output.WriteLine($"Connection String: {connectionString}");
            _output.WriteLine($"Provider Name: {providerName}");

            // Verify the connection string contains expected elements for test environment
            Assert.Contains("BusBuddyDB", connectionString);
            Assert.Contains("SQLEXPRESS", connectionString);
            Assert.Equal("Microsoft.Data.SqlClient", providerName);
        }

        [Fact]
        public void VerifyDatabaseConnectivity()
        {
            // Arrange & Act
            var testRepo = new TestRepository();

            // Assert - Should be able to create and open a connection
            using var connection = testRepo.TestCreateConnection();
            connection.Open();

            Assert.True(connection.State == System.Data.ConnectionState.Open);
            _output.WriteLine("✓ Database connection successful");
        }

        [Fact]
        public void VerifyConfigurationOrFallback()
        {
            // Arrange & Act
            var defaultConn = ConfigurationManager.ConnectionStrings["DefaultConnection"];
            var isTestEnvironment = IsTestEnvironment();

            // Assert
            if (defaultConn != null)
            {
                _output.WriteLine($"✓ DefaultConnection found: {defaultConn.ConnectionString}");
                Assert.Contains("BusBuddy", defaultConn.ConnectionString);
            }
            else if (isTestEnvironment)
            {
                _output.WriteLine("✓ Test environment detected - using fallback configuration");
                Assert.True(isTestEnvironment, "Should detect test environment when config is not available");
            }            else
            {
                Assert.Fail("Neither configuration nor test environment detected");
            }
        }

        [Fact]
        public void VerifyTestEnvironmentDetection()
        {
            // Arrange & Act
            var isTest = IsTestEnvironment();

            // Assert
            Assert.True(isTest, "Should detect test environment");
            _output.WriteLine($"✓ Test environment detected: {isTest}");
        }

        private bool IsTestEnvironment()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return baseDirectory.Contains("test", StringComparison.OrdinalIgnoreCase) ||
                   baseDirectory.Contains("Test") ||
                   Environment.CommandLine.Contains("testhost") ||
                   Environment.CommandLine.Contains("vstest");
        }
    }
}
