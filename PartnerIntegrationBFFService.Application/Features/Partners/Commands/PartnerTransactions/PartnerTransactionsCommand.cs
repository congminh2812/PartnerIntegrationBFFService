using FluentValidation;
using PartnerIntegrationBFFService.Application.Common.Constants;
using PartnerIntegrationBFFService.Application.Common.CQRS;

namespace PartnerIntegrationBFFService.Application.Features.Partners.Commands.PartnerTransactions
{
    public sealed record PartnerTransactionsCommand(
        string PartnerId,
        string TransactionReference,
        decimal Amount,
        string Currency,
        DateTime Timestamp
    ) : ICommand<PartnerTransactionsResult>;

    public sealed record PartnerTransactionsResult(bool IsSuccess);

    public sealed class PartnerTransactionsCommandValidator : AbstractValidator<PartnerTransactionsCommand>
    {
        public PartnerTransactionsCommandValidator()
        {
            RuleFor(x => x.PartnerId)
            .NotEmpty().WithMessage("PartnerId is required.");

            RuleFor(x => x.TransactionReference)
                .NotEmpty().WithMessage("TransactionReference is required.");

            RuleFor(x => x.Amount)
                .NotEmpty().WithMessage("Amount is required.")
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .Length(3).WithMessage("Currency must be a 3-letter ISO code.")
                .Must(CurrencyConstants.IsValid)
                .WithMessage("Currency code '{PropertyValue}' is not a valid or supported currency.");

            RuleFor(x => x.Timestamp)
                .NotEmpty().WithMessage("Timestamp is required.");
        }
    }
}
