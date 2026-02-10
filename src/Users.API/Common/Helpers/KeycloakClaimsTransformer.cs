using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace Users.API.Common.Helpers;
public class KeycloakClaimsTransformer : IClaimsTransformation
{
    /// <summary>
    /// jwt -> JwtBearer Middleware [validation] -> ClaimsTransformation [this] -> Authorization Middleware -> Controller
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    //transforms Keycloak specific claims to standard .net claims, to be able to authorize based on them
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;
        if (identity == null || !identity.IsAuthenticated)
            return Task.FromResult(principal);

        // 1️ username
        var preferredUsername = identity.FindFirst("preferred_username");
        if (preferredUsername != null &&
            !identity.HasClaim(c => c.Type == ClaimTypes.Name))
        {
            identity.AddClaim(
                new Claim(ClaimTypes.Name, preferredUsername.Value));
        }

        // 2️ realm roles
        var realmAccess = identity.FindFirst("realm_access");
        if (realmAccess != null)
        {
            using var doc = JsonDocument.Parse(realmAccess.Value);

            if (doc.RootElement.TryGetProperty("roles", out var roles))
            {
                foreach (var role in roles.EnumerateArray())
                {
                    identity.AddClaim(
                        new Claim(ClaimTypes.Role, role.GetString()!));
                }
            }
        }

        return Task.FromResult(principal);
    }
}
