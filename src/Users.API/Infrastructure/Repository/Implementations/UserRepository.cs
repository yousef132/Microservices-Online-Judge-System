using Microsoft.EntityFrameworkCore;
using Users.API.Domain.Models;
using Users.API.Infrastructure.Repository.Abstractions;

namespace Users.API.Infrastructure.Repository.Implementations;

public class UserRepository :GenericRepository<User,Guid>, IUserRepository
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

    public override async Task<User?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Keycloak_Id == id, cancellationToken);
    }

}