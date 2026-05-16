using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Community.API.Entities;

public class CommentNode
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Author Author { get; set; } = new();

    public string Body { get; set; } = string.Empty;

    public int VoteCount { get; set; } = 0;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<CommentNode> Replies { get; set; } = new();
}
