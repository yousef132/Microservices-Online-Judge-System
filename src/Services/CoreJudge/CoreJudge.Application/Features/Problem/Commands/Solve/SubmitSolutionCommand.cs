using BuildingBlocks.Core.CQRS;
using CoreJudge.Domain.Premitives;
using Microsoft.AspNetCore.Http;

namespace CoreJudge.Application.Features.Problems.Commands.SolveProblem
{
    public class SubmitSolutionCommand : ICommand<Response>
    {
        public int ProblemId { get; set; }
        public IFormFile Code { get; set; }
        public int ContestId { get; set; }
        public Language Language { get; set; }
    }
}
