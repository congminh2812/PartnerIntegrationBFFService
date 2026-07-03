using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using PartnerIntegrationBFFService.API.Middlewares;
using PartnerIntegrationBFFService.Application.Common.Exceptions;
using System.Text.Json;

namespace PartnerIntegrationBFFService.UnitTests
{
    public class ExceptionMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_BadRequestException_Returns400WithValidationErrors()
        {
            // Arrange
            var failures = new ValidationResult(new[] { new ValidationFailure("FieldA", "required") });
            RequestDelegate next = _ => throw new BadRequestException("Bad request", failures);

            var middleware = new ExceptionMiddleware(next);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var doc = await JsonDocument.ParseAsync(context.Response.Body);

            // Assert
            Assert.Equal(400, context.Response.StatusCode);
            Assert.Equal("Bad request", doc.RootElement.GetProperty("title").GetString());
            Assert.True(doc.RootElement.TryGetProperty("errors", out var errors));
            Assert.Contains(errors.EnumerateObject(), p => p.NameEquals("FieldA"));
        }

        [Fact]
        public async Task InvokeAsync_ValidationException_Returns400WithErrors()
        {
            // Arrange
            var failures = new[] { new ValidationFailure("X", "err") };
            RequestDelegate next = _ => throw new ValidationException(failures);

            var middleware = new ExceptionMiddleware(next);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var doc = await JsonDocument.ParseAsync(context.Response.Body);

            // Assert
            Assert.Equal(400, context.Response.StatusCode);
            Assert.Equal("Validation Failed", doc.RootElement.GetProperty("title").GetString());
            Assert.True(doc.RootElement.TryGetProperty("errors", out var errors));
            Assert.Contains(errors.EnumerateObject(), p => p.NameEquals("X"));
        }

        [Fact]
        public async Task InvokeAsync_GenericException_Returns500()
        {
            // Arrange
            RequestDelegate next = _ => throw new Exception("boom");

            var middleware = new ExceptionMiddleware(next);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var doc = await JsonDocument.ParseAsync(context.Response.Body);

            // Assert
            Assert.Equal(500, context.Response.StatusCode);
            Assert.Equal("boom", doc.RootElement.GetProperty("title").GetString());
            Assert.Equal("InternalServerError", doc.RootElement.GetProperty("type").GetString());
        }
    }
}