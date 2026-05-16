using Community.API.Entities;
using MongoDB.Driver;

namespace Community.API.Persistence;

public interface IArticleRepository
{
    Task CreateAsync(Article article);
    Task<bool> SlugExistsAsync(string slug);
    Task<Article?> GetByIdAsync(Guid id);
    Task<Article?> GetBySlugAsync(string slug);
    Task<(IEnumerable<Article> Articles, long TotalCount)> ListAsync(string? tag, Guid? authorId, string sort, int page, int pageSize);
    Task<(IEnumerable<Article> Articles, long TotalCount)> ListForAuthorAsync(Guid authorId, int page, int pageSize);
    Task IncrementViewCountAsync(Guid articleId);
    Task UpdateVoteCountAsync(Guid articleId, int delta, IClientSessionHandle session);
    Task IncrementCommentCountAsync(Guid articleId, IClientSessionHandle session);
    Task<IEnumerable<Article>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task UpdateAsync(Article article);
    Task<IEnumerable<Article>> GetAllPublishedAsync();
    Task BulkUpdateHotScoresAsync(List<WriteModel<Article>> updates);
    Task<IEnumerable<Article>> GetCandidateArticlesForRecommendationsAsync(IEnumerable<Guid> excludedArticleIds, int limit);
}
