using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PartnerIntegrationBFFService.Application.Contracts.Messaging;
using PartnerIntegrationBFFService.Application.Contracts.Services;
using PartnerIntegrationBFFService.Application.Features.Partners.Commands.PartnerTransactions;
using PartnerIntegrationBFFService.Infrastructure;

namespace PartnerIntegrationBFFService.UnitTests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void AddInfrastructureServices_RegistersExpectedServices()
        {
            // Arrange
            var inMemory = new[] { new KeyValuePair<string, string>("PartnerApi:BaseUrl", "http://localhost") };
            var config = new ConfigurationBuilder().AddInMemoryCollection(inMemory!).Build();

            var services = new ServiceCollection();

            // Act
            services.AddInfrastructureServices(config);

            // Assert: IPartnerVerificationService registered (typed http client)
            Assert.Contains(services, d => d.ServiceType == typeof(IPartnerVerificationService));

            // Assert: IMessageProducer<PartnerTransactionsCommand> registered
            Assert.Contains(services, d => d.ServiceType == typeof(IMessageProducer<PartnerTransactionsCommand>));

            // Assert: IProducer<string,string> (Kafka producer) registered as singleton
            Assert.Contains(services, d => d.ServiceType == typeof(IProducer<string, string>));
        }
    }
}