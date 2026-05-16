using Community.API.Common.Abstractions;
using Community.API.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Comments.CreateComment;

public class CreateCommentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/articles/{articleId:guid}/comments", async (
                [FromRoute] Guid articleId,
                [FromBody] CreateCommentRequest request,
                [FromServices] IMediator mediator) =>
            {
                var command = new CreateCommentCommand(articleId, request.Body, request.ParentCommentId);
                var result = await mediator.Send(command);
                return Results.StatusCode(StatusCodes.Status201Created);
            })
            .RequireAuthorization()
            .WithTags("Comments")
            .Produces<CommentNode>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }
}

public record CreateCommentRequest(string Body, Guid? ParentCommentId);
