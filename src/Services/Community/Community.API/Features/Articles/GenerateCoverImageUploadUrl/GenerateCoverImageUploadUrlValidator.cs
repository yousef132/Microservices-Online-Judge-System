using FluentValidation;

namespace Community.API.Features.Articles.GenerateCoverImageUploadUrl;

public class GenerateCoverImageUploadUrlValidator : AbstractValidator<GenerateCoverImageUploadUrlCommand>
{
    public GenerateCoverImageUploadUrlValidator()
    {
        RuleFor(x => x.ContentType)
            .Must(ct => ct is "image/jpeg" or "image/png" or "image/webp")
            .WithMessage("Content type must be one of: image/jpeg, image/png, image/webp.");
    }
}
