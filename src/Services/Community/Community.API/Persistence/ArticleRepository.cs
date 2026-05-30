using Community.API.Database;
using Community.API.Entities;
using MongoDB.Driver;

namespace Community.API.Persistence;

public class ArticleRepository(MongoDbContext context) : IArticleRepository
{
    private readonly IMongoCollection<Article> _articles = context.Articles;

    public async Task CreateAsync(Article article) =>
        await _articles.InsertOneAsync(article);

    public async Task<bool> SlugExistsAsync(string slug) =>
        await _articles.Find(Builders<Article>.Filter.Eq(a => a.Slug, slug)).AnyAsync();

    public async Task<Article?> GetByIdAsync(Guid id) =>
        await _articles.Find(Builders<Article>.Filter.Eq(a => a.Id, id)).FirstOrDefaultAsync();

    public async Task<Article?> GetBySlugAsync(string slug) =>
        await _articles.Find(Builders<Article>.Filter.Eq(a => a.Slug, slug)).FirstOrDefaultAsync();

    public async Task<(IEnumerable<Article> Articles, long TotalCount)> ListAsync(
        string? tag, Guid? authorId, string sort, int page, int pageSize)
    {
        var filterBuilder = Builders<Article>.Filter;
        var filter = filterBuilder.Eq(a => a.Status, "Published");

        if (!string.IsNullOrEmpty(tag))
            filter &= filterBuilder.AnyEq(a => a.Tags, tag);

        if (authorId.HasValue)
            filter &= filterBuilder.Eq("author.id", authorId.Value);

        var sortDefinition = sort.ToLowerInvariant() switch
        {
            "top" => Builders<Article>.Sort.Descending(a => a.VoteCount),
            "hot" => Builders<Article>.Sort.Descending(a => a.HotScore),
            _ => Builders<Article>.Sort.Descending(a => a.PublishedAt)
        };

        var articles = await _articles.Find(filter).Sort(sortDefinition)
            .Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();
        var totalCount = await _articles.CountDocumentsAsync(filter);
        return (articles, totalCount);
    }

    public async Task<(IEnumerable<Article> Articles, long TotalCount)> ListForAuthorAsync(Guid authorId, int page, int pageSize)
    {
        var filter = Builders<Article>.Filter.Eq(a => a.Author.Id, authorId) &
                     Builders<Article>.Filter.Ne(a => a.Status, "Deleted");
        var sort = Builders<Article>.Sort.Descending(a => a.CreatedAt);
        var articles = await _articles.Find(filter).Sort(sort)
            .Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();
        var totalCount = await _articles.CountDocumentsAsync(filter);
        return (articles, totalCount);
    }

    public async Task IncrementViewCountAsync(Guid articleId) =>
        await _articles.UpdateOneAsync(
            Builders<Article>.Filter.Eq(a => a.Id, articleId),
            Builders<Article>.Update.Inc(a => a.ViewCount, 1));

    public async Task UpdateVoteCountAsync(Guid articleId, int delta, IClientSessionHandle session) =>
        await _articles.UpdateOneAsync(session,
            Builders<Article>.Filter.Eq(a => a.Id, articleId),
            Builders<Article>.Update.Inc(a => a.VoteCount, delta));

    public async Task IncrementCommentCountAsync(Guid articleId, IClientSessionHandle session) =>
        await _articles.UpdateOneAsync(session,
            Builders<Article>.Filter.Eq(a => a.Id, articleId),
            Builders<Article>.Update.Inc(a => a.CommentCount, 1));

    public async Task<IEnumerable<Article>> GetByIdsAsync(IEnumerable<Guid> ids) =>
        await _articles.Find(Builders<Article>.Filter.In(a => a.Id, ids)).ToListAsync();

    public async Task UpdateAsync(Article article) =>
        await _articles.ReplaceOneAsync(Builders<Article>.Filter.Eq(a => a.Id, article.Id), article);

    public async Task<IEnumerable<Article>> GetAllPublishedAsync() =>
        await _articles.Find(Builders<Article>.Filter.Eq(a => a.Status, "Published")).ToListAsync();

    public async Task BulkUpdateHotScoresAsync(List<WriteModel<Article>> updates)
    {
        if (updates.Any())
            await _articles.BulkWriteAsync(updates);
    }

    public async Task<IEnumerable<Article>> GetCandidateArticlesForRecommendationsAsync(
        IEnumerable<Guid> excludedArticleIds, int limit)
    {
        var filter = Builders<Article>.Filter.Eq(a => a.Status, "Published") &
                     Builders<Article>.Filter.Nin(a => a.Id, excludedArticleIds);
        return await _articles.Find(filter)
            .Sort(Builders<Article>.Sort.Descending(a => a.HotScore))
            .Limit(limit).ToListAsync();
    }
}
