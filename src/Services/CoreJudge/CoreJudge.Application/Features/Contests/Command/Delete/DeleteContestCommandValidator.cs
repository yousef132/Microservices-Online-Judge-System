using FluentValidation;

namespace CoreJudge.Application.Features.Contests.Command.Delete
{
    public class DeleteContestCommandValidator : AbstractValidator<DeleteContestCommand>
    {
        public DeleteContestCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
