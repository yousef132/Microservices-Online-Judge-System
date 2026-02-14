using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;


namespace BuildingBlocks.Identity.Identity
{
    public class KeycloakClaimsTransformer : IClaimsTransformation
    {
        //HTTP Request
        //   ↓
        //Authorization: Bearer<JWT>
        //   ↓
        //JwtBearer Middleware
        //   - validates token(iss, aud, exp, signature)
        //   - creates ClaimsPrincipal
        //   ↓
        //ClaimsTransformation   
        //   - add / remove / rename claims
        //   ↓
        //Authorization Middleware
        //   - [Authorize(Roles = "...")]
        //   ↓
        //Controller

        //transforms Keycloak specific claims to standard .net claims, to be able to authorize based on them
        // e.g. map "preferred_username" to ClaimTypes.Name, map "realm_access.roles" to ClaimTypes.Role

        // Claims trasformation may be in the gateway or in each microservice
        // if each microservice needs to authorize based on roles/claims, then it should be in each microservice
        // if only the gateway needs to authorize based on roles/claims, then it should be in the gateway
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

}
