using Microsoft.Extensions.Logging;
using PartnerIntegrationBFFService.Application.Common.CQRS;
using PartnerIntegrationBFFService.Application.Common.Exceptions;
using PartnerIntegrationBFFService.Application.Contracts.Messaging;
using PartnerIntegrationBFFService.Application.Contracts.Services;

namespace PartnerIntegrationBFFService.Application.Features.Partners.Commands.PartnerTransactions
{
    public sealed class PartnerTransactionsHandler(
        IPartnerVerificationService verificationService,
        IMessageProducer<PartnerTransactionsCommand> messageProducer,
        ILogger<PartnerTransactionsHandler> logger) : ICommandHandler<PartnerTransactionsCommand, PartnerTransactionsResult>
    {
        public async Task<PartnerTransactionsResult> Handle(PartnerTransactionsCommand request, CancellationToken ct)
        {
            logger.LogInformation("Processing transaction for partner: {PartnerId}", request.PartnerId);

            // 1. Gọi Verification API
            var isVerified = await verificationService.VerifyPartnerAsync(request.PartnerId);

            if (!isVerified)
            {
                logger.LogWarning("Partner {PartnerId} verification failed.", request.PartnerId);
                throw new BadRequestException("Partner verification failed.");
            }

            // 2. Gửi vào Kafka
            try
            {
                await messageProducer.PublishAsync(request, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish transaction for partner {PartnerId} to Kafka.", request.PartnerId);
                throw new BadRequestException("Internal processing error.");
            }

            return new PartnerTransactionsResult(IsSuccess: true);
        }
    }
}
