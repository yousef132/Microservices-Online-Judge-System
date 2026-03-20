using BuildingBlocks.Core.CQRS;
using CoreJudge.Domain.Premitives;
using MediatR;
namespace CoreJudge.Application.Features.Testcases.Queries.GetTestCasesByProblemId
{
    public sealed record GetTestCasesByProblemIdQuerey(
        int ProblemId
    ) : IQuery<Response>;
}
