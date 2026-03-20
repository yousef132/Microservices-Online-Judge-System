using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreJudge.Application.Features.Plagiarism.Queries.GetByContestIdQuery
{
    public class GetByContestIdResponse
    {
        //public SubmissionDTO FirstSubmission { get; set; }
        //public SubmissionDTO SecondSubmission { get; set; }
        public decimal Similarity { get; set; }
        public int ProblemId { get; set; }
    }
}
