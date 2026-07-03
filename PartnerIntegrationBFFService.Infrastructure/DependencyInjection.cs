using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PartnerIntegrationBFFService.Application.Contracts.Messaging;
using PartnerIntegrationBFFService.Application.Contracts.Services;
using PartnerIntegrationBFFService.Application.Features.Partners.Commands.PartnerTransactions;
using PartnerIntegrationBFFService.Infrastructure.Messaging;
using PartnerIntegrationBFFService.Infrastructure.Services;
using Polly;
using Polly.Extensions.Http;

namespace PartnerIntegrationBFFService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<KafkaOptions>(
                configuration.GetSection("Kafka")
            );

            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            var partnerApiUrl = configuration["PartnerApi:BaseUrl"] ?? "";

            services.AddHttpClient<IPartnerVerificationService, PartnerVerificationService>(client =>
            {
                client.BaseAddress = new Uri(partnerApiUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyHandler(retryPolicy);

            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;

                var config = new ProducerConfig
                {
                    BootstrapServers = options.BootstrapServers,
                    Acks = Acks.All,
                    AllowAutoCreateTopics = true,
                    MessageTimeoutMs = 5000,
                    SocketTimeoutMs = 5000,
                };
                return new ProducerBuilder<string, string>(config).Build();
            });

            services.AddSingleton<IMessageProducer<PartnerTransactionsCommand>, TransactionMessageProducer>();

            return services;
        }
    }
}

