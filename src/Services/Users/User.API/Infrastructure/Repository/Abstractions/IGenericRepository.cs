namespace Users.API.Infrastructure.Repository.Abstractions;

public interface IGenericRepository<T, TKey> where T : class
{
    Task<T?> GetById(TKey id,CancellationToken cancellationToken = default);
    Task AddAsync(T entity,CancellationToken cancellationToken = default);
    void Remove(T entity);
    void Update(T entity);
}
