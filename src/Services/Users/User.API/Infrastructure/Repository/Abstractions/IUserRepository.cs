using Users.API.Domain.Models;

namespace Users.API.Infrastructure.Repository.Abstractions;


public interface IUserRepository : IGenericRepository<Domain.Models.User, Guid>
{
    public Task<Domain.Models.User?> GetByEmail(string email);
}
