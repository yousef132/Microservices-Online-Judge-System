using System.ComponentModel.DataAnnotations.Schema;

namespace CoreJudge.Domain.Models
{
    public class Testcase : BaseEntity
    {
        public Testcase(int problemId, string input, string output)
        {
            ProblemId = problemId;
            Input = input;
            Output = output;
        }

        public int ProblemId { get; set; }
        public string Input { get; set; } = default!;
        public string Output { get; set; } = default!;


        [ForeignKey(nameof(ProblemId))]
        [InverseProperty(nameof(Problem.Testcases))]
        public Problem Problem { get; set; } = default!;
    }
}
