using System;
using System.Threading.Tasks;
using Xunit;

namespace BusBuddy.Tests.Simple
{
    /// <summary>
    /// Extremely simple test class with no dependencies at all
    /// </summary>
    public class StandaloneTests
    {
        [Fact]
        public async Task DelayTest_ShouldComplete()
        {
            // Just testing the test framework itself
            await Task.Delay(100);
            Assert.True(true);
        }
    }
}
