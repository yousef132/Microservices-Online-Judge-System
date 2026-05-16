using Community.API.Database;
using Community.API.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Community.API.Persistence;

public class CommentRepository(MongoDbContext context) : ICommentRepository
{
    private readonly IMongoCollection<CommentThread> _comments = context.Comments;

    public async Task AddCommentAsync(Guid articleId, CommentNode newComment, Guid? parentCommentId, IClientSessionHandle session)
    {
        var filter = Builders<CommentThread>.Filter.Eq(c => c.ArticleId, articleId);
        var commentDocument = await _comments.Find(session, filter).FirstOrDefaultAsync();

        if (commentDocument is null)
        {
            if (parentCommentId.HasValue)
                throw new Exception("Cannot reply to a comment in an article with no comments.");

            commentDocument = new CommentThread
            {
                Id = articleId,
                ArticleId = articleId,
                Comments = [newComment]
            };
            await _comments.InsertOneAsync(session, commentDocument);
        }
        else
        {
            if (parentCommentId.HasValue)
            {
                var parentNode = FindNode(commentDocument.Comments, parentCommentId.Value);
                if (parentNode is null) throw new Exception("Parent comment not found.");
                parentNode.Replies.Add(newComment);
            }
            else
            {
                commentDocument.Comments.Add(newComment);
            }
            await _comments.ReplaceOneAsync(session, filter, commentDocument);
        }
    }

    public async Task<CommentThread?> GetByArticleIdAsync(Guid articleId) =>
        await _comments.Find(Builders<CommentThread>.Filter.Eq(c => c.ArticleId, articleId))
            .FirstOrDefaultAsync();

    public async Task UpdateCommentAsync(Guid articleId, CommentNode updatedNode)
    {
        var filter = Builders<CommentThread>.Filter.Eq(c => c.ArticleId, articleId);
        var commentDocument = await GetByArticleIdAsync(articleId);
        if (commentDocument is null) throw new Exception("Comment thread not found.");

        var nodeToUpdate = FindNode(commentDocument.Comments, updatedNode.Id);
        if (nodeToUpdate is null) throw new Exception("Comment not found.");

        nodeToUpdate.Body = updatedNode.Body;
        nodeToUpdate.UpdatedAt = updatedNode.UpdatedAt;

        await _comments.ReplaceOneAsync(filter, commentDocument);
    }

    public async Task UpdateVoteCountAsync(Guid commentId, int delta, IClientSessionHandle session)
    {
        var filter = Builders<CommentThread>.Filter.ElemMatch(
            c => c.Comments,
            Builders<CommentNode>.Filter.Eq(cn => cn.Id, commentId));
        var update = Builders<CommentThread>.Update.Inc("comments.$[elem].voteCount", delta);
        var arrayFilters = new List<ArrayFilterDefinition>
        {
            new BsonDocumentArrayFilterDefinition<BsonDocument>(
                new BsonDocument("elem._id", commentId.ToString()))
        };
        await _comments.UpdateOneAsync(session, filter, update, new UpdateOptions { ArrayFilters = arrayFilters });
    }

    public async Task DeleteCommentAsync(Guid articleId, Guid commentId)
    {
        var filter = Builders<CommentThread>.Filter.Eq(c => c.ArticleId, articleId);
        var commentDocument = await GetByArticleIdAsync(articleId);
        if (commentDocument is null) throw new Exception("Comment thread not found.");

        var nodeToDelete = FindNode(commentDocument.Comments, commentId);
        if (nodeToDelete is null) throw new Exception("Comment not found.");

        nodeToDelete.IsDeleted = true;
        nodeToDelete.Body = "[deleted]";
        nodeToDelete.UpdatedAt = DateTime.UtcNow;

        await _comments.ReplaceOneAsync(filter, commentDocument);
    }

    public async Task<CommentNode?> GetNodeByIdAsync(Guid commentId)
    {
        var filter = Builders<CommentThread>.Filter.ElemMatch(c => c.Comments, n => n.Id == commentId);
        var commentDoc = await _comments.Find(filter).FirstOrDefaultAsync();
        return commentDoc is not null ? FindNode(commentDoc.Comments, commentId) : null;
    }

    public CommentNode? FindNode(IEnumerable<CommentNode> nodes, Guid nodeId)
    {
        foreach (var node in nodes)
        {
            if (node.Id == nodeId) return node;
            var found = FindNode(node.Replies, nodeId);
            if (found != null) return found;
        }
        return null;
    }
}
