using Community.API.Common.DTOs;
using Community.API.Entities;
using MediatR;

namespace Community.API.Features.Articles.ListArticles;

public record ListArticlesQuery(
    string? Tag,
    Guid? AuthorId,
    string Sort,
    int Page,
    int PageSize) : IRequest<PaginatedListDto<Article>>;
