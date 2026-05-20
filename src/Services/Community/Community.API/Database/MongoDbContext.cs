using Community.API.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using ArticleTag = Community.API.Entities.Tag;

namespace Community.API.Database;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017";
        Console.WriteLine(connectionString);
        var databaseName = configuration["MongoDB:DatabaseName"] ?? "CommunityDb";
        Console.WriteLine(databaseName);
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
        Client = client;
    }

    public IMongoClient Client { get; }
    public IMongoDatabase Database => _database;

    public IMongoCollection<Article> Articles => _database.GetCollection<Article>("articles");
    public IMongoCollection<CommentThread> Comments => _database.GetCollection<CommentThread>("comments");
    public IMongoCollection<Vote> Votes => _database.GetCollection<Vote>("votes");
    public IMongoCollection<Bookmark> Bookmarks => _database.GetCollection<Bookmark>("bookmarks");
    public IMongoCollection<ArticleTag> Tag => _database.GetCollection<ArticleTag>("tags");
    public IMongoCollection<UserActivity> UserActivities => _database.GetCollection<UserActivity>("user_activities");
    public IMongoCollection<RecommendationCache> Recommendations => _database.GetCollection<RecommendationCache>("recommendation_caches");
}
