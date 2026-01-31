using Microsoft.EntityFrameworkCore;
using Users.API.Models;

namespace Users.API.Repository.Implementations;

public class UserRepository :GenericRepository<User,int>, IUserRepository
{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context) : base(context)
    {
        _context = context;
    }
    public Task<User?> GetByEmail(string email)
    {
        return _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}