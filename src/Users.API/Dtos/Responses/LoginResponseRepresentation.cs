using System.Text.Json.Serialization;

namespace Users.API.Dtos.Responses;

public class LoginResponseRepresentation
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
}