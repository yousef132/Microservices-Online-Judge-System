using FluentValidation;
namespace CoreJudge.Application.Features.TestCases.Commands.Create;
    public class CreateTestcaseCommandValidator : AbstractValidator<CreateTestcaseCommand>
    {
        public CreateTestcaseCommandValidator()
        {
            RuleFor(x => x.ProblemId)
               .NotEmpty().WithMessage("ProblemId is required.");

            RuleFor(x => x.Input)
                .NotEmpty().WithMessage("Input is required.")
                .MinimumLength(1).WithMessage("Input must be at least 1 character long.");

            RuleFor(x => x.ExpectedOutput)
                .NotEmpty().WithMessage("Expected output is required.")
                .MinimumLength(1).WithMessage("Output must be at least 1 character long.");
        }
    }
