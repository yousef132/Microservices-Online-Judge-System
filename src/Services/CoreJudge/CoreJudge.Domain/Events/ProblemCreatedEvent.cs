namespace CoreJudge.Domain.Events;

public class ProblemCreatedEvent
{
    public int ProblemId { get; set; }
    public string Title { get; set; }
    public string Difficulty { get; set; }
}
