namespace CoreJudge.Domain.Models
{
    public class ProblemImage : BaseEntity
    {
        public int ProblemId { get; set; }
        public string ImagePath { get; set; } = default!;
        public Problem Problem { get; set; } = default!;
    }
}
