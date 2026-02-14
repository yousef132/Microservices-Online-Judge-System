using AutoMapper;
using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CodeSphere.Domain.Abstractions.Repositories;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using MediatR;


namespace CoreJudge.Application.Features.Testcases.Queries.GetTestCasesByProblemId
{
    public class GetTestCasesByProblemIdQuereyHandler : IQueryHandler<GetTestCasesByProblemIdQuerey, Response>
    {
        private readonly IMapper mapper;
        private readonly IProblemRepository _problemRepository;
        private readonly IUnitOfWork unitOfWork;

        public GetTestCasesByProblemIdQuereyHandler(IMapper mapper, IProblemRepository problemRepository, IUnitOfWork unitOfWork)
        {
            this.mapper = mapper;
            _problemRepository = problemRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Response> Handle(GetTestCasesByProblemIdQuerey request, CancellationToken cancellationToken)
        {
            var problem = await unitOfWork.Repository<Problem>().GetByIdAsync(request.ProblemId);
            if (problem == null)
                return await Response.FailureAsync("Problem not found", System.Net.HttpStatusCode.NotFound);

            var TestCases = _problemRepository.GetTestCasesByProblemId(request.ProblemId);

            return await Response.SuccessAsync(TestCases, "TestCases fetched successfully");
        }
    }
}
