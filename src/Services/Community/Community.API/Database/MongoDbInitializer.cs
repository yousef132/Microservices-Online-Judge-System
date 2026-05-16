using Community.API.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Community.API.Database;

public static class MongoDbInitializer
{
    public static async Task InitializeAsync(IMongoDatabase database)
    {
        // Articles indexes
        var articlesCollection = database.GetCollection<Article>("articles");
        await articlesCollection.Indexes.CreateManyAsync([
            new CreateIndexModel<Article>(
                Builders<Article>.IndexKeys.Ascending(a => a.Slug),
                new CreateIndexOptions { Unique = true, Name = "idx_articles_slug" }),
            new CreateIndexModel<Article>(
                Builders<Article>.IndexKeys.Ascending(a => a.Status).Descending(a => a.PublishedAt),
                new CreateIndexOptions { Name = "idx_articles_status_published" }),
            new CreateIndexModel<Article>(
                Builders<Article>.IndexKeys.Descending(a => a.HotScore),
                new CreateIndexOptions { Name = "idx_articles_hot_score" }),
            new CreateIndexModel<Article>(
                Builders<Article>.IndexKeys.Ascending("author.id"),
                new CreateIndexOptions { Name = "idx_articles_author_id" }),
        ]);

        // Votes indexes
        var votesCollection = database.GetCollection<Vote>("votes");
        await votesCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<Vote>(
                Builders<Vote>.IndexKeys.Ascending(v => v.UserId).Ascending(v => v.TargetId).Ascending(v => v.TargetType),
                new CreateIndexOptions { Unique = true, Name = "idx_votes_user_target" }));

        // Bookmarks indexes
        var bookmarksCollection = database.GetCollection<Bookmark>("bookmarks");
        await bookmarksCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<Bookmark>(
                Builders<Bookmark>.IndexKeys.Ascending(b => b.UserId).Descending(b => b.CreatedAt),
                new CreateIndexOptions { Name = "idx_bookmarks_user_created" }));

        // User activity indexes
        var activitiesCollection = database.GetCollection<UserActivity>("user_activities");
        await activitiesCollection.Indexes.CreateManyAsync([
            new CreateIndexModel<UserActivity>(
                Builders<UserActivity>.IndexKeys.Ascending(a => a.UserId).Descending(a => a.CreatedAt),
                new CreateIndexOptions { Name = "idx_activities_user_created" }),
        ]);
    }
}
