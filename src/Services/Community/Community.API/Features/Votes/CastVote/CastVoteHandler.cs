using BuildingBlocks.Core.Exceptions;
using Community.API.Common.DTOs;
using Community.API.Common.Exceptions;
using Community.API.Database;
using Community.API.Persistence;
using MediatR;
using System.Security.Claims;

namespace Community.API.Features.Votes.CastVote;

public class CastVoteHandler(
    IVoteRepository voteRepository,
    IArticleRepository articleRepository,
    ICommentRepository commentRepository,
    IUserActivityLogRepository userActivityLogRepository,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<CastVoteCommand, CastVoteResponse>
{
    public async Task<CastVoteResponse> Handle(CastVoteCommand request, CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        bool isCommentVote = request.CommentId.HasValue;
        Guid targetId = isCommentVote ? request.CommentId!.Value : request.ArticleId;
        string targetType = isCommentVote ? "Comment" : "Article";

        int currentVoteCount;
        if (targetType == "Article")
        {
            var article = await articleRepository.GetByIdAsync(targetId)
                ?? throw new NotFoundException("Article not found.");
            if (article.Author.Id == userId)
                throw new ConflictException("Users cannot vote on their own content.");
            currentVoteCount = article.VoteCount;
        }
        else
        {
            var commentThread = await commentRepository.GetByArticleIdAsync(request.ArticleId)
                ?? throw new NotFoundException("Comment thread not found.");
            var commentNode = commentRepository.FindNode(commentThread.Comments, targetId)
                ?? throw new NotFoundException("Comment not found.");
            if (commentNode.Author.Id == userId)
                throw new ConflictException("Users cannot vote on their own content.");
            currentVoteCount = commentNode.VoteCount;
        }

        int voteDelta = 0;
        int finalUserVote = 0;

        var existingVote = await voteRepository.GetVoteAsync(userId, targetId, targetType);

        // this user didn't vote before
        if (existingVote is null) // add this new vote
        {
            voteDelta = request.Value;
            finalUserVote = request.Value;
            await voteRepository.CreateVoteAsync(userId, targetId, targetType, request.Value);
        }
        else if (existingVote.Value == request.Value) // undo this user vote
        {
            voteDelta = -request.Value;
            finalUserVote = 0;
            await voteRepository.DeleteVoteAsync(existingVote.Id);
        }
        else // Flipping a Vote (Upvote to Downvote, or vice versa)
        {
            voteDelta = request.Value * 2;
            finalUserVote = request.Value;
            await voteRepository.UpdateVoteAsync(existingVote.Id, request.Value); // update the vote document itself, to keep voting history
        }

        if (targetType == "Article")
            _ = Task.Run(() => articleRepository.UpdateVoteCountAsync(targetId, voteDelta), cancellationToken); // update vote property in article
        else
            _ = Task.Run(() => commentRepository.UpdateVoteCountAsync(request.ArticleId, targetId, voteDelta), cancellationToken);// update vote property in comment

        _ = Task.Run(async () =>
        {
            await userActivityLogRepository.LogActivityAsync(userId, request.ArticleId, Community.API.Enums.EventTypeEnum.Vote);
        }, cancellationToken);

        return new CastVoteResponse
        {
            TargetId = targetId,
            NewVoteCount = currentVoteCount + voteDelta,
            UserVote = finalUserVote
        };
    }
}
