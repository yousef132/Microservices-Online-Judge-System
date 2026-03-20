// using System.Text.Json;
// using BuildingBlocks.Identity;
// using Microsoft.Extensions.Options;
// using Users.API.Feature.User.Common;
//
// namespace Users.API.Common.Clients;
//
// internal sealed class AdminKeyCloakClient(HttpClient httpClient, IOptions<IdentityOptions> options)
// {
//     private readonly IdentityOptions _options = options.Value;
//     internal async Task<string> RegisterUserAsync(UserRepresentation user, CancellationToken cancellationToken = default)
//     {
//         try
//         {
//             HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync(
//                 "users",
//                 user,
//                 cancellationToken);
//             
//          return ExtractIdentityIdFromLocationHeader(httpResponseMessage);
//             
//         }
//         catch (Exception e)
//         { 
//             Console.Write(e.InnerException?.Message);
//             throw;
//         }
//         //httpResponseMessage.EnsureSuccessStatusCode();
//     }
//     private static string ExtractIdentityIdFromLocationHeader(
//         HttpResponseMessage httpResponseMessage)
//     {
//         const string usersSegmentName = "users/";
//
//         //http://localhost:8080/admin/realms/online_judge/users/4825c8cf-663e-41fb-8261-98a6199d0d0f
//         string? locationHeader = httpResponseMessage.Headers.Location?.PathAndQuery;
//
//         if (locationHeader is null)
//         {
//             throw new InvalidOperationException("Location header is null");
//         }
//
//         int userSegmentValueIndex = locationHeader.IndexOf(
//             usersSegmentName,
//             StringComparison.InvariantCultureIgnoreCase);
//
//         string identityId = locationHeader.Substring(userSegmentValueIndex + usersSegmentName.Length);
//         //4825c8cf-663e-41fb-8261-98a6199d0d0f
//         return identityId;
//     }
//     
//     
//     internal async Task AssignRealmRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default)
//     {
//         // 1. Get role info
//         try
//         {
//             var roleResponse = await httpClient.GetAsync($"roles/{roleName}", cancellationToken);
//             roleResponse.EnsureSuccessStatusCode();
//             var role = await roleResponse.Content.ReadFromJsonAsync<KeycloakRole>(cancellationToken: cancellationToken);
//
//             // 2. Assign role
//             await httpClient.PostAsJsonAsync($"users/{userId}/role-mappings/realm", new[] { role }, cancellationToken);
//         }
//         catch (Exception e)
//         {
//             Console.WriteLine(e);
//             throw;
//         }
//     }
//     internal async Task RemoveRealmRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default)
//     {
//         try
//         {
//             // 1. Get role info
//             var roleResponse = await httpClient.GetAsync($"roles/{roleName}", cancellationToken);
//             roleResponse.EnsureSuccessStatusCode();
//             var role = await roleResponse.Content.ReadFromJsonAsync<KeycloakRole>(cancellationToken: cancellationToken);
//
//             // 2. Remove role
//             var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"users/{userId}/role-mappings/realm")
//             {
//                 Content = JsonContent.Create(new[] { role })
//             };
//             await httpClient.SendAsync(deleteRequest, cancellationToken);
//         }
//         catch (Exception e)
//         {
//             Console.WriteLine(e);
//             throw;
//         }
//     }
// }
