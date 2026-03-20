using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Identity;

public static class Extensions
{
    public static IServiceCollection AddIdentity(
        this IServiceCollection services,
        IConfiguration configuration
        )
    {
        // var keycloakOptions = new IdentityOptions
        // {
        //     AdminUrl = configuration["KeyCloak:AdminUrl"] ?? "",
        //     TokenUrl = configuration["KeyCloak:TokenUrl"] ?? "",
        //     ConfidentialClientId = configuration["KeyCloak:ConfidentialClientId"] ?? "",
        //     ConfidentialClientSecret = configuration["KeyCloak:ConfidentialClientSecret"] ?? "",
        //     PublicClientId = configuration["KeyCloak:PublicClientId"] ?? "",
        //     Audience = configuration["KeyCloak:Audience"] ?? "",
        //     Authority = configuration["KeyCloak:Authority"] ?? ""
        // };
        // Console.WriteLine(JsonSerializer.Serialize(keycloakOptions));
        

        // services.AddScoped<IClaimsTransformation, KeycloakClaimsTransformer>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.RequireHttpsMetadata = false;

                jwtOptions.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],

                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),

                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = JwtRegisteredClaimNames.Sub,

                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });
        services.AddAuthorization();

        return services;
    }
}