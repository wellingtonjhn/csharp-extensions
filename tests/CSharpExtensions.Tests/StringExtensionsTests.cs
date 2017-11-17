using CSharpExtensions.Core;
using Xunit;

namespace CSharpExtensions.Tests
{
    [Trait("Extensions", "StringExtensions")]
    public class StringExtensionsTests
    {
        [Fact]
        public void Should_ReturnTrue_When_IsNullOrEmpty()
        {
            var name = "";
            var email = string.Empty;
            string password = null;

            Assert.True(name.IsNullOrEmpty());
            Assert.True(email.IsNullOrEmpty());
            Assert.True(password.IsNullOrEmpty());
        }
        
        [Theory]
        [InlineData("23455", 23455)]
        [InlineData("82", 82)]
        [InlineData("2342", 2342)]
        public void Should_ConvertToInt32_When_HasLiteralNumbers(string value, int expectedNumbers)
        {
            var result = value.ToInt32();
            Assert.Equal(expectedNumbers, result);
        }

        [Theory]
        [InlineData("1234231432143214321", 1234231432143214321)]
        [InlineData("645643654365234", 645643654365234)]
        public void Should_ConvertToInt64_When_HasLiteralNumbers(string value, long expectedNumbers)
        {
            var result = value.ToInt64();
            Assert.Equal(expectedNumbers, result);
        }
    }
}
