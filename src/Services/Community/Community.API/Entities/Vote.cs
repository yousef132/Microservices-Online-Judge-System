using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Community.API.Entities;

public class Vote
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = string.Empty; // userId:targetType:targetId

    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid TargetId { get; set; }

    public string TargetType { get; set; } = string.Empty; // "Article" or "Comment"

    public int Value { get; set; } // 1 or -1

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
