
using CoreJudge.Domain.Models;
using CoreJudge.Domain.Premitives;

namespace CodeSphere.Domain.Abstractions.Repositories
{
    public interface IProblemRepository
    {
        IQueryable<Testcase> GetTestCasesByProblemId(int problemId);

        Task<Problem?> GetProblemDetailsAsync(int problemId, CancellationToken cancellationToken = default);

        int GetAcceptedProblemCount(int problemId, CancellationToken cancellationToken = default);
        int GetSubmissionsProblemCount(int problemId, CancellationToken cancellationToken = default);
        Task<Problem?> GetProblemIncludingContestAndTestcases(int problemId, Language? language = null);
        bool CheckUserSolvedProblem(int problemId, string userId, CancellationToken cancellationToken = default);
        Task<List<ProblemLangeuageTemplates>> GetProblemPlaceHolders(int problemId, CancellationToken cancellationToken = default);

    }
}
