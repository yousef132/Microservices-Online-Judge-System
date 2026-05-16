using Community.API.Common.Abstractions;
using Community.API.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Articles.UpdateArticle;

public class UpdateArticleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/articles/{articleId:guid}", async (
                [FromRoute] Guid articleId,
                [FromBody] UpdateArticleRequest request,
                [FromServices] IMediator mediator) =>
            {
                var command = new UpdateArticleCommand(
                    articleId, request.Title, request.Body,
                    request.Tags, request.Status, request.CoverImageKey);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Articles")
            .Produces<Article>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}

public record UpdateArticleRequest(
    string? Title,
    string? Body,
    List<string>? Tags,
    string? Status,
    string? CoverImageKey);
