namespace Users.API.Feature.User.Common;

internal sealed record CredentialRepresentation(string Type, string Value, bool Temporary);