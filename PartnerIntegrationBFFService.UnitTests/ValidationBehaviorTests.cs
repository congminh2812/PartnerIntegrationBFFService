using FluentValidation;
using MediatR;
using PartnerIntegrationBFFService.Application.Common.Behaviors;

namespace PartnerIntegrationBFFService.UnitTests
{
    public class ValidationBehaviorTests
    {
        private sealed record Req(string Value) : IRequest<string>;

        private class ReqValidator : AbstractValidator<Req>
        {
            public ReqValidator()
            {
                RuleFor(x => x.Value).NotEmpty();
            }
        }

        [Fact]
        public async Task Handle_WithValidationFailures_ThrowsValidationException()
        {
            // Arrange
            var validators = new List<IValidator<Req>> { new ReqValidator() };
            var behavior = new ValidationBehavior<Req, string>(validators);

            var request = new Req(string.Empty);

            RequestHandlerDelegate<string> next = (CancellationToken ct) => Task.FromResult("ok");

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(request, next, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNoFailures_InvokesNext()
        {
            // Arrange
            var validators = new List<IValidator<Req>> { new ReqValidator() };
            var behavior = new ValidationBehavior<Req, string>(validators);

            var request = new Req("non-empty");

            var nextCalled = false;
            RequestHandlerDelegate<string> next = (CancellationToken ct) =>
            {
                nextCalled = true;
                return Task.FromResult("ok");
            };

            // Act
            var result = await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            Assert.True(nextCalled);
            Assert.Equal("ok", result);
        }
    }
}