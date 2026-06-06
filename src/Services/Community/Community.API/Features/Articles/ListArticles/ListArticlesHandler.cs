using Community.API.Common.DTOs;
using Community.API.Entities;
using Community.API.Persistence;
using Community.API.Services.S3;
using MediatR;

namespace Community.API.Features.Articles.ListArticles;

public class ListArticlesHandler(IArticleRepository articleRepository,IS3Service s3Service)
    : IRequestHandler<ListArticlesQuery, PaginatedListDto<Article>>
{
    public async Task<PaginatedListDto<Article>> Handle(ListArticlesQuery request, CancellationToken cancellationToken)
    {
        var (articles, totalCount) = await articleRepository.ListAsync(
            request.Tag, request.AuthorId, request.Sort, request.Page, request.PageSize);

        var newList =  articles.Select(a =>
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
