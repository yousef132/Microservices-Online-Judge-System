using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Community.API.Entities;

public class Author
{
    [BsonRepresentation(BsonType.String)]

    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
}
