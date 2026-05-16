using Community.API.Entities;
using MediatR;

namespace Community.API.Features.Comments.UpdateComment;

public record UpdateCommentCommand(
    Guid ArticleId,
    Guid CommentId,
    string Body) : IRequest<CommentNode>;
