using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using PartnerIntegrationBFFService.Application.Common.Behaviors;

namespace PartnerIntegrationBFFService.UnitTests
{
    public sealed record FakeRequest(string Data) : IRequest<FakeResponse>;
    public sealed record FakeResponse(string Result);

    public class LoggingBehaviorTests
    {
        [Fact]
        public async Task Handle_InvokesNextAndLogsStartAndEnd()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoggingBehavior<FakeRequest, FakeResponse>>>();
            var behavior = new LoggingBehavior<FakeRequest, FakeResponse>(loggerMock.Object);

            var request = new FakeRequest("payload");

            RequestHandlerDelegate<FakeResponse> next = (CancellationToken ct) => Task.FromResult(new FakeResponse("ok"));

            // Act
            var response = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("ok", response.Result);

            // Verify that start and end log messages were written
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[START]")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[END]")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task Handle_LongRunningRequest_LogsWarningForPerformance()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoggingBehavior<FakeRequest, FakeResponse>>>();
            var behavior = new LoggingBehavior<FakeRequest, FakeResponse>(loggerMock.Object);

            var request = new FakeRequest("payload");

            RequestHandlerDelegate<FakeResponse> next = async (CancellationToken ct) =>
            {
                // LoggingBehavior checks `timeTaken.Seconds > 3` (whole seconds component),
                // so delay must exceed 4 seconds to satisfy the condition reliably.
                await Task.Delay(TimeSpan.FromSeconds(4.1), ct);
                return new FakeResponse("slow");
            };

            // Act
            var response = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            Assert.Equal("slow", response.Result);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[PERFORMANCE]")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}