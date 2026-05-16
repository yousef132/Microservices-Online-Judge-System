using Community.API.Entities;
using MediatR;

namespace Community.API.Features.Articles.CreateArticle;

public record CreateArticleCommand(
    string Title,
    string Body,
    List<string> Tags,
    string Status) : IRequest<Article>;
