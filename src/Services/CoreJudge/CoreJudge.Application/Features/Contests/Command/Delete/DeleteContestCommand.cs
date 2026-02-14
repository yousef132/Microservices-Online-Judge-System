using BuildingBlocks.Core.CQRS;
using CoreJudge.Domain.Premitives;
using MediatR;

namespace CoreJudge.Application.Features.Contests.Command.Delete
{
    public class DeleteContestCommand : ICommand<Response>
    {
        public int Id { get; set; }
    }
}
