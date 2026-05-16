using Community.API.Common.Abstractions;
using Community.API.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Community.API.Features.Comments.GetCommentsForArticle;

public class GetCommentsForArticleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/articles/{articleId:guid}/comments", async (
                [FromRoute] Guid articleId,
                [FromServices] IMediator mediator) =>
            {
                var result = await mediator.Send(new GetCommentsForArticleQuery(articleId));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            })
            .WithTags("Comments")
            .Produces<List<CommentNodeDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}
