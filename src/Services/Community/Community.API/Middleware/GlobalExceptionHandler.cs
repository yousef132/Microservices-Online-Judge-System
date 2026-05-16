using System.Net;
using System.Text.Json;
using Community.API.Common.Exceptions;
using FluentValidation;

namespace Community.API.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (NotFoundException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            var response = new ErrorResponse { Status = 404, Code = "NOT_FOUND", Message = ex.Message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
        }
        catch (ForbiddenAccessException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            var response = new ErrorResponse { Status = 403, Code = "FORBIDDEN", Message = ex.Message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
        }
        catch (UnauthorizedAccessException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            var response = new ErrorResponse { Status = 401, Code = "UNAUTHORIZED", Message = ex.Message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var response = new ErrorResponse
            {
                Status = 500,
                Code = "INTERNAL_SERVER_ERROR",
                Message = "An unexpected error occurred. Please try again later."
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
        }
    }

    private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var errors = exception.Errors
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());

        var response = new ErrorResponse
        {
            Status = 400,
            Code = "VALIDATION_ERROR",
            Message = "One or more validation errors occurred.",
            Errors = errors
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
