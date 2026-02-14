using CodeSphere.Domain.Abstractions.Repositories;
using CoreJudge.Domain.Models;
using CoreJudge.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CoreJudge.Infrastructure.Implementation.Repositories
{
    public class UserContestRepository : IUserContestRepository
    {
        private readonly ApplicationDbContext _context;

        public UserContestRepository(ApplicationDbContext _context)
        {
            this._context = _context;
        }
        public async Task<UserContest?> GetUserContest(string userId, int contestId)
          => await _context.Registers.FirstOrDefaultAsync(r => r.UserId == Guid.Parse(userId) && r.ContestId == contestId);
       
        
        public async Task<UserContest?> IsRegistered(int contestId, string userId)
            => await _context.Registers.FirstOrDefaultAsync(x => x.ContestId == contestId && x.UserId == Guid.Parse(userId));
        public async Task<bool> RegisterInContest(UserContest registration)
        {
            try
            {
                await _context.Registers.AddAsync(registration);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw;
            }
        }
    }
}
