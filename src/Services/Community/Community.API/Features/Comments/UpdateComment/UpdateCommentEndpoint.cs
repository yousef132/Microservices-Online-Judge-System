using Community.API.Common.Abstractions;
using Community.API.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Comments.UpdateComment;

public class UpdateCommentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/articles/{articleId:guid}/comments/{commentId:guid}", async (
                [FromRoute] Guid articleId,
                [FromRoute] Guid commentId,
                [FromBody] UpdateCommentRequest request,
                [FromServices] IMediator mediator) =>
            {
                var command = new UpdateCommentCommand(articleId, commentId, request.Body);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Comments")
            .Produces<CommentNode>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}

public record UpdateCommentRequest(string Body);
