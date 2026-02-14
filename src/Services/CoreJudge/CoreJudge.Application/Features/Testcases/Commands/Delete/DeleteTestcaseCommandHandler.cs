using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;
using MediatR;

namespace CoreJudge.Application.Features.Testcases.Commands.Delete
{
    public class DeleteTestcaseCommandHandler : ICommandHandler<DeleteTestcaseCommand, Response>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteTestcaseCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response> Handle(DeleteTestcaseCommand request, CancellationToken cancellationToken)
        {
            var testcase = await _unitOfWork.Repository<Testcase>().GetByIdAsync(request.TestcaseId);
            if (testcase == null)
                return await Response.FailureAsync("Test case not found!", System.Net.HttpStatusCode.NotFound);

            await _unitOfWork.Repository<Testcase>().DeleteAsync(testcase);
            await _unitOfWork.CompleteAsync();

            return await Response.SuccessAsync(null, "Test case deleted successfully.", System.Net.HttpStatusCode.NoContent);
        }
    }
}
