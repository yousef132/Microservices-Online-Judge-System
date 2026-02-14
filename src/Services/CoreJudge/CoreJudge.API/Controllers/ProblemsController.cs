
using CoreJudge.Application.Features.Problems.Commands.Create;
using CoreJudge.Application.Features.Problems.Commands.Delete;
using CoreJudge.Application.Features.Problems.Commands.Run;
using CoreJudge.Application.Features.Problems.Commands.SolveProblem;
using CoreJudge.Application.Features.Problems.Queries.GetAll;
using CoreJudge.Application.Features.Problems.Queries.GetById;
using CoreJudge.Domain.Models.Entities;
using CoreJudge.Domain.Premitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreJudge.API.Controllers
{

    public class ProblemsController : BaseController
    {

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]

        public async Task<ActionResult<Response>> CreateProblemAsync([FromBody] CreateProblemCommand command)
         => ResponseResult(await mediator.Send(command));

        [HttpPost("solve")]
        //[RateLimitingFilter(5)]
        [Authorize]

        public async Task<ActionResult<Response>> SolveProblemAsync([FromForm] SubmitSolutionCommand command)
         => ResponseResult(await mediator.Send(command));



        [HttpPost("run")]
        [Authorize]
        //[RateLimitingFilter(5)]
        public async Task<ActionResult<Response>> RunProblemTestcasesAsync([FromForm] RunCodeCommand command)
         => ResponseResult(await mediator.Send(command));



        [HttpDelete("{problemId}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<Response>> DeleteProblemAsync([FromRoute] int problemId)
                     => ResponseResult(await mediator.Send(new DeleteProblemCommand(problemId)));

        [HttpGet("{problemId}")]
        public async Task<ActionResult<Response>> GetProblemById([FromRoute] int problemId)
                   => ResponseResult(await mediator.Send(new GetProblemByIdQuery(problemId)));

        [HttpGet]
        public async Task<ActionResult<Response>> GetProblemsAsync(
            [FromQuery] List<int>? Topics,
            [FromQuery] string? problemName,
            [FromQuery] Difficulty? difficulty,
            [FromQuery] ProblemStatus? status,
            [FromQuery] SortBy sortBy = SortBy.Name,
            [FromQuery] Order order = Order.Ascending,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetAllProblemsQuery(null,
                Topics,
                problemName,
                difficulty,
                pageNumber,
                pageSize,
                status,
                sortBy,
                order);
            return ResponseResult(await mediator.Send(query));
        }
    }
}
