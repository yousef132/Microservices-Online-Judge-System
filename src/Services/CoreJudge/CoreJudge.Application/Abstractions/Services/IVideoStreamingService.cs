namespace CodeSphere.Domain.Abstractions.Services
{
    public interface IVideoStreamingService
    {
        Task CreateRoom(string roomId);
    }
}
