namespace CoreJudge.Domain.Models
{
    public class Topic : BaseEntity
    {
        public string Name { get; set; } = default!;

        public ICollection<ProblemTopic> ProblemTopics { get; set; } = default!;
    }
}
