using Moq;
using Moq.Protected;
using PartnerIntegrationBFFService.Infrastructure.Services;
using System.Net;

namespace PartnerIntegrationBFFService.UnitTests
{
    public class PartnerVerificationServiceTests
    {
        [Fact]
        public async Task VerifyPartnerAsync_SuccessStatusCode_ReturnsTrue()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var httpClient = CreateHttpClient(response);
            var service = new PartnerVerificationService(httpClient);

            // Act
            var result = await service.VerifyPartnerAsync("partner-123");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyPartnerAsync_NotFoundStatusCode_ReturnsFalse()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            var httpClient = CreateHttpClient(response);
            var service = new PartnerVerificationService(httpClient);

            // Act
            var result = await service.VerifyPartnerAsync("partner-404");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task VerifyPartnerAsync_HttpClientThrows_PropagatesException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(() => Task.FromException<HttpResponseMessage>(new TimeoutException("Simulated timeout")));
            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };
            var service = new PartnerVerificationService(httpClient);

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(() => service.VerifyPartnerAsync("partner-timeout"));
        }

        private static HttpClient CreateHttpClient(HttpResponseMessage responseMessage)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage)
                .Verifiable();

            var client = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };

            return client;
        }
    }
}