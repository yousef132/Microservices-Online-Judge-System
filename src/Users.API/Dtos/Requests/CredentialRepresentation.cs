namespace Users.API.Dtos.Requests;

internal sealed record CredentialRepresentation(string Type, string Value, bool Temporary);