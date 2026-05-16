using System.Security.Claims;
using Community.API.Common.DTOs;
using Community.API.Database;
using Community.API.Persistence;
using MediatR;

namespace Community.API.Features.Votes.CastVote;

public class CastVoteHandler(
    IVoteRepository voteRepository,
    IArticleRepository articleRepository,
    ICommentRepository commentRepository,
    IHttpContextAccessor httpContextAccessor,
    MongoDbContext mongoDbContext)
    : IRequestHandler<CastVoteCommand, CastVoteResponse>
{
    public async Task<CastVoteResponse> Handle(CastVoteCommand request, CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        int currentVoteCount;
        if (request.TargetType == "Article")
        {
            var article = await articleRepository.GetByIdAsync(request.TargetId)
                ?? throw new Exception("Article not found.");
            if (article.Author.Id == userId)
                throw new Exception("Users cannot vote on their own content.");
            currentVoteCount = article.VoteCount;
        }
        else
        {
            var commentNode = await commentRepository.GetNodeByIdAsync(request.TargetId)
                ?? throw new Exception("Comment not found.");
            if (commentNode.Author.Id == userId)
                throw new Exception("Users cannot vote on their own content.");
            currentVoteCount = commentNode.VoteCount;
        }

        int voteDelta = 0;
        int finalUserVote = 0;

        using var session = await mongoDbContext.Client.StartSessionAsync(cancellationToken: cancellationToken);

        await session.WithTransactionAsync(async (s, ct) =>
        {
            var existingVote = await voteRepository.GetVoteAsync(userId, request.TargetId, request.TargetType);

            if (existingVote is null)
            {
                voteDelta = request.Value;
                finalUserVote = request.Value;
                await voteRepository.CreateVoteAsync(userId, request.TargetId, request.TargetType, request.Value, s);
            }
            else if (existingVote.Value == request.Value)
            {
                voteDelta = -request.Value;
                finalUserVote = 0;
                await voteRepository.DeleteVoteAsync(existingVote.Id, s);
            }
            else
            {
                voteDelta = request.Value * 2;
                finalUserVote = request.Value;
                await voteRepository.UpdateVoteAsync(existingVote.Id, request.Value, s);
            }

            if (request.TargetType == "Article")
                await articleRepository.UpdateVoteCountAsync(request.TargetId, voteDelta, s);
            else
                await commentRepository.UpdateVoteCountAsync(request.TargetId, voteDelta, s);

            return 1;
        }, cancellationToken: cancellationToken);

        return new CastVoteResponse
        {
            TargetId = request.TargetId,
            NewVoteCount = currentVoteCount + voteDelta,
            UserVote = finalUserVote
        };
    }
}
