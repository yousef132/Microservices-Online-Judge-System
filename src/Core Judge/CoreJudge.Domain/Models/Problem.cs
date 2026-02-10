using CoreJudge.Domain.Models.Entities;
using CoreJudge.Domain.Premitives;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace CoreJudge.Domain.Models
{
    public class Problem : BaseEntity
    {
        public Guid ProblemSetterId { get; set; }
        public int ContestId { get; set; }
        public string Name { get; set; } = default!;
        public Difficulty Difficulty { get; set; }

        public decimal RunTimeLimit { get; set; }
        public MemoryLimit MemoryLimit { get; set; }

        public string Description { get; set; } = default!;
        public ContestPoints ContestPoints { get; set; }


        [ForeignKey(nameof(ContestId))]
        public Contest Contest { get; set; } = default!;
        public ICollection<ProblemImage> Images { get; set; } = default!;

        public ICollection<Testcase> Testcases { get; set; } = default!;

        public ICollection<ProblemTopic> ProblemTopics { get; set; } = default!;

        public ICollection<Submission> Submissions { get; set; } = default!;

        //[ForeignKey(nameof(Blog))]
        //public int? BlogId { get; set; }
        //public Blog Blog { get; set; }

    }
}
