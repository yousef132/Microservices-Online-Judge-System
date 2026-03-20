using BuildingBlocks.Core.CQRS;
using CoreJudge.Domain.Premitives;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CoreJudge.Application.Features.Problems.Commands.Run
{
    public class RunCodeCommand : ICommand<Response>
    {
        public Language Language { get; set; }
        public IFormFile Code { get; set; }
        public int ProblemId { get; set; }
        public string CustomTestcasesJson { get; set; }
    }
}
