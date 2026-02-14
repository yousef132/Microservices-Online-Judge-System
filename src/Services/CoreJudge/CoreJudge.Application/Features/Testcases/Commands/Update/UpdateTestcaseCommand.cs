using BuildingBlocks.Core.CQRS;
using CoreJudge.Domain.Premitives;
using MediatR;


namespace CoreJudge.Application.Features.Testcases.Commands.Update
{
    public class UpdateTestcaseCommand : ICommand<Response>
    {
        public int TestcaseId { get; set; } 
        public int ProblemId { get; set; }
        public string Input { get; set; }
        public string ExpectedOutput { get; set; }  

    }
}
