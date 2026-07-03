using Microsoft.Extensions.Logging;
using Moq;
using PartnerIntegrationBFFService.Application.Common.Exceptions;
using PartnerIntegrationBFFService.Application.Contracts.Messaging;
using PartnerIntegrationBFFService.Application.Contracts.Services;
using PartnerIntegrationBFFService.Application.Features.Partners.Commands.PartnerTransactions;

namespace PartnerIntegrationBFFService.UnitTests
{
    public class PartnerTransactionsHandlerTests
    {
        [Fact]
        public async Task Handle_VerifiedAndPublishSucceeds_ReturnsSuccess()
        {
            // Arrange
            var verificationMock = new Mock<IPartnerVerificationService>();
            verificationMock.Setup(x => x.VerifyPartnerAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var producerMock = new Mock<IMessageProducer<PartnerTransactionsCommand>>();
            producerMock.Setup(x => x.PublishAsync(It.IsAny<PartnerTransactionsCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<PartnerTransactionsHandler>>();

            var handler = new PartnerTransactionsHandler(
                verificationMock.Object,
                producerMock.Object,
                loggerMock.Object);

            var command = new PartnerTransactionsCommand("partner-1", "tx-1", 10m, "USD", DateTime.UtcNow);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            producerMock.Verify(x => x.PublishAsync(It.IsAny<PartnerTransactionsCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_VerificationReturnsFalse_ThrowsBadRequestException()
        {
            // Arrange
            var verificationMock = new Mock<IPartnerVerificationService>();
            verificationMock.Setup(x => x.VerifyPartnerAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            var producerMock = new Mock<IMessageProducer<PartnerTransactionsCommand>>();
            var loggerMock = new Mock<ILogger<PartnerTransactionsHandler>>();

            var handler = new PartnerTransactionsHandler(
                verificationMock.Object,
                producerMock.Object,
                loggerMock.Object);

            var command = new PartnerTransactionsCommand("partner-2", "tx-2", 5m, "USD", DateTime.UtcNow);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
            producerMock.Verify(x => x.PublishAsync(It.IsAny<PartnerTransactionsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_PublishThrows_ThrowsBadRequestException()
        {
            // Arrange
            var verificationMock = new Mock<IPartnerVerificationService>();
            verificationMock.Setup(x => x.VerifyPartnerAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var producerMock = new Mock<IMessageProducer<PartnerTransactionsCommand>>();
            producerMock.Setup(x => x.PublishAsync(It.IsAny<PartnerTransactionsCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Publish failed"));

            var loggerMock = new Mock<ILogger<PartnerTransactionsHandler>>();

            var handler = new PartnerTransactionsHandler(
                verificationMock.Object,
                producerMock.Object,
                loggerMock.Object);

            var command = new PartnerTransactionsCommand("partner-3", "tx-3", 15m, "USD", DateTime.UtcNow);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
            producerMock.Verify(x => x.PublishAsync(It.IsAny<PartnerTransactionsCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_VerificationThrows_PropagatesException()
        {
            // Arrange
            var verificationMock = new Mock<IPartnerVerificationService>();
            verificationMock.Setup(x => x.VerifyPartnerAsync(It.IsAny<string>()))
                .ThrowsAsync(new TimeoutException("Verification timed out"));

            var producerMock = new Mock<IMessageProducer<PartnerTransactionsCommand>>();
            var loggerMock = new Mock<ILogger<PartnerTransactionsHandler>>();

            var handler = new PartnerTransactionsHandler(
                verificationMock.Object,
                producerMock.Object,
                loggerMock.Object);

            var command = new PartnerTransactionsCommand("partner-4", "tx-4", 20m, "USD", DateTime.UtcNow);

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(() => handler.Handle(command, CancellationToken.None));
            producerMock.Verify(x => x.PublishAsync(It.IsAny<PartnerTransactionsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}