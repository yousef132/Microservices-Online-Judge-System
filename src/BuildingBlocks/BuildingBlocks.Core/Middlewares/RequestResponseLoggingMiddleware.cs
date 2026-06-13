using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BuildingBlocks.Core.Middlewares;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestResponseLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        // 1. Log the Request
        context.Request.EnableBuffering();
        var requestBody = await ReadStreamInChunks(context.Request.Body);
        context.Request.Body.Position = 0;

        logger.LogInformation("HTTP Request Information: Method: {Method}, Path: {Path}, QueryString: {QueryString}, RequestBody: {RequestBody}", 
            context.Request.Method, 
            context.Request.Path, 
            context.Request.QueryString, 
            requestBody);

        // 2. Intercept the Response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);
            sw.Stop();

            // Log successful response or handled error response
            await LogResponse(context, responseBody, originalBodyStream, logger, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            // We just log the exception here. GlobalExceptionHandlerMiddleware will handle the actual response formatting.
            logger.LogError(ex, "An unhandled exception occurred during the request. Method: {Method}, Path: {Path}", 
                context.Request.Method, 
                context.Request.Path);
            
            // Re-throw so GlobalExceptionHandlerMiddleware can catch and format the problem details
            // The response body stream needs to be restored so the exception handler can write to it
            context.Response.Body = originalBodyStream;
            throw;
        }
    }

    private async Task LogResponse(HttpContext context, MemoryStream responseBody, Stream originalBodyStream, ILogger logger, long elapsedMs)
    {
        responseBody.Position = 0;
        var responseBodyContent = await new StreamReader(responseBody).ReadToEndAsync();
        responseBody.Position = 0;

        // Copy the content to the original stream so it returns to the client
        await responseBody.CopyToAsync(originalBodyStream);

        if (context.Response.StatusCode >= 400)
        {
            logger.LogError("HTTP Error Response: Status: {StatusCode}, Elapsed: {ElapsedMs}ms, ResponseBody: {ResponseBody}",
                context.Response.StatusCode,
                elapsedMs,
                responseBodyContent);
        }
        else
        {
            logger.LogInformation("HTTP Response: Status: {StatusCode}, Elapsed: {ElapsedMs}ms, ResponseBody: {ResponseBody}",
                context.Response.StatusCode,
                elapsedMs,
                responseBodyContent);
        }
    }

    private static async Task<string> ReadStreamInChunks(Stream stream)
    {
        const int readChunkBufferLength = 4096;
        stream.Seek(0, SeekOrigin.Begin);
        using var textWriter = new StringWriter();
        using var reader = new StreamReader(stream, Encoding.UTF8, false, readChunkBufferLength, true);
        var readChunk = new char[readChunkBufferLength];
        int readChunkLength;
        do
        {
            readChunkLength = await reader.ReadBlockAsync(readChunk, 0, readChunkBufferLength);
            textWriter.Write(readChunk, 0, readChunkLength);
        } while (readChunkLength > 0);
        return textWriter.ToString();
    }
}
