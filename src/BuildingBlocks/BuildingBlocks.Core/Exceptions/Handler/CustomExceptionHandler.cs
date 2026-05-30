using Community.API.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BuildingBlocks.Core.Exceptions.Handler
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception,
                "Unhandled exception occurred at {Time}",
                DateTime.UtcNow);

            var (title, detail, statusCode) = exception switch
            {
                // 400
                ApplicationException =>
                    (exception.GetType().Name, exception.Message, StatusCodes.Status400BadRequest),

                FluentValidation.ValidationException =>
                    (exception.GetType().Name, exception.Message, StatusCodes.Status400BadRequest),

                BadRequestException =>
                    (exception.GetType().Name, exception.Message, StatusCodes.Status400BadRequest),

                // 401
                UnauthorizedAccessException =>
                    (exception.GetType().Name, exception.Message, StatusCodes.Status401Unauthorized),

                UnAuthorizedException =>
                    (exception.GetType().Name, exception.Message, StatusCodes.Status401Unauthorized),

                // 404
                NotFoundException =>
                    (exception.GetType().Name, exception.Message, StatusCodes.Status404NotFound),

                KeyNotFoundException =>
                    (exception.GetType().Name, exception.Message, StatusCodes.Status404NotFound),

                // 409
                ConflictException =>
                    (exception.GetType().Name, exception.Message, StatusCodes.Status409Conflict),

                InvalidOperationException =>
                    (exception.GetType().Name, exception.Message, StatusCodes.Status409Conflict),

                // 500
                InternalServerException =>
                    (exception.GetType().Name, exception.Message, StatusCodes.Status500InternalServerError),

                _ =>
                    ("Internal Server Error", exception.Message, StatusCodes.Status500InternalServerError)
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var problemDetails = new
            {
                Title = title,
                Detail = detail,
                Status = statusCode,
                Instance = context.Request.Path,
                Type = exception.GetType().Name,
                traceId = context.TraceIdentifier,
                validationErrors = exception is FluentValidation.ValidationException fluentValidationException
                    ? fluentValidationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        )
                    : null
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsJsonAsync(problemDetails, jsonOptions);
        }
    }
}