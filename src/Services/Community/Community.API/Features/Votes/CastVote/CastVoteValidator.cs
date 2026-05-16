using FluentValidation;

namespace Community.API.Features.Votes.CastVote;

public class CastVoteValidator : AbstractValidator<CastVoteCommand>
{
    public CastVoteValidator()
    {
        RuleFor(x => x.TargetType).Must(t => t is "Article" or "Comment")
            .WithMessage("TargetType must be 'Article' or 'Comment'.");
        RuleFor(x => x.Value).Must(v => v is 1 or -1)
            .WithMessage("Value must be 1 or -1.");
    }
}
