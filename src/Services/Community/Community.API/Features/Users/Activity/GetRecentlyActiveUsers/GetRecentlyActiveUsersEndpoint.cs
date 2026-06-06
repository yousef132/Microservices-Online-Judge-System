using Community.API.Common.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Users.Activity.GetRecentlyActiveUsers;

public class GetRecentlyActiveUsersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/recently-active", async (
            [FromQuery] int? timeSpanInMinutes,
            [FromServices] IMediator mediator) =>
        {
            var timeSpan = timeSpanInMinutes ?? 60;
            var result = await mediator.Send(new GetRecentlyActiveUsersQuery(timeSpan));
            return Results.Ok(result);
        })
        .WithTags("Users")
        .Produces<IEnumerable<Guid>>(StatusCodes.Status200OK);
    }
}
