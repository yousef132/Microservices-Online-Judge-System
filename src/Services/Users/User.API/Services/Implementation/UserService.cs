using System.Text.Json;
using BuildingBlocks.Core.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Users.API.Domain.Models;
using Users.API.Feature.User;
using Users.API.Feature.User.Common;
using Users.API.Infrastructure.Repository.Abstractions;
using Users.API.Services.Abstraction;

namespace Users.API.Services;

public class UserService(IJwtTokenGenerator _tokenGenerator,
    ILogger<UserService> logger,
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    UserManager<User> userManager,
    SignInManager<User> signInManager) : IUserService
{
    public async Task<string> CreateUserAsync(Signin.CreateUserRequestDto createUserRequestDto, CancellationToken cancellationToken = default)
    {
        var existingUser = await userManager.FindByEmailAsync(createUserRequestDto.Email);
        if (existingUser != null)
        {
            logger.LogWarning("User creation failed. Email already exists: {Email}", createUserRequestDto.Email);
            throw new ConflictException("This Email Already Exists");
        }

        var user = new User
        {
            Email = createUserRequestDto.Email,
            UserName = createUserRequestDto.Email,
            DisplayName = createUserRequestDto.DisplayName,
            EmailConfirmed = true,
            LastLogin = DateTime.UtcNow
        };

        try
        {
            var result = await userManager.CreateAsync(user, createUserRequestDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Failed to create user: {Errors}", errors);
                throw new Exception($"Failed to create user: {errors}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return await _tokenGenerator.GenerateTokenAsync(user);
    }

    public async Task<UserDetails.UserDetailsDto> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        User? user = await userRepository.GetById(Guid.Parse(userId), cancellationToken);
        if (user == null)
        {
            logger.LogWarning("User retrieval failed. User not found: {UserId}", userId);
            throw new NotFoundException("User Not Found");
        }

        return new UserDetails.UserDetailsDto(
            user.Id,
            user.UserName,
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

    public async Task<Login.LoginUserResponse> LoginUserAsync(Login.LoginUserRequestDto loginUserRequestDto, CancellationToken cancellationToken = default)
    {
        var users =  userManager.Users.ToList();
        Console.WriteLine("loginUserRequestDto");
        Console.WriteLine(JsonSerializer.Serialize(loginUserRequestDto));
        Console.WriteLine("All Users");
        Console.WriteLine(JsonSerializer.Serialize(users));
        var user = await userManager.FindByEmailAsync(loginUserRequestDto.Email);

         if (user is null)
             throw new UnAuthorizedException("Invalid credentials");

         var result = await signInManager.CheckPasswordSignInAsync(
             user,
             loginUserRequestDto.Password,
             lockoutOnFailure: true);

         if (!result.Succeeded)
             throw new UnAuthorizedException("Invalid credentials");

         user.LastLogin = DateTime.UtcNow;
         await userManager.UpdateAsync(user);

         var accessToken = await _tokenGenerator.GenerateTokenAsync(user);
         // var refreshToken = await jwtTokenGenerator.GenerateRefreshTokenAsync(user);

         return new Login.LoginUserResponse(accessToken);
    }
    public async Task UpdateUserAsync(UpdateUser.UpdateUserDto updateUserDto, Guid userId, CancellationToken cancellationToken = default)
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
    
    public async Task AddRoleToUserAsync(Guid userId, string role, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            logger.LogWarning("Add role failed. User not found: {UserId}", userId);
            throw new NotFoundException("User Not Found");
        }

        if (await userManager.IsInRoleAsync(user, role))
        {
            logger.LogWarning("User already has role {Role}: {UserId}", role, userId);
            throw new ConflictException($"User already has role '{role}'");
        }

        var result = await userManager.AddToRoleAsync(user, role);

        if (!result.Succeeded)
        {
            var error = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to add role {Role} to user {UserId}: {Error}", role, userId, error);
            throw new Exception($"Failed to add role: {error}");
        }
    }

    public async Task RemoveRoleFromUserAsync(Guid userId, string role, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            logger.LogWarning("Remove role failed. User not found: {UserId}", userId);
            throw new NotFoundException("User Not Found");
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            logger.LogWarning("User does not have role {Role}: {UserId}", role, userId);
            throw new ConflictException($"User does not have role '{role}'");
        }

        var result = await userManager.RemoveFromRoleAsync(user, role);

        if (!result.Succeeded)
        {
            var error = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to remove role {Role} from user {UserId}: {Error}", role, userId, error);
            throw new Exception($"Failed to remove role: {error}");
        }
    }
}