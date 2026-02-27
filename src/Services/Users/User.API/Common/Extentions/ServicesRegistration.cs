using BuildingBlocks.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Users.API.Domain.Models;
using Users.API.Infrastructure;
using Users.API.Infrastructure.Repository.Abstractions;
using Users.API.Infrastructure.Repository.Implementations;
using Users.API.Services;
using Users.API.Services.Abstraction;

namespace Users.API.Common.Extentions;

public static class ServicesRegistration
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        // services.AddTransient<AdminKeyCloakAuthDelegatingHandler>();
        services.AddDbContext<UserDbContext>(opt => { opt.UseNpgsql(configuration.GetConnectionString("Default")); });
        // services.Configure<IdentityOptions>(configuration.GetSection("KeyCloak"));
        //
        // services.AddHttpClient<AdminKeyCloakClient>((sp, client) =>
        //     {
        //         IdentityOptions options = sp.GetRequiredService<IOptions<IdentityOptions>>().Value;
        //         client.BaseAddress = new Uri(options.AdminUrl);
        //     })
        //     .AddHttpMessageHandler<AdminKeyCloakAuthDelegatingHandler>(); // request interceptor
        //
        // services.AddHttpClient<TokenKeyCloakClient>((sp, client) =>
        // {
        //     IdentityOptions options = sp.GetRequiredService<IOptions<IdentityOptions>>().Value;
        //     client.BaseAddress = new Uri(options.TokenUrl);
        // });
        // services.AddScoped<IIdentityProviderService, IdentityProviderService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IUserService, UserService>();
        // services.AddScoped<IClaimsTransformation, KeycloakClaimsTransformer>();



        services
            .AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<UserDbContext>()
            .AddDefaultTokenProviders();
        services.AddScoped<IUnitOfWork>(x => x.GetRequiredService<UserDbContext>());
        return services;
    }
}