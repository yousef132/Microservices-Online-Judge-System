using System.Security.Claims;
using AutoMapper;
using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CoreJudge.Application.Features.Contests.Queries.GetContestSubmissionsHistory
{
    public class GetContestSubmissionsQuery : IQuery<Response>
    {
        public int ContestId { get; set; }


        public GetContestSubmissionsQuery(int contestId)
        {
            ContestId = contestId;
        }
    }

    public class GetContestSubmissionsQueryResponse
    {
        public int Id { get; set; }
        public string ProblemName { get; set; }
        public decimal Time { get; set; }
        public DateTime SubmissionDate { get; set; }
        public SubmissionResult Result { get; set; }
        public Language Language { get; set; }
    }

    public class GetContestSubmissionsQueryHandler : IQueryHandler<GetContestSubmissionsQuery, Response>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor contextAccessor;
        private string? userId;

        public GetContestSubmissionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor contextAccessor)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.contextAccessor = contextAccessor;
            var user = contextAccessor.HttpContext?.User;
            userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        public async Task<Response> Handle(GetContestSubmissionsQuery request, CancellationToken cancellationToken)
        {

            var contest = await unitOfWork.Repository<Contest>().GetByIdAsync(request.ContestId);
            if (contest == null)
                return await Response.FailureAsync("Contest Not Found", System.Net.HttpStatusCode.NotFound);


            var submission = await unitOfWork.SubmissionRepository.GetUserContestSubmissions(request.ContestId, userId);

            var mapppedSubmissions = mapper.Map<List<GetContestSubmissionsQueryResponse>>(submission);

            return await Response.SuccessAsync(mapppedSubmissions, "Contest Submissions fetched successfully", System.Net.HttpStatusCode.OK);

        }
    }


}
