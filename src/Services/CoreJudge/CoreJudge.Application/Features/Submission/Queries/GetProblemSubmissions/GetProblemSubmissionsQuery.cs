using BuildingBlocks.Core.CQRS;
using CoreJudge.Domain.Premitives;
using MediatR;

namespace CoreJudge.Application.Features.Submissions.Queries.GetProblemSubmissions
{
    public class GetProblemSubmissionsQuery : IQuery<Response>
    {
        public GetProblemSubmissionsQuery(int problemId, string UserId)
        {
            this.ProblemId = problemId;
            this.UserId = UserId;   
        }
        public string UserId { get; set; }
        public int ProblemId { get; set; }
    }
}
