using BuildingBlocks.Core.CQRS;
using CoreJudge.Domain.Premitives;

namespace CoreJudge.Application.Features.Problem.Queries.GetProblemLanguagesPlaceHolders
{
    public record GetProblemLanguagesPlaceholdersQuery(int ProblemId) : IQuery<Response>;
}
