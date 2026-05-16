using System.Security.Claims;
using Community.API.Common.DTOs;
using Community.API.Persistence;
using Community.API.Services.S3;
using MediatR;

namespace Community.API.Features.Articles.GetArticleBySlug;

public class GetArticleBySlugHandler(
    IArticleRepository articleRepository,
    IVoteRepository voteRepository,
    IBookmarkRepository bookmarkRepository,
    IUserActivityLogRepository userActivityLogRepository,
    IS3Service s3Service,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<GetArticleBySlugQuery, ArticleDto?>
{
    public async Task<ArticleDto?> Handle(GetArticleBySlugQuery request, CancellationToken cancellationToken)
    {
        var article = await articleRepository.GetBySlugAsync(request.Slug);

        if (article is null || article.Status != "Published")
            return null;

        var user = httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdClaim, out var userId);

        // fire-and-forget: we don't want to delay the response while we update the view count and log activity
        _ = Task.Run(async () =>
        {
            await articleRepository.IncrementViewCountAsync(article.Id);
            if (userId != Guid.Empty)
                await userActivityLogRepository.LogActivityAsync(userId, article.Id, "View");
        }, cancellationToken);

        var articleDto = ArticleDto.FromArticle(article);

        if (!string.IsNullOrEmpty(article.CoverImageKey))
            articleDto.CoverImageUrl = s3Service.GetPreSignedGetUrl(article.CoverImageKey);

        if (userId != Guid.Empty)
        {
            var userVote = await voteRepository.GetVoteAsync(userId, article.Id, "Article");
            var isBookmarked = await bookmarkRepository.IsBookmarkedAsync(userId, article.Id);
            articleDto.UserVote = userVote?.Value ?? 0;
            articleDto.Bookmarked = isBookmarked;
        }

        return articleDto;
    }
}
