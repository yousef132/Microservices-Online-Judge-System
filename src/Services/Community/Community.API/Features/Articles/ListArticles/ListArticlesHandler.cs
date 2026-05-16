using Community.API.Common.DTOs;
using Community.API.Entities;
using Community.API.Persistence;
using MediatR;

namespace Community.API.Features.Articles.ListArticles;

public class ListArticlesHandler(IArticleRepository articleRepository)
    : IRequestHandler<ListArticlesQuery, PaginatedListDto<Article>>
{
    public async Task<PaginatedListDto<Article>> Handle(ListArticlesQuery request, CancellationToken cancellationToken)
    {
        var (articles, totalCount) = await articleRepository.ListAsync(
            request.Tag, request.AuthorId, request.Sort, request.Page, request.PageSize);

        return new PaginatedListDto<Article>
        {
            Items = articles,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
