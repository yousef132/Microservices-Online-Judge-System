using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Community.API.Entities;

public class Tag
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = string.Empty; // tagName

    public string Name { get; set; } = string.Empty;

    public int ArticleCount { get; set; } = 0;
}
