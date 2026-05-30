using Community.API.Common.Exceptions;
using Community.API.Entities;
using Community.API.Features.Articles.CreateArticle;
using Community.API.Persistence;
using Community.API.Services.Auth;
using Community.API.Services.Slugs;
using MediatR;
using System.Security.Claims;

namespace Community.API.Features.Articles.ConfirmFileUpload;

public class ConfirmFileUploadHandler(
    IArticleRepository articleRepository,
     IAuthHelper authHelper,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<UploadFileCommand, Article>
{
    public async Task<Article> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        var userNameClaim = user?.FindFirstValue(ClaimTypes.Name);

        var article = await articleRepository.GetByIdAsync(request.ArticleId)
           ?? throw new NotFoundException(nameof(Article), request.ArticleId);

        if (!authHelper.IsAuthorOrAdmin(article))
            throw new ForbiddenAccessException("User is not authorized to edit this article.");

        article.CoverImageKey = request.IsUploaded ? request.ObjectKey : null;
        await articleRepository.UpdateAsync(article);
        return article;
    }
}
