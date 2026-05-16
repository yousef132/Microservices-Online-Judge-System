using Community.API.Entities;

namespace Community.API.Persistence;

public interface IRecommendationCacheRepository
{
    Task UpsertAsync(RecommendationCache cache);
    Task<RecommendationCache?> GetByUserIdAsync(Guid userId);
}
