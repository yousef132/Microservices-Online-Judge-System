using Community.API.Common.Exceptions;
using Community.API.Entities;
using Community.API.Persistence;
using Community.API.Services.Auth;
using MediatR;

namespace Community.API.Features.Comments.DeleteComment;

public class DeleteCommentHandler(
    ICommentRepository commentRepository,
    IAuthHelper authHelper) : IRequestHandler<DeleteCommentCommand>
{
    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var commentDocument = await commentRepository.GetByArticleIdAsync(request.ArticleId)
            ?? throw new NotFoundException("Comment thread not found for the article.");

        var commentNode = commentRepository.FindNode(commentDocument.Comments, request.CommentId)
            ?? throw new NotFoundException(nameof(CommentNode), request.CommentId);

        if (!authHelper.IsAuthorOrAdmin(commentNode))
            throw new ForbiddenAccessException("User is not authorized to delete this comment.");

        if (commentNode.IsDeleted) return; // Idempotent

        await commentRepository.DeleteCommentAsync(request.ArticleId, request.CommentId);
    }
}
