using MediatR;

namespace Community.API.Features.Comments.DeleteComment;

public record DeleteCommentCommand(Guid ArticleId, Guid CommentId) : IRequest;
