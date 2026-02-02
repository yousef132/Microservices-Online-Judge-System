using Microsoft.AspNetCore.Authentication;
using Users.API.Dtos.Requests;
using Users.API.Dtos.Responses;
using Users.API.Services.Dtos;

namespace Users.API.Services;

public interface IUserService
{
    public Task<Guid> CreateUserAsync(CreateUserRequestDto createUserRequestDto, CancellationToken cancellationToken = default);
    public Task<LoginUserResponse> RefreshUserAsnc(RefreshTokenRequestDto refreshTokenRequestDto, CancellationToken cancellationToken = default);
    public Task<LoginUserResponse> LoginUserAsync(LoginUserRequestDto loginUserRequestDto, CancellationToken cancellationToken = default);

    public Task<UserDetailsDto> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    public Task UpdateUserAsync(UpdateUserDto updateUserDto, Guid userId ,CancellationToken cancellationToken = default);

}
