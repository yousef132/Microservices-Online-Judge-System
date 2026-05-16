using FluentValidation;

namespace Community.API.Features.Articles.CreateArticle;

public class CreateArticleValidator : AbstractValidator<CreateArticleCommand>
{
    public CreateArticleValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(50000);
        RuleFor(x => x.Tags).NotEmpty().Must(t => t.Count is > 0 and <= 5)
            .WithMessage("An article must have between 1 and 5 tags.");
        RuleForEach(x => x.Tags).NotEmpty().Matches("^[a-z0-9-]+$").MaximumLength(30)
            .WithMessage("Tags must be lowercase, contain only a-z, 0-9, or '-', and be max 30 chars.");
        RuleFor(x => x.Status).Must(s => s == "Draft" || s == "Published")
            .WithMessage("Status must be 'Draft' or 'Published'.");
    }
}
