using Community.API.Database;
using Community.API.Entities;
using MongoDB.Driver;

namespace Community.API.Persistence;

public class UserActivityLogRepository(MongoDbContext context) : IUserActivityLogRepository
{
    private readonly IMongoCollection<UserActivity> _activities = context.UserActivities;

    public async Task LogActivityAsync(Guid userId, Guid articleId, string eventType)
    {
        var log = new UserActivity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ArticleId = articleId,
            EventType = eventType,
            CreatedAt = DateTime.UtcNow
        };
        await _activities.InsertOneAsync(log);
    }

    public async Task<IEnumerable<Guid>> GetRecentlyActiveUserIdsAsync(TimeSpan timeSpan)
    {
        var since = DateTime.UtcNow.Subtract(timeSpan);
        var filter = Builders<UserActivity>.Filter.Gte(log => log.CreatedAt, since);
        return await _activities.Distinct(log => log.UserId, filter).ToListAsync();
    }

    public async Task<IEnumerable<UserActivity>> GetRecentActivityForUserAsync(Guid userId, int limit)
    {
        var filter = Builders<UserActivity>.Filter.Eq(log => log.UserId, userId);
        var sort = Builders<UserActivity>.Sort.Descending(log => log.CreatedAt);
        return await _activities.Find(filter).Sort(sort).Limit(limit).ToListAsync();
    }
}
