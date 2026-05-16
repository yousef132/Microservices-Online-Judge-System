using Community.API.Common.DTOs;
using MediatR;

namespace Community.API.Features.Comments.GetCommentsForArticle;

public record GetCommentsForArticleQuery(Guid ArticleId) : IRequest<List<CommentNodeDto>?>;
