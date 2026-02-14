using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CodeSphere.Domain.Abstractions.Services;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using MediatR;

namespace CoreJudge.Application.Features.Contests.Queries.GetContestStanding
{
    public class GetContestStandingQuery : IQuery<Response>
    {
        public int ContestId { get; set; }
        public int Start { get; set; }
        public int Stop { get; set; }
        public GetContestStandingQuery(int contestId)
        {
            ContestId = contestId;
            Start = 0;
            Stop = 20;
        }
    }

    public class GetContestStandingQueryHandler : IQueryHandler<GetContestStandingQuery, Response>
    {
        //private readonly ICacheService cacheService;
        private readonly IUnitOfWork unitOfWork;

        public GetContestStandingQueryHandler(IUnitOfWork unitOfWork)
        {
            //this.cacheService = cacheService;
            this.unitOfWork = unitOfWork;
        }
        public async Task<Response> Handle(GetContestStandingQuery request, CancellationToken cancellationToken)
        {
            var contest = await unitOfWork.Repository<Contest>().GetByIdAsync(request.ContestId);
            if (contest == null)
                return await Response.FailureAsync("Contest Not Found", System.Net.HttpStatusCode.NotFound);

            //if (contest.ContestStatus == ContestStatus.Running)
            //{
            //    // return the data from cache
            //    var leaderboard = cacheService.GetContestStanding(request.ContestId, request.Start, request.Stop);
            //    return await Response.SuccessAsync(leaderboard, "Contest Standing Fetched Successfully", System.Net.HttpStatusCode.OK);
            //}

            //var standing = await unitOfWork.ContestRepository.GetContestStanding(request.ContestId, 0, 10);
            return await Response.SuccessAsync(null, "Contest Standing Fetched Successfully", System.Net.HttpStatusCode.OK);


        }
    }
}
