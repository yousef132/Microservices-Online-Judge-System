using System.Security.Claims;
using Community.API.Entities;

namespace Community.API.Services.Auth;

public class AuthHelper(IHttpContextAccessor httpContextAccessor) : IAuthHelper
{
    public bool IsAuthorOrAdmin(Article article)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is null) return false;
        if (user.IsInRole("Admin")) return true;
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId)) return false;
        return article.Author.Id == userId;
    }

    public bool IsAuthor(Article article)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is null) return false;
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId)) return false;
        return article.Author.Id == userId;
    }

    public bool IsAuthorOrAdmin(CommentNode commentNode)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is null) return false;
        if (user.IsInRole("Admin")) return true;
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId)) return false;
        return commentNode.Author.Id == userId;
    }

    public bool IsAuthor(CommentNode commentNode)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is null) return false;
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId)) return false;
        return commentNode.Author.Id == userId;
    }
}
