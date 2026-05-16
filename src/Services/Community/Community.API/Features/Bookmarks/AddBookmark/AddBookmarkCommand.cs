using Community.API.Common.DTOs;
using MediatR;

namespace Community.API.Features.Bookmarks.AddBookmark;

public record AddBookmarkCommand(Guid ArticleId) : IRequest<BookmarkResponse>;
