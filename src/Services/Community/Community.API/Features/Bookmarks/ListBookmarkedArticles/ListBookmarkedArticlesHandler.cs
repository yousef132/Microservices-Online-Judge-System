using System.Security.Claims;
using Community.API.Common.DTOs;
using Community.API.Entities;
using Community.API.Persistence;
using MediatR;

namespace Community.API.Features.Bookmarks.ListBookmarkedArticles;

public class ListBookmarkedArticlesHandler(
    IBookmarkRepository bookmarkRepository,
    IArticleRepository articleRepository,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<ListBookmarkedArticlesQuery, PaginatedListDto<Article>>
{
    public async Task<PaginatedListDto<Article>> Handle(ListBookmarkedArticlesQuery request, CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return new PaginatedListDto<Article>();

        var (bookmarkArticleIds, totalCount) = await bookmarkRepository.ListForUserAsync(userId, request.Page, request.PageSize);

        var articles = new List<Article>();
        if (bookmarkArticleIds.Any())
        {
            var fetchedArticles = await articleRepository.GetByIdsAsync(bookmarkArticleIds);
            articles = bookmarkArticleIds
                .Select(id => fetchedArticles.FirstOrDefault(a => a.Id == id))
                .Where(a => a is not null)
                .ToList()!;
        }

        return new PaginatedListDto<Article>
        {
            Items = articles,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
