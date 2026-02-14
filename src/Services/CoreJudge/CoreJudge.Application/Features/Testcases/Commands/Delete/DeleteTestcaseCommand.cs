using BuildingBlocks.Core.CQRS;
using CoreJudge.Domain.Premitives;
using MediatR;

namespace CoreJudge.Application.Features.Testcases.Commands.Delete
{
    public sealed record DeleteTestcaseCommand(int TestcaseId) : ICommand<Response>;
}
