namespace Community.API.Common.DTOs;

public class PaginatedListDto<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
