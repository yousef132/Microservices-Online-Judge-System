using Community.API.Common.Abstractions;
using Community.API.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Articles.GetArticleBySlug;

public class GetArticleBySlugEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/articles/{slug}", async (
                [FromRoute] string slug,
                [FromServices] IMediator mediator) =>
            {
                var result = await mediator.Send(new GetArticleBySlugQuery(slug));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("GetArticleBySlug")
            .WithTags("Articles")
            .Produces<ArticleDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}
