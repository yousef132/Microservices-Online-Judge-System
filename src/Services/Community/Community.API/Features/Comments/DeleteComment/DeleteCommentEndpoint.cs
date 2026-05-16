using Community.API.Common.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Comments.DeleteComment;

public class DeleteCommentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/articles/{articleId:guid}/comments/{commentId:guid}", async (
                [FromRoute] Guid articleId,
                [FromRoute] Guid commentId,
                [FromServices] IMediator mediator) =>
            {
                await mediator.Send(new DeleteCommentCommand(articleId, commentId));
                return Results.NoContent();
            })
            .RequireAuthorization()
            .WithTags("Comments")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
