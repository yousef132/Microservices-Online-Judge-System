using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Core.Middlewares;

public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next,ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            // Try to add it to the request headers for downstream processing, though ASP.NET Core request headers are not always modifiable directly
            try
            {
                context.Request.Headers.TryAdd(HeaderName, correlationId);
                _logger.LogWarning($"Missing Correlation-Id from request header, assigned new one : {correlationId}");
            }
            catch
            {
                // Ignore if headers are read-only
            }
        }

        // To ensure it is passed to the response headers
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(HeaderName))
            {
                context.Response.Headers.Add(HeaderName, correlationId);
            }
            return Task.CompletedTask;
        });

        // Store in HttpContext items for easy access by other components
        context.Items["CorrelationId"] = correlationId.ToString();

        // Push correlationId into the logging scope
        var scope = new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId.ToString()
        };

        using (logger.BeginScope(scope))
        {
            await _next(context);
        }
    }
}
