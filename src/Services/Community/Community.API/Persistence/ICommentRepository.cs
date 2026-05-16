using Community.API.Entities;
using MongoDB.Driver;

namespace Community.API.Persistence;

public interface ICommentRepository
{
    Task AddCommentAsync(Guid articleId, CommentNode newComment, Guid? parentCommentId, IClientSessionHandle session);
    Task<CommentThread?> GetByArticleIdAsync(Guid articleId);
    Task UpdateCommentAsync(Guid articleId, CommentNode updatedNode);
    Task UpdateVoteCountAsync(Guid commentId, int delta, IClientSessionHandle session);
    Task DeleteCommentAsync(Guid articleId, Guid commentId);
    Task<CommentNode?> GetNodeByIdAsync(Guid commentId);
    CommentNode? FindNode(IEnumerable<CommentNode> nodes, Guid nodeId);
}
