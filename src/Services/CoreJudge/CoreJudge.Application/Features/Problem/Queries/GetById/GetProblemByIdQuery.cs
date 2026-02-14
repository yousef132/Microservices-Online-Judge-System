using BuildingBlocks.Core.CQRS;
using CoreJudge.Domain.Premitives;
using MediatR;

namespace CoreJudge.Application.Features.Problems.Queries.GetById
{
    public class GetProblemByIdQuery : IQuery<Response>
    {
        public int ProblemId { get; set; }

        public GetProblemByIdQuery(int problemId)
        {
            ProblemId = problemId;
        }
    }


}
