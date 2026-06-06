using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using StackExchange.Redis;
using System.Reflection;

namespace BuildingBlocks.Logging;

public static class LoggingDependencies
{
    public static IServiceCollection AddLoggingConfigs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var assemblyName = Assembly.GetCallingAssembly().GetName().Name!;

        // Read the OTel Collector OTLP endpoint from configuration.
        // In Docker: set OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
        // Locally:   defaults to http://localhost:4317
        var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
                           ?? configuration["Jaeger:OTEL_EXPORTER_OTLP_ENDPOINT"]
                           ?? "http://localhost:4317";

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

        // ── LOGS (Serilog → OTLP → OTel Collector → Elasticsearch) ────
        // Serilog writes structured logs; the OpenTelemetry log bridge
        // forwards them via OTLP to the collector, which sends to ES.
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("service.name", assemblyName)
            .MinimumLevel.Information()
            //.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day) // optional file logging
            //.WriteTo.Seq("http://localhost:5341") // optional Seq logging
            .CreateLogger();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddSerilog(dispose: true);

            // OpenTelemetry log bridge: captures ILogger calls and
            // exports them via OTLP to the collector → Elasticsearch
            loggingBuilder.AddOpenTelemetry(otelLogging =>
            {
                otelLogging.IncludeScopes = true;
                otelLogging.IncludeFormattedMessage = true;
                otelLogging.ParseStateValues = true;

                otelLogging.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                });
            });
        });

        return services;
    }
}
