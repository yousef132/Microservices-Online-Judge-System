using Community.API.Database;
using Community.API.Entities;
using MongoDB.Driver;
using ArticleTag = Community.API.Entities.Tag;

namespace Community.API.Persistence;

public class TagRepository(MongoDbContext context) : ITagRepository
{
    private readonly IMongoCollection<ArticleTag> _tags = context.Tag;

    public async Task IncrementTagsAsync(IEnumerable<string> tagNames)
    {
        var bulkOps = tagNames.Distinct().Select(tagName =>
        {
            var filter = Builders<ArticleTag>.Filter.Eq(t => t.Name, tagName);
            var update = Builders<ArticleTag>.Update
                .Inc(t => t.ArticleCount, 1)
                .SetOnInsert(t => t.Id, tagName)
                .SetOnInsert(t => t.Name, tagName);
            return (WriteModel<ArticleTag>)new UpdateOneModel<ArticleTag>(filter, update) { IsUpsert = true };
        }).ToList();

        if (bulkOps.Any()) await _tags.BulkWriteAsync(bulkOps);
    }

    public async Task DecrementTagsAsync(IEnumerable<string> tagNames)
    {
        var bulkOps = tagNames.Distinct().Select(tagName =>
        {
            var filter = Builders<ArticleTag>.Filter.Eq(t => t.Name, tagName);
            var update = Builders<ArticleTag>.Update.Inc(t => t.ArticleCount, -1);
            return (WriteModel<ArticleTag>)new UpdateOneModel<ArticleTag>(filter, update);
        }).ToList();

        if (bulkOps.Any()) await _tags.BulkWriteAsync(bulkOps);
    }
}
