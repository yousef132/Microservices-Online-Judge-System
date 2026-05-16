using Community.API.Common.Exceptions;
using Community.API.Entities;
using Community.API.Persistence;
using Community.API.Services.Auth;
using MediatR;

namespace Community.API.Features.Comments.UpdateComment;

public class UpdateCommentHandler(
    ICommentRepository commentRepository,
    IAuthHelper authHelper)
    : IRequestHandler<UpdateCommentCommand, CommentNode>
{
    public async Task<CommentNode> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var commentDocument = await commentRepository.GetByArticleIdAsync(request.ArticleId)
            ?? throw new NotFoundException("Comment thread not found for the article.");

        var commentNode = commentRepository.FindNode(commentDocument.Comments, request.CommentId)
            ?? throw new NotFoundException(nameof(CommentNode), request.CommentId);

        if (!authHelper.IsAuthor(commentNode))
            throw new ForbiddenAccessException("User is not authorized to edit this comment.");

        if (commentNode.IsDeleted)
            throw new InvalidOperationException("Cannot edit a deleted comment.");

        commentNode.Body = request.Body;
        commentNode.UpdatedAt = DateTime.UtcNow;

        await commentRepository.UpdateCommentAsync(request.ArticleId, commentNode);

        return commentNode;
    }
}
