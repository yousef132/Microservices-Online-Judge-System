using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using System.Reflection;

namespace BuildingBlocks.Logging;

public static class LoggingDependencies
{
    public static IHostApplicationBuilder AddLoggingConfigs(
        this IHostApplicationBuilder appBuilder,
        IConfiguration configuration)
    {
        var services = appBuilder.Services;
        // Use entry assembly (the running service) — GetCallingAssembly() would return
        // BuildingBlocks.Logging (the library), not the service that called this method.
        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? "UnknownService";

        // Read the OTel Collector OTLP endpoint from configuration.
        // In Docker: set OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
        // Locally:   defaults to http://localhost:4317
        var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        Console.WriteLine($"OTLP Endpoint: {otlpEndpoint}");
        Console.WriteLine($"JWT Key: {configuration["Jwt:Key"]}");

        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource
                    .AddService(serviceName: assemblyName)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["deployment.environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development",
                    }))

            // ── TRACES ──────────────────────────────────────────────────
            .WithTracing(tracing =>
            {
                // Incoming HTTP + gRPC requests
                tracing.AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnableAspNetCoreSignalRSupport = true;
                });
                // MongoDB — uses DiagnosticSources via AddSource
                tracing.AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources");

                // Outgoing HTTP calls
                tracing.AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                });

                // PostgreSQL
                tracing.AddNpgsql();

                // Redis
                tracing.AddRedisInstrumentation();

                // gRPC client
                tracing.AddGrpcClientInstrumentation();

                // Custom spans from your code
                tracing.AddSource(assemblyName);

                // Export traces → OTel Collector (which forwards to Jaeger)
                tracing.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                });

                // Sample all traces (adjust in production)
                tracing.SetSampler(new AlwaysOnSampler());
            })

            // ── METRICS ─────────────────────────────────────────────────
            .WithMetrics(metrics =>
            {
                // Built-in ASP.NET Core metrics (request counts, latency, etc.)
                metrics.AddAspNetCoreInstrumentation();

                // Outgoing HTTP client metrics
                metrics.AddHttpClientInstrumentation();

                // .NET runtime metrics (GC, thread pool, memory)
                metrics.AddRuntimeInstrumentation();

                // Custom meters from your code
                metrics.AddMeter(assemblyName);

                // Export metrics → OTel Collector (which exposes to Prometheus)
                metrics.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                });
            });


        // LOG FLOW:
        // ILogger<T>
        //     → OpenTelemetry Logging Provider  (registered on builder.Logging, not services.AddLogging)
        //     → OTLP Exporter
        //     → OpenTelemetry Collector
        //     → Elasticsearch
        //
        // IMPORTANT: Must be registered via builder.Logging (IHostApplicationBuilder.Logging),
        // NOT services.AddLogging(). ASP.NET Core seals the logging pipeline during host build;
        // calling services.AddLogging() after the fact adds the provider to the DI container
        // but the host's ILogger factory has already been configured and won't pick it up.
        appBuilder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;

            options.SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(assemblyName)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["deployment.environment"] =
                            configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development"
                    }));

            options.AddOtlpExporter(otlp =>
            {
                otlp.Endpoint = new Uri(otlpEndpoint);
            });
        });

        return appBuilder;
    }
}
