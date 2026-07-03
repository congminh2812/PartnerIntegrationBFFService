using PartnerIntegrationBFFService.Application.Common.Constants;

namespace PartnerIntegrationBFFService.UnitTests
{
    public class CurrencyConstantsTests
    {
        [Theory]
        [InlineData("USD", true)]
        [InlineData("usd", true)]
        [InlineData("VND", true)]
        [InlineData("XYZ", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsValid_ReturnsExpected(string? code, bool expected)
        {
            var actual = CurrencyConstants.IsValid(code!);
            Assert.Equal(expected, actual);
        }
    }
}