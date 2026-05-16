using Community.API.Entities;

namespace Community.API.Persistence;

public interface IUserActivityLogRepository
{
    Task LogActivityAsync(Guid userId, Guid articleId, string eventType);
    Task<IEnumerable<Guid>> GetRecentlyActiveUserIdsAsync(TimeSpan timeSpan);
    Task<IEnumerable<UserActivity>> GetRecentActivityForUserAsync(Guid userId, int limit);
}
