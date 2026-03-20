using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using MediatR;

namespace CoreJudge.Application.Features.Contests.Command.Delete
{
    public class DeleteContestCommandHandler : ICommandHandler<DeleteContestCommand, Response>
    {
        private readonly IUnitOfWork unitOfWork;

        public DeleteContestCommandHandler(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<Response> Handle(DeleteContestCommand request, CancellationToken cancellationToken)
        {
            var contest = await unitOfWork.Repository<Contest>().GetByIdAsync(request.Id);
            if (contest == null)
            {
                return await Response.FailureAsync("Contest not found", System.Net.HttpStatusCode.NotFound);
            }

            await unitOfWork.Repository<Contest>().DeleteAsync(contest);
            await unitOfWork.CompleteAsync();

            //var responseDto = new ContestResponseDto
            //{
            //    Id = contest.Id,
            //    Name = contest.Name
            //};

            return await Response.SuccessAsync(null, "Contest deleted successfully", System.Net.HttpStatusCode.OK);
        }
    }
}
