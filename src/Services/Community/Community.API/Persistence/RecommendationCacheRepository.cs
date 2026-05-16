using Community.API.Database;
using Community.API.Entities;
using MongoDB.Driver;

namespace Community.API.Persistence;

public class RecommendationCacheRepository(MongoDbContext context) : IRecommendationCacheRepository
{
    private readonly IMongoCollection<RecommendationCache> _recommendations = context.Recommendations;

    public async Task UpsertAsync(RecommendationCache cache)
    {
        var filter = Builders<RecommendationCache>.Filter.Eq(r => r.Id, cache.Id);
        await _recommendations.ReplaceOneAsync(filter, cache, new ReplaceOptions { IsUpsert = true });
    }

    public async Task<RecommendationCache?> GetByUserIdAsync(Guid userId) =>
        await _recommendations.Find(
            Builders<RecommendationCache>.Filter.Eq(r => r.UserId, userId))
            .FirstOrDefaultAsync();
}
