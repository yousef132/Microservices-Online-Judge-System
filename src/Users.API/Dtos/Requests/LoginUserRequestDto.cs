namespace Users.API.Dtos.Requests;

public class LoginUserRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}