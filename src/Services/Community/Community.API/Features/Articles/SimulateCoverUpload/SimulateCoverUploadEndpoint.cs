using Community.API.Common.Abstractions;
using Community.API.Services.S3;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Articles.SimulateCoverUpload;

public class SimulateCoverUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/articles/{articleId:guid}/cover-image/simulate-upload", async (
                [FromRoute] Guid articleId,
                [FromForm] CoverImageUploadDto uploadData,
                [FromServices] IMediator mediator) =>
        {
            var command = new SimulateCoverUploadCommand(
                articleId,
                uploadData.File,
                uploadData.UploadUrl,
                uploadData.ObjectKey);

            var result = await mediator.Send(command);

            return Results.Ok(result);
        })
        .WithTags("Articles")
        .WithMetadata(new ConsumesAttribute("multipart/form-data"))
        .DisableAntiforgery();
    }
}

public class CoverImageUploadDto
{
    public IFormFile File { get; set; }
    public string UploadUrl { get; set; }
    public string ObjectKey { get; set; }
}