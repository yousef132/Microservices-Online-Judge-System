using Community.API.Common.Abstractions;
using Community.API.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Articles.CreateArticle;

public class ConfirmFileUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/articles/cover-image/confirm", async (
                [FromBody] UploadFileCommand command,
                [FromServices] IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.CreatedAtRoute("ConfirmFileUpload");
            })
            .RequireAuthorization()
            .WithTags("Articles")
            .Produces<Article>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
