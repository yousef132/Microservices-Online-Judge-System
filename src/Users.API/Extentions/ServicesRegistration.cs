using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Users.API.Clients;
using Users.API.Delegates;
using Users.API.Helpers;
using Users.API.Options;
using Users.API.Repository;
using Users.API.Repository.Implementations;
using Users.API.Services;

namespace Users.API.Extentions;

public static class ServicesRegistration
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<AdminKeyCloakAuthDelegatingHandler>();
       

        services.AddDbContext<UserDbContext>(opt => { opt.UseNpgsql(configuration.GetConnectionString("Default")); });
        services.Configure<KeyCloakOptions>(configuration.GetSection("KeyCloak"));

        services.AddHttpClient<AdminKeyCloakClient>((sp, client) =>
            {
                KeyCloakOptions options = sp.GetRequiredService<IOptions<KeyCloakOptions>>().Value;
                client.BaseAddress = new Uri(options.AdminUrl);
            })
            .AddHttpMessageHandler<AdminKeyCloakAuthDelegatingHandler>(); // request interceptor

        services.AddHttpClient<TokenKeyCloakClient>((sp, client) =>
        {
            KeyCloakOptions options = sp.GetRequiredService<IOptions<KeyCloakOptions>>().Value;
            client.BaseAddress = new Uri(options.TokenUrl);
        });
        services.AddScoped<IIdentityProviderService, IdentityProviderService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IClaimsTransformation, KeycloakClaimsTransformer>();

        services.AddScoped<IUnitOfWork>(x => x.GetRequiredService<UserDbContext>());
        return services;
    }

    public static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        var keycloakSection = configuration.GetSection("KeyCloak");

        // NOTE : take care of generating token from different envirnments like docker and localhost
        // because the issuer url will be different in those cases

        var tokenUrl = keycloakSection["TokenUrl"]!;
        // export authority by removing the token endpoint path
        var authority = tokenUrl.Replace("/protocol/openid-connect/token", "");

        var audience = keycloakSection["Audience"]!;

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = false; // only development

                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidIssuer = authority,

                    ValidateAudience = true,
                    ValidAudience = audience,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1), // allow 1 minute clock skew, between keycloak and api server
                    
                };
            });
        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "Enter your JWT token"
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }


}