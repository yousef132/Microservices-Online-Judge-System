using Community.API.Common.Abstractions;
using Community.API.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Articles.CreateArticle;

public class CreateArticleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/articles", async (
                [FromBody] CreateArticleCommand command,
                [FromServices] IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.CreatedAtRoute("GetArticleBySlug", new { slug = result.Slug }, result);
            })
            .RequireAuthorization()
            .WithTags("Articles")
            .Produces<Article>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
