using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Community.API.Entities;

public class CommentThread
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } // This is the ArticleId

    [BsonRepresentation(BsonType.String)]
    public Guid ArticleId { get; set; }

    public List<CommentNode> Comments { get; set; } = new();
}
