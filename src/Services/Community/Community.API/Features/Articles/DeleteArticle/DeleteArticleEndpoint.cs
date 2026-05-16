using Community.API.Common.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Articles.DeleteArticle;

public class DeleteArticleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/articles/{articleId:guid}", async (
                [FromRoute] Guid articleId,
                [FromServices] IMediator mediator) =>
            {
                await mediator.Send(new DeleteArticleCommand(articleId));
                return Results.NoContent();
            })
            .RequireAuthorization()
            .WithTags("Articles")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
