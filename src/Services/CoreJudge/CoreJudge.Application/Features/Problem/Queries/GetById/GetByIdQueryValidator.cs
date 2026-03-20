using FluentValidation;

namespace CoreJudge.Application.Features.Problems.Queries.GetById
{
    public class GetByIdQueryValidator : AbstractValidator<GetProblemByIdQuery>
    {
        public GetByIdQueryValidator()
        {

            RuleFor(x => x.ProblemId)
                .NotEmpty()
                .NotNull();
        }
    }
}
