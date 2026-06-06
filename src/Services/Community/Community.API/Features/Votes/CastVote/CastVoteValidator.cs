using FluentValidation;

namespace Community.API.Features.Votes.CastVote;

public class CastVoteValidator : AbstractValidator<CastVoteCommand>
{
    public CastVoteValidator()
    {
        RuleFor(x => x.ArticleId).NotEmpty()
            .WithMessage("ArticleId is required.");
        RuleFor(x => x.Value).Must(v => v is 1 or -1)
            .WithMessage("Value must be 1 or -1.");
    }
}
