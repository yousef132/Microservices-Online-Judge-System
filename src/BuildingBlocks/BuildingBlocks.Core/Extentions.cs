using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BuildingBlocks.Core;

public static class Extentions
{
    
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        var assemblyName = entryAssembly?.GetName().Name;
        var assemblyVersion = entryAssembly?.GetName().Version?.ToString();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = $"{assemblyName} API",
                Version = assemblyVersion ?? "v1",
                Description = $"{assemblyName} Web API Documentation"
            });

            options.AddSecurityDefinition("JWT", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "Enter your JWT token only"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "JWT"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
    
}