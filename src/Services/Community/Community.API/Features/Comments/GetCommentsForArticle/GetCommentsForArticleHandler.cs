using System.Security.Claims;
using Community.API.Common.DTOs;
using Community.API.Entities;
using Community.API.Persistence;
using MediatR;

namespace Community.API.Features.Comments.GetCommentsForArticle;

public class GetCommentsForArticleHandler(
    ICommentRepository commentRepository,
    IVoteRepository voteRepository,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<GetCommentsForArticleQuery, List<CommentNodeDto>?>
{
    public async Task<List<CommentNodeDto>?> Handle(GetCommentsForArticleQuery request, CancellationToken cancellationToken)
    {
        var commentDocument = await commentRepository.GetByArticleIdAsync(request.ArticleId);
        if (commentDocument is null) return null;

        var user = httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdClaim, out var userId);

        var userVotes = new Dictionary<Guid, int>();
        if (userId != Guid.Empty)
        {
            var allCommentIds = new List<Guid>();
            CollectCommentIds(commentDocument.Comments, allCommentIds);
            if (allCommentIds.Any())
            {
                var votes = await voteRepository.GetVotesForCommentsAsync(userId, allCommentIds);
                userVotes = votes.ToDictionary(v => v.TargetId, v => v.Value);
            }
        }

        return commentDocument.Comments
            .Select(c => CommentNodeDto.FromCommentNode(c, userVotes))
            .OrderByDescending(c => c.VoteCount)
            .ToList();
    }

    private static void CollectCommentIds(IEnumerable<CommentNode> nodes, List<Guid> ids)
    {
        foreach (var node in nodes)
        {
            ids.Add(node.Id);
            if (node.Replies.Any())
                CollectCommentIds(node.Replies, ids);
        }
    }
}
