using PartnerIntegrationBFFService.Application.Features.Partners.Commands.PartnerTransactions;

namespace PartnerIntegrationBFFService.UnitTests
{
    public class PartnerTransactionsCommandValidatorTests
    {
        private readonly PartnerTransactionsCommandValidator _validator = new();

        [Fact]
        public void Validate_ValidCommand_Passes()
        {
            // Arrange
            var cmd = new PartnerTransactionsCommand("partner-1", "ref-1", 10m, "USD", DateTime.UtcNow);

            // Act
            var result = _validator.Validate(cmd);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_MissingPartnerId_Fails()
        {
            var cmd = new PartnerTransactionsCommand(string.Empty, "ref-1", 10m, "USD", DateTime.UtcNow);

            var result = _validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(PartnerTransactionsCommand.PartnerId));
        }

        [Fact]
        public void Validate_ZeroOrNegativeAmount_Fails()
        {
            var cmd = new PartnerTransactionsCommand("partner-1", "ref-1", 0m, "USD", DateTime.UtcNow);

            var result = _validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(PartnerTransactionsCommand.Amount));
        }

        [Fact]
        public void Validate_InvalidCurrency_Fails()
        {
            var cmd = new PartnerTransactionsCommand("partner-1", "ref-1", 5m, "XXX", DateTime.UtcNow);

            var result = _validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(PartnerTransactionsCommand.Currency));
        }
    }
}