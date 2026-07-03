using PartnerIntegrationBFFService.Application.Common.Exceptions;

namespace PartnerIntegrationBFFService.UnitTests
{
    public class NotFoundExceptionTests
    {
        [Fact]
        public void Message_IsFormattedCorrectly()
        {
            var ex = new NotFoundException("Entity", 123);
            Assert.Equal("Entity 123 was not found", ex.Message);
        }
    }
}