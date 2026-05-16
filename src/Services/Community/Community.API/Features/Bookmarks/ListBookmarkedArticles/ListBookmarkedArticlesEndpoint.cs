using Community.API.Common.Abstractions;
using Community.API.Common.DTOs;
using Community.API.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Bookmarks.ListBookmarkedArticles;

public class ListBookmarkedArticlesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/bookmarks", async (
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 20,
                [FromServices] IMediator mediator = null!) =>
            {
                var result = await mediator.Send(new ListBookmarkedArticlesQuery(page, pageSize));
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Bookmarks")
            .Produces<PaginatedListDto<Article>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
