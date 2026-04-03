using AutoMapper;
using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CoreJudge.Domain.Events;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using System.Net;

namespace CoreJudge.Application.Features.Problems.Commands.Create
{
    public class CreateProblemCommandHandler :
        ICommandHandler<CreateProblemCommand, Response>
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly MassTransit.IPublishEndpoint publishEndpoint;

        public CreateProblemCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, MassTransit.IPublishEndpoint publishEndpoint)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.publishEndpoint = publishEndpoint;
        }
        public async Task<Response> Handle(CreateProblemCommand request, CancellationToken cancellationToken)
        {
            var contest = await unitOfWork.Repository<Contest>().GetByIdAsync(request.ContestId);
            if (contest == null)
                return await Response.FailureAsync("Contest Not Found!!", System.Net.HttpStatusCode.NotFound);


            bool allTopicsFound = await unitOfWork.TopicRepository.AllTopicsFound(request.Topics);

            if (!allTopicsFound)
                return await Response.FailureAsync("One or more topics not found !!", System.Net.HttpStatusCode.NotFound);

            try
            {
                var mappedProblem = mapper.Map<Domain.Models.Problem>(request);
                mappedProblem.LanguagesTemplages = request.CodeTemplate.Select(t => new ProblemLangeuageTemplates
                {
                    Language = t.Language,
                    StartingPoint = t.StartingPoint,
                    UserCodeTemplate = t.CodeTemplate,
                    UserCodeWrapper = t.CodeWrapper,

                }).ToList();

                await unitOfWork.Repository<Domain.Models.Problem>().AddAsync(mappedProblem);
                //since we configured outbox by masstransit, it will intercept the publish network call and 
                // add this message to EF core Change tracker as an OutboxMessage entity [in memory]
                // then when we call unitOfWork.CompleteAsync(), it will save the problem and the outbox message in the same transaction
                // then a background worker will send the message to the broker
                // ***** MASSTRANSIT HANDLES ALL THAT STUFF *****
                await unitOfWork.CompleteAsync(); // TODO: user Domain Events and ID will be generated in app layer 
                await publishEndpoint.Publish(new ProblemCreatedEvent
                {
                    ProblemId = mappedProblem.Id,
                    Title = mappedProblem.Name, // Note: Problem model seems to have 'Name'
                    Difficulty = mappedProblem.Difficulty.ToString()
                }, cancellationToken);

                await unitOfWork.CompleteAsync();
                return await Response.SuccessAsync(null, "Problem added successfully", HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return await Response.FailureAsync($"Error while creating problem: {ex.Message}", HttpStatusCode.InternalServerError);
            }

        }
    }
}
