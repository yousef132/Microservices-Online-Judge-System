using AutoMapper;
using CodeSphere.Domain.Abstractions;
using CoreJudge.Application.Features.Problem.Common;
using CoreJudge.Application.Features.Problems.Queries.GetAll;

namespace CoreJudge.Application.Mapping.Resolvers;

public class TopicResolver : IValueResolver<ProblemDocument, GetAllQueryResponse, List<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public TopicResolver(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public List<string> Resolve(ProblemDocument source, GetAllQueryResponse destination, List<string> destMember, ResolutionContext context)
    {
        return _unitOfWork.TopicRepository
            .GetTopicNamesByIdsAsync(source.Topics).Result;  // This is a blocking call :( 
    }
}