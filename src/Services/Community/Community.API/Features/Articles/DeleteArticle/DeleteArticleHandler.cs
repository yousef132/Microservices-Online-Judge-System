using Community.API.Common.Exceptions;
using Community.API.Entities;
using Community.API.Persistence;
using Community.API.Services.Auth;
using MediatR;

namespace Community.API.Features.Articles.DeleteArticle;

public class DeleteArticleHandler(
    IArticleRepository articleRepository,
    ITagRepository tagRepository,
    IAuthHelper authHelper) : IRequestHandler<DeleteArticleCommand>
{
    public async Task Handle(DeleteArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await articleRepository.GetByIdAsync(request.ArticleId)
            ?? throw new NotFoundException(nameof(Article), request.ArticleId);

        if (!authHelper.IsAuthorOrAdmin(article))
            throw new ForbiddenAccessException("User is not authorized to delete this article.");

        if (article.Status == "Deleted") return; // Idempotent

        var originalTags = article.Tags;
        article.Status = "Deleted";
        article.UpdatedAt = DateTime.UtcNow;

        await articleRepository.UpdateAsync(article);
        if (originalTags.Any())
            await tagRepository.DecrementTagsAsync(originalTags);
    }
}
