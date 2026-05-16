using Community.API.Common.DTOs;
using Community.API.Entities;
using MediatR;

namespace Community.API.Features.Bookmarks.ListBookmarkedArticles;

public record ListBookmarkedArticlesQuery(int Page, int PageSize) : IRequest<PaginatedListDto<Article>>;
