using Xunit;

namespace BusBuddy.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void BasicTest_ShouldPass()
        {
            // Arrange
            var expected = true;

            // Act
            var actual = true;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void StringTest_ShouldValidateEquality()
        {
            // Arrange
            var expected = "BusBuddy";

            // Act
            var actual = "BusBuddy";

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NumberTest_ShouldValidateArithmetic()
        {
            // Arrange
            var num1 = 5;
            var num2 = 3;
            var expected = 8;

            // Act
            var actual = num1 + num2;

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
