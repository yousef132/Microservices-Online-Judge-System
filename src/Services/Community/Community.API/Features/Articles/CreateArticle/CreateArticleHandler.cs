using System.Security.Claims;
using Community.API.Entities;
using Community.API.Persistence;
using Community.API.Services.Slugs;
using MediatR;

namespace Community.API.Features.Articles.CreateArticle;

public class CreateArticleHandler(
    IArticleRepository articleRepository,
    ITagRepository tagRepository,
    ISlugGenerator slugGenerator,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<CreateArticleCommand, Article>
{
    public async Task<Article> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        var userNameClaim = user?.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User is not authenticated or user ID is invalid.");

        var now = DateTime.UtcNow;
        var article = new Article
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Body = request.Body,
            Tags = request.Tags.Select(t => t.ToLowerInvariant()).Distinct().ToList(),
            Status = request.Status,
            Author = new Author { Id = userId, Username = userNameClaim ?? "unknown" },
            CreatedAt = now,
            UpdatedAt = now,
            PublishedAt = request.Status == "Published" ? now : null
        };

        article.Slug = await slugGenerator.GenerateSlugAsync(article.Title);

        await articleRepository.CreateAsync(article);
        await tagRepository.IncrementTagsAsync(article.Tags);

        return article;
    }
}
