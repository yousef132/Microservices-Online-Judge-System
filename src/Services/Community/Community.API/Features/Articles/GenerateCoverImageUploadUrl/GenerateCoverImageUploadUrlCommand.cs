using Community.API.Common.DTOs;
using MediatR;

namespace Community.API.Features.Articles.GenerateCoverImageUploadUrl;

public record GenerateCoverImageUploadUrlCommand(
    Guid ArticleId,
    string ContentType) : IRequest<GenerateCoverImageUploadUrlResponse>;
