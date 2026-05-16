using Community.API.Entities;

namespace Community.API.Common.DTOs;

public class ArticleDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public Author Author { get; set; } = null!;
    public int ViewCount { get; set; }
    public int VoteCount { get; set; }
    public int CommentCount { get; set; }
    public double HotScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    // User-specific fields
    public int UserVote { get; set; }
    public bool Bookmarked { get; set; }

    public static ArticleDto FromArticle(Article article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            Slug = article.Slug,
            Title = article.Title,
            Body = article.Body,
            Tags = article.Tags,
            Status = article.Status,
            Author = article.Author,
            ViewCount = article.ViewCount,
            VoteCount = article.VoteCount,
            CommentCount = article.CommentCount,
            HotScore = article.HotScore,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt,
            PublishedAt = article.PublishedAt
        };
    }
}
