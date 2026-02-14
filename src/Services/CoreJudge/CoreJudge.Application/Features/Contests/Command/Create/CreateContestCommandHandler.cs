using AutoMapper;
using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CoreJudge.Application.Features.Contests.Command.Create
{
    public class CreateContestCommandHandler : ICommandHandler<CreateContestCommand, Response>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor contextAccessor;
        private string UserId;
        public CreateContestCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor contextAccessor)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.contextAccessor = contextAccessor;

            var user = contextAccessor.HttpContext?.User;
            UserId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }




        public async Task<Response> Handle(CreateContestCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(UserId))
                return await Response.FailureAsync("User must be authenticated", System.Net.HttpStatusCode.Unauthorized);

            var contest = mapper.Map<Contest>(request);
            contest.ContestSetterId = Guid.Parse(UserId);
            await unitOfWork.Repository<Contest>().AddAsync(contest);
            await unitOfWork.CompleteAsync();

            //var responseDto = new ContestResponseDto
            //{
            //    Id = contest.Id,
            //    Name = contest.Name
            //};

            return await Response.SuccessAsync(null, "Contest Created Successfully", System.Net.HttpStatusCode.Created);
        }
    }
}