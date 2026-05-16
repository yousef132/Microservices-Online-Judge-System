using Community.API.Entities;

namespace Community.API.Persistence;

public interface IBookmarkRepository
{
    Task<bool> IsBookmarkedAsync(Guid userId, Guid articleId);
    Task CreateAsync(Guid userId, Guid articleId);
    Task DeleteAsync(Guid userId, Guid articleId);
    Task<(IEnumerable<Guid> ArticleIds, long TotalCount)> ListForUserAsync(Guid userId, int page, int pageSize);
}
