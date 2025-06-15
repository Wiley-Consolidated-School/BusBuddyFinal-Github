using Xunit;
using Xunit.Abstractions;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Integration test to run diagnostic program and verify all systems are working
    /// </summary>
    public class DiagnosticIntegrationTests
    {
        private readonly ITestOutputHelper _output;

        public DiagnosticIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
        }        [Fact]
        public void RunFullDiagnostics()
        {
            // Capture console output
            var originalOut = Console.Out;
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            try
            {
                // Run diagnostics
                DiagnosticTestProgram.RunDiagnostics();

                // Get output
                var output = stringWriter.ToString();

                // Log to test output
                _output.WriteLine("=== Diagnostic Output ===");
                _output.WriteLine(output);

                // Verify key indicators (more flexible checks)
                var hasConnectionInfo = output.Contains("Connection String:") || output.Contains("test environment");
                var hasRepositoryTest = output.Contains("Repository connection") || output.Contains("Repository test");
                var hasDbConnection = output.Contains("Connected to SQL Server") || output.Contains("connection successful");

                Assert.True(hasConnectionInfo, "Should have connection information");
                Assert.True(hasRepositoryTest, "Should have repository test results");
                Assert.True(hasDbConnection, "Should have database connection test");

                // Verify no critical failures
                Assert.DoesNotContain("Configuration test failed", output);
                Assert.DoesNotContain("Repository test failed", output);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
    }
}
