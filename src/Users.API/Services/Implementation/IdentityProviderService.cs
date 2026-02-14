using System.Net;
using BuildingBlocks.Core.Exceptions;
using Users.API.Clients;
using Users.API.Common.Clients;
using Users.API.Feature.User;
using Users.API.Feature.User.Common;
using Users.API.Services.Abstraction;

namespace Users.API.Services;

internal sealed class IdentityProviderService(AdminKeyCloakClient adminKeyCloakClient, TokenKeyCloakClient tokenKeyCloackCLient, ILogger<IdentityProviderService> logger)
    : IIdentityProviderService
{
    // POST /admin/realms/{realm}/users
    public async Task<Login.LoginUserResponse> LoginUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            Login.LoginUserResponse authResponse = await tokenKeyCloackCLient.LoginUserAsync(email, password, cancellationToken);
            return authResponse;
        }
        catch (HttpRequestException exception)
        {
            switch (exception.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new UnAuthorizedException("Invalid Credentials"); 
            }
            throw new NotImplementedException("Unhandled Status Code At the LoginUserAsync in IdentityProviderService with message" + exception.Message);
        }
    }

    public async Task<RefreshToken.RefreshTokenResponse> RefreshUserAsync(string token, CancellationToken cancellationToken = default)
    {
        var authResponse = await tokenKeyCloackCLient.RefreshTokenAsync(token, cancellationToken);
        return authResponse;
    }


    public async Task<string> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default)
    {
        var userRepresentation = new UserRepresentation(
            user.Email,
            user.Email,
            true,
            true,
            [new CredentialRepresentation("password", user.Password, false)]);

        try
        {
            string identityId = await adminKeyCloakClient.RegisterUserAsync(userRepresentation, cancellationToken);
            return identityId;
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
        {
            logger.LogError("User registration failed {exception}", exception);
            throw new Exception ("User.Conflict.Email"); // TODO : specify exception
        }
    }
}
