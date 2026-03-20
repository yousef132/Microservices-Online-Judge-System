
using CoreJudge.Domain.Models;

namespace CodeSphere.Domain.Abstractions.Repositories
{
    public interface IContestRepository
    {
        Task<IReadOnlyList<Problem>> GetContestProblemsByIdAsync(int contestId);
        Task<IEnumerable<Tuple<Contest, bool>>> GetAllContestWithRegisteredUserAsync(string? userId);

        //Task<IReadOnlyList<StandingDto>> GetContestStanding(int contestId, int index, int pageSize);


        Task<bool> IsRegistered(string userId, int contestId);
    }
}
