using Community.API.Common.Abstractions;
using Community.API.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Recommendations.GetRecommendations;

public class GetRecommendationsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/recommendations", async (
                [FromQuery] int limit = 10,
                [FromServices] IMediator mediator = null!) =>
            {
                var result = await mediator.Send(new GetRecommendationsQuery(limit));
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Recommendations")
            .Produces<IEnumerable<Article>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
