using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Users.API.Common.Exceptions;
using Users.API.Domain.Models;
using Users.API.Dtos.Requests;
using Users.API.Dtos.Responses;
using Users.API.Infrastructure.Repository.Abstractions;
using Users.API.Services.Abstraction;
using Users.API.Services.Dtos;

namespace Users.API.Services;

public class UserService(IIdentityProviderService identityProviderService,
    ILogger<UserService> logger,
    IUnitOfWork unitOfWork,
    IUserRepository userRepository) : IUserService
{
    public async Task<Guid> CreateUserAsync(CreateUserRequestDto createUserRequestDto, CancellationToken cancellationToken = default)
    {
        User? user = await userRepository.GetByEmail(createUserRequestDto.Email);
        if (user != null)
        {
            logger.LogWarning("User creation failed. Email already exists: {Email}", createUserRequestDto.Email);
            throw new ConflictException("This Email Already Exists");
        }
        string userIdentitfier = await identityProviderService.RegisterUserAsync(new UserModel(createUserRequestDto.Email, createUserRequestDto.Password), cancellationToken);
        user = new User()
        {
            Email = createUserRequestDto.Email,
            Keycloak_Id = Guid.Parse(userIdentitfier),
            LastLogin = DateTime.UtcNow,
            Username = createUserRequestDto.Email,
            DisplayName = createUserRequestDto.DisplayName,
        };
        await userRepository.AddAsync(user,cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return user.Id;
    }

    public async Task<UserDetailsDto> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        User? user = await userRepository.GetById(Guid.Parse(userId), cancellationToken);
        if (user == null)
        {
            logger.LogWarning("User retrieval failed. User not found: {UserId}", userId);
            throw new NotFoundException("User Not Found");
        }

        return new UserDetailsDto(
            user.Id,
            user.Username,
            user.Email,
            user.LastLogin ?? DateTime.MinValue,
            user.CreatedAt,
            user.DisplayName,
            user.Bio,
            user.LinkedInUrl,
            user.GithubUrl,
            user.FacebookUrl,
            user.IsPublicProfile,
            user.ProfilePictureUrl
        );
    }

    public async Task<LoginUserResponse> LoginUserAsync(LoginUserRequestDto loginUserRequestDto, CancellationToken cancellationToken = default)
    {
        return await identityProviderService.LoginUserAsync(loginUserRequestDto.Email, loginUserRequestDto.Password, cancellationToken);
    }

    public async Task<LoginUserResponse> RefreshUserAsnc(RefreshTokenRequestDto refreshTokenRequestDto, CancellationToken cancellationToken = default)
    {
        return await identityProviderService.RefreshUserAsync(refreshTokenRequestDto.Token, cancellationToken);
    }

    public async Task UpdateUserAsync(UpdateUserDto updateUserDto, Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetById(userId, cancellationToken);
        if (user == null)
        {
            logger.LogWarning("User update failed. User not found: {UserId}", userId);
            throw new NotFoundException("User Not Found");
        }
        user.DisplayName = updateUserDto.DisplayName ?? user.DisplayName;
        user.Bio = updateUserDto.Bio ?? user.Bio;
        user.LinkedInUrl = updateUserDto.LinkedInUrl ?? user.LinkedInUrl;
        user.GithubUrl = updateUserDto.GithubUrl ?? user.GithubUrl;
        user.FacebookUrl = updateUserDto.FacebookUrl ?? user.FacebookUrl;
        user.IsPublicProfile = updateUserDto.IsPublicProfile ?? user.IsPublicProfile;
        user.ProfilePictureUrl = updateUserDto.ProfilePictureUrl ?? user.ProfilePictureUrl;
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

    }
}