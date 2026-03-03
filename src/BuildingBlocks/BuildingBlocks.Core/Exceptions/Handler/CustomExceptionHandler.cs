using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Core.Exceptions.Handler
{
   using BuildingBlocks.Core.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Core.Exceptions.Handler
{
    public sealed class GlobalExceptionHandler
        (ILogger<GlobalExceptionHandler> logger)
        : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            logger.LogError(exception,
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

            var problemDetails = new ProblemDetails
            {
                Title = title,
                Detail = detail,
                Status = statusCode,
                Instance = context.Request.Path,
                Type = exception.GetType().Name
            };

            problemDetails.Extensions["traceId"] = context.TraceIdentifier;

            // FluentValidation support
            if (exception is FluentValidation.ValidationException fluentValidationException)
            {
                problemDetails.Extensions["validationErrors"] =
                    fluentValidationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );
            }

            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
}
