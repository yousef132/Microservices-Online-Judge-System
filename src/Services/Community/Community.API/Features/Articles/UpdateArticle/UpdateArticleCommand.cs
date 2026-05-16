using Community.API.Entities;
using MediatR;

namespace Community.API.Features.Articles.UpdateArticle;

public record UpdateArticleCommand(
    Guid ArticleId,
    string? Title,
    string? Body,
    List<string>? Tags,
    string? Status,
    string? CoverImageKey) : IRequest<Article>;
