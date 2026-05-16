using Community.API.Common.Abstractions;
using Community.API.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Bookmarks.RemoveBookmark;

public class RemoveBookmarkEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/bookmarks/{articleId:guid}", async (
                [FromRoute] Guid articleId,
                [FromServices] IMediator mediator) =>
            {
                var result = await mediator.Send(new RemoveBookmarkCommand(articleId));
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Bookmarks")
            .Produces<BookmarkResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
