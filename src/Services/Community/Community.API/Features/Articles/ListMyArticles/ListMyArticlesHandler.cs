using System.Security.Claims;
using Community.API.Common.DTOs;
using Community.API.Entities;
using Community.API.Persistence;
using MediatR;

namespace Community.API.Features.Articles.ListMyArticles;

public class ListMyArticlesHandler(
    IArticleRepository articleRepository,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<ListMyArticlesQuery, PaginatedListDto<Article>>
{
    public async Task<PaginatedListDto<Article>> Handle(ListMyArticlesQuery request, CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
            return new PaginatedListDto<Article>();

        var (articles, totalCount) = await articleRepository.ListForAuthorAsync(userId, request.Page, request.PageSize);

        return new PaginatedListDto<Article>
        {
            Items = articles,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
