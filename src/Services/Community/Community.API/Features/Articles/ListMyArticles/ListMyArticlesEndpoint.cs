using Community.API.Common.Abstractions;
using Community.API.Common.DTOs;
using Community.API.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Articles.ListMyArticles;

public class ListMyArticlesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/articles/me", async (
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 20,
                [FromServices] IMediator mediator = null!) =>
            {
                var result = await mediator.Send(new ListMyArticlesQuery(page, pageSize));
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Articles")
            .Produces<PaginatedListDto<Article>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
