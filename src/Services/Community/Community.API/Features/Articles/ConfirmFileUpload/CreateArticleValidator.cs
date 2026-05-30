using Community.API.Features.Articles.CreateArticle;
using FluentValidation;

namespace Community.API.Features.Articles.ConfirmFileUpload;

public class ConfirmFileUploadValidator : AbstractValidator<UploadFileCommand>
{
    public ConfirmFileUploadValidator()
    {
        RuleFor(x => x.ArticleId).NotEqual(Guid.Empty);
        RuleFor(x => x.IsUploaded).Equal(true);
        RuleFor(x => x.ObjectKey).NotEmpty().MaximumLength(200);
    }
}
