namespace Users.API.Dtos.Requests;

internal sealed record UserRepresentation(
    string Username,
    string Email,
    bool EmailVerified,
    bool Enabled,
    CredentialRepresentation[] Credentials);