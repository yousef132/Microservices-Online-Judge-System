using Community.API.Common.Abstractions;
using Community.API.Common.DTOs;
using Community.API.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Articles.ListArticles;

public class ListArticlesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/articles", async (
                [FromQuery] string? tag,
                [FromQuery] Guid? authorId,
                [FromQuery] string sort = "new",
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 20,
                [FromServices] IMediator mediator = null!) =>
            {
                var result = await mediator.Send(new ListArticlesQuery(tag, authorId, sort, page, pageSize));
                return Results.Ok(result);
            })
            .WithTags("Articles")
            .Produces<PaginatedListDto<Article>>(StatusCodes.Status200OK);
    }
}
