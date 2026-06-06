using System.Security.Claims;
using Community.API.Common.Exceptions;
using Community.API.Database;
using Community.API.Entities;
using Community.API.Persistence;
using MediatR;

namespace Community.API.Features.Comments.CreateComment;

public class CreateCommentHandler(
    ICommentRepository commentRepository,
    IArticleRepository articleRepository,
    IUserActivityLogRepository userActivityLogRepository,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<CreateCommentCommand, CommentNode>
{
    public async Task<CommentNode> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        var userNameClaim = user?.FindFirstValue(ClaimTypes.Name);

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        var article = await articleRepository.GetByIdAsync(request.ArticleId)
            ?? throw new NotFoundException(nameof(Article), request.ArticleId);

        var now = DateTime.UtcNow;
        var newCommentNode = new CommentNode
        {
            Id = Guid.NewGuid(),
            Author = new Author { Id = userId, Username = userNameClaim ?? "unknown" },
            Body = request.Body,
            CreatedAt = now,
            UpdatedAt = now
        };

        await commentRepository.AddCommentAsync(request.ArticleId, newCommentNode, request.ParentCommentId);
        _ = Task.Run(() => articleRepository.IncrementCommentCountAsync(request.ArticleId), cancellationToken);

        _ = Task.Run(async () =>
        {
            await userActivityLogRepository.LogActivityAsync(userId, request.ArticleId, Community.API.Enums.EventTypeEnum.Comment);
        }, cancellationToken);

        return newCommentNode;
    }
}
