using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PartnerIntegrationBFFService.Application.Features.Partners.Commands.PartnerTransactions;
using PartnerIntegrationBFFService.Infrastructure.Messaging;

namespace PartnerIntegrationBFFService.UnitTests
{
    public class TransactionMessageProducerTests
    {
        private readonly IOptions<KafkaOptions> _options = Options.Create(new KafkaOptions { Topic = "topic-test", BootstrapServers = "localhost:9092" });
        private readonly Mock<ILogger<TransactionMessageProducer>> _logger = new();

        [Fact]
        public async Task PublishAsync_WhenFailsThreeTimesThenSucceeds_AttemptsFourTimes()
        {
            // Arrange
            var producerMock = new Mock<IProducer<string, string>>(MockBehavior.Strict);

            // First three calls throw, fourth call succeeds.
            producerMock
                .SetupSequence(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KafkaException(new Error(ErrorCode.Unknown)))
                .ThrowsAsync(new KafkaException(new Error(ErrorCode.Unknown)))
                .ThrowsAsync(new KafkaException(new Error(ErrorCode.Unknown)))
                .ReturnsAsync(new DeliveryResult<string, string> { Topic = "topic-test", Offset = new Offset(7) });

            var sut = new TransactionMessageProducer(producerMock.Object, _options, _logger.Object);

            var message = new PartnerTransactionsCommand("p-key", "r", 1m, "USD", DateTime.UtcNow);

            // Act
            await sut.PublishAsync(message, CancellationToken.None);

            // Assert: initial attempt + 3 retries = 4
            producerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
        }

        [Fact]
        public async Task PublishAsync_SerializesMessage_KeyAndValueAreCorrect()
        {
            // Arrange
            var producerMock = new Mock<IProducer<string, string>>(MockBehavior.Strict);
            Message<string, string>? captured = null;

            producerMock
                .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
                .Callback<string, Message<string, string>, CancellationToken>((topic, msg, ct) => captured = msg)
                .ReturnsAsync(new DeliveryResult<string, string> { Topic = "topic-test", Offset = new Offset(1) });

            var sut = new TransactionMessageProducer(producerMock.Object, _options, _logger.Object);

            var partnerId = "partner-serialize";
            var txRef = "tx-serialize";
            var message = new PartnerTransactionsCommand(partnerId, txRef, 99.9m, "USD", DateTime.UtcNow);

            // Act
            await sut.PublishAsync(message, CancellationToken.None);

            // Assert
            Assert.NotNull(captured);
            Assert.Equal(partnerId, captured!.Key);
            Assert.NotNull(captured.Value);
            Assert.Contains(partnerId, captured.Value);
            Assert.Contains(txRef, captured.Value);
        }

        [Fact]
        public async Task PublishAsync_WhenAlwaysThrows_ThrowsKafkaExceptionAndAttemptsFourTimes()
        {
            // Arrange
            var producerMock = new Mock<IProducer<string, string>>(MockBehavior.Strict);

            producerMock
                .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KafkaException(new Error(ErrorCode.Unknown)));

            var sut = new TransactionMessageProducer(producerMock.Object, _options, _logger.Object);

            var message = new PartnerTransactionsCommand("p", "r", 1m, "USD", DateTime.UtcNow);

            // Act & Assert
            await Assert.ThrowsAsync<KafkaException>(() => sut.PublishAsync(message, CancellationToken.None));
            // Policy is configured with 3 retries -> total attempts = 4
            producerMock.Verify(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
        }
    }
}