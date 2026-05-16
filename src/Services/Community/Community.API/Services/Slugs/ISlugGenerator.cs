namespace Community.API.Services.Slugs;

public interface ISlugGenerator
{
    Task<string> GenerateSlugAsync(string title);
}
