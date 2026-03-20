using AutoMapper;
using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CoreJudge.Application.Features.Contests.Command.Update;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using MediatR;

namespace CoreJudge.Application.Features.Contests.Command.Update
{
    public class UpdateContestCommandHandler : ICommandHandler<UpdateContestCommand, Response>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public UpdateContestCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<Response> Handle(UpdateContestCommand request, CancellationToken cancellationToken)
        {
            var contest = await unitOfWork.Repository<Contest>().GetByIdAsync(request.Id);
            if (contest == null)
            {
                return await Response.FailureAsync("Contest not found", System.Net.HttpStatusCode.NotFound);
            }

            var mappedContest = mapper.Map(request, contest);
            await unitOfWork.Repository<Contest>().UpdateAsync(mappedContest);
            await unitOfWork.CompleteAsync();



            return await Response.SuccessAsync(request, "Contest updated successfully", System.Net.HttpStatusCode.OK);
        }
    }
}
