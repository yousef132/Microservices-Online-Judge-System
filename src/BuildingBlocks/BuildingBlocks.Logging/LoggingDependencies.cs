using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using Serilog;

namespace BuildingBlocks.Logging;

public static class LoggingDependencies
{
    public static IServiceCollection AddLoggingConfigs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var assemblyName = Assembly.GetCallingAssembly().GetName().Name!;

        // اقرأ URL من IConfiguration بدل DotNetEnv
        var jaegerUrl = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317";

        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource.AddService(serviceName: assemblyName))
            .WithTracing(tracing =>
            {
                // Incoming HTTP + gRPC requests
                tracing.AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnableAspNetCoreSignalRSupport = true;
                });

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

                // Export to Jaeger via OTLP
                tracing.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(jaegerUrl); // Jaeger OTLP port
                });

                // Optional: always sample all traces
                tracing.SetSampler(new AlwaysOnSampler());
            });

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Debug()
            //.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day) // optional file logging
            //.WriteTo.Seq("http://localhost:5341") // optional Seq logging
            .CreateLogger();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddSerilog(dispose: true); // add Serilog
        });

        return services;
    }
}
