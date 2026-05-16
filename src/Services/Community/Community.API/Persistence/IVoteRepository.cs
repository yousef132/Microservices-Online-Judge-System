using Community.API.Entities;
using MongoDB.Driver;

namespace Community.API.Persistence;

public interface IVoteRepository
{
    Task<Vote?> GetVoteAsync(Guid userId, Guid targetId, string targetType);
    Task<List<Vote>> GetVotesForCommentsAsync(Guid userId, IEnumerable<Guid> commentIds);
    Task CreateVoteAsync(Guid userId, Guid targetId, string targetType, int value, IClientSessionHandle session);
    Task UpdateVoteAsync(string voteId, int newValue, IClientSessionHandle session);
    Task DeleteVoteAsync(string voteId, IClientSessionHandle session);
}
