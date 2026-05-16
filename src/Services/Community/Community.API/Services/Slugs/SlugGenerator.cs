using System.Text.RegularExpressions;
using Community.API.Persistence;

namespace Community.API.Services.Slugs;

public partial class SlugGenerator(IArticleRepository articleRepository) : ISlugGenerator
{
    public async Task<string> GenerateSlugAsync(string title)
    {
        var baseSlug = SlugRegex().Replace(title.ToLowerInvariant(), "")
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');

        if (string.IsNullOrWhiteSpace(baseSlug))
            baseSlug = "article";

        var slug = baseSlug;
        var suffix = 1;
        while (await articleRepository.SlugExistsAsync(slug))
            slug = $"{baseSlug}-{++suffix}";

        return slug;
    }

    [GeneratedRegex("[^a-z0-9\\s-]", RegexOptions.Compiled)]
    private static partial Regex SlugRegex();
}
