using FluentValidation;
using PartnerIntegrationBFFService.API.Models;
using PartnerIntegrationBFFService.Application.Common.Exceptions;
using System.Net;

namespace PartnerIntegrationBFFService.API.Middlewares
{
    public class ExceptionMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            CustomValidationProbemDetails problem;

            switch (ex)
            {
                case BadRequestException BadRequest:
                    statusCode = HttpStatusCode.BadRequest;
                    problem = new CustomValidationProbemDetails
                    {
                        Title = BadRequest.Message,
                        Status = (int)statusCode,
                        Detail = BadRequest.InnerException?.Message,
                        Type = nameof(BadRequestException),
                        Errors = BadRequest.ValidationErrors,
                    };
                    break;
                case NotFoundException NotFound:
                    statusCode = HttpStatusCode.NotFound;
                    problem = new CustomValidationProbemDetails
                    {
                        Title = NotFound.Message,
                        Status = (int)statusCode,
                        Detail = NotFound.InnerException?.Message,
                        Type = nameof(NotFoundException),
                    };
                    break;
                case ValidationException Validation:
                    statusCode = HttpStatusCode.BadRequest;

                    var errorDict = Validation.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );

                    problem = new CustomValidationProbemDetails
                    {
                        Title = "Validation Failed",
                        Status = (int)statusCode,
                        Type = nameof(ValidationException),
                        Errors = errorDict
                    };
                    break;
                default:
                    problem = new CustomValidationProbemDetails
                    {
                        Title = ex.Message,
                        Status = (int)statusCode,
                        Detail = ex.StackTrace,
                        Type = nameof(HttpStatusCode.InternalServerError),
                    };
                    break;
            }

            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}

