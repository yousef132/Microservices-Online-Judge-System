namespace Users.API.Feature.User.Common;

internal record KeycloakRole(string Id, string Name, bool Composite = false, bool ClientRole = false, string ContainerId = "");