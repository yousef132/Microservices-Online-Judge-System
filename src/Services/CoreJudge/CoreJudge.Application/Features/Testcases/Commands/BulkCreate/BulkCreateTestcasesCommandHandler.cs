using BuildingBlocks.Core.CQRS;
using CodeSphere.Domain.Abstractions;
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;

namespace CoreJudge.Application.Features.TestCases.Commands.BulkCreate
{
    public class BulkCreateTestcasesCommandHandler : ICommandHandler<BulkCreateTestcasesCommand, Response>
    {
        private readonly IUnitOfWork _unitOfWork;

        public BulkCreateTestcasesCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response> Handle(BulkCreateTestcasesCommand request, CancellationToken cancellationToken)
        {
            var problem = await _unitOfWork.Repository<Domain.Models.Problem>().GetByIdAsync(request.ProblemId);
            if (problem == null)
                return await Response.FailureAsync("Problem not found!", System.Net.HttpStatusCode.NotFound);

            if (request.Testcases == null || request.Testcases.Count == 0)
                return await Response.FailureAsync("At least one testcase is required.", System.Net.HttpStatusCode.BadRequest);

            var testcases = request.Testcases
                .Select(tc => new Testcase(request.ProblemId, tc.Input, tc.ExpectedOutput))
                .ToList();

            foreach (var tc in testcases)
                await _unitOfWork.Repository<Testcase>().AddAsync(tc);

            await _unitOfWork.CompleteAsync();

            return await Response.SuccessAsync(
                new { count = testcases.Count },
                $"{testcases.Count} testcase(s) added successfully.",
                System.Net.HttpStatusCode.Created);
        }
    }
}
