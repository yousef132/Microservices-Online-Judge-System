using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CoreJudge.Domain.Premitives;
using System.Net;

namespace CoreJudge.Application.Features.Problem.Queries.GetProblemLanguagesPlaceHolders
{
    public class GetProblemCodePlaceHolderQueryHandler(IUnitOfWork _unitOfWork) : IQueryHandler<GetProblemLanguagesPlaceholdersQuery, Response>
    {
        public async Task<Response> Handle(GetProblemLanguagesPlaceholdersQuery query, CancellationToken cancellationToken)

        {

            var PlaceHolders = await _unitOfWork.ProblemRepository.GetProblemPlaceHolders(query.ProblemId, cancellationToken);

            var mappedPlaceHoldersList = PlaceHolders.Select(x => new ProblemLanguagePlaceholder(x.Language, x.UserCodeTemplate)).ToList();

            return await Response.SuccessAsync(mappedPlaceHoldersList, "Problem PlaceHolders Retrieved", HttpStatusCode.OK);

        }

    }
}
