using Community.API.Common.Exceptions;
using Community.API.Entities;
using Community.API.Features.Articles.CreateArticle;
using Community.API.Persistence;
using MediatR;
using System.Net.Http.Headers;

namespace Community.API.Features.Articles.SimulateCoverUpload;

public class SimulateCoverUploadHandler(
    IArticleRepository articleRepository,
    IHttpClientFactory httpClientFactory,
    IMediator mediator)
    : IRequestHandler<SimulateCoverUploadCommand, SimulateCoverUploadResponse>
{
    public async Task<SimulateCoverUploadResponse> Handle(
        SimulateCoverUploadCommand request,
        CancellationToken cancellationToken)
    {
        var article = await articleRepository.GetByIdAsync(request.ArticleId)
            ?? throw new NotFoundException(nameof(Article), request.ArticleId);

        // 1. upload file to S3 using presigned URL
        var client = httpClientFactory.CreateClient();

        using var stream = request.File.OpenReadStream();
        using var content = new StreamContent(stream);

        content.Headers.ContentType =
            new MediaTypeHeaderValue(request.File.ContentType);

        var uploadResponse =
            await client.PutAsync(request.PreSignedUrl, content, cancellationToken);

        if (!uploadResponse.IsSuccessStatusCode)
            throw new Exception("Simulated upload failed");

        // 2. call confirm endpoint logic (IMPORTANT)
        var confirmCommand = new UploadFileCommand(
            ArticleId: request.ArticleId,
            IsUploaded: true,
            ObjectKey: request.ObjectKey
        );

        // send via mediatr for now, instead of http request
        var updatedArticle = await mediator.Send(confirmCommand, cancellationToken);

        // 3. return result
        return new SimulateCoverUploadResponse(
            ObjectKey: request.ObjectKey,
            UploadUrl: request.PreSignedUrl,
            Uploaded: true,
            Article: updatedArticle
        );
    }
}