namespace Users.API.Dtos.Requests;

public sealed record CreateUserRequestDto(string Email,string DisplayName, string Password);