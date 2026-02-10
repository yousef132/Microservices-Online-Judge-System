using Users.API.Infrastructure.Repository.Abstractions;

namespace Users.API.Infrastructure.Repository.Implementations;

public class GenericRepository<T,TKey> (UserDbContext context) : IGenericRepository<T, TKey> where T : class
{
    public virtual async Task<T?> GetById(TKey id, CancellationToken cancellationToken = default)
        => await context.Set<T>().FindAsync(id,cancellationToken);
    
    public async Task AddAsync(T entity,CancellationToken cancellationToken = default)
      => await context.Set<T>().AddAsync(entity,cancellationToken);
    

    public void Remove(T entity)
    =>  context.Set<T>().Remove(entity);
    public void Update(T entity)
    => context.Set<T>().Update(entity); 
}