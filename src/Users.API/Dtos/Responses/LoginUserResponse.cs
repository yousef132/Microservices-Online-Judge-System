namespace Users.API.Dtos.Responses;

public class LoginUserResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}