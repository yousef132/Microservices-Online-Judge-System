using Users.API.Dtos.Responses;
using Users.API.Models;
using Users.API.Services.Dtos;

namespace Users.API.Services;

public interface IIdentityProviderService
{
    Task<string> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default);
    Task<LoginUserResponse> RefreshUserAsync(string token, CancellationToken cancellationToken = default);
    Task<LoginUserResponse> LoginUserAsync(string email, string password, CancellationToken cancellationToken = default);
}
