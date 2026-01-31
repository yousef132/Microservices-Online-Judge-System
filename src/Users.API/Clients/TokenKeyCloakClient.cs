using Microsoft.Extensions.Options;
using Users.API.Dtos.Responses;
using Users.API.Options;

namespace Users.API.Clients;

    internal class TokenKeyCloakClient(HttpClient httpClient, IOptions<KeyCloakOptions> options)
    {
        private readonly KeyCloakOptions _options = options.Value;
        internal async Task<LoginResponseRepresentation> RefreshTokenAsync(string token, CancellationToken cancellationToken = default)
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

            return await httpResponseMessage.Content.ReadFromJsonAsync<LoginResponseRepresentation>(cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to read authorization token from response.");
        }
        internal async Task<LoginResponseRepresentation> LoginUserAsync(string email, string password, CancellationToken cancellationToken = default)
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
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                "",
                authRequestContent,
                cancellationToken);

            httpResponseMessage.EnsureSuccessStatusCode();

            return await httpResponseMessage.Content.ReadFromJsonAsync<LoginResponseRepresentation>(cancellationToken: cancellationToken) ?? throw new InvalidOperationException("Failed to read authorization token from response.");
        }
    }
