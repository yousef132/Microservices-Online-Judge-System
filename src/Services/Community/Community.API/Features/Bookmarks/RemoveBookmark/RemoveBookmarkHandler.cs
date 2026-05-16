using System.Security.Claims;
using Community.API.Common.DTOs;
using Community.API.Persistence;
using MediatR;

namespace Community.API.Features.Bookmarks.RemoveBookmark;

public class RemoveBookmarkHandler(
    IBookmarkRepository bookmarkRepository,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<RemoveBookmarkCommand, BookmarkResponse>
{
    public async Task<BookmarkResponse> Handle(RemoveBookmarkCommand request, CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        await bookmarkRepository.DeleteAsync(userId, request.ArticleId);

        return new BookmarkResponse { ArticleId = request.ArticleId, Bookmarked = false };
    }
}
