using FluentValidation;

namespace CoreJudge.Application.Features.Contests.Queries.GetContestProblems
{
    public class GetContestProblemsQueryValidator : AbstractValidator<GetContestProblemsQuery>
    {
        public GetContestProblemsQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

}
