using Community.API.Common.Abstractions;
using Community.API.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Votes.CastVote;

public class CastVoteEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/votes", async (
                [FromBody] CastVoteCommand command,
                [FromServices] IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Votes")
            .Produces<CastVoteResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }
}
