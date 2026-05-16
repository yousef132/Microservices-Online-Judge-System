using Community.API.Entities;

namespace Community.API.Common.DTOs;

public class CommentNodeDto
{
    public Guid Id { get; set; }
    public Author Author { get; set; } = null!;
    public string Body { get; set; } = string.Empty;
    public int VoteCount { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<CommentNodeDto> Replies { get; set; } = new();

    // User-specific field
    public int UserVote { get; set; }

    public static CommentNodeDto FromCommentNode(CommentNode node, Dictionary<Guid, int> userVotes)
    {
        return new CommentNodeDto
        {
            Id = node.Id,
            Author = node.Author,
            Body = node.Body,
            VoteCount = node.VoteCount,
            IsDeleted = node.IsDeleted,
            CreatedAt = node.CreatedAt,
            UpdatedAt = node.UpdatedAt,
            UserVote = userVotes.GetValueOrDefault(node.Id, 0),
            Replies = node.Replies
                .Select(reply => FromCommentNode(reply, userVotes))
                .OrderBy(r => r.CreatedAt)
                .ToList()
        };
    }
}
