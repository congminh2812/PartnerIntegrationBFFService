using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PartnerIntegrationBFFService.Application;
using PartnerIntegrationBFFService.Application.Features.Partners.Commands.PartnerTransactions;

namespace PartnerIntegrationBFFService.UnitTests
{
    public class ApplicationDependencyInjectionTests
    {
        [Fact]
        public void AddApplicationServices_RegistersBehaviorsAndValidators()
        {
            // Arrange
            var config = new ConfigurationBuilder().Build();
            var services = new ServiceCollection();

            // Add logging before MediatR registration to satisfy MediatR dependency
            services.AddLogging();

            // Act
            services.AddApplicationServices(config);
            var provider = services.BuildServiceProvider();

            // Assert: MediatR should be registered (IMediator)
            var mediator = provider.GetService<IMediator>();
            Assert.NotNull(mediator);

            // Assert: pipeline behaviors for the concrete command should be resolvable
            var behaviors = provider.GetService<IEnumerable<IPipelineBehavior<PartnerTransactionsCommand, PartnerTransactionsResult>>>();
            Assert.NotNull(behaviors);
            Assert.True(behaviors.Count() >= 2);
        }
    }
}