using CoreJudge.Domain.Models.Entities;
using CoreJudge.Domain.Premitives;

namespace CoreJudge.Application.Abstractions.Elasticsearch;

public class ProblemDocument
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Difficulty Difficulty { get; set; }
    public decimal AcceptanceRate { get; set; }
    public List<ProblemTopicField> Topics { get; set; } = new();
}
public record ProblemTopicField(int Id, string Name);

public class UserAttemptDocument
{
    public string UserId { get; set; } = string.Empty;
    public int ProblemId { get; set; }
    public SubmissionResult? Status { get; set; }
}
