using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Users.API.Common.Exceptions;

namespace Users.API.Common.Middlewares
{
    internal sealed class GlobalExceptionHandlerMiddleware(
         RequestDelegate next,
         ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception occurred");

                // Make sure to set the status code before writing to the response body
                context.Response.StatusCode = ex switch
                {
                    ApplicationException => StatusCodes.Status400BadRequest,
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    InvalidOperationException => StatusCodes.Status409Conflict,
                    ConflictException => StatusCodes.Status409Conflict,
                    ValidationException => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status500InternalServerError
                };

                await context.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Type = ex.GetType().Name,
                        Title = "An error occured",
                        Detail = ex.Message
                    });
            }
        }
    }
}
