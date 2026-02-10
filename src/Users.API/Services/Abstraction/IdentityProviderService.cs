using Users.API.Feature.User;
using Users.API.Feature.User.Common;

namespace Users.API.Services.Abstraction;

public interface IIdentityProviderService
{
    Task<string> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default);
    Task<RefreshToken.RefreshTokenResponse> RefreshUserAsync(string token, CancellationToken cancellationToken = default);
    Task<Login.LoginUserResponse> LoginUserAsync(string email, string password, CancellationToken cancellationToken = default);
}
