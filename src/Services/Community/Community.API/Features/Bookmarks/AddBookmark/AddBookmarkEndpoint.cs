using Community.API.Common.Abstractions;
using Community.API.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Bookmarks.AddBookmark;

public class AddBookmarkEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bookmarks", async (
                [FromBody] AddBookmarkCommand command,
                [FromServices] IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Bookmarks")
            .Produces<BookmarkResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
