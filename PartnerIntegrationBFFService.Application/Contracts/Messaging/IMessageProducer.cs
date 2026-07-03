namespace PartnerIntegrationBFFService.Application.Contracts.Messaging
{
    public interface IMessageProducer<in T> where T : class
    {
        Task PublishAsync(T message, CancellationToken ct = default);
    }
}
