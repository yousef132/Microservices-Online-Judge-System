using MediatR;

namespace Community.API.Features.Articles.SimulateCoverUpload;

public record SimulateCoverUploadCommand(
    Guid ArticleId,
    IFormFile File,
    string PreSignedUrl,
    string ObjectKey
) : IRequest<SimulateCoverUploadResponse>;
