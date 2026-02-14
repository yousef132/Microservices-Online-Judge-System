using CodeSphere.Domain.Abstractions.Services;

using Microsoft.Extensions.Options;
using System.Text;

namespace CodeSphere.Infrastructure.Implementation.Services
{
    public class VideoStreamingService : IVideoStreamingService
    {
        //private readonly VideoSDK _videoSdkOptions;
        //private readonly IHttpClientFactory _httpClientFactory;
        //private readonly string baseUrl = "https://api.videosdk.live/v2/rooms";
        //public VideoStreamingService(IOptions<VideoSDK> videoSdkOptions, IHttpClientFactory httpClientFactory)
        //{
        //    this._videoSdkOptions = videoSdkOptions.Value;
        //    this._httpClientFactory = httpClientFactory;
        //}

        //public async Task CreateRoom(string roomId)
        //{
        //    var httpClient = _httpClientFactory.CreateClient();
        //    var token = GenerateToken();

        //    var request = new HttpRequestMessage(HttpMethod.Post, baseUrl);
        //    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(token);
        //    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        //    var body = new
        //    {
        //        customRoomId = roomId,
        //        webhook = new
        //        {
        //            endPoint = "https://localhost:7050/api/videowebhook",
        //            events = new[]
        //            {
        //                "participant-joined",
        //                "participant-left",
        //                "recording-started",
        //                "hls-started"
        //            }
        //        },
        //        autoCloseConfig = new
        //        {
        //            type = "session-end-and-deactivate",
        //            duration = 60
        //        },
        //        autoStartConfig = new
        //        {
        //            recording = new
        //            {
        //                transcription = new
        //                {
        //                    enabled = true,
        //                    summary = new
        //                    {
        //                        enabled = true,
        //                        prompt = "Write summary in sections like Title, Agenda, Speakers, Action Items, Outlines, Notes and Summary"
        //                    }
        //                },
        //                config = new
        //                {
        //                    layout = new
        //                    {
        //                        type = "GRID",
        //                        priority = "SPEAKER",
        //                        gridSize = 4
        //                    }
        //                }
        //            },
        //            hls = new
        //            {
        //                transcription = new
        //                {
        //                    enabled = true,
        //                    summary = new
        //                    {
        //                        enabled = true,
        //                        prompt = "Write summary in sections like Title, Agenda, Speakers, Action Items, Outlines, Notes and Summary"
        //                    }
        //                },
        //                config = new
        //                {
        //                    layout = new
        //                    {
        //                        type = "GRID",
        //                        priority = "SPEAKER",
        //                        gridSize = 4
        //                    },
        //                    recording = new
        //                    {
        //                        enabled = true
        //                    }
        //                }
        //            }
        //        }
        //    };

        //    string jsonBody = System.Text.Json.JsonSerializer.Serialize(body);
        //    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        //    var response = await httpClient.SendAsync(request);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var error = await response.Content.ReadAsStringAsync();
        //        throw new Exception($"Error creating room: {response.StatusCode} - {error}");
        //    }

        //    var result = await response.Content.ReadAsStringAsync();
        //    Console.WriteLine(result);
        //}

        //private string GenerateToken()
        //{

        //    var token = JwtBuilder.Create()
        //        .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
        //        .WithSecret(_videoSdkOptions.SECRET_KEY)
        //        .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds())
        //        .AddClaim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        //        .AddClaim("apikey", _videoSdkOptions.API_KEY)
        //        .AddClaim("permissions", new string[1] { "allow_join" }) // "ask_join" || "allow_mod" 
        //        .AddClaim("version", 2) //OPTIONAL
        //        .AddClaim("roomId", "2kyv-gzay-64pg")
        //        .AddClaim("participantId", "lxvdplwt")
        //        .AddClaim("roles", new string[1] { "crawler" }) //OPTIONAL
        //        .Encode();

        //    return token;

        //}
        public Task CreateRoom(string roomId)
        {
            throw new NotImplementedException();
        }
    }
}
