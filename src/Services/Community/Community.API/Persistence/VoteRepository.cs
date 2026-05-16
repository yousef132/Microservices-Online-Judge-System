using Community.API.Database;
using Community.API.Entities;
using MongoDB.Driver;

namespace Community.API.Persistence;

public class VoteRepository(MongoDbContext context) : IVoteRepository
{
    private readonly IMongoCollection<Vote> _votes = context.Votes;

    public async Task<Vote?> GetVoteAsync(Guid userId, Guid targetId, string targetType)
    {
        var compositeId = $"{userId}:{targetType}:{targetId}";
        return await _votes.Find(Builders<Vote>.Filter.Eq(v => v.Id, compositeId)).FirstOrDefaultAsync();
    }

    public async Task<List<Vote>> GetVotesForCommentsAsync(Guid userId, IEnumerable<Guid> commentIds) =>
        await _votes.Find(
            Builders<Vote>.Filter.And(
                Builders<Vote>.Filter.Eq(v => v.UserId, userId),
                Builders<Vote>.Filter.Eq(v => v.TargetType, "Comment"),
                Builders<Vote>.Filter.In(v => v.TargetId, commentIds)))
            .ToListAsync();

    public async Task CreateVoteAsync(Guid userId, Guid targetId, string targetType, int value, IClientSessionHandle session)
    {
        var vote = new Vote
        {
            Id = $"{userId}:{targetType}:{targetId}",
            UserId = userId,
            TargetId = targetId,
            TargetType = targetType,
            Value = value,
            CreatedAt = DateTime.UtcNow
        };
        await _votes.InsertOneAsync(session, vote);
    }

    public async Task UpdateVoteAsync(string voteId, int newValue, IClientSessionHandle session) =>
        await _votes.UpdateOneAsync(session,
            Builders<Vote>.Filter.Eq(v => v.Id, voteId),
            Builders<Vote>.Update.Set(v => v.Value, newValue).Set(v => v.CreatedAt, DateTime.UtcNow));

    public async Task DeleteVoteAsync(string voteId, IClientSessionHandle session) =>
        await _votes.DeleteOneAsync(session, Builders<Vote>.Filter.Eq(v => v.Id, voteId));
}
