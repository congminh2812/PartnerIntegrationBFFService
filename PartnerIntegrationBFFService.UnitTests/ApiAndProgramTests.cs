using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace PartnerIntegrationBFFService.UnitTests
{
    public class ApiDependencyInjectionTests
    {
        [Fact]
        public void AddCustomSerilog_DoesNotThrow_ReturnsBuilder()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder(new string[] { });

            // Act
            var returned = PartnerIntegrationBFFService.API.DependencyInjection.AddCustomSerilog(builder);

            // Assert
            Assert.Same(builder, returned);
        }
    }

    public class ProgramStartupTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProgramStartupTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }
    }
}