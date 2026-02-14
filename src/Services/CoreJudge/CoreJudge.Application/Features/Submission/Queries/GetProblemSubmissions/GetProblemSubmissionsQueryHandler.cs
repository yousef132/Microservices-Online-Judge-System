using AutoMapper;
using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CodeSphere.Domain.Abstractions.Repositories;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Security.Claims;

namespace CoreJudge.Application.Features.Submissions.Queries.GetProblemSubmissions
{
    public class GetProblemSubmissionsQueryHandler : IQueryHandler<GetProblemSubmissionsQuery, Response>
    {
        private readonly IMapper _mapper;
        private readonly ISubmissionRepository _submissionRepository;
        private readonly IUnitOfWork unitOfWork;

        public GetProblemSubmissionsQueryHandler(IMapper mapper,
            ISubmissionRepository submissionRepository,
            IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _submissionRepository = submissionRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Response> Handle(GetProblemSubmissionsQuery request, CancellationToken cancellationToken)
        {

            var problem = await unitOfWork.Repository<Problem>().GetByIdAsync(request.ProblemId);

            if (problem is null)
                return await Response.FailureAsync( "Problem Not Found", HttpStatusCode.NotFound);

            //var user =await userManager.FindByIdAsync(request.UserId);
            //if (user is null)
            //    return await Response.FailureAsync( "user Not Found", HttpStatusCode.NotFound);

            var submissions = await _submissionRepository.GetAllSubmissions(request.ProblemId, request.UserId);
            var submissionsList = submissions.ToList();
            if (submissionsList.Count() == 0)
                return await Response.SuccessAsync(null, "No Submissions", HttpStatusCode.NoContent);

            var mappedSubmissions = _mapper.Map<List<GetProblemSubmissionsResponse>>(submissionsList);

            return await Response.SuccessAsync(mappedSubmissions, "Submissions fetched successfully", HttpStatusCode.Found);
        }
    }
}
