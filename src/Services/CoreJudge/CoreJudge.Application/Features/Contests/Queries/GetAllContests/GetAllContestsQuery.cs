using BuildingBlocks.Core.CQRS;
using CoreJudge.Domain.Premitives;
using MediatR;

namespace CoreJudge.Application.Features.Contests.Queries.GetAllContests
{
    public class GetAllContestsQuery : IQuery<Response>
    {
    }
}
