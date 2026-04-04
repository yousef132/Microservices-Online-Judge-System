using BuildingBlocks.Core.CQRS;
using CoreJudge.Domain.Premitives;

namespace CoreJudge.Application.Features.TestCases.Commands.BulkCreate
{
    public class BulkCreateTestcasesCommand : ICommand<Response>
    {
        public int ProblemId { get; set; }
        public List<TestcaseDto> Testcases { get; set; } = new();
    }

    public class TestcaseDto
    {
        public string Input { get; set; } = default!;
        public string ExpectedOutput { get; set; } = default!;
    }
}
