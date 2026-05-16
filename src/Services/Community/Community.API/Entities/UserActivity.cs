using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Community.API.Entities;

public class UserActivity
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid ArticleId { get; set; }

    public string EventType { get; set; } = string.Empty; // "View", "Vote", "Comment", "Bookmark"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
