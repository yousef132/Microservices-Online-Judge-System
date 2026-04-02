using FluentValidation;

namespace CoreJudge.Application.Features.TestCases.Commands.BulkCreate
{
    public class BulkCreateTestcasesCommandValidator : AbstractValidator<BulkCreateTestcasesCommand>
    {
        public BulkCreateTestcasesCommandValidator()
        {
            RuleFor(x => x.ProblemId)
                .GreaterThan(0).WithMessage("ProblemId is required.");

            RuleFor(x => x.Testcases)
                .NotEmpty().WithMessage("At least one testcase is required.")
                .Must(list => list.Count <= 500).WithMessage("Cannot add more than 500 testcases at once.");

            RuleForEach(x => x.Testcases).ChildRules(tc =>
            {
                tc.RuleFor(t => t.Input)
                    .NotEmpty().WithMessage("Testcase input is required.");

                tc.RuleFor(t => t.ExpectedOutput)
                    .NotEmpty().WithMessage("Testcase expected output is required.");
            });
        }
    }
}
