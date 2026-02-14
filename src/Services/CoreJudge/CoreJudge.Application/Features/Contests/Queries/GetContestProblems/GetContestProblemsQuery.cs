using BuildingBlocks.Core.CQRS;
using CoreJudge.Domain.Premitives;
using MediatR;

namespace CoreJudge.Application.Features.Contests.Queries.GetContestProblems
{
    public class GetContestProblemsQuery : IQuery<Response>
    {
        public int Id { get; set; }

        public GetContestProblemsQuery(int id)
        {
            Id = id;
        }
    }


}
