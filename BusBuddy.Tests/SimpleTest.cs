using Xunit;

namespace BusBuddy.Tests
{
    public class SimpleTest
    {
        [Fact]
        public void SimpleTest_ShouldPass()
        {
            // Arrange
            var expected = 2;

            // Act
            var result = 1 + 1;

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
