namespace CodeSphere.Domain.Abstractions.Repositories
{
    public interface ITopicRepository
    {
        Task<List<string>> GetTopicNamesByIdsAsync(IEnumerable<int> topicIds);
        Task<List<int>> GetTopicIDsByNamesAsync(IEnumerable<string> topicsNames);
        Task<bool> AllTopicsFound(IEnumerable<int> topicIds);

    }
}
