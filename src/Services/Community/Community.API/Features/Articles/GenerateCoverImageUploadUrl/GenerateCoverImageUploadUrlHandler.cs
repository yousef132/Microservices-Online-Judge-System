using Community.API.Common.DTOs;
using Community.API.Common.Exceptions;
using Community.API.Entities;
using Community.API.Persistence;
using Community.API.Services.Auth;
using Community.API.Services.S3;
using MediatR;

namespace Community.API.Features.Articles.GenerateCoverImageUploadUrl;

public class GenerateCoverImageUploadUrlHandler(
    IArticleRepository articleRepository,
    IS3Service s3Service,
    IAuthHelper authHelper)
    : IRequestHandler<GenerateCoverImageUploadUrlCommand, GenerateCoverImageUploadUrlResponse>
{
    public async Task<GenerateCoverImageUploadUrlResponse> Handle(
        GenerateCoverImageUploadUrlCommand request, CancellationToken cancellationToken)
    {
        var article = await articleRepository.GetByIdAsync(request.ArticleId)
            ?? throw new NotFoundException(nameof(Article), request.ArticleId);

        if (!authHelper.IsAuthor(article))
            throw new ForbiddenAccessException("User is not authorized to manage the cover image for this article.");

        var (uploadUrl, objectKey) = s3Service.GetPreSignedPutUrl(article.Id, request.ContentType);

        return new GenerateCoverImageUploadUrlResponse
        {
            UploadUrl = uploadUrl,
            ObjectKey = objectKey
        };
    }
}
