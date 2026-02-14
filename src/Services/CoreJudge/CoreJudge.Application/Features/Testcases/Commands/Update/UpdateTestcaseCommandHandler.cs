using AutoMapper;
using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using MediatR;

namespace CoreJudge.Application.Features.Testcases.Commands.Update
{
    public class UpdateTestcaseCommandHandler : ICommandHandler<UpdateTestcaseCommand, Response>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTestcaseCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response> Handle(UpdateTestcaseCommand request, CancellationToken cancellationToken)
        {
            var existingTestcase = await _unitOfWork.Repository<Testcase>().GetByIdAsync(request.TestcaseId);
            if (existingTestcase == null)
                return await Response.FailureAsync("Testcase not found!", System.Net.HttpStatusCode.NotFound);

            var problem = await _unitOfWork.Repository<Problem>().GetByIdAsync(request.ProblemId);
            if (problem == null)
                return await Response.FailureAsync("Problem not found!", System.Net.HttpStatusCode.NotFound);

            _mapper.Map(request, existingTestcase);

            await _unitOfWork.Repository<Testcase>().UpdateAsync(existingTestcase);
            await _unitOfWork.CompleteAsync();

            return await Response.SuccessAsync(existingTestcase, "Test case updated successfully.", System.Net.HttpStatusCode.OK);
        }
    }
}
