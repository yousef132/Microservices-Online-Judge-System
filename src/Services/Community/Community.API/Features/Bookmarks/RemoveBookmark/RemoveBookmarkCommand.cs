using Community.API.Common.DTOs;
using MediatR;

namespace Community.API.Features.Bookmarks.RemoveBookmark;

public record RemoveBookmarkCommand(Guid ArticleId) : IRequest<BookmarkResponse>;
