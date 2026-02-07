using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Users.API.Common.Options;
using Users.API.Dtos.Requests;

namespace Users.API.Common.Clients;

internal sealed class AdminKeyCloakClient(HttpClient httpClient, IOptions<KeyCloakOptions> options)
{
    private readonly KeyCloakOptions _options = options.Value;
    internal async Task<string> RegisterUserAsync(UserRepresentation user, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync(
                "users",
                user,
                cancellationToken);
            
         return ExtractIdentityIdFromLocationHeader(httpResponseMessage);
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.Write(e.InnerException?.Message);
            throw;
        }

        // httpResponseMessage.EnsureSuccessStatusCode();

    }
    private static string ExtractIdentityIdFromLocationHeader(
        HttpResponseMessage httpResponseMessage)
    {
        const string usersSegmentName = "users/";

        //http://localhost:8080/admin/realms/rommie/users/4825c8cf-663e-41fb-8261-98a6199d0d0f
        string? locationHeader = httpResponseMessage.Headers.Location?.PathAndQuery;

        if (locationHeader is null)
        {
            throw new InvalidOperationException("Location header is null");
        }

        int userSegmentValueIndex = locationHeader.IndexOf(
            usersSegmentName,
            StringComparison.InvariantCultureIgnoreCase);

        string identityId = locationHeader.Substring(userSegmentValueIndex + usersSegmentName.Length);
        //4825c8cf-663e-41fb-8261-98a6199d0d0f
        return identityId;
    }
}
