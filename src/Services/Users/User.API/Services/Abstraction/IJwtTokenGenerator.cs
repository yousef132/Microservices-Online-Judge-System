using System.Security.Claims;
using Users.API.Domain.Models;

namespace Users.API.Services.Abstraction;

public interface IJwtTokenGenerator
{
    Task<string> GenerateTokenAsync(User user);
    Task<string> GenerateRefreshTokenAsync(User user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}