using Community.API.Entities;

using Community.API.Enums;

namespace Community.API.Persistence;

public interface IUserActivityLogRepository
{
    Task LogActivityAsync(Guid userId, Guid articleId, EventTypeEnum eventType);
    Task<IEnumerable<Guid>> GetRecentlyActiveUserIdsAsync(TimeSpan timeSpan);
    Task<IEnumerable<UserActivity>> GetRecentActivityForUserAsync(Guid userId, int limit);
}
