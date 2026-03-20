namespace Users.API.Feature.User.Common;

internal sealed record UserRepresentation(
    string Username,
    string Email,
    bool EmailVerified,
    bool Enabled,
    CredentialRepresentation[] Credentials);
