using Community.API.Common.Abstractions;
using Community.API.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Users.Activity.GetRecentActivity;

public class GetRecentActivityEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/{userId:guid}/recent-activity", async (
            [FromRoute] Guid userId,
            [FromQuery] int? limit,
            [FromServices] IMediator mediator) =>
        {
            var itemsLimit = limit ?? 10;
            var result = await mediator.Send(new GetRecentActivityQuery(userId, itemsLimit));
            return Results.Ok(result);
        })
        .WithTags("Users")
        .Produces<IEnumerable<UserActivity>>(StatusCodes.Status200OK);
    }
}
