using Community.API.Common.Abstractions;
using Community.API.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Articles.GenerateCoverImageUploadUrl;

public class GenerateCoverImageUploadUrlEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/articles/{articleId:guid}/cover-image-upload-url", async (
                [FromRoute] Guid articleId,
                [FromBody] CoverImageUploadRequest request,
                [FromServices] IMediator mediator) =>
            {
                var command = new GenerateCoverImageUploadUrlCommand(articleId, request.ContentType);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Articles")
            .Produces<GenerateCoverImageUploadUrlResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}

public record CoverImageUploadRequest(string ContentType);
