
using CoreJudge.Domain.Models;

namespace CodeSphere.Domain.Abstractions.Repositories
{
    public interface IBlogRepository
    {
        // Problem methods
        Task<IEnumerable<Problem>> GetProblemsForBlogAsync();
        Task AddProblemToBlogAsync(int blogId, int problemId);
        // Solution methods
        Task AddSolutionToBlogAsync(int blogId, string solutionContent);
    }
}
