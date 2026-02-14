using CoreJudge.Domain.Premitives;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreJudge.Domain.Models
{
    public class Submission : BaseEntity
    {
        public Guid AttemperId { get; set; }
        public int ProblemId { get; set; }
        public int? ContestId { get; set; }
        public string? Error { get; set; }
        // the code execution time
        public decimal? SubmitTime { get; set; }
        // the code execution memory
        public decimal? SubmitMemory { get; set; }
        public SubmissionResult Result { get; set; }
        public string Code { get; set; } = default!;
        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
        public Language Language { get; set; }

        [ForeignKey(nameof(ProblemId))]
        public Problem Problem { get; set; } = default!;

        [ForeignKey(nameof(ContestId))]
        public Contest Contest { get; set; } = default!;
    }
}
