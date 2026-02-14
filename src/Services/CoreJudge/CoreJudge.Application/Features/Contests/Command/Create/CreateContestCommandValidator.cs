using FluentValidation;

namespace CoreJudge.Application.Features.Contests.Command.Create
{
    public class CreateContestCommandValidator : AbstractValidator<CreateContestCommand>
    {
        public CreateContestCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().NotNull();
            RuleFor(x => x.Description).NotEmpty().NotNull();
            RuleFor(x => x.StartTime).NotEmpty().NotNull();
            RuleFor(x => x.EndTime).NotEmpty().NotNull();
        }
    }
}
