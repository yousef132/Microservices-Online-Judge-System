using Community.API.Entities;

namespace Community.API.Features.Articles.SimulateCoverUpload;

public record SimulateCoverUploadResponse(
    string ObjectKey,
    string UploadUrl,
    bool Uploaded,
    Article Article
);
