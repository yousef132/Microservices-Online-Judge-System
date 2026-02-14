using System.Text.Json.Serialization;

namespace Users.API.Feature.User.Common;

public record KeycloakLoginResponse([property: JsonPropertyName("access_token")] string AccessToken, [property: JsonPropertyName("expires_in")] int ExpiresIn, [property: JsonPropertyName("refresh_expires_in")] int RefreshExpiresIn, [property: JsonPropertyName("refresh_token")] string RefreshToken, [property: JsonPropertyName("token_type")] string TokenType, [property: JsonPropertyName("id_token")] string IdToken, [property: JsonPropertyName("not-before-policy")] int NotBeforePolicy, [property: JsonPropertyName("session_state")] string SessionState, [property: JsonPropertyName("scope")] string Scope);
