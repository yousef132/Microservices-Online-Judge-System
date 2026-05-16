using System.Security.Claims;
using Community.API.Entities;
using Community.API.Persistence;
using MediatR;

namespace Community.API.Features.Recommendations.GetRecommendations;

public class GetRecommendationsHandler(
    IRecommendationCacheRepository recommendationRepo,
    IArticleRepository articleRepo,
    IUserActivityLogRepository userActivityRepo,
    IHttpContextAccessor httpContextAccessor,
    ILogger<GetRecommendationsHandler> logger)
    : IRequestHandler<GetRecommendationsQuery, IEnumerable<Article>>
{
    public async Task<IEnumerable<Article>> Handle(GetRecommendationsQuery request, CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Enumerable.Empty<Article>();

        var cachedRecommendations = await recommendationRepo.GetByUserIdAsync(userId);
        if (cachedRecommendations != null)
        {
            logger.LogInformation("Recommendation cache HIT for user {UserId}", userId);
            var articles = await articleRepo.GetByIdsAsync(cachedRecommendations.ArticleIds);
            return articles.Take(request.Limit);
        }

        logger.LogInformation("Recommendation cache MISS for user {UserId}. Computing inline.", userId);
        var recommendedArticleIds = await ComputeForUser(userId);

        if (recommendedArticleIds.Any())
        {
            await recommendationRepo.UpsertAsync(new RecommendationCache
            {
                Id = userId,
                UserId = userId,
                ArticleIds = recommendedArticleIds,
                ComputedAt = DateTime.UtcNow
            });
        }

        var finalArticles = await articleRepo.GetByIdsAsync(recommendedArticleIds);
        return finalArticles.Take(request.Limit);
    }

    private async Task<List<Guid>> ComputeForUser(Guid userId)
    {
        var recentActivities = (await userActivityRepo.GetRecentActivityForUserAsync(userId, 50)).ToList();
        var interactedArticleIds = recentActivities.Select(a => a.ArticleId).ToHashSet();
        var interactedTags = (await articleRepo.GetByIdsAsync(interactedArticleIds))
            .SelectMany(a => a.Tags)
            .GroupBy(t => t)
            .ToDictionary(g => g.Key, g => g.Count());

        var interactedAuthors = (await articleRepo.GetByIdsAsync(interactedArticleIds))
            .Select(a => a.Author.Id)
            .ToHashSet();

        var candidateArticles = (await articleRepo.GetCandidateArticlesForRecommendationsAsync(interactedArticleIds, 200)).ToList();
        if (!candidateArticles.Any()) return new List<Guid>();

        var maxHotScore = candidateArticles.Max(a => a.HotScore);
        if (maxHotScore == 0) maxHotScore = 1.0;

        return candidateArticles
            .Select(candidate =>
            {
                double tagOverlap = candidate.Tags.Sum(tag => interactedTags.GetValueOrDefault(tag, 0));
                double authorAffinity = interactedAuthors.Contains(candidate.Author.Id) ? 1.0 : 0.0;
                double normalizedHot = candidate.HotScore / maxHotScore;
                double score = (0.5 * tagOverlap) + (0.3 * authorAffinity) + (0.2 * normalizedHot);
                return (candidate, score);
            })
            .OrderByDescending(x => x.score)
            .Take(20)
            .Select(x => x.candidate.Id)
            .ToList();
    }
}
