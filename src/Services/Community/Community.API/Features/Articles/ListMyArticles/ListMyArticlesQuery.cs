using Community.API.Common.DTOs;
using Community.API.Entities;
using MediatR;

namespace Community.API.Features.Articles.ListMyArticles;

public record ListMyArticlesQuery(int Page, int PageSize) : IRequest<PaginatedListDto<Article>>;
