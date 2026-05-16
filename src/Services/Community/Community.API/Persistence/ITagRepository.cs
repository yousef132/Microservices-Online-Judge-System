namespace Community.API.Persistence;

public interface ITagRepository
{
    Task IncrementTagsAsync(IEnumerable<string> tagNames);
    Task DecrementTagsAsync(IEnumerable<string> tagNames);
}
