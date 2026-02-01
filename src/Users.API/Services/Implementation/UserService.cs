using Users.API.Dtos.Requests;
using Users.API.Dtos.Responses;
using Users.API.Exceptions;
using Users.API.Models;
using Users.API.Repository;
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
        string userIdentitfier = await identityProviderService.RegisterUserAsync(new UserModel(createUserRequestDto.Email, createUserRequestDto.Password, createUserRequestDto.FirstName, createUserRequestDto.LastName), cancellationToken);
        user = new User()
        {
            Email = createUserRequestDto.Email,
            Keycloak_Id = Guid.Parse(userIdentitfier),
            LastLogin = DateTime.UtcNow,
            Username = createUserRequestDto.Email
        };
        await userRepository.AddAsync(user,cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return user.Id;
    }

    public async Task<LoginUserResponse> LoginUserAsync(LoginUserRequestDto loginUserRequestDto, CancellationToken cancellationToken = default)
    {
        return await identityProviderService.LoginUserAsync(loginUserRequestDto.Email, loginUserRequestDto.Password, cancellationToken);
    }

    public async Task<LoginUserResponse> RefreshUserAsnc(RefreshTokenRequestDto refreshTokenRequestDto, CancellationToken cancellationToken = default)
    {
        return await identityProviderService.RefreshUserAsync(refreshTokenRequestDto.Token, cancellationToken);
    }
}