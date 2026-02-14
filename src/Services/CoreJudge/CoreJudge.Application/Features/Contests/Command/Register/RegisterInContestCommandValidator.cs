using FluentValidation;

namespace CoreJudge.Application.Features.Contests.Command.Register
{
    public class RegisterInContestCommandValidator : AbstractValidator<RegisterInContestCommand>
    {
        public RegisterInContestCommandValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty();
        }
    }
}
