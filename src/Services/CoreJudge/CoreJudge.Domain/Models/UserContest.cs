using System.ComponentModel.DataAnnotations.Schema;

namespace CoreJudge.Domain.Models
{
    public class UserContest
    {
        public Guid UserId { get; set; }
        public int ContestId { get; set; }

        [ForeignKey(nameof(ContestId))]
        public Contest Contest { get; set; } = default!;

        // the increase || decrease of rank
        public short RankChange { get; set; } = 0;
    }
}
