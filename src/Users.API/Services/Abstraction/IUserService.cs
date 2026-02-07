using Microsoft.AspNetCore.Authentication;
using Users.API.Feature.User;
using Users.API.Feature.User.Common;

namespace Users.API.Services;

public interface IUserService
{
    public Task<Guid> CreateUserAsync(Signin.CreateUserRequestDto createUserRequestDto, CancellationToken cancellationToken = default);
    public Task<RefreshToken.RefreshTokenResponse> RefreshUserAsnc(RefreshToken.RefreshTokenRequestDto refreshTokenRequestDto, CancellationToken cancellationToken = default);
    public Task<Login.LoginUserResponse> LoginUserAsync(Login.LoginUserRequestDto loginUserRequestDto, CancellationToken cancellationToken = default);

    public Task<UserDetails.UserDetailsDto> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    public Task UpdateUserAsync(UpdateUser.UpdateUserDto updateUserDto, Guid userId ,CancellationToken cancellationToken = default);

}
