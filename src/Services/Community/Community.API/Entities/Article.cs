using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Community.API.Entities;

public class Article
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string? CoverImageKey { get; set; }

    public List<string> Tags { get; set; } = new();

    public string Status { get; set; } = "Draft"; // "Draft", "Published", "Deleted"

    public Author Author { get; set; } = new();

    public int ViewCount { get; set; } = 0;
    public int VoteCount { get; set; } = 0;
    public int CommentCount { get; set; } = 0;
    public double HotScore { get; set; } = 0.0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
}
