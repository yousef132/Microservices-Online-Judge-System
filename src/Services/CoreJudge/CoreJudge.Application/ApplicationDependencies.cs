using System.Reflection;
using BuildingBlocks.Core.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreJudge.Application;

public static class ApplicationDependencies
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(ApplicationDependencies).Assembly;

        // services.AddScoped<IClaimsTransformation, KeycloakClaimsTransformer>();
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient<IActionContextAccessor, ActionContextAccessor>();
        services.AddHttpContextAccessor();

        // auto mapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        return services;
    }
}