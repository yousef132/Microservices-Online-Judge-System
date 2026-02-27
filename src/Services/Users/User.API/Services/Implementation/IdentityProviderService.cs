// using System.Net;
// using BuildingBlocks.Core.Exceptions;
// using Users.API.Domain.Models;
// using Users.API.Feature.User;
// using Users.API.Feature.User.Common;
// using Users.API.Services.Abstraction;
// using Microsoft.AspNetCore.Identity;
// using BuildingBlocks.Core.Exceptions;
//
// namespace Users.API.Services;
//
// internal sealed class IdentityProviderService(
//     UserManager<User> userManager,
//     SignInManager<User> signInManager,
//     IJwtTokenGenerator jwtTokenGenerator,
//     ILogger<IdentityProviderService> logger)
//     : IIdentityProviderService
// {
//     public async Task<Login.LoginUserResponse> LoginUserAsync(
//         string email,
//         string password,
//         CancellationToken cancellationToken = default)
//     {
//         var user = await userManager.FindByEmailAsync(email);
//
//         if (user is null)
//             throw new UnAuthorizedException("Invalid credentials");
//
//         var result = await signInManager.CheckPasswordSignInAsync(
//             user,
//             password,
//             lockoutOnFailure: true);
//
//         if (!result.Succeeded)
//             throw new UnAuthorizedException("Invalid credentials");
//
//         user.LastLogin = DateTime.UtcNow;
//         await userManager.UpdateAsync(user);
//
//         var accessToken = await jwtTokenGenerator.GenerateTokenAsync(user);
//         var refreshToken = await jwtTokenGenerator.GenerateRefreshTokenAsync(user);
//
//         return new Login.LoginUserResponse(accessToken, refreshToken);
//     }
//
//     public async Task<RefreshToken.RefreshTokenResponse> RefreshUserAsync(
//         string token,
//         CancellationToken cancellationToken = default)
//     {
//         var principal = jwtTokenGenerator.GetPrincipalFromExpiredToken(token);
//
//         if (principal is null)
//             throw new UnAuthorizedException("Invalid refresh token");
//
//         var userId = principal.FindFirst("sub")?.Value;
//
//         if (userId is null)
//             throw new UnAuthorizedException("Invalid refresh token");
//
//         var user = await userManager.FindByIdAsync(userId);
//
//         if (user is null)
//             throw new UnAuthorizedException("Invalid refresh token");
//
//         var newAccessToken = await jwtTokenGenerator.GenerateTokenAsync(user);
//         var newRefreshToken = await jwtTokenGenerator.GenerateRefreshTokenAsync(user);
//
//         return new RefreshToken.RefreshTokenResponse(
//             newAccessToken,
//             newRefreshToken);
//     }
//
//     public async Task<string> RegisterUserAsync(
//         UserModel model,
//         CancellationToken cancellationToken = default)
//     {
//         var user = new User
//         {
//             Id = Guid.NewGuid(),
//             Email = model.Email,
//             UserName = model.Email,
//             DisplayName = model.DisplayName,
//             CreatedAt = DateTime.UtcNow,
//             IsPublicProfile = true
//         };
//
//         var result = await userManager.CreateAsync(user, model.Password);
//
//         if (!result.Succeeded)
//         {
//             var error = result.Errors.First().Description;
//             logger.LogWarning("User registration failed: {Error}", error);
//             throw new ConflictException(error);
//         }
//
//         return user.Id.ToString();
//     }
// }