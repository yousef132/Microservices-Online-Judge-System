using Community.API.Entities;
using MediatR;

namespace Community.API.Features.Comments.CreateComment;

public record CreateCommentCommand(
    Guid ArticleId,
    string Body,
    Guid? ParentCommentId) : IRequest<CommentNode>;
