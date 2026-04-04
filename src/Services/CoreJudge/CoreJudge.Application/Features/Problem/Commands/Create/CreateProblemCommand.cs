using BuildingBlocks.Core.CQRS;
using CoreJudge.Domain.Models.Entities;
using CoreJudge.Domain.Premitives;

namespace CoreJudge.Application.Features.Problems.Commands.Create
{
    public class CreateProblemCommand : ICommand<Response>
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public List<ProblemCodeTemplate> CodeTemplate { get; init; }
        public int ContestId { get; init; }
        public Difficulty Difficulty { get; init; }
        public decimal RunTimeLimit { get; init; }
        public MemoryLimit MemoryLimit { get; init; }
        public IReadOnlyList<int> Topics { get; init; }

    }

    public record ProblemCodeTemplate
    {
        public string CodeWrapper { get; set; }
        public string StartingPoint { get; set; }
        public string CodeTemplate { get; set; }
        public Language Language { get; set; }

    }
}
