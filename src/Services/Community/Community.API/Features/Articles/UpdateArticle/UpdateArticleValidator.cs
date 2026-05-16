using FluentValidation;

namespace Community.API.Features.Articles.UpdateArticle;

public class UpdateArticleValidator : AbstractValidator<UpdateArticleCommand>
{
    public UpdateArticleValidator()
    {
        RuleFor(x => x.Title).MaximumLength(200).When(x => x.Title != null);
        RuleFor(x => x.Body).MaximumLength(50000).When(x => x.Body != null);
        RuleFor(x => x.Tags).Must(t => t!.Count is > 0 and <= 5)
            .WithMessage("An article must have between 1 and 5 tags.").When(x => x.Tags != null);
        RuleForEach(x => x.Tags).NotEmpty().Matches("^[a-z0-9-]+$").MaximumLength(30)
            .WithMessage("Tags must be lowercase, contain only a-z, 0-9, or '-', and be max 30 chars.")
            .When(x => x.Tags != null);
        RuleFor(x => x.Status).Must(s => s == "Published")
            .WithMessage("Status can only be changed to 'Published'.").When(x => x.Status != null);
    }
}
