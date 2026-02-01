using System.Net;
using Users.API.Clients;
using Users.API.Dtos.Requests;
using Users.API.Dtos.Responses;
using Users.API.Exceptions;
using Users.API.Services.Dtos;

namespace Users.API.Services;

internal sealed class IdentityProviderService(AdminKeyCloakClient adminKeyCloakClient, TokenKeyCloakClient tokenKeyCloackCLient, ILogger<IdentityProviderService> logger)
    : IIdentityProviderService
{
    // POST /admin/realms/{realm}/users
    public async Task<LoginUserResponse> LoginUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var authResponse = await tokenKeyCloackCLient.LoginUserAsync(email, password, cancellationToken);
            return new LoginUserResponse()
            {
                AccessToken = authResponse.AccessToken,
                RefreshToken = authResponse.RefreshToken
            };
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

    public Task<LoginUserResponse> RefreshUserAsync(string token, CancellationToken cancellationToken = default)
    {
        var authResponse = tokenKeyCloackCLient.RefreshTokenAsync(token, cancellationToken);
        return Task.FromResult(new LoginUserResponse()
        {
            AccessToken = authResponse.Result.AccessToken,
            RefreshToken = authResponse.Result.RefreshToken
        });
    }


    public async Task<string> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default)
    {
        var userRepresentation = new UserRepresentation(
            user.Email,
            user.Email,
            user.FirstName,
            user.LastName,
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
