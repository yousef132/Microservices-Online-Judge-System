using Community.API.Entities;
using MediatR;

namespace Community.API.Features.Articles.CreateArticle;

public record UploadFileCommand(
    Guid ArticleId,
    bool IsUploaded,
    string ObjectKey) : IRequest<Article>;
