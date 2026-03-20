using BuildingBlocks.Core.CQRS;
using CoreJudge.Domain.Premitives;
using MediatR;

namespace CoreJudge.Application.Features.Contests.Command.Register
{
    public class RegisterInContestCommand : ICommand<Response>
    {
        public int Id { get; set; }

        public RegisterInContestCommand(int id)
        {
            this.Id = id;
        }
    }
}
