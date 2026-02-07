using Users.API.Domain.Models;

namespace Users.API.Infrastructure.Repository.Abstractions;


public interface IUserRepository : IGenericRepository<User, Guid>
{
    public Task<User?> GetByEmail(string email);
}
