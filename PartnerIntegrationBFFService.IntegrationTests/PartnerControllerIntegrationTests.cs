using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using PartnerIntegrationBFFService.Application.Contracts.Messaging;
using PartnerIntegrationBFFService.Application.Contracts.Services;
using PartnerIntegrationBFFService.Application.Features.Partners.Commands.PartnerTransactions;
using System.Net;
using System.Net.Http.Json;

namespace PartnerIntegrationBFFService.IntegrationTests
{
    public class PartnerControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public PartnerControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task PostTransactions_VerifiedAndPublished_ReturnsOkAndPublishesMessage()
        {
            // Arrange: replace verification and producer with test fakes
            var fakeProducer = new FakeProducer();
            var fakeVerifier = new FakeVerificationService(true);

            using var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing IMessageProducer<PartnerTransactionsCommand> descriptor(s)
                    var producerDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMessageProducer<PartnerTransactionsCommand>));
                    if (producerDescriptor != null) services.Remove(producerDescriptor);

                    // Replace with the fake producer
                    services.AddSingleton<IMessageProducer<PartnerTransactionsCommand>>(fakeProducer);

                    // Remove and replace verification service
                    var verifierDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IPartnerVerificationService));
                    if (verifierDescriptor != null) services.Remove(verifierDescriptor);

                    services.AddSingleton<IPartnerVerificationService>(fakeVerifier);
                });
            }).CreateClient();

            var command = new PartnerTransactionsCommand(
                PartnerId: "integration-partner",
                TransactionReference: "tx-100",
                Amount: 123.45m,
                Currency: "USD",
                Timestamp: DateTime.UtcNow
            );

            // Act
            var response = await client.PostAsJsonAsync("/api/v1/Partner/transactions", command);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<PartnerTransactionsResult>();
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);

            Assert.True(fakeProducer.Published);
            Assert.NotNull(fakeProducer.LastMessage);
            Assert.Equal(command.PartnerId, fakeProducer.LastMessage.PartnerId);
            Assert.Equal(command.TransactionReference, fakeProducer.LastMessage.TransactionReference);
        }

        // Simple test double for IMessageProducer<T>
        private class FakeProducer : IMessageProducer<PartnerTransactionsCommand>
        {
            public bool Published { get; private set; }
            public PartnerTransactionsCommand? LastMessage { get; private set; }

            public Task PublishAsync(PartnerTransactionsCommand message, CancellationToken ct = default)
            {
                Published = true;
                LastMessage = message;
                return Task.CompletedTask;
            }
        }

        // Simple test double for IPartnerVerificationService
        private class FakeVerificationService : IPartnerVerificationService
        {
            private readonly bool _result;
            private readonly Exception? _toThrow;

            public FakeVerificationService(bool result) => _result = result;

            public FakeVerificationService(Exception ex) { _toThrow = ex; _result = false; }

            public Task<bool> VerifyPartnerAsync(string partnerId)
            {
                if (_toThrow is not null) return Task.FromException<bool>(_toThrow);
                return Task.FromResult(_result);
            }
        }
    }
}