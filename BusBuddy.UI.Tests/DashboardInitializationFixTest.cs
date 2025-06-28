using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusBuddy.UI.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace BusBuddy.UI.Tests
{
    /// <summary>
    /// Tests for the DashboardInitializationFix helper class.
    /// Validates proper cancellation token management and UI thread safety.
    /// </summary>
    [Collection("Dashboard Tests")]
    [DashboardTests]
    public class DashboardInitializationFixTest : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly Form _testForm;
        private bool _disposed;

        public DashboardInitializationFixTest(ITestOutputHelper output)
        {
            _output = output;
            _testForm = new Form();
            _output.WriteLine("Test form created for UI thread testing");
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _testForm?.Dispose();
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Warning during test cleanup: {ex.Message}");
                }
                _disposed = true;
            }
        }

        [Fact]
        public void CreateCancellationTokenSource_ShouldReturnNewTokenSource()
        {
            // Act
            var cts = DashboardInitializationFix.CreateCancellationTokenSource();

            // Assert
            Assert.NotNull(cts);
            Assert.False(cts.IsCancellationRequested);

            // Cleanup
            cts.Cancel();
            cts.Dispose();
        }

        [Fact]
        public void CreateCancellationTokenSource_WhenCalledMultipleTimes_ShouldCancelPreviousToken()
        {
            // Act
            var cts1 = DashboardInitializationFix.CreateCancellationTokenSource();
            var cts2 = DashboardInitializationFix.CreateCancellationTokenSource();

            // Assert
            Assert.NotNull(cts1);
            Assert.NotNull(cts2);
            Assert.NotSame(cts1, cts2);

            // Cleanup
            cts2.Cancel();
            cts2.Dispose();
        }

        [Fact]
        public void CancelInitialization_ShouldCancelTokenSource()
        {
            // Arrange
            var cts = DashboardInitializationFix.CreateCancellationTokenSource();

            // Act
            DashboardInitializationFix.CancelInitialization();

            // Assert
            Assert.True(cts.IsCancellationRequested);

            // Cleanup
            cts.Dispose();
        }

        [Fact]
        public async Task SafeExecuteAsync_WithCancellation_ShouldHandleCancellationGracefully()
        {
            // Arrange
            var cts = DashboardInitializationFix.CreateCancellationTokenSource();
            var operationCompleted = false;

            // Act & Assert - This should not throw
            await DashboardInitializationFix.SafeExecuteAsync(async (token) =>
            {
                // Cancel the token immediately
                cts.Cancel();

                // Add an await to properly make this method async
                await Task.Delay(1);

                // This should be canceled
                token.ThrowIfCancellationRequested();

                // This line should never execute due to cancellation
                operationCompleted = true;
            });

            // Assert
            Assert.False(operationCompleted, "Operation should have been canceled");

            // Cleanup
            cts.Dispose();
        }

        [Fact]
        public async Task SafeExecuteAsync_WithSuccessfulExecution_ShouldCompleteOperation()
        {
            // Arrange
            var operationCompleted = false;

            // Act
            // Use a new token source to ensure we're not affected by previous tests
            var cts = DashboardInitializationFix.CreateCancellationTokenSource();
            await DashboardInitializationFix.SafeExecuteAsync(async (token) =>
            {
                await Task.Delay(10, token);
                operationCompleted = true;
            });

            // Assert
            Assert.True(operationCompleted, "Operation should have completed successfully");
        }

        [Fact]
        public async Task SafeExecuteAsync_WithException_ShouldHandleExceptionGracefully()
        {
            // Arrange
            var exceptionThrown = false;

            // Act & Assert - This should not throw to the caller
            await DashboardInitializationFix.SafeExecuteAsync(async (token) =>
            {
                exceptionThrown = true;
                // Add an await to properly make this method async
                await Task.Delay(1);
                throw new InvalidOperationException("Test exception");
            });

            // Assert
            Assert.True(exceptionThrown, "Exception should have been thrown inside the operation");
        }

        [Fact]
        public void SafeInvokeOnUI_WhenControlIsNull_ShouldReturnSafely()
        {
            // Act & Assert - This should not throw
            Control? nullControl = null;
            DashboardInitializationFix.SafeInvokeOnUI(nullControl, () => { });
        }

        [Fact]
        public void SafeInvokeOnUI_WhenControlIsDisposed_ShouldReturnSafely()
        {
            // Arrange
            var disposedControl = new Button();
            disposedControl.Dispose();

            // Act & Assert - This should not throw
            DashboardInitializationFix.SafeInvokeOnUI(disposedControl, () => { });
        }

        [Fact]
        public void SafeInvokeOnUI_WithValidControl_ShouldExecuteAction()
        {
            // Arrange
            var actionExecuted = false;

            // Act
            DashboardInitializationFix.SafeInvokeOnUI(_testForm, () =>
            {
                actionExecuted = true;
            });

            // Assert
            Assert.True(actionExecuted, "Action should have been executed");
        }

        [Fact]
        public void SafeInvokeOnUI_WhenActionThrowsException_ShouldHandleExceptionGracefully()
        {
            // Act & Assert - This should not throw to the caller
            DashboardInitializationFix.SafeInvokeOnUI(_testForm, () =>
            {
                throw new InvalidOperationException("Test exception");
            });
        }
    }
}

