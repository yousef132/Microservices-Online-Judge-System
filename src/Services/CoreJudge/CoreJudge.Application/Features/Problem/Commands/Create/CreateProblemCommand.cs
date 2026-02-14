using BuildingBlocks.Core.CQRS;
using CoreJudge.Domain.Models.Entities;
using CoreJudge.Domain.Premitives;
using MediatR;

namespace CoreJudge.Application.Features.Problems.Commands.Create
{
    public class CreateProblemCommand : ICommand<Response>
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public int ContestId { get; init; }
        public string ProblemSetterId { get; init; }
        public Difficulty Difficulty { get; init; }
        public decimal RunTimeLimit { get; init; }
        public MemoryLimit MemoryLimit { get; init; }
        public IReadOnlyList<int> Topics { get; init; }

    }
}
