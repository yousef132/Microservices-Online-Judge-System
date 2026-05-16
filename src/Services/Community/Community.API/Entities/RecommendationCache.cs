using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Community.API.Entities;

public class RecommendationCache
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } // This is the UserId

    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    public List<Guid> ArticleIds { get; set; } = new();

    public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
}
