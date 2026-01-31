namespace Users.API.Dtos.Requests;

public sealed record CreateUserRequestDto(string Email, string FirstName, string LastName, string Password);