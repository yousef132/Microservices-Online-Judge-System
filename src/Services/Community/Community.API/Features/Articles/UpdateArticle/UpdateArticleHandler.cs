using Community.API.Common.Exceptions;
using Community.API.Entities;
using Community.API.Persistence;
using Community.API.Services.Auth;
using MediatR;

namespace Community.API.Features.Articles.UpdateArticle;

public class UpdateArticleHandler(
    IArticleRepository articleRepository,
    ITagRepository tagRepository,
    IAuthHelper authHelper)
    : IRequestHandler<UpdateArticleCommand, Article>
{
    public async Task<Article> Handle(UpdateArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await articleRepository.GetByIdAsync(request.ArticleId)
            ?? throw new NotFoundException(nameof(Article), request.ArticleId);

        if (!authHelper.IsAuthorOrAdmin(article))
            throw new ForbiddenAccessException("User is not authorized to edit this article.");

        var originalTags = new List<string>(article.Tags);
        var now = DateTime.UtcNow;

        if (request.Title is not null) article.Title = request.Title;
        if (request.Body is not null) article.Body = request.Body;
        if (request.CoverImageKey is not null) article.CoverImageKey = request.CoverImageKey;

        if (request.Tags is not null)
            article.Tags = request.Tags.Select(t => t.ToLowerInvariant()).Distinct().ToList();

        if (request.Status is not null && article.Status == "Draft" && request.Status == "Published")
        {
            article.Status = "Published";
            article.PublishedAt = now;
        }

        article.UpdatedAt = now;

        var tagsToAdd = article.Tags.Except(originalTags).ToList();
        var tagsToRemove = originalTags.Except(article.Tags).ToList();

        await articleRepository.UpdateAsync(article);

        if (tagsToAdd.Any()) await tagRepository.IncrementTagsAsync(tagsToAdd);
        if (tagsToRemove.Any()) await tagRepository.DecrementTagsAsync(tagsToRemove);

        return article;
    }
}
