using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PartnerIntegrationBFFService.API.Controllers.V1;
using PartnerIntegrationBFFService.Application.Features.Partners.Commands.PartnerTransactions;

namespace PartnerIntegrationBFFService.UnitTests
{
    public class PartnerControllerTests
    {
        [Fact]
        public async Task Transactions_SenderReturnsResult_ReturnsOkWithResult()
        {
            // Arrange
            var senderMock = new Mock<ISender>();
            var expected = new PartnerTransactionsResult(IsSuccess: true);

            senderMock
                .Setup(s => s.Send(It.IsAny<PartnerTransactionsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var controller = new PartnerController(senderMock.Object);

            var command = new PartnerTransactionsCommand("partner-1", "ref-1", 10m, "USD", DateTime.UtcNow);

            // Act
            var actionResult = await controller.Transactions(command);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(actionResult);
            var actual = Assert.IsType<PartnerTransactionsResult>(ok.Value);
            Assert.True(actual.IsSuccess);
            senderMock.Verify(s => s.Send(It.Is<PartnerTransactionsCommand>(c => c.PartnerId == command.PartnerId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Transactions_SenderThrows_PropagatesException()
        {
            // Arrange
            var senderMock = new Mock<ISender>();
            senderMock
                .Setup(s => s.Send(It.IsAny<PartnerTransactionsCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("send failed"));

            var controller = new PartnerController(senderMock.Object);

            var command = new PartnerTransactionsCommand("partner-2", "ref-2", 5m, "USD", DateTime.UtcNow);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => controller.Transactions(command));
            senderMock.Verify(s => s.Send(It.IsAny<PartnerTransactionsCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}