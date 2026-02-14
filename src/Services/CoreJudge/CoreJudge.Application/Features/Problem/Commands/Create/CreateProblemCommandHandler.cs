using AutoMapper;
using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CodeSphere.Domain.Abstractions.Repositories;
using CoreJudge.Application.Abstractions.Repositories;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using MediatR;
using System.Net;

namespace CoreJudge.Application.Features.Problems.Commands.Create
{
    public class CreateProblemCommandHandler :
        ICommandHandler<CreateProblemCommand, Response>
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IElasticSearchRepository elasticSearchRepository;

        public CreateProblemCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IElasticSearchRepository elasticSearchRepository)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.elasticSearchRepository = elasticSearchRepository;
        }
        public async Task<Response> Handle(CreateProblemCommand request, CancellationToken cancellationToken)
        {
            var contest = await unitOfWork.Repository<Contest>().GetByIdAsync(request.ContestId);
            if (contest == null)
                return await Response.FailureAsync("Contest Not Found!!", System.Net.HttpStatusCode.NotFound);


            // TODO : TO MINIMIZE DB CALLS => load all topics in memory then check if incomming topics are valid or not
            Topic currentTopic = default;
            foreach (var topic in request.Topics)
            {
                currentTopic = await unitOfWork.Repository<Topic>().GetByIdAsync(topic);
                if (currentTopic == null)
                    return await Response.FailureAsync($"Topic {topic} Not Found !!", System.Net.HttpStatusCode.NotFound);
            }


            var mappedProblem = mapper.Map<Problem>(request);

            await unitOfWork.Repository<Problem>().AddAsync(mappedProblem);
            await unitOfWork.CompleteAsync();

            //var document = new ProblemDocument
            //{
            //    Difficulty = mappedProblem.Difficulty,
            //    Id = mappedProblem.Id,
            //    Name = mappedProblem.Name,
            //    Topics = request.Topics
            //};

            //var result = await elasticSearchRepository.IndexDocumentAsync(document, "problems");

            //if (!result)
            //    return await Response.FailureAsync("failed to index problem", HttpStatusCode.Created);

            //var response = mapper.Map<CreateProblemCommandResponse>(mappedProblem);

            return await Response.SuccessAsync(null, "Problem added successfully", HttpStatusCode.Created);
        }
    }
}
