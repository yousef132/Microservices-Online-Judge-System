using MediatR;

namespace Community.API.Features.Articles.DeleteArticle;

public record DeleteArticleCommand(Guid ArticleId) : IRequest;
