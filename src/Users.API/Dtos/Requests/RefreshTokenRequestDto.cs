namespace Users.API.Dtos.Requests;

public record RefreshTokenRequestDto
{
    public string Token { get; set; } = string.Empty;
}