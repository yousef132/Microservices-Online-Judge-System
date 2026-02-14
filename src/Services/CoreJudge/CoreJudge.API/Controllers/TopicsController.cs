
using CoreJudge.Application.Features.Topics.Commands;
using CoreJudge.Application.Features.Topics.Queries.GetAll;
using CoreJudge.Domain.Premitives;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreJudge.API.Controllers
{
    public class TopicsController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<Response>> GetTopics() 
            => Ok(await mediator.Send(new GetAllTopicsQuery()));

        [HttpPost]
        public async Task<ActionResult<Response>> CreateTopic([FromBody] CreateTopicCommand command)
            => Ok(await mediator.Send(command));
    }
}
