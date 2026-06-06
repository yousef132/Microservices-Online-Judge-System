using Community.API.Common.DTOs;
using Community.API.Entities;
using Community.API.Persistence;
using Community.API.Services.S3;
using MediatR;
using System.Security.Claims;

namespace Community.API.Features.Articles.ListMyArticles;

public class ListMyArticlesHandler(
    IArticleRepository articleRepository,
    IHttpContextAccessor httpContextAccessor,
    IS3Service s3Service)
    : IRequestHandler<ListMyArticlesQuery, PaginatedListDto<Article>>
{
    public async Task<PaginatedListDto<Article>> Handle(ListMyArticlesQuery request, CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
            return new PaginatedListDto<Article>();

        var (articles, totalCount) = await articleRepository.ListForAuthorAsync(userId, request.Page, request.PageSize);
        var newList = articles.Select(a =>
        {
            if (!string.IsNullOrEmpty(a.CoverImageKey))
            {
                a.CoverImageKey = s3Service.GetPublicUrl(a.CoverImageKey);
            }
            return a;
        }).ToList();
        return new PaginatedListDto<Article>
        {
            Items = newList,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
