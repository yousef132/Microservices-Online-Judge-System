using System.Text.Json;
using Microsoft.Extensions.Options;
using Users.API.Common.Options;
using Users.API.Feature.User;
using Users.API.Feature.User.Common;

namespace Users.API.Clients;

    internal class TokenKeyCloakClient(HttpClient httpClient, IOptions<KeyCloakOptions> options)
    {
        private readonly KeyCloakOptions _options = options.Value;
        internal async Task<RefreshToken.RefreshTokenResponse> RefreshTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            var refreshRepresentation = new KeyValuePair<string, string>[]
            {
                new ("client_id", _options.ConfidentialClientId),
                new ("client_secret", _options.ConfidentialClientSecret),
                new ("grant_type", "refresh_token"),
                new ("refresh_token", token)
            };

            var refreshRequestContent = new FormUrlEncodedContent(refreshRepresentation);
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                "",
                refreshRequestContent,
                cancellationToken);

            httpResponseMessage.EnsureSuccessStatusCode();

            return await httpResponseMessage.Content.ReadFromJsonAsync<RefreshToken.RefreshTokenResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to read authorization token from response.");
        }
    internal async Task<KeycloakLoginResponse> LoginUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var loginRepresentation = new KeyValuePair<string, string>[]
        {
        new ("client_id", _options.ConfidentialClientId),
        new ("username", email),
        new ("password", password),
        new ("client_secret", _options.ConfidentialClientSecret),
        new ("grant_type", "password"),
        new ("scope", "openid email")
        };

        var authRequestContent = new FormUrlEncodedContent(loginRepresentation);

        // Log the request
        var requestBody = await authRequestContent.ReadAsStringAsync();
        Console.WriteLine("=== REQUEST ===");
        Console.WriteLine($"URL: {httpClient.BaseAddress}");
        Console.WriteLine($"Body: {requestBody}");
        Console.WriteLine("================");

        HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
            "",
            authRequestContent,
            cancellationToken);

        // Read the response content once
        var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

        // Log the response
        Console.WriteLine("=== RESPONSE ===");
        Console.WriteLine($"Status Code: {(int)httpResponseMessage.StatusCode} {httpResponseMessage.StatusCode}");
        Console.WriteLine($"Headers: {string.Join(", ", httpResponseMessage.Headers.Select(h => $"{h.Key}: {string.Join(",", h.Value)}"))}");
        Console.WriteLine($"Body: {responseContent}");
        Console.WriteLine("=================");

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Keycloak returned {httpResponseMessage.StatusCode}: {responseContent}");
        }

        // Deserialize from the string we already read
        var result = JsonSerializer.Deserialize<KeycloakLoginResponse>(
            responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result ?? throw new InvalidOperationException("Failed to read authorization token from response.");
    }
}
