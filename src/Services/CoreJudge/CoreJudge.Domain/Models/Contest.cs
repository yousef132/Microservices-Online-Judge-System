using CoreJudge.Domain.Premitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace CoreJudge.Domain.Models
{
    public class Contest : BaseEntity
    {
        public Guid ContestSetterId { get; set; }
        public string Name { get; set; } = default!;
        public TimeSpan Duration => EndDate.Subtract(StartDate);
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } = DateTime.Now;

        public ContestStatus ContestStatus
            => DateTime.Now < StartDate ? ContestStatus.Upcoming : DateTime.Now > EndDate ? ContestStatus.Ended : ContestStatus.Running;

        public ICollection<UserContest> Registrations { get; set; } = default!;
        public ICollection<Problem> Problems { get; set; } = default!;
        public ICollection<Submission> Submissions { get; set; } = default!;
    }
}
