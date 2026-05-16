using Community.API.Entities;

namespace Community.API.Services.Auth;

public interface IAuthHelper
{
    bool IsAuthorOrAdmin(Article article);
    bool IsAuthor(Article article);
    bool IsAuthorOrAdmin(CommentNode commentNode);
    bool IsAuthor(CommentNode commentNode);
}
