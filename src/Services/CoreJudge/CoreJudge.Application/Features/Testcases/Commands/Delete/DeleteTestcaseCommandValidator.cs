using FluentValidation;

namespace CoreJudge.Application.Features.Testcases.Commands.Delete
{
    public class DeleteTestcaseCommandValidator : AbstractValidator<DeleteTestcaseCommand>
    {
        public DeleteTestcaseCommandValidator()
        {
            RuleFor(x => x.TestcaseId)
                .NotEmpty().WithMessage("TestcaseId is required.");
        }
    }
}
