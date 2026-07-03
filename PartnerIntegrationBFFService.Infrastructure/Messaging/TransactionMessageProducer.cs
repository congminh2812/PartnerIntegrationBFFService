using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PartnerIntegrationBFFService.Application.Common.Exceptions;
using PartnerIntegrationBFFService.Application.Contracts.Messaging;
using PartnerIntegrationBFFService.Application.Features.Partners.Commands.PartnerTransactions;
using Polly;
using System.Text.Json;

namespace PartnerIntegrationBFFService.Infrastructure.Messaging
{
    public class TransactionMessageProducer(
        IProducer<string, string> producer,
        IOptions<KafkaOptions> options,
        ILogger<TransactionMessageProducer> logger) : IMessageProducer<PartnerTransactionsCommand>
    {
        private readonly KafkaOptions _options = options.Value;

        public async Task PublishAsync(PartnerTransactionsCommand message, CancellationToken ct = default)
        {
            var topic = _options.Topic;

            var jsonPayload = JsonSerializer.Serialize(message);

            var kafkaMessage = new Message<string, string>
            {
                Key = message.PartnerId,
                Value = jsonPayload
            };

            var retryPolicy = Policy
                .Handle<KafkaException>()
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            await retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var deliveryResult = await producer.ProduceAsync(topic, kafkaMessage, ct);

                    logger.LogInformation("Message delivered to {Topic} at offset {Offset}",
                        deliveryResult.Topic, deliveryResult.Offset);
                }
                catch (Exception ex)
                {
                    logger.LogError("Message delivered failed with {message}", ex.Message);
                    throw new BadRequestException(ex.Message);
                }


                return Task.CompletedTask;
            });


        }
    }
}
