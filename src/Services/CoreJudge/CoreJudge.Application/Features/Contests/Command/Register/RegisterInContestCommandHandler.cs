using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Security.Claims;

namespace CoreJudge.Application.Features.Contests.Command.Register
{
    public class RegisterInContestCommandHandler : ICommandHandler<RegisterInContestCommand, Response>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IHttpContextAccessor contextAccessor;
        private string UserId;
        public RegisterInContestCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor contextAccessor)
        {
            this.unitOfWork = unitOfWork;
            this.contextAccessor = contextAccessor;

            var user = contextAccessor.HttpContext?.User;
            UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        public async Task<Response> Handle(RegisterInContestCommand request, CancellationToken cancellationToken)
        {
            var contest = await unitOfWork.Repository<Contest>().GetByIdAsync(request.Id);
            if (contest == null)
                return await Response.FailureAsync("No Contest Found", HttpStatusCode.NotFound);

            if (contest.ContestStatus != ContestStatus.Running)
                return await Response.FailureAsync("Contest is not Running Now !!", HttpStatusCode.Forbidden);

            var isRegistered = await unitOfWork.UserContestRepository.IsRegistered(request.Id, UserId);
            if (isRegistered != null)
                return await Response.FailureAsync("Already registered in this contest", System.Net.HttpStatusCode.BadRequest);



            var registration = new UserContest
            {
                UserId = Guid.Parse(UserId),
                ContestId = request.Id,
            };

            var result = await unitOfWork.UserContestRepository.RegisterInContest(registration);

            return result ? await Response.SuccessAsync(null, message: "registered successfully", HttpStatusCode.OK)
                          : await Response.FailureAsync(message: "Failed To Register", HttpStatusCode.InternalServerError);


        }
    }
}
