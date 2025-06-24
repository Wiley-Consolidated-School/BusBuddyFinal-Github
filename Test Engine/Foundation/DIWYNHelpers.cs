using System;
using System.Threading.Tasks;
using Xunit;

namespace BusBuddy.TestEngine.Foundation
{
    /// <summary>
    /// DIWYN (Does It Work? Yes/No) Helpers - Simple, outcome-focused testing for novice users.
    ///
    /// These helpers provide a straightforward way to test functionality with clear yes/no results.
    /// Perfect for quick validation, smoke tests, and learning test-driven development.
    ///
    /// Philosophy: Focus on what matters - does the code work as expected?
    /// </summary>
    public static class DIWYNHelpers
    {
        #region Basic DIWYN Operations

        /// <summary>
        /// Tests if an action works without throwing an exception.
        /// Returns: Yes (true) if it works, No (false) if it fails.
        /// </summary>
        /// <param name="action">The action to test</param>
        /// <param name="testName">Optional name for the test (for reporting)</param>
        /// <returns>True if the action works, false if it throws an exception</returns>
        public static bool DoesItWork(Action action, string? testName = null)
        {
            try
            {
                action?.Invoke();
                LogResult(true, testName, "Action completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                LogResult(false, testName, $"Action failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tests if a function works and returns the expected result.
        /// Returns: Yes (true) if it works and returns expected value, No (false) otherwise.
        /// </summary>
        /// <typeparam name="T">The return type of the function</typeparam>
        /// <param name="func">The function to test</param>
        /// <param name="expectedValue">The expected return value</param>
        /// <param name="testName">Optional name for the test (for reporting)</param>
        /// <returns>True if the function works and returns the expected value</returns>
        public static bool DoesItWork<T>(Func<T> func, T expectedValue, string? testName = null)
        {
            try
            {
                var result = func();
                bool matches = result?.Equals(expectedValue) ?? expectedValue == null;

                LogResult(matches, testName, matches
                    ? $"Function returned expected value: {expectedValue}"
                    : $"Function returned {result}, expected {expectedValue}");

                return matches;
            }
            catch (Exception ex)
            {
                LogResult(false, testName, $"Function failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tests if an async action works without throwing an exception.
        /// Returns: Yes (true) if it works, No (false) if it fails.
        /// </summary>
        /// <param name="asyncAction">The async action to test</param>
        /// <param name="testName">Optional name for the test (for reporting)</param>
        /// <returns>True if the async action works, false if it throws an exception</returns>
        public static async Task<bool> DoesItWorkAsync(Func<Task> asyncAction, string? testName = null)
        {
            try
            {
                if (asyncAction != null)
                    await asyncAction();

                LogResult(true, testName, "Async action completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                LogResult(false, testName, $"Async action failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tests if an async function works and returns the expected result.
        /// Returns: Yes (true) if it works and returns expected value, No (false) otherwise.
        /// </summary>
        /// <typeparam name="T">The return type of the async function</typeparam>
        /// <param name="asyncFunc">The async function to test</param>
        /// <param name="expectedValue">The expected return value</param>
        /// <param name="testName">Optional name for the test (for reporting)</param>
        /// <returns>True if the async function works and returns the expected value</returns>
        public static async Task<bool> DoesItWorkAsync<T>(Func<Task<T>> asyncFunc, T expectedValue, string? testName = null)
        {
            try
            {
                var result = await asyncFunc();
                bool matches = result?.Equals(expectedValue) ?? expectedValue == null;

                LogResult(matches, testName, matches
                    ? $"Async function returned expected value: {expectedValue}"
                    : $"Async function returned {result}, expected {expectedValue}");

                return matches;
            }
            catch (Exception ex)
            {
                LogResult(false, testName, $"Async function failed: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region xUnit Integration

        /// <summary>
        /// Asserts that an action works. Throws exception if it doesn't.
        /// Use this in xUnit test methods to fail the test if the action doesn't work.
        /// </summary>
        /// <param name="action">The action to test</param>
        /// <param name="testName">Optional name for the test (for error messages)</param>
        public static void AssertItWorks(Action action, string? testName = null)
        {
            if (!DoesItWork(action, testName))
            {
                throw new Xunit.Sdk.XunitException($"DIWYN Test Failed: {testName ?? "Action"} did not work as expected.");
            }
        }

        /// <summary>
        /// Asserts that a function works and returns the expected value.
        /// Throws exception if it doesn't.
        /// </summary>
        /// <typeparam name="T">The return type of the function</typeparam>
        /// <param name="func">The function to test</param>
        /// <param name="expectedValue">The expected return value</param>
        /// <param name="testName">Optional name for the test (for error messages)</param>
        public static void AssertItWorks<T>(Func<T> func, T expectedValue, string? testName = null)
        {
            if (!DoesItWork(func, expectedValue, testName))
            {
                throw new Xunit.Sdk.XunitException($"DIWYN Test Failed: {testName ?? "Function"} did not work as expected.");
            }
        }

        /// <summary>
        /// Asserts that an async action works. Throws exception if it doesn't.
        /// </summary>
        /// <param name="asyncAction">The async action to test</param>
        /// <param name="testName">Optional name for the test (for error messages)</param>
        public static async Task AssertItWorksAsync(Func<Task> asyncAction, string? testName = null)
        {
            if (!await DoesItWorkAsync(asyncAction, testName))
            {
                throw new Xunit.Sdk.XunitException($"DIWYN Test Failed: {testName ?? "Async Action"} did not work as expected.");
            }
        }

        /// <summary>
        /// Asserts that an async function works and returns the expected value.
        /// Throws exception if it doesn't.
        /// </summary>
        /// <typeparam name="T">The return type of the async function</typeparam>
        /// <param name="asyncFunc">The async function to test</param>
        /// <param name="expectedValue">The expected return value</param>
        /// <param name="testName">Optional name for the test (for error messages)</param>
        public static async Task AssertItWorksAsync<T>(Func<Task<T>> asyncFunc, T expectedValue, string? testName = null)
        {
            if (!await DoesItWorkAsync(asyncFunc, expectedValue, testName))
            {
                throw new Xunit.Sdk.XunitException($"DIWYN Test Failed: {testName ?? "Async Function"} did not work as expected.");
            }
        }

        #endregion

        #region Specialized DIWYN Tests

        /// <summary>
        /// Tests if a database connection works.
        /// Returns: Yes (true) if connection succeeds, No (false) if it fails.
        /// </summary>
        /// <param name="connectionString">The database connection string</param>
        /// <param name="testName">Optional name for the test</param>
        /// <returns>True if database connection works</returns>
        public static bool DoesDataBaseWork(string connectionString, string? testName = "Database Connection")
        {
            return DoesItWork(() =>
            {
                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();
                    // If we get here, connection worked
                }
            }, testName);
        }

        /// <summary>
        /// Tests if a service/class can be instantiated without errors.
        /// Returns: Yes (true) if instantiation works, No (false) if it fails.
        /// </summary>
        /// <typeparam name="T">The type to instantiate</typeparam>
        /// <param name="constructorArgs">Arguments for the constructor</param>
        /// <param name="testName">Optional name for the test</param>
        /// <returns>True if instantiation works</returns>
        public static bool DoesInstantiationWork<T>(object[]? constructorArgs = null, string? testName = null) where T : class
        {
            testName = testName ?? $"Instantiate {typeof(T).Name}";

            return DoesItWork(() =>
            {
                var instance = constructorArgs == null
                    ? Activator.CreateInstance<T>()
                    : (T?)Activator.CreateInstance(typeof(T), constructorArgs);

                if (instance == null)
                    throw new InvalidOperationException("Instance was null after creation");
            }, testName);
        }

        /// <summary>
        /// Tests if a property can be set and retrieved correctly.
        /// Returns: Yes (true) if property works, No (false) if it fails.
        /// </summary>
        /// <typeparam name="T">The type of the property value</typeparam>
        /// <param name="setter">Action to set the property</param>
        /// <param name="getter">Function to get the property</param>
        /// <param name="testValue">Value to test with</param>
        /// <param name="testName">Optional name for the test</param>
        /// <returns>True if property set/get works correctly</returns>
        public static bool DoesPropertyWork<T>(Action<T> setter, Func<T> getter, T testValue, string? testName = null)
        {
            testName = testName ?? "Property Set/Get";

            return DoesItWork(() =>
            {
                setter(testValue);
                var retrievedValue = getter();

                if (!retrievedValue?.Equals(testValue) ?? testValue != null)
                {
                    throw new InvalidOperationException($"Property returned {retrievedValue}, expected {testValue}");
                }
            }, testName);
        }

        #endregion

        #region DIWYN Reporting

        /// <summary>
        /// Logs the result of a DIWYN test for reporting and diagnostics.
        /// </summary>
        /// <param name="success">Whether the test succeeded</param>
        /// <param name="testName">Name of the test</param>
        /// <param name="details">Additional details about the result</param>
        private static void LogResult(bool success, string? testName, string details)
        {
            var result = success ? "✓ YES" : "✗ NO";
            var message = $"DIWYN: {result} - {testName ?? "Test"}: {details}";

            // Use TestDiagnostics if available, otherwise use Console
            try
            {
                TestDiagnostics.Log(testName ?? "Test", details, success);
            }
            catch
            {
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Runs multiple DIWYN tests and reports a summary.
        /// Returns the number of tests that passed.
        /// </summary>
        /// <param name="tests">Dictionary of test names and test functions</param>
        /// <returns>Number of tests that passed</returns>
        public static int RunDIWYNSuite(System.Collections.Generic.Dictionary<string, Func<bool>> tests)
        {
            int passed = 0;
            int total = tests.Count;

            Console.WriteLine($"\n=== DIWYN Test Suite: Running {total} tests ===");

            foreach (var test in tests)
            {
                bool result = test.Value();
                if (result) passed++;

                Console.WriteLine($"{(result ? "✓" : "✗")} {test.Key}");
            }

            Console.WriteLine($"\n=== DIWYN Results: {passed}/{total} tests passed ===");
            return passed;
        }

        #endregion
    }
}
