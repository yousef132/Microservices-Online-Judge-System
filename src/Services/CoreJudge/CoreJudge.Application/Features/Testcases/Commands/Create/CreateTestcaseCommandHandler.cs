using AutoMapper;
using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using MediatR;

namespace CoreJudge.Application.Features.TestCases.Commands.Create
{
    public class CreateTestcaseCommandHandler : ICommandHandler<CreateTestcaseCommand, Response>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTestcaseCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response> Handle(CreateTestcaseCommand request, CancellationToken cancellationToken)
        {
            var problem = await _unitOfWork.Repository<Problem>().GetByIdAsync(request.ProblemId);
            if (problem == null)
                return await Response.FailureAsync("Problem not found!", System.Net.HttpStatusCode.NotFound);

            var newTestcase = new Testcase(request.ProblemId , request.Input, request.ExpectedOutput);


            await _unitOfWork.Repository<Testcase>().AddAsync(newTestcase);
            await _unitOfWork.CompleteAsync();

            return await Response.SuccessAsync(newTestcase, "Test case added successfully.", System.Net.HttpStatusCode.Created);
        }
    }
}
