namespace Users.API.Infrastructure.Repository.Abstractions;
    public interface IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginPessimisticTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitPessimisticTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }