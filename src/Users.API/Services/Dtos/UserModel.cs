namespace Users.API.Services.Dtos;

public sealed record UserModel(string Email, string Password);
public sealed record UserDetailsDto(
    Guid Id,
    string Username,
    string Email,
    DateTime LastLogin,
    DateTime CreatedAt,
    string DisplayName,
    string? Bio,
    string? LinkedInUrl,
    string? GithubUrl,
    string? FacebookUrl,
    bool IsPublicProfile,
    string? ProfilePictureUrl
);

public sealed record UpdateUserDto(
    string? DisplayName,
    string? Bio,
    string? LinkedInUrl,
    string? GithubUrl,
    string? FacebookUrl,
    bool? IsPublicProfile,
    string? ProfilePictureUrl
);
