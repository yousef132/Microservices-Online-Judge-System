using Community.API.Database;
using Community.API.Entities;
using MongoDB.Driver;

namespace Community.API.Persistence;

public class BookmarkRepository(MongoDbContext context) : IBookmarkRepository
{
    private readonly IMongoCollection<Bookmark> _bookmarks = context.Bookmarks;

    public async Task<bool> IsBookmarkedAsync(Guid userId, Guid articleId)
    {
        var compositeId = $"{userId}:{articleId}";
        return await _bookmarks.Find(Builders<Bookmark>.Filter.Eq(b => b.Id, compositeId)).AnyAsync();
    }

    public async Task CreateAsync(Guid userId, Guid articleId)
    {
        var bookmark = new Bookmark
        {
            Id = $"{userId}:{articleId}",
            UserId = userId,
            ArticleId = articleId,
            CreatedAt = DateTime.UtcNow
        };
        var filter = Builders<Bookmark>.Filter.Eq(b => b.Id, bookmark.Id);
        await _bookmarks.ReplaceOneAsync(filter, bookmark, new ReplaceOptions { IsUpsert = true });
    }

    public async Task DeleteAsync(Guid userId, Guid articleId)
    {
        var compositeId = $"{userId}:{articleId}";
        await _bookmarks.DeleteOneAsync(Builders<Bookmark>.Filter.Eq(b => b.Id, compositeId));
    }

    public async Task<(IEnumerable<Guid> ArticleIds, long TotalCount)> ListForUserAsync(Guid userId, int page, int pageSize)
    {
        var filter = Builders<Bookmark>.Filter.Eq(b => b.UserId, userId);
        var sort = Builders<Bookmark>.Sort.Descending(b => b.CreatedAt);
        var bookmarks = await _bookmarks.Find(filter).Sort(sort)
            .Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();
        var totalCount = await _bookmarks.CountDocumentsAsync(filter);
        return (bookmarks.Select(b => b.ArticleId), totalCount);
    }
}
