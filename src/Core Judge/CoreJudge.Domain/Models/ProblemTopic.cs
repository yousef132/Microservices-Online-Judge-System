using System.ComponentModel.DataAnnotations.Schema;

namespace CoreJudge.Domain.Models
{
    public class ProblemTopic
    {
        public int ProblemId { get; set; }
        public int TopicId { get; set; }

        [ForeignKey(nameof(ProblemId))]
        public Problem Problem { get; set; } = default!;

        [ForeignKey(nameof(TopicId))]
        public Topic Topic { get; set; } = default!;
    }
}
