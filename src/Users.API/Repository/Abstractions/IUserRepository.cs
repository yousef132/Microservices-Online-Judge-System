using Users.API.Models;

namespace Users.API.Repository;

public interface IUserRepository : IGenericRepository<User,int>
{
    public Task<User?> GetByEmail(string email);
}
